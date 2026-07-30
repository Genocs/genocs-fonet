using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.Fo;

internal class NumberProperty : Property
{
    internal class Maker(string propName) : PropertyMaker(propName)
    {
        public override Property? ConvertProperty(Property property, PropertyList propertyList, FObj? fo)
        {
            if (property is NumberProperty)
            {
                return property;
            }

            Number? number = property.GetNumber();
            if (number != null)
            {
                return new NumberProperty(number);
            }

            return ConvertPropertyDataType(property, propertyList, fo);
        }
    }

    private readonly decimal _number;

    public NumberProperty(Number number)
       => _number = number.DecimalValue();

    public NumberProperty(decimal number)
        => _number = number;

    public NumberProperty(double number)
        => _number = (decimal)number;

    public NumberProperty(int number)
        => _number = number;

    public override Number GetNumber()
        => new(_number);

    public override object GetObject()
        => _number;

    public override Numeric GetNumeric()
        => new(_number);

    public override ColorType GetColorType()
        => ColorType.Empty;
}