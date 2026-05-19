using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class BidiOverride : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new BidiOverride(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    protected BidiOverride(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:bidi-override";
    }

    public override Status Layout(Area area)
    {
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        RelativePositionProps mProps = _propertyManager.GetRelativePositionProps();
        return base.Layout(area);
    }
}