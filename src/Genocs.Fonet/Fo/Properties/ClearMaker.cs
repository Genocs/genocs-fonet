namespace Genocs.Fonet.Fo.Properties;

internal class ClearMaker : EnumProperty.Maker
{
    private static readonly EnumProperty s_propNONE = new(Clear.NONE);
    private static readonly EnumProperty s_propLEFT = new(Clear.LEFT);
    private static readonly EnumProperty s_propRIGHT = new(Clear.RIGHT);
    private static readonly EnumProperty s_propBOTH = new(Clear.BOTH);

    public static PropertyMaker Maker(string propName) => new ClearMaker(propName);

    protected ClearMaker(string name) : base(name) { }

    public override bool IsInherited() => false;

    public override Property CheckEnumValues(string value)
    {
        if (value.Equals("none"))
        {
            return s_propNONE;
        }

        if (value.Equals("left"))
        {
            return s_propLEFT;
        }

        if (value.Equals("right"))
        {
            return s_propRIGHT;
        }

        if (value.Equals("both"))
        {
            return s_propBOTH;
        }

        return base.CheckEnumValues(value);
    }

    private Property? m_defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "none", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}
