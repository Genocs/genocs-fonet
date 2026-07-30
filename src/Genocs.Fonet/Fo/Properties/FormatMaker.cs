namespace Genocs.Fonet.Fo.Properties
{
    internal class FormatMaker : StringProperty.Maker
    {
        new public static PropertyMaker Maker(string propName)
        {
            return new FormatMaker(propName);
        }

        protected FormatMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "1", propertyList.GetParentFObj());
            }
            return m_defaultProp;

        }

    }
}