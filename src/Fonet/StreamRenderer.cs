using System.Collections;
using Genocs.Fonet.Apps;
using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Flow;
using Genocs.Fonet.Fo.Pagination;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet;

/// <summary>
/// This class acts as a bridge between the XML:FO parser and the
/// formatting/rendering classes. It will queue PageSequences up until
/// all the IDs required by them are satisfied, at which time it will
/// Render the pages.
/// StreamRenderer is created by Driver and called from FOTreeBuilder
/// when a PageSequence is created, and AreaTree when a Page is formatted.
/// </summary>
internal class StreamRenderer(PdfRenderer renderer)
{
    /// <summary>
    /// Keep track of the number of pages rendered.
    /// </summary>
    private int _pageCount = 0;

    /// <summary>
    /// The renderer being used.
    /// </summary>
    private readonly PdfRenderer _renderer = renderer;

    /// <summary>
    /// The formatting results to be handed back to the caller.
    /// </summary>
    private FormattingResults _results = new();

    /// <summary>
    /// The FontInfo for this renderer.
    /// </summary>
    private readonly FontInfo fontInfo = new();

    /// <summary>
    /// The list of pages waiting to be renderered.
    /// </summary>
    private readonly ArrayList renderQueue = new();

    /// <summary>
    /// The current set of IDReferences, passed to the areatrees 
    /// and pages. This is used by the AreaTree as a single map of 
    /// all IDs.
    /// </summary>
    private readonly IDReferences idReferences = new();

    /// <summary>
    /// The list of markers.
    /// </summary>
    private ArrayList? documentMarkers;

    private ArrayList? currentPageSequenceMarkers;
    private PageSequence? currentPageSequence;

    public IDReferences GetIDReferences()
    {
        return idReferences;
    }

    public FormattingResults getResults()
        => _results;

    public void StartRenderer()
    {
        _pageCount = 0;

        _renderer.SetupFontInfo(fontInfo);
        _renderer.StartRenderer();
    }

    public void StopRenderer()
    {
        // Force the processing of any more queue elements, even if they 
        // are not resolved.
        ProcessQueue(true);
        _renderer.StopRenderer();
    }

    /// <summary>
    /// Format the PageSequence. The PageSequence formats Pages and adds 
    /// them to the AreaTree, which subsequently calls the StreamRenderer
    /// instance (this) again to Render the page.  At this time the page 
    /// might be printed or it might be queued. A page might not be 
    /// renderable immediately if the IDReferences are not all valid. In 
    /// this case we defer the rendering until they are all valid.
    /// </summary>
    /// <param name="pageSequence"></param>
    public void Render(PageSequence pageSequence)
    {
        AreaTree a = new(this);
        a.FontInfo = fontInfo;

        var sw = System.Diagnostics.Stopwatch.StartNew();

        pageSequence.Format(a);

        FonetDriver.ActiveDriver?.FireFonetInfo($"Rendered Page Sequence Output in [{sw.Elapsed.TotalSeconds}] seconds.");

        _results.HaveFormattedPageSequence(pageSequence);

        FonetDriver.ActiveDriver?.FireFonetInfo($"Last page-sequence produced {pageSequence.PageCount} page(s).");
    }

    public void QueuePage(Page page)
    {
        // Process markers
        PageSequence pageSequence = page.PageSequence;
        if (pageSequence != currentPageSequence)
        {
            currentPageSequence = pageSequence;
            currentPageSequenceMarkers = null;
        }

        ArrayList markers = page.getMarkers();
        if (markers != null)
        {
            documentMarkers ??= [];
            currentPageSequenceMarkers ??= [];

            for (int i = 0; i < markers.Count; i++)
            {
                Marker marker = (Marker)markers[i];
                marker.releaseRegistryArea();
                currentPageSequenceMarkers.Add(marker);
                documentMarkers.Add(marker);
            }
        }

        // Try to optimise on the common case that there are no pages pending
        // and that all ID references are valid on the current pages. This
        // short-cuts the pipeline and renders the area immediately.
        if ((renderQueue.Count == 0) && idReferences.IsEveryIdValid())
        {
            _renderer.Render(page);
        }
        else
        {
            AddToRenderQueue(page);
        }

        _pageCount++;
    }

    private void AddToRenderQueue(Page page)
    {
        RenderQueueEntry entry = new RenderQueueEntry(this, page);
        renderQueue.Add(entry);

        // The just-added entry could (possibly) resolve the waiting entries,
        // so we try to process the queue now to see.
        ProcessQueue(false);
    }

    /// <summary>
    /// Try to process the queue from the first entry forward. If an
    /// entry can't be processed, then the queue can't move forward,
    /// so return.
    /// </summary>
    /// <param name="force"></param>
    private void ProcessQueue(bool force)
    {
        while (renderQueue.Count > 0)
        {
            RenderQueueEntry entry = (RenderQueueEntry)renderQueue[0];
            if ((!force) && (!entry.IsResolved()))
            {
                break;
            }

            _renderer.Render(entry.Page);
            renderQueue.RemoveAt(0);
        }
    }

    /// <summary>
    /// A RenderQueueEntry consists of the Page to be queued, plus a list
    /// of outstanding ID references that need to be resolved before the
    /// Page can be renderered.
    /// </summary>
    private class RenderQueueEntry
    {
        /// <summary>
        /// The Page that has outstanding ID references.
        /// </summary>
        public Page Page { get; }

        private readonly StreamRenderer _outer;

        /// <summary>
        /// A list of ID references (names).
        /// </summary>
        private readonly ArrayList _unresolvedIdReferences = [];

        public RenderQueueEntry(StreamRenderer outer, Page page)
        {
            _outer = outer;
            Page = page;

            foreach (object o in _outer.idReferences.GetInvalidElements())
            {
                _unresolvedIdReferences.Add(o);
            }
        }

        /// <summary>
        /// See if the outstanding references are resolved in the current
        /// copy of IDReferences.
        /// </summary>
        /// <returns>True if all references are resolved, false otherwise.</returns>
        public bool IsResolved()
        {
            if ((_unresolvedIdReferences.Count == 0) || _outer.idReferences.IsEveryIdValid())
            {
                return true;
            }

            // See if any of the unresolved references are still unresolved.
            foreach (string s in _unresolvedIdReferences)
            {
                if (!_outer.idReferences.DoesIDExist(s))
                {
                    return false;
                }
            }

            _unresolvedIdReferences.RemoveRange(0, _unresolvedIdReferences.Count);
            return true;
        }
    }

    /// <summary>
    /// Auxillary function for retrieving markers.
    /// </summary>
    /// <returns>The list of markers for the current page sequence.</returns>
    public ArrayList? GetDocumentMarkers()
    {
        return documentMarkers;
    }

    /// <summary>
    /// Auxillary function for retrieving markers.
    /// </summary>
    /// <returns>The current page sequence.</returns>
    public PageSequence? GetCurrentPageSequence()
    {
        return currentPageSequence;
    }

    /// <summary>
    /// Auxillary function for retrieving markers.
    /// </summary>
    /// <returns>The list of markers for the current page sequence.</returns>
    public ArrayList? GetCurrentPageSequenceMarkers()
    {
        return currentPageSequenceMarkers;
    }
}
