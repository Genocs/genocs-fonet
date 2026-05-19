namespace Genocs.Fonet.Apps;

internal class PageSequenceResults
{
    private readonly string _id;
    private readonly int _pageCount;

    internal PageSequenceResults(string id, int pageCount)
    {
        _id = id;
        _pageCount = pageCount;
    }

    internal string GetID()
        => _id;

    internal int GetPageCount()
        => _pageCount;
}