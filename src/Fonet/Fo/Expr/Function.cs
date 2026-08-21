using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Expr;

internal interface IFunction
{
    int NumArgs { get; }

    IPercentBase GetPercentBase();

    Property Eval(Property[] args, PropertyInfo propInfo);
}