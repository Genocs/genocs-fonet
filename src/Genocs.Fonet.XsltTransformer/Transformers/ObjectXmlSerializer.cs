using System.Xml;
using System.Xml.Serialization;

namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// ObjectXmlSerializer is a serialization helper class.
/// </summary>
/// <remarks>
/// Author: <a href="mailto: giovanni.nocco@gmail.com">Giovanni Nocco</a>.
/// </remarks>
public class ObjectXmlSerializer
{
    /// <summary>
    /// Serializes input object as XML string using <see cref="System.Xml.Serialization.XmlSerializer"/>.
    /// </summary>
    public static string SerializeObjectToXmlFormattedString(object obj)
    {
        XmlWriterSettings writerSettings = new()
        {
            OmitXmlDeclaration = true
        };

        XmlSerializer serializer = new(obj.GetType());
        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        using StringWriter stringWriter = new();
        using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, writerSettings);
        serializer.Serialize(xmlWriter, obj, ns);
        return stringWriter.ToString();
    }
}
