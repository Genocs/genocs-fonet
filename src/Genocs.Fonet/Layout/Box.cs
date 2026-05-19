using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout;

internal abstract class Box
{
    protected internal Area? parent;

    protected internal AreaTree? areaTree;

    public abstract void Render(PdfRenderer renderer);
}