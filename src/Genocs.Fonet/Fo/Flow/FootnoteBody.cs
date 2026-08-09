using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class FootnoteBody : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new FootnoteBody(parent, props));

    private int align = 0;

    private int alignLast = 0;

    private int lineHeight = 0;

    private int startIndent = 0;

    private int endIndent = 0;

    private int textIndent = 0;

    public FootnoteBody(FObj? parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:footnote-body";
        AreaClass = Fonet.Layout.AreaClass.SetAreaClass(Fonet.Layout.AreaClass.XSL_FOOTNOTE);
    }

    public override Status Layout(Area area)
    {
        if (_marker == MarkerStart)
        {
            _marker = 0;
        }

        BlockArea blockArea = new BlockArea(_propertyManager.GetFontState(area.GetFontInfo()),
                          area.getAllocationWidth(), area.spaceLeft(),
                          startIndent, endIndent, textIndent, align,
                          alignLast, lineHeight, area)
        {
            GeneratedBy = this
        };

        blockArea.IsFirst = true;
        blockArea.Page = area.Page;
        blockArea.start();

        blockArea.setAbsoluteHeight(area.getAbsoluteHeight());
        blockArea.setIDReferences(area.GetIDReferences());

        blockArea.setTableCellXOffset(area.getTableCellXOffset());

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];
            Status status;
            if ((status = fo.Layout(blockArea)).IsIncomplete())
            {
                this.ResetMarker();
                return status;
            }
        }

        blockArea.end();
        area.AddChild(blockArea);
        area.increaseHeight(blockArea.GetHeight());
        blockArea.IsLast = true;
        return new Status(Status.OK);
    }
}