namespace Genocs.Fonet.Fo.Properties;

internal class FloatMaker : EnumProperty.Maker
{
    private static readonly EnumProperty s_propNONE = new(FloatAlign.NONE);
    private static readonly EnumProperty s_propLEFT = new(FloatAlign.LEFT);
    private static readonly EnumProperty s_propRIGHT = new(FloatAlign.RIGHT);

    public static PropertyMaker Maker(string propName) => new FloatMaker(propName);

    protected FloatMaker(string name) : base(name) { }

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

        return base.CheckEnumValues(value);
    }

    private Property? m_defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "none", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}
