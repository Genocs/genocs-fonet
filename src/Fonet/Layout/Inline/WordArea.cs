using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout.Inline;

internal class WordArea : InlineArea
{
    private string text;

    public WordArea(FontState fontState, float red, float green, float blue, string text, int width)
        : base(fontState, width, red, green, blue)
    {
        this.text = text;
        this.contentRectangleWidth = width;
    }

    public override void Render(PdfRenderer renderer)
    {
        renderer.RenderWordArea(this);
    }

    public string getText()
    {
        return this.text;
    }
}