using Genocs.Fonet.Fo;

namespace Genocs.Fonet.DataTypes;

internal class LengthPair : ICompoundDataType
{
    private Property? _ipd;
    private Property? _bpd;

    public void SetComponent(string componentName, Property componentValue, bool isDefault)
    {
        if (componentName.Equals("block-progression-direction"))
        {
            _bpd = componentValue;
        }
        else if (componentName.Equals("inline-progression-direction"))
        {
            _ipd = componentValue;
        }
    }

    public Property? GetComponent(string componentName)
    {
        if (componentName.Equals("block-progression-direction"))
        {
            return GetBPD();
        }
        else if (componentName.Equals("inline-progression-direction"))
        {
            return GetIPD();
        }
        else
        {
            return null;
        }
    }

    public Property? GetIPD()
        => _ipd;

    public Property? GetBPD()
        => _bpd;
}