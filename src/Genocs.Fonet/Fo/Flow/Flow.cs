using Genocs.Fonet.Fo.Pagination;
using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo.Flow;

internal class Flow : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Flow(parent, props));

    private PageSequence pageSequence;
    private ArrayList markerSnapshot;
    private string _flowName;
    private int contentWidth;
    private Status _status = new Status(Status.AREA_FULL_NONE);

    protected Flow(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = GetElementName();

        if (parent.Name.Equals("fo:page-sequence"))
        {
            this.pageSequence = (PageSequence)parent;
        }
        else
        {
            throw new FonetException("flow must be child of "
                + "page-sequence, not "
                + parent.Name);
        }
        SetFlowName(GetProperty("flow-name").GetString());

        if (pageSequence.IsFlowSet)
        {
            if (Name.Equals("fo:flow"))
            {
                throw new FonetException("Only a single fo:flow permitted per fo:page-sequence");
            }
            else
            {
                throw new FonetException($"{Name} not allowed after fo:flow");
            }
        }
        pageSequence.AddFlow(this);
    }

    protected virtual void SetFlowName(string name)
    {
        if (name == null || name.Equals(""))
        {
            FonetDriver.ActiveDriver.FireFonetWarning(
                "A 'flow-name' is required for " + GetElementName() + ".");
            _flowName = "xsl-region-body";
        }
        else
        {
            _flowName = name;
        }
    }

    public string GetFlowName()
    {
        return _flowName;
    }

    public override Status Layout(Area area)
    {
        return Layout(area, null);
    }

    public virtual Status Layout(Area area, Region region)
    {
        if (this._marker == MarkerStart)
        {
            this._marker = 0;
        }

        BodyAreaContainer bac = (BodyAreaContainer)area;

        bool prevChildMustKeepWithNext = false;
        ArrayList pageMarker = this.GetMarkerSnapshot(new ArrayList());

        int numChildren = this._children.Count;
        if (numChildren == 0)
        {
            throw new FonetException("fo:flow must contain block-level children");
        }
        for (int i = this._marker; i < numChildren; i++)
        {
            FObj fo = (FObj)_children[i];

            if (bac.isBalancingRequired(fo))
            {
                bac.resetSpanArea();

                this.Rollback(markerSnapshot);
                i = this._marker - 1;
                continue;
            }
            Area currentArea = bac.getNextArea(fo);
            currentArea.setIDReferences(bac.GetIDReferences());
            if (bac.isNewSpanArea())
            {
                this._marker = i;
                markerSnapshot = this.GetMarkerSnapshot(new ArrayList());
            }
            SetContentWidth(currentArea.getContentWidth());

            _status = fo.Layout(currentArea);

            if (_status.IsIncomplete())
            {
                if ((prevChildMustKeepWithNext) && (_status.LaidOutNone()))
                {
                    this._marker = i - 1;
                    FObj prevChild = (FObj)_children[this._marker];
                    prevChild.RemoveAreas();
                    prevChild.ResetMarker();
                    prevChild.RemoveID(area.GetIDReferences());
                    _status = new Status(Status.AREA_FULL_SOME);
                    return _status;
                }
                if (bac.isLastColumn())
                {
                    if (_status.GetCode() == Status.FORCE_COLUMN_BREAK)
                    {
                        this._marker = i;
                        _status =
                            new Status(Status.FORCE_PAGE_BREAK);
                        return _status;
                    }
                    else
                    {
                        this._marker = i;
                        return _status;
                    }
                }
                else
                {
                    if (_status.IsPageBreak())
                    {
                        this._marker = i;
                        return _status;
                    }
                    ((ColumnArea)currentArea).incrementSpanIndex();
                    i--;
                }
            }
            if (_status.GetCode() == Status.KEEP_WITH_NEXT)
            {
                prevChildMustKeepWithNext = true;
            }
            else
            {
                prevChildMustKeepWithNext = false;
            }
        }
        return _status;
    }

    protected void SetContentWidth(int contentWidth)
    {
        this.contentWidth = contentWidth;
    }

    public override int GetContentWidth()
    {
        return this.contentWidth;
    }

    protected virtual string GetElementName()
    {
        return "fo:flow";
    }

    public Status GetStatus()
    {
        return _status;
    }

    public override bool GeneratesReferenceAreas()
    {
        return true;
    }
}