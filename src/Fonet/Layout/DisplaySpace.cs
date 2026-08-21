using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout
{
    internal class DisplaySpace : Space
    {
        private int size;

        public DisplaySpace(int size)
        {
            this.size = size;
        }

        public int getSize()
        {
            return size;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderDisplaySpace(this);
        }

    }
}