using Genocs.Fonet.Fo;

namespace Genocs.Fonet.DataTypes;

internal interface ICompoundDataType
{
    void SetComponent(string componentName, Property componentValue, bool isDefault);

    Property? GetComponent(string componentName);
}