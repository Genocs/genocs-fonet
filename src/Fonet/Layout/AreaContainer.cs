using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout;

internal class AreaContainer(FontState? fontState, int xPosition, int yPosition, int allocationWidth, int maxHeight, int position, Area? parent)
    : Area(fontState, allocationWidth, maxHeight, parent)
{
    internal int XPosition { get; set; } = xPosition;
    internal int YPosition { get; set; } = yPosition;
    internal int Position { get; set; } = position;

    public string? AreaName { get; set; }

    public override void Render(PdfRenderer renderer)
        => renderer.RenderAreaContainer(this);

    public int getPosition()
    {
        return Position;
    }

    public int GetCurrentYPosition()
    {
        return YPosition;
    }

    public void shiftYPosition(int value)
    {
        YPosition += value;
    }
}