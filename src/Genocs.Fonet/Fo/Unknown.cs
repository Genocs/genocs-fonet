using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class Unknown : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Unknown(parent, props));

    protected Unknown(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "unknown";
    }

    public override Status Layout(Area area)
        => new Status(Status.OK);
}