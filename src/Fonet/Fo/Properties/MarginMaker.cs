namespace Genocs.Fonet.Fo.Properties;

internal class MarginMaker : ListProperty.Maker
{
    public static PropertyMaker Maker(string propName) => new MarginMaker(propName);

    protected MarginMaker(string name) : base(name) { }

    public override bool IsInherited() => false;
}
