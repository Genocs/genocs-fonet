using System.Text;

namespace Genocs.Fonet.Fo.Properties;

internal class BorderBottomColorMaker : GenericColor
{
    new public static PropertyMaker Maker(string propName)
    {
        return new BorderBottomColorMaker(propName);
    }

    protected BorderBottomColorMaker(string name) : base(name) { }

    public override Property? Compute(PropertyList propertyList)
    {
        FObj? parentFO = propertyList.GetParentFObj();
        StringBuilder sbExpr = new StringBuilder();
        Property? p = null;
        sbExpr.Append("border-");
        sbExpr.Append(propertyList.AbsoluteToRelative(PropertyList.BOTTOM));
        sbExpr.Append("-color");
        p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

        if (p != null)
        {
            p = ConvertProperty(p, propertyList, parentFO);
        }

        return p;
    }

    public override Property? GetShorthand(PropertyList propertyList)
    {
        Property? p = null;
        ListProperty listprop;

        if (p == null)
        {
            listprop = (ListProperty)propertyList.GetExplicitProperty("border-bottom");
            if (listprop != null)
            {
                IShortHandParser shortHandParser = new GenericShorthandParser(listprop);
                p = shortHandParser.GetValueForProperty(PropertyName, this, propertyList);
            }
        }

        if (p == null)
        {
            listprop = (ListProperty)propertyList.GetExplicitProperty("border-color");
            if (listprop != null)
            {
                IShortHandParser shparser = new BoxPropShorthandParser(listprop);
                p = shparser.GetValueForProperty(PropertyName, this, propertyList);
            }
        }

        if (p == null)
        {
            listprop = (ListProperty)propertyList.GetExplicitProperty("border");
            if (listprop != null)
            {
                IShortHandParser shparser = new GenericShorthandParser(listprop);
                p = shparser.GetValueForProperty(PropertyName, this, propertyList);
            }
        }

        return p;
    }

    private Property? _defaultProperty = null;

    public override Property Make(PropertyList propertyList)
        => _defaultProperty ??= Make(propertyList, "black", propertyList.GetParentFObj());

}