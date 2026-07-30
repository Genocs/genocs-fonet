using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;
using System.Text;
using SkiaSharp;
using Svg.Skia;

namespace Genocs.Fonet.Image;

/// <summary>
/// Creates FonetImage instances.
/// </summary>
internal sealed class FonetImageFactory
{
    private const int SvgSupersampleScale = 6;
    private const int SvgMaxRasterDimension = 4096;

    private static readonly HttpClient SharedHttpClient = CreateSharedHttpClient();

    private static HttpClient CreateSharedHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };
        return new HttpClient(handler, disposeHandler: true);
    }

    internal static FonetImage MakeFromResource(string key)
    {
        Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(key)
            ?? throw new FonetImageException($"Embedded image resource not found: {key}");

        byte[] buffer = new byte[s.Length];
        s.ReadExactly(buffer);

        return new FonetImage("file://" + key, buffer);
    }

    /// <summary>
    /// Creates a FonetImage from the supplied resource locator.  The 
    /// FonetImageFactory does cache images, therefore this method may 
    /// return a reference to an existing FonetImage
    /// </summary>
    /// <param name="href">A Uniform Resource Identifier</param>
    /// <returns>A reference to a  FonetImage</returns>
    /// <exception cref="FonetImageException"></exception>
    public static FonetImage Make(string href)
    {
        if (FonetDriver.ActiveDriver?.ImageHandler != null)
        {
            byte[]? data = FonetDriver.ActiveDriver!.ImageHandler(href);
            if (data != null)
            {
                return new FonetImage(href, NormalizeImageData(href, data));
            }
        }

        Uri? absoluteURL;
        UriSpecificationParser up = new(href);
        string path = up.Uri;

        try
        {
            absoluteURL = new Uri(path);
        }
        catch
        {
            if (File.Exists(path))
            {
                absoluteURL = new Uri("file://" + Path.Combine(Directory.GetCurrentDirectory(), path));
            }
            else
            {
                string baseDir = FonetDriver.ActiveDriver!.BaseDirectory!.FullName;
                string baseDirPath = Path.Combine(baseDir, path);
                if (File.Exists(baseDirPath))
                {
                    absoluteURL = new Uri("file://" + Path.Combine(Directory.GetCurrentDirectory(), baseDirPath));
                }
                else
                {
                    throw new FonetImageException("Unable to retrieve graphic from " + path);
                }
            }
        }

        byte[] imageData = ExtractImageData(absoluteURL);
        imageData = NormalizeImageData(absoluteURL.AbsoluteUri, imageData);
        return new FonetImage(absoluteURL.AbsoluteUri, imageData);
    }

    private static byte[] NormalizeImageData(string source, byte[] imageData)
    {
        if (!LooksLikeSvg(source, imageData))
        {
            return imageData;
        }

        return RasterizeSvgToPng(imageData, source);
    }

    private static bool LooksLikeSvg(string source, byte[] imageData)
    {
        if (source.StartsWith("data:image/svg+xml", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (source.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (Uri.TryCreate(source, UriKind.Absolute, out var uri)
            && uri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        int sampleLength = Math.Min(imageData.Length, 1024);
        if (sampleLength == 0)
        {
            return false;
        }

        string sampleText = Encoding.UTF8.GetString(imageData, 0, sampleLength);
        return sampleText.Contains("<svg", StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] RasterizeSvgToPng(byte[] svgData, string source)
    {
        try
        {
            using var stream = new MemoryStream(svgData);
            var svg = new SKSvg();
            SKPicture? picture = svg.Load(stream);

            if (picture == null)
            {
                throw new FonetImageException($"Unable to parse SVG image from {source}.");
            }

            SKRect bounds = picture.CullRect;
            int width = (int)Math.Ceiling(bounds.Width);
            int height = (int)Math.Ceiling(bounds.Height);

            if (width <= 0 || height <= 0)
            {
                width = 256;
                height = 256;
            }

            // Render the SVG on a higher resolution surface to reduce aliasing.
            int rasterWidth = Math.Clamp(width * SvgSupersampleScale, 1, SvgMaxRasterDimension);
            int rasterHeight = Math.Clamp(height * SvgSupersampleScale, 1, SvgMaxRasterDimension);

            using var surface = SKSurface.Create(new SKImageInfo(rasterWidth, rasterHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            if (surface == null)
            {
                throw new FonetImageException($"Unable to allocate raster surface for SVG image from {source}.");
            }

            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            float scaleX = bounds.Width > 0 ? rasterWidth / bounds.Width : 1f;
            float scaleY = bounds.Height > 0 ? rasterHeight / bounds.Height : 1f;
            canvas.Scale(scaleX, scaleY);
            canvas.Translate(-bounds.Left, -bounds.Top);
            canvas.DrawPicture(picture);
            canvas.Flush();

            using SKImage image = surface.Snapshot();
            using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);

            if (encoded == null)
            {
                throw new FonetImageException($"Unable to encode SVG image from {source} as PNG.");
            }

            return encoded.ToArray();
        }
        catch (FonetImageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FonetImageException($"Unable to rasterize SVG image from {source}: {ex.Message}", ex);
        }
    }

    private static Stream GetImageStream(Uri uri)
    {
        try
        {
            if (uri.IsFile)
            {
                return File.OpenRead(uri.LocalPath);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var credential = FonetDriver.ActiveDriver?.Credentials?.GetCredential(uri, "Basic")
                ?? FonetDriver.ActiveDriver?.Credentials?.GetCredential(uri, "Digest");

            if (credential != null)
            {
                string authValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{credential.UserName}:{credential.Password}"));

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            }

            using var cts = new CancellationTokenSource(FonetDriver.ActiveDriver!.Timeout);
            var response = SharedHttpClient.Send(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new FonetImageException($"HTTP {(int)response.StatusCode} while fetching image from {uri}");
            }

            return response.Content.ReadAsStream();
        }
        catch (FonetImageException)
        {
            throw;
        }
        catch (SecurityException se)
        {
            throw new FonetImageException($"Detected security exception while fetching image from {uri}: {se.Message}");
        }
        catch (UriFormatException ue)
        {
            throw new FonetImageException($"Badly formed Uri {uri}: {ue.Message}");
        }
        catch (HttpRequestException we)
        {
            throw new FonetImageException($"Encountered web exception while fetching image from {uri}: {we.Message}");
        }
        catch (Exception e)
        {
            throw new FonetImageException($"Encountered unexpected exception while fetching image from {uri}: {e.Message}");
        }
    }

    private static byte[] ExtractImageData(Uri absoluteURL)
    {
        Stream imageStream = GetImageStream(absoluteURL);

        try
        {
            using var ms = new MemoryStream();
            byte[] buf = new byte[4096];
            int numBytesRead;

            while ((numBytesRead = imageStream.Read(buf, 0, 4096)) != 0)
            {
                ms.Write(buf, 0, numBytesRead);
            }

            return ms.ToArray();
        }
        catch (Exception e)
        {
            // Send a warning to the driver that the image could not be read
            FonetDriver.ActiveDriver?.FireFonetWarning($"Unable to read image data from {absoluteURL}: {e.Message}");
        }
        finally
        {
            imageStream.Dispose();
        }

        return [];
    }
}
