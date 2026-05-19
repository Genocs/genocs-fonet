using Genocs.Fonet.Render.Pdf;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// PdfPrinterDriver is a <see cref="FonetDriver"/> wrapper.
/// </summary>
/// <remarks>
/// Author:
/// </remarks>
public static class PdfPrinterDriver
{
    /// <summary>
    /// Generates a PDF file from the specified XSL-FO document.
    /// </summary>
    /// <param name="xslFoDocument">XSL-FO document.</param>
    /// <param name="outputFileAbsolutePath">Output PDF absolute file path.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void MakePdf(XmlDocument xslFoDocument, string outputFileAbsolutePath, string? fontDir = null)
    {
        using (var fileStream = File.Create(outputFileAbsolutePath))
        {
            MakePdf(xslFoDocument, fileStream, fontDir);
        }
    }

    /// <summary>
    /// Generates a PDF from XSL-FO document and sends it to the output stream
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void MakePdf(XmlDocument xslFoDocument, Stream outputStream, string? fontDir = null)
    {
        var driver = InitFonetDriver();
        SetupFonts(fontDir, driver);
        driver.Render(xslFoDocument, outputStream);
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
    /// Returns PDF bytes generated from XSL-FO document.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static byte[] MakePdf(XmlDocument xslFoDocument)
    {
        var driver = InitFonetDriver();

        var pdfBytes = Array.Empty<byte>();

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

    /// <summary>
    /// Initializes FonetDriver.
    /// </summary>
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
    private static void OnInfo(object driver, FonetEventArgs e)
    {
        //Log.InfoFormat("PdfPrinter: {0}", e.GetMessage());
    }

    /// <summary>
    /// Handles OnWarning events triggered by FonetDriver.
    /// </summary>  
    private static void OnWarning(object driver, FonetEventArgs e)
    {
        //Log.WarnFormat("PdfPrinter: {0}", e.GetMessage());
    }

    /// <summary>
    /// leave Fonet default management, simply return null
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

        var base64String = src.Split(',').LastOrDefault();
        if (string.IsNullOrWhiteSpace(base64String))
            return null;

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error: " + ex);
            return null;
        }
    }
}
