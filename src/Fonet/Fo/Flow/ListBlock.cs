using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ListBlock : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new ListBlock(parent, props));

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
        Name = "fo:list-block";
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

            this.align = this.Properties.GetProperty("text-align").GetEnum();
            this.alignLast = this.Properties.GetProperty("text-align-last").GetEnum();
            this.lineHeight = this.Properties.GetProperty("line-height").GetLength().Millipoints();
            this.startIndent = this.Properties.GetProperty("start-indent").GetLength().Millipoints();
            this.endIndent = this.Properties.GetProperty("end-indent").GetLength().Millipoints();
            this.spaceBefore = this.Properties.GetProperty("space-before.optimum").GetLength().Millipoints();
            this.spaceAfter = this.Properties.GetProperty("space-after.optimum").GetLength().Millipoints();

            this._marker = 0;

            if (area is BlockArea)
            {
                area.end();
            }

            if (spaceBefore != 0)
            {
                area.AddDisplaySpace(spaceBefore);
            }

            if (this._isInTableCell)
            {
                startIndent += _forcedStartOffset;
                endIndent += area.getAllocationWidth() - _forcedWidth
                    - _forcedStartOffset;
            }

            string id = this.Properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        BlockArea blockArea = new BlockArea(_propertyManager.GetFontState(area.GetFontInfo()),
                          area.getAllocationWidth(), area.spaceLeft(),
                          startIndent, endIndent, 0, align, alignLast,
                          lineHeight, area)
        {
            GeneratedBy = this
        };

        blockArea.setTableCellXOffset(area.getTableCellXOffset());
        _areasGenerated++;
        if (_areasGenerated == 1)
        {
            blockArea.IsFirst = true;
        }

        blockArea.addLineagePair(this, _areasGenerated);

        blockArea.Page = area.Page;
        blockArea.Background = _propertyManager.GetBackgroundProps();
        blockArea.start();

        blockArea.setAbsoluteHeight(area.getAbsoluteHeight());
        blockArea.setIDReferences(area.GetIDReferences());

        int numChildren = _children.Count;
        for (int i = _marker; i < numChildren; i++)
        {
            if (!(_children[i] is ListItem))
            {
                FonetDriver.ActiveDriver.FireFonetError("Children of list-blocks must be list-items");
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
                area.AddChild(blockArea);
                area.increaseHeight(blockArea.GetHeight());
                return status;
            }
        }

        blockArea.end();
        area.AddChild(blockArea);
        area.increaseHeight(blockArea.GetHeight());

        if (spaceAfter != 0)
        {
            area.AddDisplaySpace(spaceAfter);
        }

        if (area is BlockArea)
        {
            area.start();
        }

        blockArea.IsLast = true;
        return new Status(Status.OK);
    }
}