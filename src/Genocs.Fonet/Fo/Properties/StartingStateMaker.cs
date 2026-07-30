using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties
{
    internal class StartingStateMaker : ToBeImplementedProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new StartingStateMaker(propName);
        }

        protected StartingStateMaker(string name) : base(name) { }

        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "show", propertyList.GetParentFObj());
            }
            return m_defaultProp;
        }

    }
}