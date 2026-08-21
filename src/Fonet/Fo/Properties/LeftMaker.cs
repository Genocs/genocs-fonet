namespace Genocs.Fonet.Fo.Properties;

internal class LeftMaker : LengthProperty.Maker
{
    private Property? _defaultProperty;

    new public static PropertyMaker Maker(string propName)
    {
        return new LeftMaker(propName);
    }

    protected LeftMaker(string name) 
        : base(name)
    { 
    }


    public override bool IsInherited()
        => false;

    protected override bool IsAutoLengthAllowed()
        => true;

    public override Property Make(PropertyList propertyList)
    {
        _defaultProperty ??= Make(propertyList, "auto", propertyList.GetParentFObj());
        return _defaultProperty;
    }
}