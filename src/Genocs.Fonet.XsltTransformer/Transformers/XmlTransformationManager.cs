using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// XmlTransformationManager handles transformation between XML culture and XSLT.    
/// </summary>
public class XmlTransformationManager
{
    /// <summary>
    /// XSLT extensions, used to extend the functionality of style sheets.
    /// </summary>
    public XsltArgumentList XsltExtensions;

    /// <summary>
    /// Transformation style sheet.
    /// </summary>
    public IXPathNavigable XsltStyleSheet;

    /// <summary>
    /// Localization resource XML.
    /// </summary>
    public IXPathNavigable? LocalizedXml;

    /// <summary>
    /// IPrintableDocument instance.
    /// </summary>
    public IPrintableDocument Document { get; set; }

    /// <summary>
    /// Creates a new instance of XmlTransformationManager with <see cref="IPrintableDocument"/> and a trasformation style sheet.
    /// </summary>
    public XmlTransformationManager(IPrintableDocument document, IXPathNavigable xsltStyleSheet) : this(document, xsltStyleSheet, null) { }

    /// <summary>
    /// Creates a new instance of XmlTransformationManager with <see cref="IPrintableDocument"/>, a transformation style sheet and a localized XML.
    /// </summary>
    public XmlTransformationManager(IPrintableDocument document, IXPathNavigable xsltStyleSheet, IXPathNavigable? localizedXml)
    {
        if (document == null || xsltStyleSheet == null)
            throw new ArgumentException("Invalid arguments");

        Document = document;
        XsltStyleSheet = xsltStyleSheet;
        LocalizedXml = localizedXml;

        XsltExtensions = new XsltArgumentList();

        // Add the XsltExtensionService to the XsltArgumentList with the namespace "pdfprinter:extensions:utility"
        XsltExtensions.AddExtensionObject("pdfprinter:extensions:utility", new XsltExtensions());
    }

    /// <summary>
    /// Returns an <see cref="XmlDocument"/> as a result of the trasformation.
    /// <code>
    /// XML + IPrintableDocument.ToXml() -> XSLT -> Ouput XmlDocument
    /// </code>
    /// </summary>        
    public XmlDocument Transform()
    {
        var content = new StringBuilder();
        content.Append("<PdfPrinter>");

        if (LocalizedXml != null)
            content.Append(LocalizedXml.CreateNavigator()?.InnerXml);

        content.Append(Document.ToXml());
        content.Append("</PdfPrinter>");

        var xmlContent = new XPathDocument(new XmlTextReader(new StringReader(content.ToString()))).CreateNavigator();

        return Transform(XsltStyleSheet, xmlContent, this.XsltExtensions);
    }

    /// <summary>
    /// Transforms the specified XML content using the provided XSLT template and optional XSLT extensions.
    /// </summary>
    /// <param name="xsltTemplate">The XSLT template to use for the transformation.</param>
    /// <param name="xmlContent">The XML content to be transformed.</param>
    /// <param name="xsltExtensions">Optional XSLT extensions to be used during the transformation.</param>
    /// <returns>The transformed XML document.</returns>
    public static XmlDocument Transform(IXPathNavigable xsltTemplate, IXPathNavigable xmlContent, XsltArgumentList xsltExtensions)
    {
        using var xslFoStream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            ConformanceLevel = ConformanceLevel.Document,
            Indent = true
        };

        using (XmlWriter xwriter = XmlWriter.Create(xslFoStream, settings))
        {
            var transformer = new XslCompiledTransform(false);

            // Allow resolving external xsl:include/xsl:import files by providing an XmlResolver.
            // Use XsltSettings.Default to keep advanced features disabled unless explicitly needed.
            var resolver = new XmlUrlResolver();
            transformer.Load(xsltTemplate, XsltSettings.Default, resolver);
            transformer.Transform(xmlContent, xsltExtensions, xwriter);

            xwriter.Flush();
        }

        var transformedXml = Encoding.UTF8.GetString(xslFoStream.ToArray());
        var transformedXmlDoc = new XmlDocument();
        transformedXmlDoc.LoadXml(transformedXml);

        return transformedXmlDoc;
    }
}