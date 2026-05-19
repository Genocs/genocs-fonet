using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;

namespace Genocs.Fonet.Image;

/// <summary>
/// Creates FonetImage instances.
/// </summary>
internal class FonetImageFactory
{
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
    ///     Creates a FonetImage from the supplied resource locator.  The 
    ///     FonetImageFactory does cache images, therefore this method may 
    ///     return a reference to an existing FonetImage
    /// </summary>
    /// <param name="href">A Uniform Resource Identifier</param>
    /// <returns>A reference to a  FonetImage</returns>
    /// <exception cref="FonetImageException"></exception>
    public static FonetImage Make(string href)
    {
        if (FonetDriver.ActiveDriver?.ImageHandler != null)
        {
            byte[] data = FonetDriver.ActiveDriver.ImageHandler(href);
            if (data != null)
            {
                return new FonetImage(href, data);
            }
        }

        Uri? absoluteURL;
        UriSpecificationParser up = new UriSpecificationParser(href);
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
                string baseDir = FonetDriver.ActiveDriver?.BaseDirectory.FullName;
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

        return new FonetImage(
            absoluteURL.AbsoluteUri,
            ExtractImageData(absoluteURL));
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
            var credential = FonetDriver.ActiveDriver.Credentials?.GetCredential(uri, "Basic")
                ?? FonetDriver.ActiveDriver.Credentials?.GetCredential(uri, "Digest");
            if (credential != null)
            {
                string authValue = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{credential.UserName}:{credential.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            }

            using var cts = new CancellationTokenSource(FonetDriver.ActiveDriver.Timeout);
            var response = SharedHttpClient.Send(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new FonetImageException(
                    $"HTTP {(int)response.StatusCode} while fetching image from {uri}");
            }

            return response.Content.ReadAsStream();
        }
        catch (FonetImageException)
        {
            throw;
        }
        catch (SecurityException se)
        {
            throw new FonetImageException(
                String.Format("Detected security exception while fetching image from {0}: {1}", uri, se.Message));
        }
        catch (UriFormatException ue)
        {
            throw new FonetImageException(
                String.Format("Badly formed Uri {0}: {1}", uri, ue.Message));
        }
        catch (HttpRequestException we)
        {
            throw new FonetImageException(
                String.Format("Encountered web exception while fetching image from {0}: {1}", uri, we.Message));
        }
        catch (Exception e)
        {
            throw new FonetImageException(
                String.Format("Encountered unexpected exception while fetching image from {0}: {1}", uri, e.Message));
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
        finally
        {
            imageStream.Dispose();
        }
    }
}
