using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ListItem : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new ListItem(parent, props));

    private int align;
    private int alignLast;
    private int lineHeight;
    private int spaceBefore;
    private int spaceAfter;
    private string id;
    private BlockArea blockArea;

    public ListItem(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:list-item";
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
            this.lineHeight = this.Properties.GetProperty("line-height").GetLength().MValue();
            this.spaceBefore = this.Properties.GetProperty("space-before.optimum").GetLength().MValue();
            this.spaceAfter = this.Properties.GetProperty("space-after.optimum").GetLength().MValue();
            this.id = this.Properties.GetProperty("id").GetString();

            area.GetIDReferences().CreateID(id);

            this._marker = 0;
        }

        if (area is BlockArea)
        {
            area.end();
        }

        if (spaceBefore != 0)
        {
            area.AddDisplaySpace(spaceBefore);
        }

        this.blockArea = new BlockArea(_propertyManager.GetFontState(area.GetFontInfo()),
                          area.getAllocationWidth(), area.spaceLeft(), 0, 0,
                          0, align, alignLast, lineHeight, area)
        {
            GeneratedBy = this
        };

        this.blockArea.setTableCellXOffset(area.getTableCellXOffset());
        this._areasGenerated++;
        if (this._areasGenerated == 1)
        {
            this.blockArea.IsFirst = true;
        }
        this.blockArea.addLineagePair(this, this._areasGenerated);

        blockArea.Page = area.Page;
        blockArea.start();

        blockArea.setAbsoluteHeight(area.getAbsoluteHeight());
        blockArea.setIDReferences(area.GetIDReferences());

        int numChildren = this._children.Count;
        if (numChildren != 2)
        {
            throw new FonetException("list-item must have exactly two children");
        }
        ListItemLabel label = (ListItemLabel)_children[0];
        ListItemBody body = (ListItemBody)_children[1];

        Status status;

        if (this._marker == 0)
        {
            area.GetIDReferences().ConfigureID(id, area);

            status = label.Layout(blockArea);
            if (status.IsIncomplete())
            {
                return status;
            }
        }

        status = body.Layout(blockArea);
        if (status.IsIncomplete())
        {
            blockArea.end();
            area.AddChild(blockArea);
            area.increaseHeight(blockArea.GetHeight());
            this._marker = 1;
            return status;
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
        this.blockArea.IsLast = true;
        return new Status(Status.OK);
    }

    public override int GetContentWidth()
    {
        if (blockArea != null)
        {
            return blockArea.getContentWidth();
        }
        else
        {
            return 0;
        }
    }
}