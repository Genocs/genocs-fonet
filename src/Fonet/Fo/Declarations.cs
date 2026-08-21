namespace Genocs.Fonet.Fo;

internal class Declarations : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Declarations(parent, props));

    protected Declarations(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:declarations";
    }
}