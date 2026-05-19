using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class ToBeImplementedElement : FObj
{
    protected ToBeImplementedElement(FObj parent, PropertyList propertyList)
        : base(parent, propertyList) { }

    public override Status Layout(Area area)
        => new(Status.OK);
}