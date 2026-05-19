using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class PageNumber : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new PageNumber(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    private float red;
    private float green;
    private float blue;
    private int wrapOption;
    private int whiteSpaceCollapse;
    private TextState ts;

    public PageNumber(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:page-number";
    }

    public override Status Layout(Area area)
    {
        if (area is not BlockArea)
        {
            FonetDriver.ActiveDriver.FireFonetWarning("page-number outside block area");
            return new Status(Status.OK);
        }

        if (_marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            ColorType c = _properties.GetProperty("color").GetColorType();
            red = c.Red;
            green = c.Green;
            blue = c.Blue;

            wrapOption = _properties.GetProperty("wrap-option").GetEnum();
            whiteSpaceCollapse =_properties.GetProperty("white-space-collapse").GetEnum();
            ts = new TextState();
            _marker = 0;

            string id = _properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        string p = area.getPage().getFormattedNumber();
        _marker = FOText.addText((BlockArea)area,
                                     _propertyManager.GetFontState(area.getFontInfo()),
                                     red, green, blue, wrapOption, null,
                                     whiteSpaceCollapse, p.ToCharArray(), 0,
                                     p.Length, ts, VerticalAlign.BASELINE);

        return new Status(Status.OK);
    }
}