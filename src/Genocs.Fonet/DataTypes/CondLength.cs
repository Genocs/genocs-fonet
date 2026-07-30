using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Properties;

namespace Genocs.Fonet.DataTypes;

internal class CondLength : ICompoundDataType
{
    private Property? _length;

    private Property? _conditionality;

    public void SetComponent(string componentName, Property componentValue, bool isDefault)
    {
        if (componentName.Equals("length"))
        {
            _length = componentValue;
        }
        else if (componentName.Equals("conditionality"))
        {
            _conditionality = componentValue;
        }
    }

    public Property? GetComponent(string name)
    {
        if (name.Equals("length"))
        {
            return _length;
        }
        else if (name.Equals("conditionality"))
        {
            return _conditionality;
        }
        else
        {
            return null;
        }
    }

    public Property? GetConditionality()
        => _conditionality;

    public Property? GetLength()
        => _length;

    public bool IsDiscard()
        => _conditionality?.GetEnum() == Constants.DISCARD;

    public int MValue()
        => _length?.GetLength()?.MValue() ?? 0;
}