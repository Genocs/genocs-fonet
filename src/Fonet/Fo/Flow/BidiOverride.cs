using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class BidiOverride : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new BidiOverride(parent, props));

    protected BidiOverride(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:bidi-override";
    }

    public override Status Layout(Area area)
    {
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        RelativePositionProps mProps = _propertyManager.GetRelativePositionProps();
        return base.Layout(area);
    }
}