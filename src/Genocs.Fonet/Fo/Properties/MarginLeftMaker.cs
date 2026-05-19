namespace Genocs.Fonet.Fo.Properties;

internal class MarginLeftMaker : GenericMargin
{
    public static PropertyMaker Maker(string propName) => new MarginLeftMaker(propName);

    protected MarginLeftMaker(string name) : base(name) { }

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
