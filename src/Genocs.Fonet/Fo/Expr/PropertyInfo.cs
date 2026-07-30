using Genocs.Fonet.DataTypes;
using System.Collections;

namespace Genocs.Fonet.Fo.Expr;

internal class PropertyInfo(PropertyMaker maker, PropertyList propertyList, FObj? fo)
{
    public PropertyList PropertyList { get; } = propertyList;
    public FObj? FObj { get; } = fo;

    private Stack? _stkFunction;

    public bool InheritsSpecified()
        => maker.InheritsSpecified();

    public IPercentBase? GetPercentBase()
    {
        IPercentBase? percentageBase = getFunctionPercentBase();
        return (percentageBase != null) ? percentageBase : maker.GetPercentBase(FObj, PropertyList);
    }

    public int currentFontSize()
    {
        return PropertyList.GetProperty("font-size")?.GetLength()?.MValue() ?? 0;
    }

    public void Push(IFunction func)
    {
        _stkFunction ??= new Stack();
        _stkFunction.Push(func);
    }

    public void Pop()
        => _stkFunction?.Pop();

    private IPercentBase? getFunctionPercentBase()
    {
        if (_stkFunction != null)
        {
            IFunction? f = (IFunction?)_stkFunction.Peek();
            if (f != null)
            {
                return f.GetPercentBase();
            }
        }

        return null;
    }
}