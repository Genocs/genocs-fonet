using System.Xml.Serialization;
using Genocs.Fonet.XsltTransformer.Transformers;

namespace Genocs.Fonet.WebApi.Models;

/// <summary>
/// Printable model matching the Host <c>books.fo</c> XPath
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

/// <summary>
/// Printable model matching the Host <c>books.fo</c> XPath
/// <c>/PdfPrinter/Books/BookList/Book</c>.
/// </summary>
public sealed class BookItem
{
    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the book author.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets or sets the book description.
    /// </summary>
    public string? Description { get; init; }
}
