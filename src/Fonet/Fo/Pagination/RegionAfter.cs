using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Pagination;

internal class RegionAfter : Region
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new RegionAfter(parent, props));

    public const string REGION_CLASS = "after";

    private int precedence;

    protected RegionAfter(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        precedence = this.Properties.GetProperty("precedence").GetEnum();
    }

    public override RegionArea MakeRegionArea(int allocationRectangleXPosition,
                                              int allocationRectangleYPosition,
                                              int allocationRectangleWidth,
                                              int allocationRectangleHeight)
    {
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        int extent = this.Properties.GetProperty("extent").GetLength().Millipoints();

        RegionArea area = new RegionArea(
            allocationRectangleXPosition,
            allocationRectangleYPosition - allocationRectangleHeight + extent,
            allocationRectangleWidth,
            extent);
        area.setBackground(bProps);

        return area;
    }


    protected override string GetDefaultRegionName()
    {
        return "xsl-region-after";
    }

    protected override string GetElementName()
    {
        return "fo:region-after";
    }

    public override string GetRegionClass()
    {
        return REGION_CLASS;
    }

    public bool getPrecedence()
    {
        return (precedence == Precedence.TRUE ? true : false);
    }
}