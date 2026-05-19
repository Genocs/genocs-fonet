using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo;

internal class ColorTypeProperty(ColorType colorType) : Property
{
    private readonly ColorType _colorType = colorType;

    internal class Maker(string propName) : PropertyMaker(propName)
    {
        public override Property ConvertProperty(Property p, PropertyList propertyList, FObj fo)
        {
            if (p is ColorTypeProperty)
            {
                return p;
            }

            ColorType val = p.GetColorType();
            if (val != null)
            {
                return new ColorTypeProperty(val);
            }

            return ConvertPropertyDatatype(p, propertyList, fo);
        }

    }

    public override ColorType GetColorType()
        => _colorType;

    public override object GetObject()
        => _colorType;
}