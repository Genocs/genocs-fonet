using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class TableCell : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new TableCell(parent, props));

    private string id;
    private int numColumnsSpanned;
    private int numRowsSpanned;
    private int iColNumber = -1;
    protected int startOffset;
    protected int width;
    protected int beforeOffset = 0;
    protected int startAdjust = 0;
    protected int widthAdjust = 0;
    protected int borderHeight = 0;
    protected int minCellHeight = 0;
    protected int height = 0;
    protected int top;
    protected int verticalAlign;
    protected bool bRelativeAlign = false;
    private bool bSepBorders = true;
    private bool bDone = false;
    private int m_borderSeparation = 0;
    private AreaContainer cellArea;

    public TableCell(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:table-cell";
        DoSetup();
    }

    public void SetStartOffset(int offset)
    {
        startOffset = offset;
    }

    public void SetWidth(int width)
    {
        this.width = width;
    }

    public int GetColumnNumber()
    {
        return iColNumber;
    }

    public int GetNumColumnsSpanned()
    {
        return numColumnsSpanned;
    }

    public int GetNumRowsSpanned()
    {
        return numRowsSpanned;
    }

    public void DoSetup()
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

        this.iColNumber = Properties.GetProperty("column-number").GetNumber().IntValue();
        if (iColNumber < 0)
        {
            iColNumber = 0;
        }

        this.numColumnsSpanned = Properties.GetProperty("number-columns-spanned").GetNumber().IntValue();
        if (numColumnsSpanned < 1)
        {
            numColumnsSpanned = 1;
        }

        this.numRowsSpanned = Properties.GetProperty("number-rows-spanned").GetNumber().IntValue();
        if (numRowsSpanned < 1)
        {
            numRowsSpanned = 1;
        }

        this.id = this.Properties.GetProperty("id").GetString();

        bSepBorders = (this.Properties.GetProperty("border-collapse").GetEnum()
            == BorderCollapse.SEPARATE);

        CalcBorders(_propertyManager.GetBorderAndPadding());

        verticalAlign = Properties.GetProperty("display-align").GetEnum();
        if (verticalAlign == DisplayAlign.AUTO)
        {
            bRelativeAlign = true;
            verticalAlign = Properties.GetProperty("relative-align").GetEnum();
        }
        else
        {
            bRelativeAlign = false;
        }

        this.minCellHeight = Properties.GetProperty("height").GetLength().MValue();
    }


    public override Status Layout(Area area)
    {
        int originalAbsoluteHeight = area.getAbsoluteHeight();
        if (this._marker == MarkerBreakAfter)
        {
            return new Status(Status.OK);
        }

        if (this._marker == MarkerStart)
        {

            area.GetIDReferences().CreateID(id);

            this._marker = 0;
            this.bDone = false;
        }

        if (_marker == 0)
        {
            area.GetIDReferences().ConfigureID(id, area);
        }

        int spaceLeft = area.spaceLeft() - m_borderSeparation;
        this.cellArea =
            new AreaContainer(_propertyManager.GetFontState(area.GetFontInfo()),
                              startOffset + startAdjust, beforeOffset,
                              width - widthAdjust, spaceLeft,
                              Position.RELATIVE, area);

        cellArea.foCreator = this;
        cellArea.Page = area.Page;
        cellArea.setBorderAndPadding((BorderAndPadding)_propertyManager.GetBorderAndPadding().Clone());
        cellArea.setBackground(_propertyManager.GetBackgroundProps());
        cellArea.start();

        cellArea.setAbsoluteHeight(area.getAbsoluteHeight());
        cellArea.setIDReferences(area.GetIDReferences());
        cellArea.setTableCellXOffset(startOffset + startAdjust);

        int numChildren = this._children.Count;
        for (int i = this._marker; bDone == false && i < numChildren; i++)
        {
            FObj fo = (FObj)_children[i];
            fo.SetIsInTableCell();
            fo.ForceWidth(width);

            this._marker = i;

            Status status;
            if ((status = fo.Layout(cellArea)).IsIncomplete())
            {
                if ((i == 0) && (status.GetCode() == Status.AREA_FULL_NONE))
                {
                    return new Status(Status.AREA_FULL_NONE);
                }
                else
                {
                    area.AddChild(cellArea);
                    return new Status(Status.AREA_FULL_SOME);
                }
            }

            area.setMaxHeight(area.getMaxHeight() - spaceLeft
                + this.cellArea.getMaxHeight());
        }
        this.bDone = true;
        cellArea.end();
        area.AddChild(cellArea);

        if (minCellHeight > cellArea.getContentHeight())
        {
            cellArea.SetHeight(minCellHeight);
        }

        height = cellArea.GetHeight();
        top = cellArea.GetCurrentYPosition();

        return new Status(Status.OK);
    }

    public int GetHeight()
    {
        return cellArea.GetHeight() + m_borderSeparation - borderHeight;
    }

    public void SetRowHeight(int h)
    {
        int delta = h - GetHeight();
        if (bRelativeAlign)
        {
            cellArea.increaseHeight(delta);
        }
        else if (delta > 0)
        {
            BorderAndPadding cellBP = cellArea.GetBorderAndPadding();
            switch (verticalAlign)
            {
                case DisplayAlign.CENTER:
                    cellArea.shiftYPosition(delta / 2);
                    cellBP.SetPaddingLength(BorderAndPadding.TOP,
                                            cellBP.GetPaddingTop(false)
                                                + delta / 2);
                    cellBP.SetPaddingLength(BorderAndPadding.BOTTOM,
                                            cellBP.GetPaddingBottom(false)
                                                + delta - delta / 2);
                    break;
                case DisplayAlign.AFTER:
                    cellBP.SetPaddingLength(BorderAndPadding.TOP,
                                            cellBP.GetPaddingTop(false) + delta);
                    cellArea.shiftYPosition(delta);
                    break;
                case DisplayAlign.BEFORE:
                    cellBP.SetPaddingLength(BorderAndPadding.BOTTOM,
                                            cellBP.GetPaddingBottom(false)
                                                + delta);
                    break;
                default:
                    break;
            }
        }
    }

    private void CalcBorders(BorderAndPadding bp)
    {
        if (this.bSepBorders)
        {
            int iSep =
                Properties.GetProperty("border-separation.inline-progression-direction").GetLength().MValue();
            this.startAdjust = iSep / 2 + bp.GetBorderLeftWidth(false)
                + bp.GetPaddingLeft(false);
            this.widthAdjust = startAdjust + iSep - iSep / 2
                + bp.GetBorderRightWidth(false)
                + bp.GetPaddingRight(false);
            m_borderSeparation =
                Properties.GetProperty("border-separation.block-progression-direction").GetLength().MValue();
            this.beforeOffset = m_borderSeparation / 2
                + bp.GetBorderTopWidth(false)
                + bp.GetPaddingTop(false);

        }
        else
        {
            int borderStart = bp.GetBorderLeftWidth(false);
            int borderEnd = bp.GetBorderRightWidth(false);
            int borderBefore = bp.GetBorderTopWidth(false);
            int borderAfter = bp.GetBorderBottomWidth(false);

            this.startAdjust = borderStart / 2 + bp.GetPaddingLeft(false);

            this.widthAdjust = startAdjust + borderEnd / 2
                + bp.GetPaddingRight(false);
            this.beforeOffset = borderBefore / 2 + bp.GetPaddingTop(false);
            this.borderHeight = (borderBefore + borderAfter) / 2;
        }
    }
}