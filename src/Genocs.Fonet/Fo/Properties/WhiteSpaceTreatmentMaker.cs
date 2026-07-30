using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties
{
    internal class WhiteSpaceTreatmentMaker : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new WhiteSpaceTreatmentMaker(propName);
        }

        protected WhiteSpaceTreatmentMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return true;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "preserve", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
}