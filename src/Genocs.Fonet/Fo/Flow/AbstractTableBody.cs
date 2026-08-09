using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo.Flow;

internal abstract class AbstractTableBody : FObj
{
    protected int spaceBefore;
    protected int spaceAfter;
    protected string id;
    protected ArrayList columns;
    protected RowSpanMgr rowSpanMgr;
    protected AreaContainer areaContainer;

    public AbstractTableBody(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        if (parent is not Table)
        {
            FonetDriver.ActiveDriver?.FireFonetError($"A table body must be child of fo:table, not {parent.Name}");
        }
    }

    public void SetColumns(ArrayList columns)
    {
        this.columns = columns;
    }

    public virtual void SetYPosition(int value)
        => areaContainer.YPosition = value;

    public virtual int GetYPosition()
        => areaContainer.GetCurrentYPosition();

    public int GetHeight()
    {
        return areaContainer.GetHeight() + spaceBefore + spaceAfter;
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
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            this.spaceBefore = this.Properties.GetProperty("space-before.optimum").GetLength().MValue();
            this.spaceAfter = this.Properties.GetProperty("space-after.optimum").GetLength().MValue();
            this.id = this.Properties.GetProperty("id").GetString();

            area.GetIDReferences().CreateID(id);

            if (area is BlockArea)
            {
                area.end();
            }

            rowSpanMgr ??= new RowSpanMgr(columns.Count);

            _marker = 0;
        }

        if ((spaceBefore != 0) && (this._marker == 0))
        {
            area.increaseHeight(spaceBefore);
        }

        if (_marker == 0)
        {
            area.GetIDReferences().ConfigureID(id, area);
        }

        int spaceLeft = area.spaceLeft();

        this.areaContainer = new AreaContainer(_propertyManager.GetFontState(area.GetFontInfo()), 0,
                              area.getContentHeight(),
                              area.getContentWidth(),
                              area.spaceLeft(),
                              Position.RELATIVE, area);

        areaContainer.foCreator = this;
        areaContainer.Page = area.Page;
        areaContainer.setBackground(_propertyManager.GetBackgroundProps());
        areaContainer.setBorderAndPadding(_propertyManager.GetBorderAndPadding());
        areaContainer.start();

        areaContainer.setAbsoluteHeight(area.getAbsoluteHeight());
        areaContainer.setIDReferences(area.GetIDReferences());

        Hashtable keepWith = new Hashtable();
        int numChildren = _children.Count;
        TableRow? lastRow = null;
        bool endKeepGroup = true;
        for (int i = this._marker; i < numChildren; i++)
        {
            object child = _children[i];
            if (child is Marker)
            {
                ((Marker)child).Layout(area);
                continue;
            }

            if (!(child is TableRow))
            {
                throw new FonetException("Currently only Table Rows are supported in table body, header and footer");
            }

            TableRow row = (TableRow)child;

            row.SetRowSpanMgr(rowSpanMgr);
            row.SetColumns(columns);
            row.DoSetup(areaContainer);
            if ((row.GetKeepWithPrevious().KeepType != KeepValue.KEEP_WITH_AUTO ||
                row.GetKeepWithNext().KeepType != KeepValue.KEEP_WITH_AUTO ||
                row.GetKeepTogether().KeepType != KeepValue.KEEP_WITH_AUTO) &&
                lastRow != null && !keepWith.Contains(lastRow))
            {
                keepWith.Add(lastRow, null);
            }
            else
            {
                if (endKeepGroup && keepWith.Count > 0)
                {
                    keepWith = new Hashtable();
                }

                if (endKeepGroup && i > this._marker)
                {
                    rowSpanMgr.SetIgnoreKeeps(false);
                }
            }

            bool rowStartsArea = i == _marker;
            if (!rowStartsArea && keepWith.Count > 0 && _children.IndexOf(keepWith[0]) == _marker)
            {
                rowStartsArea = true;
            }

            row.setIgnoreKeepTogether(rowStartsArea && StartsAC(area));
            Status status = row.Layout(areaContainer);
            if (status.IsIncomplete())
            {
                if (status.IsPageBreak())
                {
                    _marker = i;
                    area.AddChild(areaContainer);

                    area.increaseHeight(areaContainer.GetHeight());
                    if (i == numChildren - 1)
                    {
                        this._marker = MarkerBreakAfter;
                        if (spaceAfter != 0)
                        {
                            area.increaseHeight(spaceAfter);
                        }
                    }

                    return status;
                }

                if ((keepWith.Count > 0)
                    && (!rowSpanMgr.IgnoreKeeps()))
                {
                    row.RemoveLayout(areaContainer);
                    foreach (TableRow tr in keepWith.Keys)
                    {
                        tr.RemoveLayout(areaContainer);
                        i--;
                    }

                    if (i == 0)
                    {
                        ResetMarker();

                        rowSpanMgr.SetIgnoreKeeps(true);

                        return new Status(Status.AREA_FULL_NONE);
                    }
                }

                _marker = i;
                if ((i != 0) && (status.GetCode() == Status.AREA_FULL_NONE))
                {
                    status = new Status(Status.AREA_FULL_SOME);
                }

                if (!((i == 0) && (areaContainer.getContentHeight() <= 0)))
                {
                    area.AddChild(areaContainer);

                    area.increaseHeight(areaContainer.GetHeight());
                }

                rowSpanMgr.SetIgnoreKeeps(true);

                return status;
            }
            else if (status.GetCode() == Status.KEEP_WITH_NEXT
                || rowSpanMgr.HasUnfinishedSpans())
            {
                keepWith.Add(row, null);
                endKeepGroup = false;
            }
            else
            {
                endKeepGroup = true;
            }

            lastRow = row;
            area.setMaxHeight(area.getMaxHeight() - spaceLeft + this.areaContainer.getMaxHeight());
            spaceLeft = area.spaceLeft();
        }

        area.AddChild(areaContainer);
        areaContainer.end();

        area.increaseHeight(areaContainer.GetHeight());

        if (spaceAfter != 0)
        {
            area.increaseHeight(spaceAfter);
            area.setMaxHeight(area.getMaxHeight() - spaceAfter);
        }

        if (area is BlockArea)
        {
            area.start();
        }

        return new Status(Status.OK);
    }

    internal void RemoveLayout(Area area)
    {
        if (areaContainer != null)
        {
            area.removeChild(areaContainer);
        }

        if (spaceBefore != 0)
        {
            area.increaseHeight(-spaceBefore);
        }

        if (spaceAfter != 0)
        {
            area.increaseHeight(-spaceAfter);
        }

        ResetMarker();
        RemoveID(area.GetIDReferences());
    }

    private bool StartsAC(Area area)
    {
        Area? parent;

        while ((parent = area.Parent) != null && parent.HasNonSpaceChildren() == false)
        {
            if (parent is AreaContainer container && container.getPosition() == Position.ABSOLUTE)
            {
                return true;
            }

            area = parent;
        }

        return false;
    }
}