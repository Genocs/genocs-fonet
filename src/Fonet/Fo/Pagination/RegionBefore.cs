using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Pagination;

internal class RegionBefore : Region
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new RegionBefore(parent, props));

    public const string REGION_CLASS = "before";

    private int precedence;

    protected RegionBefore(FObj parent, PropertyList propertyList)
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
            allocationRectangleYPosition,
            allocationRectangleWidth,
            extent);
        area.setBackground(bProps);

        return area;
    }

    protected override string GetDefaultRegionName()
    {
        return "xsl-region-before";
    }

    protected override string GetElementName()
    {
        return "fo:region-before";
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