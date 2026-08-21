namespace Genocs.Fonet.Pdf;

public sealed class PdfGoTo : PdfDictionary, IPdfAction
{
    private PdfObjectReference? _pageReference;

    private decimal xPosition = 0;

    private decimal yPosition = 0;

    public PdfGoTo(PdfObjectReference? pageReference, PdfObjectId objectId)
        : base(objectId)
    {
        this[PdfName.Names.Type] = PdfName.Names.Action;
        this[PdfName.Names.S] = PdfName.Names.GoTo;
        this._pageReference = pageReference;
    }

    public PdfObjectReference? PageReference
    {
        set { _pageReference = value; }
    }

    public int X
    {
        set { xPosition = (value / 1000m); }
    }

    public int Y
    {
        set { yPosition = (value / 1000m); }
    }

    public PdfObject GetAction()
        => GetReference();

    protected internal override void Write(PdfWriter writer)
    {
        PdfArray dest =
        [
            _pageReference,
            PdfName.Names.XYZ,
            new PdfNumeric(xPosition),
            new PdfNumeric(yPosition),
            PdfNull.Null,
        ];

        this[PdfName.Names.D] = dest;

        base.Write(writer);
    }
}