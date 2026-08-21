using Genocs.Fonet.Layout;
using Genocs.Fonet.Layout.Inline;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Image;

internal class ImageArea : InlineArea
{
    protected int xOffset;
    protected int align;
    protected int valign;
    protected FonetImage image;

    public ImageArea(FontState fontState, FonetImage img, int allocationWidth, int width, int height, int startIndent, int endIndent, int align)
        : base(fontState, width, 0, 0, 0)
    {
        _currentHeight = height;
        this.contentRectangleWidth = width;
        this.height = height;
        this.image = img;
        this.align = align;
    }

    public override int getXOffset()
    {
        return this.xOffset;
    }

    public FonetImage getImage()
    {
        return this.image;
    }

    public override void Render(PdfRenderer renderer)
    {
        renderer.RenderImageArea(this);
    }

    public int getImageHeight()
    {
        return _currentHeight;
    }

    public void setAlign(int align)
    {
        this.align = align;
    }

    public int getAlign()
    {
        return this.align;
    }

    public override void setVerticalAlign(int align)
    {
        this.valign = align;
    }

    public override int getVerticalAlign()
    {
        return this.valign;
    }

    public void setStartIndent(int startIndent)
    {
        xOffset = startIndent;
    }
}