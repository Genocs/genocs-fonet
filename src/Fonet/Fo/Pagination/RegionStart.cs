using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Pagination;

internal class RegionStart : Region
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new RegionStart(parent, props));

    public const string REGION_CLASS = "start";

    protected RegionStart(FObj parent, PropertyList propertyList)
        : base(parent, propertyList) { }

    internal RegionArea MakeRegionArea(
        int allocationRectangleXPosition,
        int allocationRectangleYPosition,
        int allocationRectangleWidth,
        int allocationRectangleHeight,
        bool beforePrecedence,
        bool afterPrecedence,
        int beforeHeight,
        int afterHeight)
    {
        int extent = this.Properties.GetProperty("extent").GetLength().Millipoints();
        int startY = allocationRectangleYPosition;
        int startH = allocationRectangleHeight;
        if (beforePrecedence)
        {
            startY -= beforeHeight;
            startH -= beforeHeight;
        }
        if (afterPrecedence)
        {
            startH -= afterHeight;
        }

        RegionArea area = new RegionArea(
            allocationRectangleXPosition, startY, extent, startH);
        area.setBackground(_propertyManager.GetBackgroundProps());

        return area;
    }

    public override RegionArea MakeRegionArea(int allocationRectangleXPosition,
                                              int allocationRectangleYPosition,
                                              int allocationRectangleWidth,
                                              int allocationRectangleHeight)
    {
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        int extent = this.Properties.GetProperty("extent").GetLength().Millipoints();

        return MakeRegionArea(allocationRectangleXPosition,
                              allocationRectangleYPosition,
                              allocationRectangleWidth, extent, false, false,
                              0, 0);
    }

    protected override string GetDefaultRegionName()
    {
        return "xsl-region-start";
    }

    protected override string GetElementName()
    {
        return "fo:region-start";
    }

    public override string GetRegionClass()
    {
        return REGION_CLASS;
    }
}