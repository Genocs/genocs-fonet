using System.Xml.Serialization;
using Genocs.Fonet.XsltTransformer.Transformers;

namespace Genocs.Fonet.WebApi.Models;

/// <summary>
/// Printable model matching the Host <c>books.xslt</c> XPath
/// <c>/PdfPrinter/Books/BookList/Book</c>.
/// </summary>
[XmlRoot("Books")]
public sealed class BooksDocument : IPrintableDocument
{
    public string? DocumentName { get; set; }

    [XmlArray("BookList")]
    [XmlArrayItem("Book")]
    public List<BookItem>? BookList { get; set; }

    public string ToXml()
        => ObjectXmlSerializer.SerializeObjectToXmlFormattedString(this);
}

public sealed class BookItem
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
}
