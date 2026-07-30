using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class TableColLength(double tableUnits) : Length
{
    public override double GetTableUnits()
        =>  tableUnits;

    public override void ResolveTableUnit(double milliPointsPerUnit)
        => SetComputedValue((int)(tableUnits * milliPointsPerUnit));

    public override string ToString()
        => $"{tableUnits} table-column-units";

    public override Numeric AsNumeric()
        => new(this);
}