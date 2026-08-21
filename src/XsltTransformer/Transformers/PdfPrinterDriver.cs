using System.Runtime.CompilerServices;
using System.Xml;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// PdfPrinterDriver is a <see cref="FonetDriver"/> wrapper.
/// </summary>
public static class PdfPrinterDriver
{
    /// <summary>
    /// Generates a PDF file from the specified XSL-FO document.
    /// </summary>
    /// <param name="xslFoDocument">XSL-FO document.</param>
    /// <param name="outputFileAbsolutePath">Output PDF absolute file path.</param>
    /// <param name="fontDir">Optional font directory path.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void MakePdf(XmlDocument xslFoDocument, string outputFileAbsolutePath, string? fontDir = null)
    {
        using var fileStream = File.Create(outputFileAbsolutePath);
        MakePdf(xslFoDocument, fileStream, fontDir);
    }

    /// <summary>
    /// Generates a PDF from XSL-FO document and sends it to the output stream.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void MakePdf(XmlDocument xslFoDocument, Stream outputStream, string? fontDir = null)
    {
        var driver = InitFonetDriver();
        SetupFonts(fontDir, driver);
        driver.Render(xslFoDocument, outputStream);
    }

    /// <summary>
    /// Returns PDF bytes generated from XSL-FO document.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static byte[] MakePdf(XmlDocument xslFoDocument)
    {
        var driver = InitFonetDriver();

        byte[] pdfBytes = [];

        using (var ms = new MemoryStream())
        {
            driver.Render(xslFoDocument, ms);
            pdfBytes = ms.ToArray();
        }

        return pdfBytes;
    }

    /// <summary>
    /// Returns PDF bytes generated from XSL-FO document.
    /// </summary>
    /// <param name="xslFoDocument">XSL-FO document.</param>
    /// <param name="fontDir">Optional font directory path.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static Stream MakePdfStream(XmlDocument xslFoDocument, string? fontDir = null)
    {
        var driver = InitFonetDriver(false);
        SetupFonts(fontDir, driver);

        var ms = new MemoryStream();
        driver.Render(xslFoDocument, ms);
        ms.Seek(0, SeekOrigin.Begin);

        return ms;
    }

    private static void SetupFonts(string? fontDir, FonetDriver driver)
    {
        if (!string.IsNullOrWhiteSpace(fontDir))
        {
            var files = new DirectoryInfo(fontDir).GetFiles();

            // Embed the font program into the PDF so it renders identically everywhere.
            driver.Options!.FontType = FontType.Embed;

            foreach (var file in files)
            {
                driver.Options!.AddPrivateFont(file);
            }
        }
    }

    /// <summary>
    /// Initializes FonetDriver.
    /// </summary>
    /// <param name="closeOnExit">Indicates whether to close the driver on exit.</param>
    private static FonetDriver InitFonetDriver(bool closeOnExit = true)
    {
        // Creating Fonet Driver and generating PDF file...
        var driver = FonetDriver.Make();

        driver.Options ??= new PdfRendererOptions
        {
            Title = "Genocs Sample",
            Author = "Giovanni Emanuele Nocco"
        };

        driver.ImageHandler = AnalyzeSource;
        driver.CloseOnExit = closeOnExit;

        driver.OnInfo += OnInfo;
        driver.OnWarning += OnWarning;

        return driver;
    }

    /// <summary>
    /// Handles OnInfo events triggered by FonetDriver.
    /// </summary>
    /// <param name="driver">The FonetDriver instance.</param>
    /// <param name="e">The FonetEventArgs containing event data.</param>
    private static void OnInfo(object driver, FonetEventArgs e)
    {
        // Get the Log instance from the driver if available, otherwise use a default logger.
        // Logger.InfoFormat("PdfPrinter: {0}", e.GetMessage());
    }

    /// <summary>
    /// Handles OnWarning events triggered by FonetDriver.
    /// </summary>
    /// <param name="driver">The FonetDriver instance.</param>
    /// <param name="e">The FonetEventArgs containing event data.</param>
    private static void OnWarning(object driver, FonetEventArgs e)
    {
        // Logger.WarnFormat("PdfPrinter: {0}", e.GetMessage());
    }

    /// <summary>
    /// Leave Fonet default management, simply return null.
    /// </summary>
    /// <param name="src">The source string to analyze.</param>
    /// <returns>A byte array if the source is a valid base64 image, otherwise null.</returns>
    private static byte[]? AnalyzeSource(string src)
    {
        if (string.IsNullOrWhiteSpace(src))
            return null;

        if (src.StartsWith("url"))
        {
            src = src.Replace("url(", string.Empty)
                     .Replace(")", string.Empty)
                     .Replace("\"", string.Empty)
                     .Replace("'", string.Empty);
        }

        if (!src.StartsWith("data:image") || !src.Contains("base64"))
            return null;

        string? base64String = src.Split(',').LastOrDefault();
        if (string.IsNullOrWhiteSpace(base64String))
            return null;

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
}
