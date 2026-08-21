using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties;

internal class BackgroundColorMaker : GenericColor
{
    new public static PropertyMaker Maker(string propName)
    {
        return new BackgroundColorMaker(propName);
    }

    protected BackgroundColorMaker(string name) : base(name) { }

    public override bool IsInherited()
    {
        return false;
    }

    private Property m_defaultProp = null;

    public override Property Make(PropertyList propertyList)
    {
        if (m_defaultProp == null)
        {
            m_defaultProp = Make(propertyList, "transparent", propertyList.GetParentFObj());
        }
        return m_defaultProp;
    }

    protected override Property? ConvertPropertyDataType(Property p, PropertyList propertyList, FObj? fo)
    {
        string? nameval = p.GetNCname();
        if (nameval != null)
        {
            return new ColorTypeProperty(new ColorType(nameval));
        }
        return base.ConvertPropertyDataType(p, propertyList, fo);
    }
}