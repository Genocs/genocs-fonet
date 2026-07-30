using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class Title : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Title(parent, props));

    protected Title(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:title";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        FontState fontState = _propertyManager.GetFontState(area.GetFontInfo());
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();

        Property? prop = Properties.GetProperty("baseline-shift");

        if (prop is LengthProperty)
        {
            Length? bShift = prop.GetLength();
        }
        else if (prop is EnumProperty)
        {
            int bShift = prop.GetEnum();
        }

        ColorType col = Properties.GetProperty("color").GetColorType();
        Length lHeight = Properties.GetProperty("line-height").GetLength();
        int lShiftAdj = Properties.GetProperty("line-height-shift-adjustment").GetEnum();
        int vis = Properties.GetProperty("visibility").GetEnum();
        Length zIndex = Properties.GetProperty("z-index").GetLength();

        return base.Layout(area);
    }
}