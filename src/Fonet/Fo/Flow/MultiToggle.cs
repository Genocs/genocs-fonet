using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiToggle : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new MultiToggle(parent, props));

    protected MultiToggle(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:multi-toggle";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        return base.Layout(area);
    }
}