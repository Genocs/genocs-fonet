namespace Genocs.Fonet.XsltTransformer.Transformers;


/// <summary>
/// The XslFoPdfService class implements the IPdfWriterService interface to generate a PDF stream from a document, template, and resources using XSL-FO transformation.
/// </summary>
public class XslFoPdfService : IPdfWriterService
{
    /// <summary>
    /// Generates a PDF stream from the specified document, template, and resources using XSL-FO transformation.
    /// </summary>
    /// <param name="document">The printable document.</param>
    /// <param name="templateName">The name of the XSLT template file.</param>
    /// <param name="resourcesName">The name of the localized resources file.</param>
    /// <param name="fontsDirectory">The directory containing font files.</param>
    /// <param name="countryId">The country identifier for localization.</param>
    /// <returns>A stream containing the generated PDF.</returns>
    public Stream Print(IPrintableDocument document, string templateName, string? resourcesName, string? fontsDirectory, string? countryId = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new ArgumentNullException(nameof(templateName));
        }

        string debug = document.ToXml();

        var xsltStyleSheet = ResourceManager.GetXsltFileContent(templateName, countryId);

        var localizedXml = ResourceManager.GetLocalizedXmlFileContent(resourcesName, countryId);

        var fontDir = XsltExtensions.GetExecutingFolder(fontsDirectory);
        var transformer = new XmlTransformationManager(document, xsltStyleSheet, localizedXml);
        var xslFoDocument = transformer.Transform();
        var stream = PdfPrinterDriver.MakePdfStream(xslFoDocument, fontDir);

        return stream;
    }
}
