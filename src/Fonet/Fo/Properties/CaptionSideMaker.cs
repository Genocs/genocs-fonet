namespace Genocs.Fonet.Fo.Properties;

internal class CaptionSideMaker : EnumProperty.Maker
{
    private static readonly EnumProperty s_propBEFORE = new(CaptionSide.BEFORE);
    private static readonly EnumProperty s_propAFTER = new(CaptionSide.AFTER);
    private static readonly EnumProperty s_propSTART = new(CaptionSide.START);
    private static readonly EnumProperty s_propEND = new(CaptionSide.END);

    public static PropertyMaker Maker(string propName) => new CaptionSideMaker(propName);

    protected CaptionSideMaker(string name) : base(name) { }

    public override bool IsInherited() => true;

    public override Property CheckEnumValues(string value)
    {
        if (value.Equals("before"))
        {
            return s_propBEFORE;
        }

        if (value.Equals("after"))
        {
            return s_propAFTER;
        }

        if (value.Equals("start"))
        {
            return s_propSTART;
        }

        if (value.Equals("end"))
        {
            return s_propEND;
        }

        return base.CheckEnumValues(value);
    }

    private Property? m_defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "before", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}
