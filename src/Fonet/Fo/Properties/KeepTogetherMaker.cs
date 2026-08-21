namespace Genocs.Fonet.Fo.Properties;

internal class KeepTogetherMaker : GenericKeep
{
    private Property? _defaultProperty;

    new public static PropertyMaker Maker(string propName)
    {
        return new KeepTogetherMaker(propName);
    }

    protected KeepTogetherMaker(string name) 
        : base(name)
    {
    }


    public override bool IsInherited()
        => false;

    public override Property Make(PropertyList propertyList)
    {
        _defaultProperty ??= Make(propertyList, "auto", propertyList.GetParentFObj());
        return _defaultProperty;
    }
}