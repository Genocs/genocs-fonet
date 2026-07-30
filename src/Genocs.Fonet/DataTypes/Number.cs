namespace Genocs.Fonet.DataTypes;

internal class Number
{
    private readonly decimal _value;

    public Number(int n)
        => _value = n;

    public Number(decimal n)
        => _value = n;

    public Number(double n)
        => _value = (decimal)n;

    public int IntValue()
        => (int)_value;

    public double DoubleValue()
        => (double)_value;

    public float FloatValue()
        => (float)_value;

    public decimal DecimalValue()
        => _value;
}