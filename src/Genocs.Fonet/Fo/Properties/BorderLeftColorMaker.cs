using Genocs.Fonet.Fo;
using System.Text;

namespace Genocs.Fonet.Fo.Properties;

internal class BorderLeftColorMaker : GenericColor
{
    new public static PropertyMaker Maker(string propName)
    {
        return new BorderLeftColorMaker(propName);
    }

    protected BorderLeftColorMaker(string name) : base(name) { }


    public override bool IsInherited()
    {
        return false;
    }


    public override Property Compute(PropertyList propertyList)
    {
        FObj parentFO = propertyList.GetParentFObj();
        StringBuilder sbExpr = new StringBuilder();
        Property p = null;
        sbExpr.Append("border-");
        sbExpr.Append(propertyList.AbsoluteToRelative(PropertyList.LEFT));
        sbExpr.Append("-color");
        p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

        if (p != null)
        {
            p = ConvertProperty(p, propertyList, parentFO);
        }

        return p;
    }

    public override Property GetShorthand(PropertyList propertyList)
    {
        Property p = null;
        ListProperty listprop;

        if (p == null)
        {
            listprop = (ListProperty)propertyList.GetExplicitProperty("border-left");
            if (listprop != null)
            {
                // Get a parser for the shorthand to set the individual properties
                IShortHandParser shparser = new GenericShorthandParser(listprop);
                p = shparser.GetValueForProperty(PropertyName, this, propertyList);
            }
        }

        if (p == null)
        {
            listprop = (ListProperty)propertyList.GetExplicitProperty("border-color");
            if (listprop != null)
            {
                // Get a parser for the shorthand to set the individual properties
                IShortHandParser shparser = new BoxPropShorthandParser(listprop);
                p = shparser.GetValueForProperty(PropertyName, this, propertyList);
            }
        }

        if (p == null)
        {
            listprop = (ListProperty)propertyList.GetExplicitProperty("border");
            if (listprop != null)
            {
                // Get a parser for the shorthand to set the individual properties
                IShortHandParser shparser = new GenericShorthandParser(listprop);
                p = shparser.GetValueForProperty(PropertyName, this, propertyList);
            }
        }

        return p;
    }

    private Property m_defaultProp = null;

    public override Property Make(PropertyList propertyList)
    {
        m_defaultProp ??= Make(propertyList, "black", propertyList.GetParentFObj());
        return m_defaultProp;

    }
}