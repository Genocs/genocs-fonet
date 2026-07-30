namespace Genocs.Fonet.Fo.Pagination;

internal class SinglePageMasterReference : PageMasterReference, ISubSequenceSpecifier
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new SinglePageMasterReference(parent, props));

    private const int FIRST = 0;

    private const int DONE = 1;

    private int state;

    public SinglePageMasterReference(
        FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this.state = FIRST;
    }

    public override string GetNextPageMaster(int currentPageNumber,
                                             bool thisIsFirstPage,
                                             bool isEmptyPage)
    {
        if (this.state == FIRST)
        {
            this.state = DONE;
            return MasterName;
        }
        else
        {
            return null;
        }
    }

    public override void Reset()
    {
        this.state = FIRST;
    }

    protected override string GetElementName()
    {
        return "fo:single-page-master-reference";
    }
}