using Genocs.Fonet.Fo.Flow;

namespace Genocs.Fonet.Fo.Expr;

internal class BodyStartFunction : FunctionBase
{
    public override int NumArgs
    {
        get
        {
            return 0;
        }
    }

    public override Property Eval(Property[] args, PropertyInfo pInfo)
    {
        Numeric distance = pInfo.PropertyList.GetProperty("provisional-distance-between-starts").GetNumeric();

        FObj? item = pInfo.FObj;
        while (item != null && !(item is ListItem))
        {
            item = item.Parent;
        }
        if (item == null)
        {
            throw new PropertyException("body-start() called from outside an fo:list-item");
        }

        Numeric startIndent = item.Properties.GetProperty("start-indent").GetNumeric();

        return new NumericProperty(distance.Add(startIndent));
    }
}