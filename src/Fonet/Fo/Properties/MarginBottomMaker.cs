namespace Genocs.Fonet.Fo.Properties;

internal class MarginBottomMaker : GenericMargin
{
    public static PropertyMaker Maker(string propName) => new MarginBottomMaker(propName);

    protected MarginBottomMaker(string name) : base(name) { }

    public override Property Compute(PropertyList propertyList)
    {
        FObj parentFO = propertyList.GetParentFObj();
        Property? p = propertyList.GetExplicitOrShorthandProperty(PropertyName);
        return p != null ? ConvertProperty(p, propertyList, parentFO) : null;
    }

    private Property? m_defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "0pt", propertyList.GetParentFObj());
        return m_defaultProp;
    }
}
