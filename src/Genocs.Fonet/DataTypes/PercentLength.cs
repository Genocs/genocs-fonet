using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class PercentLength(double factor, IPercentBase? baseLength) : Length
{
    private readonly double _factor = factor;
    public IPercentBase? BaseLength { get; set; } = baseLength;


    public PercentLength(double factor) : this(factor, null)
    {
    }


    public override void ComputeValue()
    {
        SetComputedValue((int)(_factor * BaseLength?.GetBaseLength() ?? 0));
    }

    public double Value()
    {
        return _factor;
    }

    public override string ToString()
    {
        return (_factor * 100.0).ToString() + "%";
    }

    public override Numeric AsNumeric()
    {
        return new Numeric(this);
    }
}