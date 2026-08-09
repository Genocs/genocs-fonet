namespace Genocs.Fonet.XsltTransformer.Transformers;

/// <summary>
/// IPrintableDocument interface.
/// </summary>
public interface IPrintableDocument
{
    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    string? DocumentName { get; set; }

    /// <summary>
    /// Returns an XML representation of this instance.
    /// </summary>
    string ToXml();
}
