using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ListBlock : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new ListBlock(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    private int align;
    private int alignLast;
    private int lineHeight;
    private int startIndent;
    private int endIndent;
    private int spaceBefore;
    private int spaceAfter;

    public ListBlock(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:list-block";
    }

    public override Status Layout(Area area)
    {
        if (this._marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginProps mProps = _propertyManager.GetMarginProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            this.align = this._properties.GetProperty("text-align").GetEnum();
            this.alignLast = this._properties.GetProperty("text-align-last").GetEnum();
            this.lineHeight =
                this._properties.GetProperty("line-height").GetLength().MValue();
            this.startIndent =
                this._properties.GetProperty("start-indent").GetLength().MValue();
            this.endIndent =
                this._properties.GetProperty("end-indent").GetLength().MValue();
            this.spaceBefore =
                this._properties.GetProperty("space-before.optimum").GetLength().MValue();
            this.spaceAfter =
                this._properties.GetProperty("space-after.optimum").GetLength().MValue();

            this._marker = 0;

            if (area is BlockArea)
            {
                area.end();
            }

            if (spaceBefore != 0)
            {
                area.addDisplaySpace(spaceBefore);
            }

            if (this._isInTableCell)
            {
                startIndent += _forcedStartOffset;
                endIndent += area.getAllocationWidth() - _forcedWidth
                    - _forcedStartOffset;
            }

            string id = this._properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        BlockArea blockArea =
            new BlockArea(_propertyManager.GetFontState(area.getFontInfo()),
                          area.getAllocationWidth(), area.spaceLeft(),
                          startIndent, endIndent, 0, align, alignLast,
                          lineHeight);
        blockArea.setTableCellXOffset(area.getTableCellXOffset());
        blockArea.GeneratedBy = this;
        this._areasGenerated++;
        if (this._areasGenerated == 1)
        {
            blockArea.IsFirst = true;
        }
        blockArea.addLineagePair(this, this._areasGenerated);

        blockArea.setParent(area);
        blockArea.setPage(area.getPage());
        blockArea.setBackground(_propertyManager.GetBackgroundProps());
        blockArea.start();

        blockArea.setAbsoluteHeight(area.getAbsoluteHeight());
        blockArea.setIDReferences(area.GetIDReferences());

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            if (!(_children[i] is ListItem))
            {
                FonetDriver.ActiveDriver.FireFonetError(
                    "Children of list-blocks must be list-items");
                return new Status(Status.OK);
            }
            ListItem listItem = (ListItem)_children[i];
            Status status;
            if ((status = listItem.Layout(blockArea)).IsIncomplete())
            {
                if (status.GetCode() == Status.AREA_FULL_NONE && i > 0)
                {
                    status = new Status(Status.AREA_FULL_SOME);
                }
                this._marker = i;
                blockArea.end();
                area.addChild(blockArea);
                area.increaseHeight(blockArea.GetHeight());
                return status;
            }
        }

        blockArea.end();
        area.addChild(blockArea);
        area.increaseHeight(blockArea.GetHeight());

        if (spaceAfter != 0)
        {
            area.addDisplaySpace(spaceAfter);
        }

        if (area is BlockArea)
        {
            area.start();
        }

        blockArea.IsLast = true;
        return new Status(Status.OK);
    }
}