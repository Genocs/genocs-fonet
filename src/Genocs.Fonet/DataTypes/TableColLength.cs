using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class TableColLength : Length
{
    private double _columns;

    public TableColLength(double tcolUnits)
    {
        _columns = tcolUnits;
    }

    public override double GetTableUnits()
    {
        return _columns;
    }

    public override void ResolveTableUnit(double mpointsPerUnit)
    {
        SetComputedValue((int)(_columns * mpointsPerUnit));
    }

    public override string ToString()
    {
        return $"{_columns} table-column-units";
    }

    public override Numeric AsNumeric()
    {
        return new Numeric(this);
    }
}