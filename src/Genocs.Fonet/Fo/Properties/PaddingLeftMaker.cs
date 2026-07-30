using Genocs.Fonet.Fo;
using System.Text;

namespace Genocs.Fonet.Fo.Properties
{
    internal class PaddingLeftMaker : GenericPadding
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new PaddingLeftMaker(propName);
        }

        protected PaddingLeftMaker(string name) : base(name) { }


        public override Property Compute(PropertyList propertyList)
        {
            FObj parentFO = propertyList.GetParentFObj();
            StringBuilder sbExpr = new StringBuilder();
            Property p = null;
            sbExpr.Append("padding-");
            sbExpr.Append(propertyList.AbsoluteToRelative(PropertyList.LEFT));

            p = propertyList.GetExplicitOrShorthandProperty(sbExpr.ToString());

            if (p != null)
            {
                p = ConvertProperty(p, propertyList, parentFO);
            }

            return p;
        }

    }
}