using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.Fo;

internal class NumberProperty : Property
{
    internal class Maker(string propName) : PropertyMaker(propName)
    {
        public override Property ConvertProperty(Property p, PropertyList propertyList, FObj fo)
        {
            if (p is NumberProperty)
            {
                return p;
            }
            Number val = p.GetNumber();
            if (val != null)
            {
                return new NumberProperty(val);
            }
            return ConvertPropertyDatatype(p, propertyList, fo);
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