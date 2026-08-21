using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo.Flow;

internal class Table : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Table(parent, props));

    private const int MINCOLWIDTH = 10000;

    private int breakBefore;

    private int breakAfter;

    private int spaceBefore;

    private int spaceAfter;

    private LengthRange ipd;

    private int height;

    private string id;

    private TableHeader tableHeader = null;

    private TableFooter tableFooter = null;

    private bool omitHeaderAtBreak = false;

    private bool omitFooterAtBreak = false;

    private ArrayList columns = new ArrayList();

    private int bodyCount = 0;

    private bool bAutoLayout = false;

    private int contentWidth = 0;

    private int optIPD;

    private int minIPD;

    private int maxIPD;

    private AreaContainer areaContainer;

    public Table(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:table";
    }

    public override Status Layout(Area area)
    {
        if (this._marker == MarkerBreakAfter)
        {
            return new Status(Status.OK);
        }

        if (this._marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginProps mProps = _propertyManager.GetMarginProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            this.breakBefore = this.Properties.GetProperty("break-before").GetEnum();
            this.breakAfter = this.Properties.GetProperty("break-after").GetEnum();
            this.spaceBefore = this.Properties.GetProperty("space-before.optimum").GetLength().Millipoints();
            this.spaceAfter = this.Properties.GetProperty("space-after.optimum").GetLength().Millipoints();
            this.ipd = this.Properties.GetProperty("inline-progression-dimension").GetLengthRange();
            this.height = this.Properties.GetProperty("height").GetLength().Millipoints();
            this.bAutoLayout = (this.Properties.GetProperty("table-layout").GetEnum() == TableLayout.AUTO);

            this.id = this.Properties.GetProperty("id").GetString();

            this.omitHeaderAtBreak = this.Properties.GetProperty("table-omit-header-at-break").GetEnum() == TableOmitHeaderAtBreak.TRUE;
            this.omitFooterAtBreak = this.Properties.GetProperty("table-omit-footer-at-break").GetEnum() == TableOmitFooterAtBreak.TRUE;

            if (area is BlockArea)
            {
                area.end();
            }
            if (this.areaContainer
                == null)
            {
                area.GetIDReferences().CreateID(id);
            }

            this._marker = 0;

            if (breakBefore == BreakBefore.PAGE)
            {
                return new Status(Status.FORCE_PAGE_BREAK);
            }

            if (breakBefore == BreakBefore.ODD_PAGE)
            {
                return new Status(Status.FORCE_PAGE_BREAK_ODD);
            }

            if (breakBefore == BreakBefore.EVEN_PAGE)
            {
                return new Status(Status.FORCE_PAGE_BREAK_EVEN);
            }

        }

        if ((spaceBefore != 0) && (this._marker == 0))
        {
            area.AddDisplaySpace(spaceBefore);
        }

        if (_marker == 0 && areaContainer == null)
        {
            area.GetIDReferences().ConfigureID(id, area);
        }

        int spaceLeft = area.spaceLeft();
        this.areaContainer = new AreaContainer(_propertyManager.GetFontState(area.GetFontInfo()), 0, 0,
                              area.getAllocationWidth(), area.spaceLeft(),
                              Position.STATIC, area);

        areaContainer._foCreator = this;
        areaContainer.Page = area.Page;
        areaContainer.Background = (_propertyManager.GetBackgroundProps());
        areaContainer.setBorderAndPadding(_propertyManager.GetBorderAndPadding());
        areaContainer.start();

        areaContainer.setAbsoluteHeight(area.getAbsoluteHeight());
        areaContainer.setIDReferences(area.GetIDReferences());

        bool addedHeader = false;
        bool addedFooter = false;
        int numChildren = this._children.Count;

        if (columns.Count == 0)
        {
            FindColumns(areaContainer);
            if (this.bAutoLayout)
            {
                FonetDriver.ActiveDriver.FireFonetWarning("table-layout=auto is not supported, using fixed!");
            }
            this.contentWidth =
                CalcFixedColumnWidths(areaContainer.getAllocationWidth());
        }
        areaContainer.setAllocationWidth(this.contentWidth);
        layoutColumns(areaContainer);

        for (int i = this._marker; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];
            if (fo is TableHeader)
            {
                if (columns.Count == 0)
                {
                    FonetDriver.ActiveDriver.FireFonetWarning("Current implementation of tables requires a table-column for each column, indicating column-width");
                    return new Status(Status.OK);
                }
                tableHeader = (TableHeader)fo;
                tableHeader.SetColumns(columns);
            }
            else if (fo is TableFooter)
            {
                if (columns.Count == 0)
                {
                    FonetDriver.ActiveDriver.FireFonetWarning("Current implementation of tables requires a table-column for each column, indicating column-width");
                    return new Status(Status.OK);
                }
                tableFooter = (TableFooter)fo;
                tableFooter.SetColumns(columns);
            }
            else if (fo is TableBody)
            {
                if (columns.Count == 0)
                {
                    FonetDriver.ActiveDriver.FireFonetWarning("Current implementation of tables requires a table-column for each column, indicating column-width");
                    return new Status(Status.OK);
                }
                Status status;
                if (tableHeader != null && !addedHeader)
                {
                    if ((status =
                        tableHeader.Layout(areaContainer)).IsIncomplete())
                    {
                        tableHeader.ResetMarker();
                        return new Status(Status.AREA_FULL_NONE);
                    }
                    addedHeader = true;
                    tableHeader.ResetMarker();
                    area.setMaxHeight(area.getMaxHeight() - spaceLeft
                        + this.areaContainer.getMaxHeight());
                }
                if (tableFooter != null && !this.omitFooterAtBreak
                    && !addedFooter)
                {
                    if ((status =
                        tableFooter.Layout(areaContainer)).IsIncomplete())
                    {
                        return new Status(Status.AREA_FULL_NONE);
                    }
                    addedFooter = true;
                    tableFooter.ResetMarker();
                }
                fo.SetWidows(_widows);
                fo.SetOrphans(_orphans);
                ((TableBody)fo).SetColumns(columns);

                if ((status = fo.Layout(areaContainer)).IsIncomplete())
                {
                    this._marker = i;
                    if (bodyCount == 0 && status.GetCode() == Status.AREA_FULL_NONE)
                    {
                        tableHeader?.RemoveLayout(areaContainer);
                        tableFooter?.RemoveLayout(areaContainer);

                        ResetMarker();
                    }

                    if (areaContainer.getContentHeight() > 0)
                    {
                        area.AddChild(areaContainer);
                        area.increaseHeight(areaContainer.GetHeight());
                        if (this.omitHeaderAtBreak)
                        {
                            tableHeader = null;
                        }
                        if (tableFooter != null && !this.omitFooterAtBreak)
                        {
                            ((TableBody)fo).SetYPosition(tableFooter.GetYPosition());
                            tableFooter.SetYPosition(tableFooter.GetYPosition()
                                + ((TableBody)fo).GetHeight());
                        }
                        SetupColumnHeights();
                        status = new Status(Status.AREA_FULL_SOME);
                    }
                    return status;
                }
                else
                {
                    bodyCount++;
                }
                area.setMaxHeight(area.getMaxHeight() - spaceLeft + this.areaContainer.getMaxHeight());
                if (tableFooter != null && !this.omitFooterAtBreak)
                {
                    ((TableBody)fo).SetYPosition(tableFooter.GetYPosition());
                    tableFooter.SetYPosition(tableFooter.GetYPosition()
                        + ((TableBody)fo).GetHeight());
                }
            }
        }

        if (tableFooter != null && this.omitFooterAtBreak)
        {
            if (tableFooter.Layout(areaContainer).IsIncomplete())
            {
                FonetDriver.ActiveDriver?.FireFonetWarning("Footer could not fit on page, moving last body row to next page");
                area.AddChild(areaContainer);
                area.increaseHeight(areaContainer.GetHeight());
                if (this.omitHeaderAtBreak)
                {
                    tableHeader = null;
                }
                tableFooter.RemoveLayout(areaContainer);
                tableFooter.ResetMarker();
                return new Status(Status.AREA_FULL_SOME);
            }
        }

        if (height != 0)
        {
            areaContainer.SetHeight(height);
        }

        SetupColumnHeights();

        areaContainer.end();
        area.AddChild(areaContainer);

        area.increaseHeight(areaContainer.GetHeight());

        if (spaceAfter != 0)
        {
            area.AddDisplaySpace(spaceAfter);
        }

        if (area is BlockArea)
        {
            area.start();
        }

        if (breakAfter == BreakAfter.PAGE)
        {
            this._marker = MarkerBreakAfter;
            return new Status(Status.FORCE_PAGE_BREAK);
        }

        if (breakAfter == BreakAfter.ODD_PAGE)
        {
            this._marker = MarkerBreakAfter;
            return new Status(Status.FORCE_PAGE_BREAK_ODD);
        }

        if (breakAfter == BreakAfter.EVEN_PAGE)
        {
            this._marker = MarkerBreakAfter;
            return new Status(Status.FORCE_PAGE_BREAK_EVEN);
        }

        return new Status(Status.OK);
    }

    protected void SetupColumnHeights()
    {
        foreach (TableColumn c in columns)
        {
            if (c != null)
            {
                c.SetHeight(areaContainer.getContentHeight());
            }
        }
    }

    private void FindColumns(Area areaContainer)
    {
        int nextColumnNumber = 1;
        foreach (FONode fo in _children)
        {
            if (fo is TableColumn)
            {
                TableColumn c = (TableColumn)fo;
                c.DoSetup(areaContainer);
                int numColumnsRepeated = c.GetNumColumnsRepeated();
                int currentColumnNumber = c.GetColumnNumber();
                if (currentColumnNumber == 0)
                {
                    currentColumnNumber = nextColumnNumber;
                }
                for (int j = 0; j < numColumnsRepeated; j++)
                {
                    if (currentColumnNumber < columns.Count)
                    {
                        if (columns[currentColumnNumber - 1] != null)
                        {
                            FonetDriver.ActiveDriver?.FireFonetWarning($"More than one column object assigned to column {currentColumnNumber}");
                        }
                    }
                    columns.Insert(currentColumnNumber - 1, c);
                    currentColumnNumber++;
                }
                nextColumnNumber = currentColumnNumber;
            }
        }
    }

    private int CalcFixedColumnWidths(int maxAllocationWidth)
    {
        int nextColumnNumber = 1;
        int iEmptyCols = 0;
        double dTblUnits = 0.0;
        int iFixedWidth = 0;
        double dWidthFactor = 0.0;
        double dUnitLength = 0.0;
        double tuMin = 100000.0;

        foreach (TableColumn c in columns)
        {
            if (c == null)
            {
                FonetDriver.ActiveDriver?.FireFonetWarning($"No table-column specification for column {nextColumnNumber}");
                iEmptyCols++;
            }
            else
            {
                Length colLength = c.GetColumnWidthAsLength();
                double tu = colLength.GetTableUnits();
                if (tu > 0 && tu < tuMin && colLength.Millipoints() == 0)
                {
                    tuMin = tu;
                }
                dTblUnits += tu;
                iFixedWidth += colLength.Millipoints();
            }
            nextColumnNumber++;
        }

        SetIPD((dTblUnits > 0.0), maxAllocationWidth);
        if (dTblUnits > 0.0)
        {
            int iProportionalWidth = 0;
            if (this.optIPD > iFixedWidth)
            {
                iProportionalWidth = this.optIPD - iFixedWidth;
            }
            else if (this.maxIPD > iFixedWidth)
            {
                iProportionalWidth = this.maxIPD - iFixedWidth;
            }
            else
            {
                iProportionalWidth = maxAllocationWidth - iFixedWidth;
            }
            if (iProportionalWidth > 0)
            {
                dUnitLength = ((double)iProportionalWidth) / dTblUnits;
            }
            else
            {
                FonetDriver.ActiveDriver?.FireFonetWarning($"Sum of fixed column widths {iFixedWidth} greater than maximum available IPD {maxAllocationWidth}; no space for {dTblUnits} propertional units");
                dUnitLength = MINCOLWIDTH / tuMin;
            }
        }
        else
        {
            int iTableWidth = iFixedWidth;
            if (this.minIPD > iFixedWidth)
            {
                iTableWidth = this.minIPD;
                dWidthFactor = (double)this.minIPD / (double)iFixedWidth;
            }
            else if (this.maxIPD < iFixedWidth)
            {
                if (this.maxIPD != 0)
                {
                    FonetDriver.ActiveDriver?.FireFonetWarning($"Sum of fixed column widths {iFixedWidth} greater than maximum specified IPD {this.maxIPD}");
                }
            }
            else if (this.optIPD != -1 && iFixedWidth != this.optIPD)
            {
                FonetDriver.ActiveDriver?.FireFonetWarning($"Sum of fixed column widths {iFixedWidth} differs from specified optimum IPD {this.optIPD}");
            }
        }
        int offset = 0;
        foreach (TableColumn c in columns)
        {
            if (c != null)
            {
                c.SetColumnOffset(offset);

                Length l = c.GetColumnWidthAsLength();

                if (dUnitLength > 0)
                {
                    l.ResolveTableUnit(dUnitLength);
                }


                int colWidth = l.Millipoints();

                if (colWidth <= 0)
                {
                    FonetDriver.ActiveDriver?.FireFonetWarning("Zero-width table column!");
                }

                if (dWidthFactor > 0.0)
                {
                    colWidth = (int)(colWidth * dWidthFactor);
                }

                c.SetColumnWidth(colWidth);
                offset += colWidth;
            }
        }
        return offset;
    }

    private void layoutColumns(Area tableArea)
    {
        foreach (TableColumn c in columns)
        {
            c?.Layout(tableArea);
        }
    }

    public int GetAreaHeight()
    {
        return areaContainer.GetHeight();
    }

    public override int GetContentWidth()
    {
        if (areaContainer != null)
        {
            return areaContainer.getContentWidth();
        }
        else
        {
            return 0;
        }
    }

    private void SetIPD(bool bHasProportionalUnits, int maxAllocIPD)
    {
        bool bMaxIsSpecified = !this.ipd.GetMaximum().GetLength().IsAuto();
        if (bMaxIsSpecified)
        {
            this.maxIPD = ipd.GetMaximum().GetLength().Millipoints();
        }
        else
        {
            this.maxIPD = maxAllocIPD;
        }

        if (ipd.GetOptimum().GetLength().IsAuto())
        {
            this.optIPD = -1;
        }
        else
        {
            this.optIPD = ipd.GetMaximum().GetLength().Millipoints();
        }
        if (ipd.GetMinimum().GetLength().IsAuto())
        {
            this.minIPD = -1;
        }
        else
        {
            this.minIPD = ipd.GetMinimum().GetLength().Millipoints();
        }
        if (bHasProportionalUnits && this.optIPD < 0)
        {
            if (this.minIPD > 0)
            {
                if (bMaxIsSpecified)
                {
                    this.optIPD = (minIPD + maxIPD) / 2;
                }
                else
                {
                    this.optIPD = this.minIPD;
                }
            }
            else if (bMaxIsSpecified)
            {
                this.optIPD = this.maxIPD;
            }
            else
            {
                FonetDriver.ActiveDriver?.FireFonetError($"At least one of minimum, optimum, or maximum IPD must be specified on table.");
                this.optIPD = this.maxIPD;
            }
        }
    }
}