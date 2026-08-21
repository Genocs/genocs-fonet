using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Expr;

internal class NumericProperty : Property
{
    internal Numeric Numeric { get; set; }

    internal NumericProperty(Numeric value)
    {
        Numeric = value;
    }

    public override Numeric GetNumeric()
    {
        return this.Numeric;
    }

    public override Number GetNumber()
    {
        return Numeric.AsNumber();
    }

    public override Length GetLength()
    {
        return Numeric.AsLength();
    }

    public override ColorType GetColorType()
    {
        return null;
    }

    public override object GetObject()
    {
        return Numeric;
    }
}