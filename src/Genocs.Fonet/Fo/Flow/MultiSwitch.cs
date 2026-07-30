using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiSwitch : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new MultiSwitch(parent, props));

    protected MultiSwitch(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:multi-switch";
    }

    public override Status Layout(Area area)
    {
        // TODO: Implement the layout logic for the MultiSwitch element.
        // AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        return base.Layout(area);
    }
}