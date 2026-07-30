using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Flow;

namespace Genocs.Fonet.Fo.Expr;

internal class LabelEndFunction : FunctionBase
{
    public override int NumArgs { get { return 0; } }

    public override Property Eval(Property[] args, PropertyInfo pInfo)
    {
        Length distance = pInfo.PropertyList.GetProperty("provisional-distance-between-starts").GetLength();
        Length separation = pInfo.PropertyList.GetNearestSpecifiedProperty("provisional-label-separation").GetLength();

        FObj? item = pInfo.FObj;

        while (item != null && item is not ListItem)
        {
            item = item.Parent;
        }

        if (item == null)
        {
            throw new PropertyException("label-end() called from outside an fo:list-item");
        }

        Length startIndent = item.Properties.GetProperty("start-indent").GetLength();

        LinearCombinationLength labelEnd = new LinearCombinationLength();

        LengthBase bse = new LengthBase(item, pInfo.PropertyList, LengthBase.CONTAINING_BOX);
        PercentLength refWidth = new PercentLength(1.0, bse);

        labelEnd.AddTerm(1.0, refWidth);
        labelEnd.AddTerm(-1.0, distance);
        labelEnd.AddTerm(-1.0, startIndent);
        labelEnd.AddTerm(1.0, separation);

        labelEnd.ComputeValue();

        return new LengthProperty(labelEnd);
    }
}