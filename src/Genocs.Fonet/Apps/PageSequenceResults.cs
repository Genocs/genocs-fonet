namespace Genocs.Fonet.Apps;

internal sealed class PageSequenceResults
{
    public string? Id { get; }
    public int PageCount { get; }

    internal PageSequenceResults(string? id, int pageCount)
    {
        Id = id;
        PageCount = pageCount;
    }
}