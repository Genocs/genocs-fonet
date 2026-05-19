namespace Genocs.Fonet.XsltTransformer.Transformers;


/// <summary>
/// The XslFoPdfService class implements the IPdfWriterService interface to generate a PDF stream from a document, template, and resources using XSL-FO transformation.
/// </summary>
public class XslFoPdfService : IPdfWriterService
{
    public Stream Print(IPrintableDocument document, string templateName, string? resourcesName, string? fontsDirectory, string? countryId = null)
    {
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
