namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// IPdfWriterService defines a contract for a service that generates a PDF stream from a document, template, and resources.
/// </summary>
public interface IPdfWriterService
{
    /// <summary>
    /// The actual implementation of the Print method is responsible for generating a PDF stream from the provided document, template, and resources. The method takes in an object representing the document, the name of the XSLT template, the name of the resources file, and an optional country ID for localization. It returns a Stream containing the generated PDF.
    /// </summary>
    /// <param name="document">The document object to be transformed into a PDF.</param>
    /// <param name="templateName">The name of the XSLT template to be used for the transformation.</param>
    /// <param name="resourcesName">The name of the resources file to be used for localization.</param>
    /// <param name="fontsDirectory">The directory containing the fonts to be used in the PDF generation.</param>
    /// <param name="countryId">An optional country ID for localization purposes.</param>
    /// <returns>A Stream containing the generated PDF.</returns>
    Stream Print(IPrintableDocument document, string templateName, string? resourcesName, string? fontsDirectory, string? countryId = null);
}
