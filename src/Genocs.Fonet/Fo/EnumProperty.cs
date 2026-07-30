namespace Genocs.Fonet.Fo;

internal class EnumProperty(int explicitValue) : Property
{
    internal class Maker : PropertyMaker
    {
        protected Maker(string propName)
            : base(propName)
        {
        }

        public override Property? CheckEnumValues(string value)
        {
            FonetDriver.ActiveDriver?.FireFonetError($"Unknown enumerated value for property '{PropertyName}': {value}");
            return null;
        }

        protected Property? FindConstant(string value)
        {
            return null;
        }

        public override Property? ConvertProperty(Property p, PropertyList propertyList, FObj fo)
        {
            if (p is EnumProperty)
            {
                return p;
            }
            else
            {
                return null;
            }
        }
    }

    private readonly int _value = explicitValue;

    public override int GetEnum()
        => _value;

    public override object GetObject()
        => _value;
}