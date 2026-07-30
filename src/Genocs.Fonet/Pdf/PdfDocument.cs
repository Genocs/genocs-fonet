using Genocs.Fonet.Pdf.Security;

namespace Genocs.Fonet.Pdf;

/// <summary>
/// A class that enables a well structured PDF document to be generated.
/// </summary>
/// <remarks>
/// Responsible for allocating object identifiers.
/// </remarks>
public class PdfDocument
{
    private uint nextObjectNumber = 1;

    public PdfDocument(Stream stream) : this(new PdfWriter(stream)) { }
    public PdfDocument(PdfWriter writer)
    {
        Writer = writer;
        Catalog = new PdfCatalog(NextObjectId());
        Pages = new PdfPageTree(NextObjectId());
        Catalog.Pages = Pages;
    }

    public PdfVersion Version { get; set; } = PdfVersion.V14;
    public FileIdentifier FileIdentifier { get; } = new FileIdentifier();
    public PdfCatalog Catalog { get; }
    public PdfPageTree Pages { get; }
    public PdfWriter Writer { get; }

    public SecurityOptions SecurityOptions
    {
        set { Writer.SecurityManager = new SecurityManager(value, FileIdentifier); }
    }

    public PdfObjectId NextObjectId()
    {
        return new PdfObjectId(nextObjectNumber++, 0);
    }

    public uint ObjectCount
    {
        get { return nextObjectNumber - 1; }
    }

    public void WriteHeader()
    {
        Writer.WriteHeader(Version);
        Writer.WriteBinaryComment();
    }
}