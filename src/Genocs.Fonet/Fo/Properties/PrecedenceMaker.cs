namespace Genocs.Fonet.Fo.Properties;

internal class PrecedenceMaker : EnumProperty.Maker
{
    protected static readonly EnumProperty s_propTRUE = new(Constants.TRUE);

    protected static readonly EnumProperty s_propFALSE = new(Constants.FALSE);

    new public static PropertyMaker Maker(string propName)
    {
        return new PrecedenceMaker(propName);
    }

    protected PrecedenceMaker(string name) : base(name) { }


    public override bool IsInherited()
    {
        return false;
    }

    public override Property CheckEnumValues(string value)
    {
        if (value.Equals("true"))
        {
            return s_propTRUE;
        }

        if (value.Equals("false"))
        {
            return s_propFALSE;
        }

        return base.CheckEnumValues(value);
    }

    private Property m_defaultProp = null;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "false", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}