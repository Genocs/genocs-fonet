using System.Collections;

namespace Genocs.Fonet.Fo.Pagination;

internal class PageSequenceMaster : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new PageSequenceMaster(parent, props));

    private LayoutMasterSet layoutMasterSet;

    private ArrayList subSequenceSpecifiers;

    protected PageSequenceMaster(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:page-sequence-master";

        subSequenceSpecifiers = new ArrayList();

        if (parent.Name.Equals("fo:layout-master-set"))
        {
            this.layoutMasterSet = (LayoutMasterSet)parent;
            string pm = this.Properties.GetProperty("master-name").GetString();
            if (pm == null)
            {
                FonetDriver.ActiveDriver?.FireFonetWarning("page-sequence-master does not have a page-master-name and so is being ignored");
            }
            else
            {
                this.layoutMasterSet.addPageSequenceMaster(pm, this);
            }
        }
        else
        {
            throw new FonetException($"fo:page-sequence-master must be child of fo:layout-master-set, not {parent.Name}");
        }
    }

    protected internal void AddSubsequenceSpecifier(ISubSequenceSpecifier pageMasterReference)
    {
        subSequenceSpecifiers.Add(pageMasterReference);
    }

    protected internal ISubSequenceSpecifier? getSubSequenceSpecifier(int sequenceNumber)
    {
        if (sequenceNumber >= 0
            && sequenceNumber < GetSubSequenceSpecifierCount())
        {
            return (ISubSequenceSpecifier?)subSequenceSpecifiers[sequenceNumber];
        }
        return null;
    }

    protected internal int GetSubSequenceSpecifierCount()
    {
        return subSequenceSpecifiers.Count;
    }

    public void Reset()
    {
        foreach (ISubSequenceSpecifier s in subSequenceSpecifiers)
        {
            s.Reset();
        }
    }
}