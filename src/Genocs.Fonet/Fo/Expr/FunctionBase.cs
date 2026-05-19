using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Expr;

internal abstract class FunctionBase : IFunction
{
    public abstract int NumArgs { get; }
    public virtual IPercentBase GetPercentBase() => null;
    public abstract Property Eval(Property[] args, PropertyInfo propInfo);
}