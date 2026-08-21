using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class FixedLength : Length
{
    public FixedLength(double numRelUnits, int fontSize)
    {
        SetComputedValue((int)(numRelUnits * fontSize));
    }

    public FixedLength(double numUnits, string unitOfMeasure)
    {
        Convert(numUnits, unitOfMeasure.FromString());
    }

    public FixedLength(int baseUnits)
    {
        SetComputedValue(baseUnits);
    }

    public override Numeric AsNumeric()
        => new(this);

    protected void Convert(double value, UnitOfMeasure unitOfMeasure)
    {
        const int standardResolution = 1;

        switch (unitOfMeasure)
        {
            case UnitOfMeasure.Inch:
                value *= 72;
                break;
            case UnitOfMeasure.Centimenter:
                value *= 28.3464567;
                break;
            case UnitOfMeasure.Millimenter:
                value *= 2.83464567;
                break;
            case UnitOfMeasure.Point:
                break;
            case UnitOfMeasure.PC:
                value *= 12;
                break;
            case UnitOfMeasure.Pixel:
                value *= standardResolution;
                break;
            default:
                value = 0;
                FonetDriver.ActiveDriver?.FireFonetError($"Unknown length unit '{unitOfMeasure}'");
                break;
        }

        SetComputedValue((int)(value * 1000));
    }
}