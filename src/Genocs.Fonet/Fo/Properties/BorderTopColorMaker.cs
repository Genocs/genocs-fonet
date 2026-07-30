using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Properties;
using System.Text;

namespace Genocs.Fonet.Fo.Properties
{
    internal class BorderTopColorMaker : GenericColor
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new BorderTopColorMaker(propName);
        }

        protected BorderTopColorMaker(string name) : base(name) { }


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
            sbExpr.Append(propertyList.AbsoluteToRelative(PropertyList.TOP));
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
                listprop = (ListProperty)propertyList.GetExplicitProperty("border-top");
                if (listprop != null)
                {
                    IShortHandParser shparser = new GenericShorthandParser(listprop);
                    p = shparser.GetValueForProperty(PropertyName, this, propertyList);
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

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "black", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }
    }
}