namespace Genocs.Fonet.Fo.Flow;

internal class Wrapper : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Wrapper(parent, props));

    public Wrapper(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:wrapper";
    }

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        FOText ft = new FOText(data, start, length, this);
        _children.Add(ft);
    }

}