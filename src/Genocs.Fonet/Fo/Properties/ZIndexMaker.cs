namespace Genocs.Fonet.Fo.Properties;

internal class ZIndexMaker : LengthProperty.Maker
{
    public static PropertyMaker Maker(string propName) => new ZIndexMaker(propName);

    protected ZIndexMaker(string name) : base(name) { }

    protected override bool IsAutoLengthAllowed() => true;

    public override bool IsInherited() => false;

    public override Property Make(PropertyList propertyList) =>
        Make(propertyList, "auto", propertyList.getParentFObj());
}
