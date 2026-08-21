using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Pdf.Filter;
using SkiaSharp;

namespace Genocs.Fonet.Image;

/// <summary>
/// A bitmap image that will be referenced by fo:external-graphic.
/// </summary>
internal sealed class FonetImage
{
    public const int DEFAULT_BITPLANES = 8;

    /// <summary>
    /// The width of the image in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the image in pixels.
    /// </summary>
    public int Height { get; private set; }

    // Bits per pixel
    private int _bitsPerPixel = 0;
    // Image data 
    private byte[]? _bitmaps = null;

    /// <summary>
    /// Filter that will be applied to image data
    /// </summary>
    private IFilter? filter = null;
    /// <summary>
    /// Constructs a new FonetImage using the supplied bitmap.
    /// </summary>
    /// <remarks>
    /// Does not hold a reference to the passed bitmap.  Instead the
    /// image data is extracted from <b>bitmap</b> on construction.
    /// </remarks>
    /// <param name="href">The location of <i>bitmap</i></param>
    /// <param name="imageData">The image data</param>
    public FonetImage(string href, byte[] imageData)
    {
        Uri = href;
        ColorSpace = new ColorSpace(ColorSpace.DeviceRgb);
        _bitsPerPixel = DEFAULT_BITPLANES; // 8

        // Detect image format and load image info using SkiaSharp
        using (var stream = new MemoryStream(imageData))
        using (var managedStream = new SKManagedStream(stream))
        using (var codec = SKCodec.Create(managedStream))
        {
            if (codec == null)
            {
                throw new FonetImageException("Unsupported or invalid image format.");
            }
            Width = codec.Info.Width;
            Height = codec.Info.Height;
        }

        this._bitmaps = imageData;
        ExtractImage(imageData);
    }
    /// <summary>
    /// Return the image URL.
    /// </summary>
    /// <returns>the image URL (as a string)</returns>
    public string Uri { get; }

    /// <summary>
    /// Return the number of bits per pixel. 
    /// </summary>
    /// <returns>number of bits per pixel</returns>
    public int BitsPerPixel
    {
        get
        {
            return _bitsPerPixel;
        }
    }

    /// <summary>
    /// Return the image data size
    /// </summary>
    /// <returns>The image data size</returns>
    public int BitmapsSize
    {
        get
        {
            return (_bitmaps != null) ? _bitmaps.Length : 0;
        }
    }

    /// <summary>
    /// Return the image data (uncompressed). 
    /// </summary>
    /// <returns>the image data</returns>
    public byte[]? Bitmaps
    {
        get
        {
            return _bitmaps;
        }
    }

    /// <summary>
    /// Return the image color space. 
    /// </summary>
    /// <returns>the image color space (Fonet.Datatypes.ColorSpace)</returns>
    public ColorSpace ColorSpace { get; private set; }

    /// <summary>
    /// Returns the filter that should be applied to the bitmap data.
    /// </summary>
    public IFilter? Filter
    {
        get
        {
            return filter;
        }
    }

    /// <summary>
    /// Extracts the raw data from the image into a byte array suitable
    /// for including in the PDF document.  The image is always extracted
    /// as a 24-bit RGB image, regardless of it's original colour space
    /// and colour depth.
    /// </summary>
    /// <param name="imageData">The original image bytes.</param>
    /// <returns>A byte array containing the raw 24-bit RGB data</returns>
    private void ExtractImage(byte[] imageData)
    {
        // Detect format from image data using SkiaSharp
        using var stream = new MemoryStream(imageData);
        using var managedStream = new SKManagedStream(stream);
        using var codec = SKCodec.Create(managedStream) ?? throw new FonetImageException("Unable to decode image data.");

        // This should be a factory when we handle more image types
        if (String.Equals(codec.EncodedFormat.ToString(), "Jpeg", StringComparison.OrdinalIgnoreCase))
        {
            JpegParser parser = new JpegParser(_bitmaps);
            JpegInfo info = parser.Parse();
            _bitsPerPixel = info.BitsPerSample;
            ColorSpace = new ColorSpace(info.ColourSpace);
            Width = info.Width;
            Height = info.Height;

            // A "no-op" filter since the JPEG data is already compressed
            filter = new DctFilter();
        }
        else
        {
            ExtractOtherImageBits(imageData);
            // Performs zip compression
            filter = new FlateFilter();
        }
    }
    private void ExtractOtherImageBits(byte[] imageData)
    {
        try
        {
            using var source = SKBitmap.Decode(imageData) ?? throw new FonetImageException("Unable to decode image data.");
            using var bitmap = new SKBitmap(source.Width, source.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);

            // Composite on white so transparent pixels do not become black in RGB output.
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(source, 0, 0);
            }

            _bitmaps = new byte[bitmap.Width * bitmap.Height * 3];
            var pixmap = bitmap.PeekPixels() ?? throw new FonetImageException("Unable to access image pixels.");
            var pixelSpan = pixmap.GetPixelSpan();
            int rowBytes = pixmap.RowBytes;
            int destinationIndex = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                ReadOnlySpan<byte> row = pixelSpan.Slice(y * rowBytes, rowBytes);
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int offset = x * 4;
                    _bitmaps[destinationIndex++] = row[offset];
                    _bitmaps[destinationIndex++] = row[offset + 1];
                    _bitmaps[destinationIndex++] = row[offset + 2];
                }
            }
        }
        catch (FonetImageException fonetImageException)
        {
            FonetDriver.ActiveDriver?.FireFonetError($"Image decode failed for {Uri}: Unable to decode image data: {fonetImageException.Message}");
            throw;
        }
        catch (Exception e)
        {
            FonetDriver.ActiveDriver?.FireFonetError($"Image decode failed for {Uri}: {e.Message}");
            throw new FonetImageException("Unable to decode image data.", e);
        }
    }
}
