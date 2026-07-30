using Genocs.Fonet.Util;
using System.Diagnostics;

namespace Genocs.Fonet.Pdf;

// TODO: rename to PdfLinkAnnotation?
public sealed class PdfLink : PdfDictionary
{
    private static readonly PdfArray DefaultColor;
    private static readonly PdfArray DefaultBorder;

    private IPdfAction? _action;

    static PdfLink()
    {
        DefaultColor = [new PdfNumeric(0), new PdfNumeric(0), new PdfNumeric(0)];
        DefaultBorder = [new PdfNumeric(0), new PdfNumeric(0), new PdfNumeric(0)];
    }

    public PdfLink(PdfObjectId objectId, IntRectangle r)
        : base(objectId)
    {
        this[PdfName.Names.Type] = PdfName.Names.Annot;
        this[PdfName.Names.Subtype] = PdfName.Names.Link;
        PdfArray rect =
        [
            new PdfNumeric(r.X / 1000m),
            new PdfNumeric(r.Y / 1000m),
            new PdfNumeric((r.X + r.Width) / 1000m),
            new PdfNumeric((r.Y - r.Height) / 1000m),
        ];
        this[PdfName.Names.Rect] = rect;
        this[PdfName.Names.H] = PdfName.Names.I;
        this[PdfName.Names.C] = DefaultColor;
        this[PdfName.Names.Border] = DefaultBorder;
    }

    public void SetAction(IPdfAction action)
        => _action = action;

    protected internal override void Write(PdfWriter writer)
    {
        Debug.Assert(_action != null, "PdfLink must be given an IAction before writing.");
        this[PdfName.Names.A] = _action.GetAction();
        base.Write(writer);
    }
}