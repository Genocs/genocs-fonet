namespace Genocs.Fonet.Fo.Properties
{
    internal class ContentHeightMaker : LengthProperty.Maker
    {
        protected static readonly EnumProperty s_propSCALE_TO_FIT = new EnumProperty(Constants.SCALE_TO_FIT);

        protected static readonly EnumProperty s_propSCALE_DOWN_TO_FIT = new EnumProperty(Constants.SCALE_DOWN_TO_FIT);

        protected static readonly EnumProperty s_propSCALE_UP_TO_FIT = new EnumProperty(Constants.SCALE_UP_TO_FIT);

        new public static PropertyMaker Maker(string propName)
        {
            return new ContentHeightMaker(propName);
        }

        protected ContentHeightMaker(string name) : base(name) { }


        public override bool IsInherited()
        {
            return false;
        }

        protected override bool IsAutoLengthAllowed()
        {
            return true;
        }

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("scale-to-fit"))
            {
                return s_propSCALE_TO_FIT;
            }

            if (value.Equals("scale-down-to-fit"))
            {
                return s_propSCALE_DOWN_TO_FIT;
            }

            if (value.Equals("scale-up-to-fit"))
            {
                return s_propSCALE_UP_TO_FIT;
            }

            return base.CheckEnumValues(value);
        }

        private Property m_defaultProp = null;

        public override Property Make(PropertyList propertyList)
        {
            if (m_defaultProp == null)
            {
                m_defaultProp = Make(propertyList, "auto", propertyList.getParentFObj());
            }
            return m_defaultProp;

        }

    }
}