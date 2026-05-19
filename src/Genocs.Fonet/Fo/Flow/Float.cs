using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Float : FObjMixed
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList) =>
            new Float(parent, propertyList);
    }

    new public static FObj.Maker GetMaker() => new Maker();

    protected Float(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:float";
    }

    public override Status Layout(Area area)
    {
        if (!_propertyManager.IsVisible())
        {
            return new Status(Status.OK);
        }

        // Full side-float placement is deferred; render floated content in flow order.
        return base.Layout(area);
    }
}
