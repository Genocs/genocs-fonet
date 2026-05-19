using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class Length
{
    protected int _millipoints;
    protected bool _isComputed;

    public int MValue()
    {
        if (!_isComputed)
        {
            ComputeValue();
        }
        return _millipoints;
    }

    public virtual void ComputeValue()
    {
    }

    protected void SetComputedValue(int millipoints)
    {
        SetComputedValue(millipoints, true);
    }

    protected void SetComputedValue(int millipoints, bool bSetComputed)
    {
        _millipoints = millipoints;
        _isComputed = bSetComputed;
    }

    public virtual bool IsAuto()
        => false;

    public bool IsComputed()
        => _isComputed;

    public virtual double GetTableUnits()
        => 0.0;

    public virtual void ResolveTableUnit(double dTableUnit)
    {
    }

    public virtual Numeric? AsNumeric()
        => null;

    public override string ToString()
        => $"{_millipoints}mpt";
}