using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiPropertySet : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new MultiPropertySet(parent, props));

    protected MultiPropertySet(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:multi-property-set";
    }

    public override Status Layout(Area area)
    {
        return base.Layout(area);
    }
}