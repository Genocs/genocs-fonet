using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiProperties : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new MultiProperties(parent, props));

    protected MultiProperties(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:multi-properties";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        return base.Layout(area);
    }
}