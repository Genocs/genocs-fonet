using Genocs.Fonet.Fo;

namespace Genocs.Fonet.DataTypes;

internal class LengthRange : ICompoundDataType
{
    private Property? _minimum;
    private Property? _optimum;
    private Property? _maximum;
    private const int MINSET = 1;
    private const int OPTSET = 2;
    private const int MAXSET = 4;
    private int bfSet = 0;
    private bool bChecked = false;

    public virtual void SetComponent(string componentName, Property componentValue, bool isDefault)
    {
        if (componentName.Equals("minimum"))
        {
            SetMinimum(componentValue, isDefault);
        }
        else if (componentName.Equals("optimum"))
        {
            SetOptimum(componentValue, isDefault);
        }
        else if (componentName.Equals("maximum"))
        {
            SetMaximum(componentValue, isDefault);
        }
    }

    public virtual Property? GetComponent(string sCmpnName)
    {
        if (sCmpnName.Equals("minimum"))
        {
            return GetMinimum();
        }
        else if (sCmpnName.Equals("optimum"))
        {
            return GetOptimum();
        }
        else if (sCmpnName.Equals("maximum"))
        {
            return GetMaximum();
        }
        else
        {
            return null;
        }
    }

    protected void SetMinimum(Property minimum, bool bIsDefault)
    {
        _minimum = minimum;
        if (!bIsDefault)
        {
            bfSet |= MINSET;
        }
    }

    protected void SetMaximum(Property max, bool bIsDefault)
    {
        _maximum = max;
        if (!bIsDefault)
        {
            bfSet |= MAXSET;
        }
    }

    protected void SetOptimum(Property opt, bool bIsDefault)
    {
        _optimum = opt;
        if (!bIsDefault)
        {
            bfSet |= OPTSET;
        }
    }

    private void CheckConsistency()
    {
        if (bChecked)
        {
            return;
        }
        bChecked = true;
    }

    public Property? GetMinimum()
    {
        CheckConsistency();
        return _minimum;
    }

    public Property? GetMaximum()
    {
        CheckConsistency();
        return _maximum;
    }

    public Property? GetOptimum()
    {
        CheckConsistency();
        return _optimum;
    }
}