using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Pagination;
using System.Collections;

namespace Genocs.Fonet.Layout;

internal class AreaTree(StreamRenderer streamRenderer)
{
   public FontInfo? FontInfo { get; set; }

    private readonly StreamRenderer streamRenderer = streamRenderer;

    public void addPage(Page page)
    {
        try
        {
            streamRenderer.QueuePage(page);
        }
        catch (IOException e)
        {
            throw new FonetException("", e);
        }
    }

    public IDReferences getIDReferences()
    {
        return streamRenderer.GetIDReferences();
    }

    public ArrayList GetDocumentMarkers()
    {
        return streamRenderer.GetDocumentMarkers();
    }

    public PageSequence GetCurrentPageSequence()
    {
        return streamRenderer.GetCurrentPageSequence();
    }

    public ArrayList GetCurrentPageSequenceMarkers()
    {
        return streamRenderer.GetCurrentPageSequenceMarkers();
    }
}
