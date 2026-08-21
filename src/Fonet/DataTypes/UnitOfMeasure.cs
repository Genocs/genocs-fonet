using System.Runtime.CompilerServices;

namespace Genocs.Fonet.DataTypes;

/// <summary>
/// The unit of measure handled by the library.
/// </summary>
internal enum UnitOfMeasure
{
    /// <summary>
    /// Standard, (multiplay value by 1).
    /// </summary>
    Standard,

    /// <summary>
    /// Inch.
    /// </summary>
    Inch,

    /// <summary>
    /// Centimenter.
    /// </summary>
    Centimenter,

    /// <summary>
    /// Millimenter.
    /// </summary>
    Millimenter,

    /// <summary>
    /// Point.
    /// </summary>
    Point,

    /// <summary>
    /// Point.
    /// </summary>
    PC,

    /// <summary>
    /// Pixel.
    /// </summary>
    Pixel
}

internal static class UnitOfMeasureExtensions
{

    public static UnitOfMeasure FromString(this string unitOfMeasure)
    {
        if (unitOfMeasure.Equals("in"))
        {
            return UnitOfMeasure.Inch;
        }

        if (unitOfMeasure.Equals("cm"))
        {
            return UnitOfMeasure.Centimenter;
        }

        if (unitOfMeasure.Equals("mm"))
        {
            return UnitOfMeasure.Millimenter;
        }

        if (unitOfMeasure.Equals("pt"))
        {
            return UnitOfMeasure.Point;
        }

        if (unitOfMeasure.Equals("pc"))
        {
            return UnitOfMeasure.PC;
        }

        if (unitOfMeasure.Equals("px"))
        {
            return UnitOfMeasure.Pixel;
        }

        FonetDriver.ActiveDriver?.FireFonetError($"Unknown length unit '{unitOfMeasure}'");
        return UnitOfMeasure.Standard;
    }
}