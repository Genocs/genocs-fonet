using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class MultiPropertySet : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new MultiPropertySet(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
        => new Maker();

    protected MultiPropertySet(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:multi-property-set";
    }

    public override Status Layout(Area area)
    {
        return base.Layout(area);
    }
}