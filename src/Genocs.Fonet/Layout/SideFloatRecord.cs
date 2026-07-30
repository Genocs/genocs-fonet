namespace Genocs.Fonet.Layout;

internal sealed class SideFloatRecord(int side, int yStart, int width, int height, SideFloatArea area)
{
    internal int Side { get; } = side;
    internal int YStart { get; } = yStart;
    internal int Width { get; } = width;
    internal int Height { get; } = height;
    internal SideFloatArea Area { get; } = area;

    internal int YEnd => YStart + Height;

    internal bool Overlaps(int yPosition, int lineHeight)
        => yPosition < YEnd && yPosition + lineHeight > YStart;
}
