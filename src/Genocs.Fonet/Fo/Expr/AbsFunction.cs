namespace Genocs.Fonet.Fo.Expr;

internal class AbsFunction : FunctionBase
{
    public override int NumArgs
    {
        get
        {
            return 1;
        }
    }

    public override Property Eval(Property[] args, PropertyInfo propInfo)
    {
        Numeric num = args[0].GetNumeric();
        return num == null ? throw new PropertyException("Non numeric operand to abs function") : (Property)new NumericProperty(num.abs());
    }

}