using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class FootnoteBody : FObj
{
    private int align = 0;

    private int alignLast = 0;

    private int lineHeight = 0;

    private int startIndent = 0;

    private int endIndent = 0;

    private int textIndent = 0;

    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new FootnoteBody(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public FootnoteBody(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:footnote-body";
        _areaClass = AreaClass.SetAreaClass(AreaClass.XSL_FOOTNOTE);
    }

    public override Status Layout(Area area)
    {
        if (_marker == MarkerStart)
        {
            _marker = 0;
        }

        BlockArea blockArea =
            new BlockArea(_propertyManager.GetFontState(area.getFontInfo()),
                          area.getAllocationWidth(), area.spaceLeft(),
                          startIndent, endIndent, textIndent, align,
                          alignLast, lineHeight);

        blockArea.GeneratedBy = this;
        blockArea.IsFirst = true;
        blockArea.setParent(area);
        blockArea.setPage(area.getPage());
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
        area.addChild(blockArea);
        area.increaseHeight(blockArea.GetHeight());
        blockArea.IsLast = true;
        return new Status(Status.OK);
    }
}