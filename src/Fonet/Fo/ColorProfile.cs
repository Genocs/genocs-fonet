namespace Genocs.Fonet.Fo;

internal class ColorProfile : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new ColorProfile(parent, props));

    protected ColorProfile(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:color-profile";
    }
}