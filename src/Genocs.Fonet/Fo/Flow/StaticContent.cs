using Genocs.Fonet.Fo.Pagination;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class StaticContent : Flow
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new StaticContent(parent, props));

    protected StaticContent(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        ((PageSequence)parent).IsFlowSet = false;
    }

    public override Status Layout(Area area)
    {
        return Layout(area, null);
    }

    public override Status Layout(Area area, Region region)
    {
        ResetMarker();

        int numChildren = _children.Count;
        string regionClass = "none";
        if (region != null)
        {
            regionClass = region.GetRegionClass();
        }
        else
        {
            if (GetFlowName().Equals("xsl-region-before"))
            {
                regionClass = RegionBefore.REGION_CLASS;
            }
            else if (GetFlowName().Equals("xsl-region-after"))
            {
                regionClass = RegionAfter.REGION_CLASS;
            }
            else if (GetFlowName().Equals("xsl-region-start"))
            {
                regionClass = RegionStart.REGION_CLASS;
            }
            else if (GetFlowName().Equals("xsl-region-end"))
            {
                regionClass = RegionEnd.REGION_CLASS;
            }
        }

        if (area is AreaContainer)
        {
            ((AreaContainer)area).setAreaName(regionClass);
        }

        area.setAbsoluteHeight(0);

        SetContentWidth(area.getContentWidth());

        for (int i = 0; i < numChildren; i++)
        {
            FObj fo = (FObj)_children[i];

            Status status;
            if ((status = fo.Layout(area)).IsIncomplete())
            {
                FonetDriver.ActiveDriver.FireFonetWarning("Some static content could not fit in the area.");
                _marker = i;
                if ((i != 0) && (status.GetCode() == Status.AREA_FULL_NONE))
                {
                    status = new Status(Status.AREA_FULL_SOME);
                }
                return (status);
            }
        }
        ResetMarker();
        return new Status(Status.OK);
    }

    protected override string GetElementName()
        => "fo:static-content";

    protected override void SetFlowName(string name)
    {
        if (name == null || name.Equals(""))
        {
            throw new FonetException($"A 'flow-name' is required for {GetElementName()}.");
        }
        else
        {
            base.SetFlowName(name);
        }
    }
}