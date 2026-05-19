using System.Collections;

namespace Genocs.Fonet.Fo.Pagination;

internal class Root : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new Root(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public LayoutMasterSet? LayoutMasterSet { get; set; }

    private ArrayList pageSequences;

    private int runningPageNumberCounter = 0;

    protected internal Root(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:root";
        pageSequences = new ArrayList();
        if (parent != null)
        {
            throw new FonetException("root must be root element");
        }
    }

    protected internal int getRunningPageNumberCounter()
    {
        return this.runningPageNumberCounter;
    }

    protected internal void SetRunningPageNumberCounter(int count)
        => runningPageNumberCounter = count;

    public int getPageSequenceCount()
    {
        return pageSequences.Count;
    }

    public PageSequence getSucceedingPageSequence(PageSequence current)
    {
        int currentIndex = pageSequences.IndexOf(current);
        if (currentIndex == -1)
        {
            return null;
        }
        if (currentIndex < (pageSequences.Count - 1))
        {
            return (PageSequence)pageSequences[currentIndex + 1];
        }
        else
        {
            return null;
        }
    }
}