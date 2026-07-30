using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout;

/// <summary>
/// Side-float reference area (xsl-side-float) for fo:float content.
/// </summary>
internal class SideFloatArea : AreaContainer
{
    private int _contentWidth;

    internal SideFloatArea(
        FontState fontState,
        int xPosition,
        int yPosition,
        int allocationWidth,
        int maxHeight,
        int floatSide)
        : base(fontState, xPosition, yPosition, allocationWidth, maxHeight, Fo.Properties.Position.ABSOLUTE, null)
    {
        FloatSide = floatSide;
        areaClass = AreaClass.XSL_SIDE_FLOAT;
    }

    internal int FloatSide { get; }

    public override void Render(PdfRenderer renderer)
        => renderer.RenderAreaContainer(this);

    public override void end()
    {
    }

    public override void start()
    {
    }

    public override int spaceLeft()
        => maxHeight - currentHeight;

    public override int getContentWidth()
        => _contentWidth > 0 ? _contentWidth : contentRectangleWidth;

    internal void SetContentWidth(int width)
    {
        _contentWidth = width;
        contentRectangleWidth = width;
    }

    internal void FinalizePosition(int columnXPosition, int columnWidth)
    {
        if (FloatSide == FloatAlign.RIGHT)
        {
            XPosition = columnXPosition + columnWidth - _contentWidth;
        }
        else
        {
            XPosition = columnXPosition;
        }
    }
}
