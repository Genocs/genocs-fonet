namespace Genocs.Fonet.DataTypes;

internal class LinearCombinationLength : Length
{
    protected List<double> _factors = new(10);
    protected List<Length> _lengths = new(10);

    public LinearCombinationLength()
    {
    }

    public void AddTerm(double factor, Length length)
    {
        _factors.Add(factor);
        _lengths.Add(length);
    }

    public override void ComputeValue()
    {
        int result = 0;
        int numFactors = _factors.Count;
        for (int i = 0; i < numFactors; ++i)
        {
            double d = _factors[i];
            Length l = _lengths[i];
            result += (int)(d * l.MValue());
        }
        SetComputedValue(result);
    }
}