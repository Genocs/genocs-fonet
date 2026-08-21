namespace Genocs.Fonet.Fo.Properties;

internal class KeepWithNextMaker : GenericKeep
{
    new public static PropertyMaker Maker(string propName)
    {
        return new KeepWithNextMaker(propName);
    }

    protected KeepWithNextMaker(string name) 
        : base(name)
    { 
    }


    public override bool IsInherited()
        => false;

    private Property m_defaultProp = null;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "auto", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}