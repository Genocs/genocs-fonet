using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class InitialPropertySet : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new InitialPropertySet(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    protected InitialPropertySet(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:initial-property-set";
    }

    public override Status Layout(Area area)
    {
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

        return base.Layout(area);
    }
}