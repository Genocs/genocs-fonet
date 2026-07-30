using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties
{
    internal class IndicateDestinationMaker : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new IndicateDestinationMaker(propName);
        }

        protected IndicateDestinationMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "false", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}