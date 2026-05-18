namespace Fonet.Image
{
    using System;
    using System.IO;
    using Fonet.DataTypes;
    using Fonet.Pdf.Filter;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.PixelFormats;

    /// <summary>
    /// A bitmap image that will be referenced by fo:external-graphic.
    /// </summary>
    internal sealed class FonetImage
    {
        public const int DEFAULT_BITPLANES = 8;

        // Image URL
        private string m_href = null;

        // image width
        private int width = 0;

        // image height
        private int height = 0;

        // Image color space 
        private ColorSpace m_colorSpace = null;

        // Bits per pixel
        private int m_bitsPerPixel = 0;

        // Image data 
        private byte[] m_bitmaps = null;

        /// <summary>
        ///     Filter that will be applied to image data
        /// </summary>
        private IFilter filter = null;

        /// <summary>
        ///     Constructs a new FonetImage using the supplied bitmap.
        /// </summary>
        /// <remarks>
        ///     Does not hold a reference to the passed bitmap.  Instead the
        ///     image data is extracted from <b>bitmap</b> on construction.
        /// </remarks>
        /// <param name="href">The location of <i>bitmap</i></param>
        /// <param name="imageData">The image data</param>
        public FonetImage(string href, byte[] imageData)
        {
            this.m_href = href;

            m_colorSpace = new ColorSpace(ColorSpace.DeviceRgb);
            m_bitsPerPixel = DEFAULT_BITPLANES; // 8

            // Bitmap does not seem to be thread-safe.  The only situation
            // Where this causes a problem is when the evaluation image is
            // used.  Each thread is given the same instance of Bitmap from
            // the resource manager.
            IImageFormat imageFormat = Image.DetectFormat(imageData);
            ImageInfo imageInfo = Image.Identify(imageData);
            if (imageFormat == null || imageInfo == null)
            {
                throw new FonetImageException("Unsupported or invalid image format.");
            }

            this.width = imageInfo.Width;
            this.height = imageInfo.Height;
            this.m_bitmaps = imageData;

            ExtractImage(imageData, imageFormat);
        }

        /// <summary>
        ///     Return the image URL.
        /// </summary>
        /// <returns>the image URL (as a string)</returns>
        public string Uri
        {
            get
            {
                return m_href;
            }
        }

        /// <summary>
        ///     Return the image width. 
        /// </summary>
        /// <returns>the image width</returns>
        public int Width
        {
            get
            {
                return this.width;
            }
        }

        /// <summary>
        ///     Return the image height. 
        /// </summary>
        /// <returns>the image height</returns>
        public int Height
        {
            get
            {
                return this.height;
            }
        }

        /// <summary>
        ///     Return the number of bits per pixel. 
        /// </summary>
        /// <returns>number of bits per pixel</returns>
        public int BitsPerPixel
        {
            get
            {
                return m_bitsPerPixel;
            }
        }

        /// <summary>
        ///     Return the image data size
        /// </summary>
        /// <returns>The image data size</returns>
        public int BitmapsSize
        {
            get
            {
                return (m_bitmaps != null) ? m_bitmaps.Length : 0;
            }
        }

        /// <summary>
        ///     Return the image data (uncompressed). 
        /// </summary>
        /// <returns>the image data</returns>
        public byte[] Bitmaps
        {
            get
            {
                return m_bitmaps;
            }
        }

        /// <summary>
        ///     Return the image color space. 
        /// </summary>
        /// <returns>the image color space (Fonet.Datatypes.ColorSpace)</returns>
        public ColorSpace ColorSpace
        {
            get
            {
                return m_colorSpace;
            }
        }

        /// <summary>
        ///     Returns the filter that should be applied to the bitmap data.
        /// </summary>
        public IFilter Filter
        {
            get
            {
                return filter;
            }
        }

        /// <summary>
        ///     Extracts the raw data from the image into a byte array suitable
        ///     for including in the PDF document.  The image is always extracted
        ///     as a 24-bit RGB image, regardless of it's original colour space
        ///     and colour depth.
        /// </summary>
        /// <param name="imageData">The original image bytes.</param>
        /// <param name="imageFormat">Detected image format.</param>
        /// <returns>A byte array containing the raw 24-bit RGB data</returns>
        private void ExtractImage(byte[] imageData, IImageFormat imageFormat)
        {
            // This should be a factory when we handle more image types
            if (String.Equals(imageFormat.Name, "JPEG", StringComparison.OrdinalIgnoreCase))
            {
                JpegParser parser = new JpegParser(m_bitmaps);
                JpegInfo info = parser.Parse();

                m_bitsPerPixel = info.BitsPerSample;
                m_colorSpace = new ColorSpace(info.ColourSpace);
                width = info.Width;
                height = info.Height;

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
                using Image<Rgb24> image = Image.Load<Rgb24>(imageData);

                // The size of the required byte array is not only a factor of the
                // width and height, but also the color components of each pixel.
                // Each pixel requires three bytes of storage - one byte each for
                // the red, green and blue components.
                m_bitmaps = new byte[image.Width * image.Height * 3];

                int destinationIndex = 0;
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgb24> pixelRow = accessor.GetRowSpan(y);
                        for (int x = 0; x < accessor.Width; x++)
                        {
                            Rgb24 pixel = pixelRow[x];
                            m_bitmaps[destinationIndex++] = pixel.R;
                            m_bitmaps[destinationIndex++] = pixel.G;
                            m_bitmaps[destinationIndex++] = pixel.B;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                FonetDriver.ActiveDriver.FireFonetError(e.ToString());
                throw new FonetImageException("Unable to decode image data.", e);
            }
        }
    }
}