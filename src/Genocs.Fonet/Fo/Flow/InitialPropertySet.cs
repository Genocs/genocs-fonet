using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class InitialPropertySet : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new InitialPropertySet(parent, props));

    protected InitialPropertySet(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:initial-property-set";
    }

    public override Status Layout(Area area)
    {
        // TODO: Implement the layout if necessary. For now, we just return the base layout status.
        //AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        //AuralProps mAurProps = _propertyManager.GetAuralProps();
        //BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        //BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        //RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

        return base.Layout(area);
    }
}