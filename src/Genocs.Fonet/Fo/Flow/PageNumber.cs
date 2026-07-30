using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class PageNumber : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new PageNumber(parent, props));

    private float red;
    private float green;
    private float blue;
    private int wrapOption;
    private int whiteSpaceCollapse;
    private TextState ts;

    public PageNumber(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:page-number";
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

            ColorType c = Properties.GetProperty("color").GetColorType();
            red = c.Red;
            green = c.Green;
            blue = c.Blue;

            wrapOption = Properties.GetProperty("wrap-option").GetEnum();
            whiteSpaceCollapse = Properties.GetProperty("white-space-collapse").GetEnum();
            ts = new TextState();
            _marker = 0;

            string id = Properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        string? p = area.Page?.getFormattedNumber();
        _marker = FOText.addText((BlockArea)area,
                                     _propertyManager.GetFontState(area.GetFontInfo()),
                                     red, green, blue, wrapOption, null,
                                     whiteSpaceCollapse, p.ToCharArray(), 0,
                                     p.Length, ts, VerticalAlign.BASELINE);

        return new Status(Status.OK);
    }
}