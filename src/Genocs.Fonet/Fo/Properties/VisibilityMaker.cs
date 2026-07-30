namespace Genocs.Fonet.Fo.Properties;

internal class VisibilityMaker : EnumProperty.Maker
{
    private static readonly EnumProperty s_propVISIBLE = new(Visibility.VISIBLE);
    private static readonly EnumProperty s_propHIDDEN = new(Visibility.HIDDEN);
    private static readonly EnumProperty s_propCOLLAPSE = new(Visibility.COLLAPSE);

    public static PropertyMaker Maker(string propName) => new VisibilityMaker(propName);

    protected VisibilityMaker(string name) : base(name) { }

    public override bool IsInherited() => true;

    public override Property CheckEnumValues(string value)
    {
        if (value.Equals("visible"))
        {
            return s_propVISIBLE;
        }

        if (value.Equals("hidden"))
        {
            return s_propHIDDEN;
        }

        if (value.Equals("collapse"))
        {
            return s_propCOLLAPSE;
        }

        return base.CheckEnumValues(value);
    }

    private Property? m_defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "visible", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}
