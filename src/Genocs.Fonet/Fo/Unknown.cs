using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class Unknown : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
            => new Unknown(parent, propertyList);
    }

    new public static FObj.Maker GetMaker()
        => new Maker();

    protected Unknown(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "unknown";
    }

    public override Status Layout(Area area)
        => new Status(Status.OK);
}