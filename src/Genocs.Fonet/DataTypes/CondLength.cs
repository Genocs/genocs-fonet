using Genocs.Fonet.Fo;
using Genocs.Fonet.Fo.Properties;

namespace Genocs.Fonet.DataTypes;

internal class CondLength : ICompoundDatatype
{
    private Property? _length;

    private Property? _conditionality;

    public void SetComponent(string sCmpnName, Property cmpnValue, bool bIsDefault)
    {
        if (sCmpnName.Equals("length"))
        {
            _length = cmpnValue;
        }
        else if (sCmpnName.Equals("conditionality"))
        {
            _conditionality = cmpnValue;
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
        => _length?.GetLength().MValue() ?? 0;
}