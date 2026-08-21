namespace Genocs.Fonet.Pdf;

public sealed class PdfNumeric : PdfObject
{
    private readonly decimal _val;

    public PdfNumeric(decimal val)
        => _val = val;

    public PdfNumeric(decimal val, PdfObjectId objectId)
        : base(objectId)
        => _val = val;

    protected internal override void Write(PdfWriter writer)
        => writer.Write(_val);
}