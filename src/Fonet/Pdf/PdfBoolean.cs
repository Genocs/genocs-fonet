namespace Genocs.Fonet.Pdf;

public sealed class PdfBoolean : PdfObject
{
    private readonly bool _val;

    public PdfBoolean(bool val)
    {
        _val = val;
    }

    public PdfBoolean(bool val, PdfObjectId objectId)
        : base(objectId)
    {
        _val = val;
    }

    protected internal override void Write(PdfWriter writer)
    {
        writer.Write(_val ? KeywordEntries.GetKeyword(Keyword.True)
            : KeywordEntries.GetKeyword(Keyword.False));
    }
}