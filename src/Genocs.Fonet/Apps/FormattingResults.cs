using Genocs.Fonet.Fo.Pagination;
using System.Collections;

namespace Genocs.Fonet.Apps;

internal sealed class FormattingResults
{
    public int PageCount { get; private set; }

    public ArrayList PageSequences { get; private set; } = [];

    internal void Reset()
    {
        PageCount = 0;
        PageSequences.Clear();
    }

    internal void HaveFormattedPageSequence(PageSequence pageSequence)
    {
        PageCount += pageSequence.PageCount;
        PageSequences.Add(new PageSequenceResults(pageSequence.GetProperty("id")?.GetString(), pageSequence.PageCount));
    }
}