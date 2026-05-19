namespace Genocs.Fonet.Fo.Properties;

internal class MarginRightMaker : GenericMargin
{
    public static PropertyMaker Maker(string propName) => new MarginRightMaker(propName);

    protected MarginRightMaker(string name) : base(name) { }

    public override Property Compute(PropertyList propertyList)
    {
        FObj parentFO = propertyList.getParentFObj();
        Property? p = propertyList.GetExplicitOrShorthandProperty(PropName);
        return p != null ? ConvertProperty(p, propertyList, parentFO) : null;
    }

    private Property? m_defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "0pt", propertyList.getParentFObj());
        return m_defaultProp;
    }
}
