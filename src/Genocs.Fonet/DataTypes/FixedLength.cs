using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class FixedLength : Length
{
    public FixedLength(double numRelUnits, int iCurFontSize)
    {
        SetComputedValue((int)(numRelUnits * (double)iCurFontSize));
    }

    public FixedLength(double numUnits, string units)
    {
        Convert(numUnits, units);
    }

    public FixedLength(int baseUnits)
    {
        SetComputedValue(baseUnits);
    }

    protected void Convert(double dvalue, string unit)
    {
        int assumed_resolution = 1;

        if (unit.Equals("in"))
        {
            dvalue *= 72;
        }
        else if (unit.Equals("cm"))
        {
            dvalue *= 28.3464567;
        }
        else if (unit.Equals("mm"))
        {
            dvalue *= 2.83464567;
        }
        else if (unit.Equals("pt"))
        {
        }
        else if (unit.Equals("pc"))
        {
            dvalue *= 12;
        }
        else if (unit.Equals("px"))
        {
            dvalue *= assumed_resolution;
        }
        else
        {
            dvalue = 0;
            FonetDriver.ActiveDriver?.FireFonetError($"Unknown length unit '{unit}'");
        }

        SetComputedValue((int)(dvalue * 1000));
    }

    public override Numeric AsNumeric()
        => new(this);
}