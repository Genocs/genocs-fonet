using System.Text;
using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.DataTypes;

internal class MixedLength(List<Length> lengths) : Length
{
    public override void ComputeValue()
    {
        int computedValue = 0;
        bool allComputed = true;
        foreach (Length l in lengths)
        {
            computedValue += l.Millipoints();
            if (!l.IsComputed())
            {
                allComputed = false;
            }
        }

        SetComputedValue(computedValue, allComputed);
    }

    public override double GetTableUnits()
    {
        double tableUnits = 0.0;
        foreach (Length l in lengths)
        {
            tableUnits += l.GetTableUnits();
        }
        return tableUnits;
    }

    public override void ResolveTableUnit(double tableUnit)
    {
        foreach (Length l in lengths)
        {
            l.ResolveTableUnit(tableUnit);
        }
    }

    public override string ToString()
    {
        StringBuilder sbuf = new();
        foreach (Length l in lengths)
        {
            if (sbuf.Length > 0)
            {
                sbuf.Append('+');
            }

            sbuf.Append(l.ToString());
        }

        return sbuf.ToString();
    }

    public override Numeric? AsNumeric()
    {
        Numeric? numeric = null;
        foreach (Length l in lengths)
        {
            if (numeric == null)
            {
                numeric = l.AsNumeric();
            }
            else
            {
                try
                {
                    Numeric sum = numeric.Add(l.AsNumeric());
                    numeric = sum;
                }
                catch (PropertyException pe)
                {
                    Console.Error.WriteLine($"Can't convert MixedLength to Numeric: {pe}");
                }
            }
        }

        return numeric;
    }
}