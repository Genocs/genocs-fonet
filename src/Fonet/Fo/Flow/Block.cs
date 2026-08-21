using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Block : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Block(parent, props));

    private int align;
    private int alignLast;
    private int breakAfter;
    private int lineHeight;
    private int startIndent;
    private int endIndent;
    private int spaceBefore;
    private int spaceAfter;
    private int textIndent;
    private int keepWithNext;
    private int blockWidows;
    private int blockOrphans;
    private int areaHeight = 0;
    private int contentWidth = 0;
    private string id;
    private int span;
    private bool anythingLaidOut = false;

    public Block(FObj parent, PropertyList propertyList) : base(parent, propertyList)
    {
        Name = "fo:block";

        switch (parent.Name)
        {
            case "fo:basic-link":
            case "fo:block":
            case "fo:block-container":
            case "fo:float":
            case "fo:flow":
            case "fo:footnote-body":
            case "fo:inline":
            case "fo:inline-container":
            case "fo:list-item-body":
            case "fo:list-item-label":
            case "fo:marker":
            case "fo:multi-case":
            case "fo:static-content":
            case "fo:table-caption":
            case "fo:table-cell":
            case "fo:wrapper":
                break;
            default:
                throw new FonetException(
                    "fo:block must be child of " +
                        "fo:basic-link, fo:block, fo:block-container, fo:float, fo:flow, fo:footnote-body, fo:inline, fo:inline-container, fo:list-item-body, fo:list-item-label, fo:marker, fo:multi-case, fo:static-content, fo:table-caption, fo:table-cell or fo:wrapper " +
                        "not " + parent.Name);
        }
        this.span = this.Properties.GetProperty("span").GetEnum();
        _textState = _propertyManager.getTextDecoration(parent);
    }

    public override Status Layout(Area area)
    {
        BlockArea blockArea;

        if (this._marker == MarkerBreakAfter)
        {
            return new Status(Status.OK);
        }

        if (this._marker == MarkerStart)
        {
            if (!_propertyManager.IsVisible())
            {
                this._marker = MarkerBreakAfter;
                return new Status(Status.OK);
            }

            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            HyphenationProps mHyphProps = _propertyManager.GetHyphenationProps();
            MarginProps mProps = _propertyManager.GetMarginProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            this.align = this.Properties.GetProperty("text-align").GetEnum();
            this.alignLast = this.Properties.GetProperty("text-align-last").GetEnum();
            this.breakAfter = this.Properties.GetProperty("break-after").GetEnum();
            this.lineHeight =
                this.Properties.GetProperty("line-height").GetLength().Millipoints();
            this.startIndent =
                this.Properties.GetProperty("start-indent").GetLength().Millipoints();
            this.endIndent =
                this.Properties.GetProperty("end-indent").GetLength().Millipoints();
            this.spaceBefore =
                this.Properties.GetProperty("space-before.optimum").GetLength().Millipoints();
            this.spaceAfter =
                this.Properties.GetProperty("space-after.optimum").GetLength().Millipoints();
            this.textIndent =
                this.Properties.GetProperty("text-indent").GetLength().Millipoints();
            this.keepWithNext =
                this.Properties.GetProperty("keep-with-next").GetEnum();

            this.blockWidows =
                this.Properties.GetProperty("widows").GetNumber().IntValue();
            this.blockOrphans = (int)
                this.Properties.GetProperty("orphans").GetNumber().IntValue();
            this.id = this.Properties.GetProperty("id").GetString();

            if (area is BlockArea)
            {
                area.end();
            }

            if (area.GetIDReferences() != null)
            {
                area.GetIDReferences().CreateID(id);
            }

            this._marker = 0;

            int breakBeforeStatus = _propertyManager.CheckBreakBefore(area);
            if (breakBeforeStatus != Status.OK)
            {
                return new Status(breakBeforeStatus);
            }

            int numChildren = this._children.Count;
            for (int i = 0; i < numChildren; i++)
            {
                FONode fo = (FONode)_children[i];
                if (fo is FOText)
                {
                    if (((FOText)fo).willCreateArea())
                    {
                        fo.SetWidows(blockWidows);
                        break;
                    }
                    else
                    {
                        _children.RemoveAt(i);
                        numChildren = this._children.Count;
                        i--;
                    }
                }
                else
                {
                    fo.SetWidows(blockWidows);
                    break;
                }
            }

            for (int i = numChildren - 1; i >= 0; i--)
            {
                FONode fo = (FONode)_children[i];
                if (fo is FOText)
                {
                    if (((FOText)fo).willCreateArea())
                    {
                        fo.SetOrphans(blockOrphans);
                        break;
                    }
                }
                else
                {
                    fo.SetOrphans(blockOrphans);
                    break;
                }
            }
        }

        if (_areasGenerated == 0 && _marker == 0 && area is ColumnArea column)
        {
            int clearOffset = column.GetClearOffset(column.getContentHeight(), _propertyManager.GetClear());
            if (clearOffset > 0)
            {
                column.AddDisplaySpace(clearOffset);
            }
        }

        if ((spaceBefore != 0) && (this._marker == 0))
        {
            area.AddDisplaySpace(spaceBefore);
        }

        if (anythingLaidOut)
        {
            this.textIndent = 0;
        }

        if (_marker == 0 && area.GetIDReferences() != null)
        {
            area.GetIDReferences().ConfigureID(id, area);
        }

        int spaceLeft = area.spaceLeft();
        blockArea = new BlockArea(_propertyManager.GetFontState(area.GetFontInfo()),
                          area.getAllocationWidth(), area.spaceLeft(),
                          startIndent, endIndent, textIndent, align,
                          alignLast, lineHeight, area)
        {
            GeneratedBy = this,
            ZIndex = _propertyManager.GetZIndex(),
        };

        this._areasGenerated++;
        if (this._areasGenerated == 1)
        {
            blockArea.IsFirst = true;
        }

        blockArea.addLineagePair(this, this._areasGenerated);
        blockArea.Page = area.Page;
        blockArea.Background = _propertyManager.GetBackgroundProps();
        blockArea.setBorderAndPadding(_propertyManager.GetBorderAndPadding());
        blockArea.setHyphenation(_propertyManager.GetHyphenationProps());
        blockArea.start();

        blockArea.setAbsoluteHeight(area.getAbsoluteHeight());
        blockArea.setIDReferences(area.GetIDReferences());

        blockArea.setTableCellXOffset(area.getTableCellXOffset());

        for (int i = this._marker; i < _children.Count; i++)
        {
            FONode fo = (FONode)_children[i];
            Status status;
            if ((status = fo.Layout(blockArea)).IsIncomplete())
            {
                this._marker = i;
                if (status.GetCode() == Status.AREA_FULL_NONE)
                {
                    if ((i != 0))
                    {
                        status = new Status(Status.AREA_FULL_SOME);
                        area.AddChild(blockArea);
                        area.setMaxHeight(area.getMaxHeight() - spaceLeft
                            + blockArea.getMaxHeight());
                        area.increaseHeight(blockArea.GetHeight());
                        anythingLaidOut = true;

                        return status;
                    }
                    else
                    {
                        anythingLaidOut = false;
                        return status;
                    }
                }
                area.AddChild(blockArea);
                area.setMaxHeight(area.getMaxHeight() - spaceLeft
                    + blockArea.getMaxHeight());
                area.increaseHeight(blockArea.GetHeight());
                anythingLaidOut = true;
                return status;
            }
            anythingLaidOut = true;
        }

        blockArea.end();

        area.setMaxHeight(area.getMaxHeight() - spaceLeft
            + blockArea.getMaxHeight());

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
        areaHeight = blockArea.GetHeight();
        contentWidth = blockArea.getContentWidth();
        int breakAfterStatus = _propertyManager.CheckBreakAfter(area);
        if (breakAfterStatus != Status.OK)
        {
            this._marker = MarkerBreakAfter;
            blockArea = null;
            return new Status(breakAfterStatus);
        }

        if (keepWithNext != 0)
        {
            blockArea = null;
            return new Status(Status.KEEP_WITH_NEXT);
        }

        blockArea.IsLast = true;
        blockArea = null;
        return new Status(Status.OK);
    }

    public int GetAreaHeight()
    {
        return areaHeight;
    }

    public override int GetContentWidth()
    {
        return contentWidth;
    }

    public int GetSpan()
    {
        return this.span;
    }

    public override void ResetMarker()
    {
        anythingLaidOut = false;
        base.ResetMarker();
    }
}