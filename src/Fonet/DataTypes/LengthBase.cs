using Genocs.Fonet.Fo;

namespace Genocs.Fonet.DataTypes;

internal class LengthBase(FObj? parentFO, PropertyList propertyList, int baseType) : IPercentBase
{
    public const int CUSTOM_BASE = 0;
    public const int FONTSIZE = 1;
    public const int INH_FONTSIZE = 2;
    public const int CONTAINING_BOX = 3;
    public const int CONTAINING_REFAREA = 4;

    protected FObj? parentFO = parentFO;

    protected FObj? GetParentFO()
        => parentFO;

    protected PropertyList GetPropertyList()
        => propertyList;

    public int GetDimension()
        => 1;

    public double GetBaseValue()
        => 1.0;

    public int GetBaseLength()
    {
        switch (baseType)
        {
            case FONTSIZE:
                return propertyList.GetProperty("font-size")?.GetLength()?.Millipoints() ?? 0;
            case INH_FONTSIZE:
                return propertyList.GetInheritedProperty("font-size")?.GetLength()?.Millipoints() ?? 0;
            case CONTAINING_BOX:
                return parentFO?.GetContentWidth() ?? 0;
            case CONTAINING_REFAREA:
                {
                    FObj? fo;
                    for (fo = parentFO; fo?.GeneratesReferenceAreas() == false; fo = fo.Parent)
                    {
                    }

                    return fo?.GetContentWidth() ?? 0;
                }

            case CUSTOM_BASE:
                FonetDriver.ActiveDriver?.FireFonetError("LengthBase.getBaseLength() called on CUSTOM_BASE type");
                return 0;
            default:
                FonetDriver.ActiveDriver?.FireFonetError("Unknown base type for LengthBase");
                return 0;
        }
    }
}