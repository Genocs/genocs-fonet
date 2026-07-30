using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal abstract class Length
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

    protected void SetComputedValue(int millipoints, bool isComputed)
    {
        _millipoints = millipoints;
        _isComputed = isComputed;
    }

    public virtual bool IsAuto()
        => false;

    public bool IsComputed()
        => _isComputed;

    public virtual double GetTableUnits()
        => 0.0;

    public virtual Numeric? AsNumeric()
        => null;

    public override string ToString()
        => $"{_millipoints}mpt";

    public virtual void ResolveTableUnit(double tableUnit)
        => FonetDriver.ActiveDriver?.FireFonetError($"NOP for ResolveTableUnit: {tableUnit}");
}