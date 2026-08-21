using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.XPath;
using Genocs.Fonet.XsltTransformer.Configurations;

namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// ResourceManager retrieves XML and XSLT file contents.
/// XML and XSLT contents are cached in Application with file dependency.
/// </summary>
public static class ResourceManager
{
    /// <summary>
    /// Returns XSLT file as <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="fileName">The XSLT template file path.</param>
    /// <param name="culture">The XSLT file culture.</param>
    public static IXPathNavigable GetXsltFileContent(string fileName, string? culture = null)
    {
        if (!fileName.EndsWith(".fo"))
            fileName += ".fo";

        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = GlobalSettings.DefaultCulture;
        }

        string resourceAbsolutePath = XsltExtensions.MapPath($"{fileName}");

        return GetFileContent(resourceAbsolutePath);
    }

    /// <summary>
    /// Returns XML file as <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="fileName">The XML file path.</param>
    /// <param name="culture">The XML file culture.</param>
    public static IXPathNavigable? GetLocalizedXmlFileContent(string? fileName, string? culture = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        string resourceAbsolutePath = fileName;

        if (!string.IsNullOrWhiteSpace(culture) && !Path.IsPathRooted(fileName))
        {
            string culturedPath = Path.Combine(culture, fileName);
            if (File.Exists(XsltExtensions.MapPath(culturedPath)))
            {
                resourceAbsolutePath = culturedPath;
            }
        }

        return GetFileContent(XsltExtensions.MapPath(resourceAbsolutePath));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static IXPathNavigable GetFileContent(string filePath, bool editable = false)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Unable to find the specified file", filePath);

        using var reader = XmlReader.Create(filePath);
        if (!editable)
            return new XPathDocument(reader);

        var document = new XmlDocument();
        document.Load(reader);
        return document;
    }
}
