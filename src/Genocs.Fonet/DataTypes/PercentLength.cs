using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal sealed class PercentLength(double factor, IPercentBase? baseLength) : Length
{
    private readonly double _factor = factor;
    public IPercentBase? BaseLength { get; set; } = baseLength;

    public PercentLength(double factor) : this(factor, null)
    {
    }

    public override void ComputeValue()
        => SetComputedValue((int)(_factor * BaseLength?.GetBaseLength() ?? 0));

    public double Value()
    => _factor;

    public override string ToString()
        => $"{_factor * 100.0:0.##}%";

    public override Numeric AsNumeric()
        => new(this);
}