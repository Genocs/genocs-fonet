using Genocs.Fonet.Fo.Pagination;
using System.Collections;

namespace Genocs.Fonet.Apps;

internal class FormattingResults
{
    private int _pageCount;

    private ArrayList? _pageSequences;

    internal int GetPageCount()
    {
        return _pageCount;
    }

    internal ArrayList? GetPageSequences()
    {
        return _pageSequences;
    }

    internal void Reset()
    {
        _pageCount = 0;
        _pageSequences?.Clear();
    }

    internal void HaveFormattedPageSequence(PageSequence pageSequence)
    {
        _pageCount += pageSequence.PageCount;
        _pageSequences ??= [];

        _pageSequences.Add(
            new PageSequenceResults(
                pageSequence.GetProperty("id").GetString(),
                pageSequence.PageCount));
    }
}