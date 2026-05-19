using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class Title : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
            => new Title(parent, propertyList);
    }

    new public static FObj.Maker GetMaker()
        => new Maker();

    protected Title(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:title";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        FontState fontState = _propertyManager.GetFontState(area.getFontInfo());
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();

        Property prop = _properties.GetProperty("baseline-shift");

        if (prop is LengthProperty)
        {
            Length bShift = prop.GetLength();
        }
        else if (prop is EnumProperty)
        {
            int bShift = prop.GetEnum();
        }

        ColorType col = _properties.GetProperty("color").GetColorType();
        Length lHeight = _properties.GetProperty("line-height").GetLength();
        int lShiftAdj = _properties.GetProperty("line-height-shift-adjustment").GetEnum();
        int vis = _properties.GetProperty("visibility").GetEnum();
        Length zIndex = _properties.GetProperty("z-index").GetLength();

        return base.Layout(area);
    }
}