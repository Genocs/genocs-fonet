using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiToggle : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new MultiToggle(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    protected MultiToggle(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:multi-toggle";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        return base.Layout(area);
    }
}