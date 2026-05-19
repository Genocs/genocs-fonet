using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiSwitch : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new MultiSwitch(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    protected MultiSwitch(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:multi-switch";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        return base.Layout(area);
    }
}