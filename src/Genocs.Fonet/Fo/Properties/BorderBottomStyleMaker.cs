using Genocs.Fonet.Fo;
using System.Text;

namespace Genocs.Fonet.Fo.Properties
{
    internal class BorderBottomStyleMaker : GenericBorderStyle
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new BorderBottomStyleMaker(propName);
        }

        protected BorderBottomStyleMaker(string name) : base(name) { }


        public override Property Compute(PropertyList propertyList)
        {
            FObj parentFO = propertyList.GetParentFObj();
            StringBuilder sbExpr = new StringBuilder();
            Property p = null;
            sbExpr.Append("border-");
            sbExpr.Append(propertyList.AbsoluteToRelative(PropertyList.BOTTOM));
            sbExpr.Append("-style");
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
                listprop = (ListProperty)propertyList.GetExplicitProperty("border-bottom");
                if (listprop != null)
                {
                    IShortHandParser shparser = new GenericShorthandParser(listprop);
                    p = shparser.GetValueForProperty(PropertyName, this, propertyList);
                }
            }

            if (p == null)
            {
                listprop = (ListProperty)propertyList.GetExplicitProperty("border-style");
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

    }
}