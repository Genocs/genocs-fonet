namespace Genocs.Fonet.Pdf;

public sealed class PdfNull : PdfObject
{
    public static readonly PdfNull Null = new();

    public PdfNull(PdfObjectId objectId)
        : base(objectId)
    {
    }

    private PdfNull()
    {
    }

    protected internal override void Write(PdfWriter writer)
        => writer.WriteKeyword(Keyword.Null);
}