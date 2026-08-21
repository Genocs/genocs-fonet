using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Footnote : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Footnote(parent, props));

    public Footnote(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:footnote";
    }

    public override Status Layout(Area area)
    {
        FONode inline = null;
        FONode fbody = null;
        if (this._marker == MarkerStart)
        {
            this._marker = 0;
        }
        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];
            if (fo is Inline)
            {
                inline = fo;
                Status status = fo.Layout(area);
                if (status.IsIncomplete())
                {
                    return status;
                }
            }
            else if (inline != null && fo is FootnoteBody)
            {
                fbody = fo;
                if (area is BlockArea)
                {
                    ((BlockArea)area).addFootnote((FootnoteBody)fbody);
                }
                else
                {
                    Page? page = area.Page;
                    LayoutFootnote(page, (FootnoteBody)fbody, area);
                }
            }
        }

        if (fbody == null)
        {
            FonetDriver.ActiveDriver?.FireFonetWarning("No footnote-body in footnote");
        }

        if (area is BlockArea) { }
        return new Status(Status.OK);
    }

    public static bool LayoutFootnote(Page p, FootnoteBody fb, Area area)
    {
        try
        {
            BodyAreaContainer bac = p.getBody();
            AreaContainer footArea = bac.getFootnoteReferenceArea();
            footArea.setIDReferences(bac.GetIDReferences());
            int basePos = footArea.GetCurrentYPosition()
                - footArea.GetHeight();
            int oldHeight = footArea.GetHeight();
            if (area != null)
            {
                footArea.setMaxHeight(area.getMaxHeight() - area.GetHeight()
                    + footArea.GetHeight());
            }
            else
            {
                footArea.setMaxHeight(bac.getMaxHeight()
                    + footArea.GetHeight());
            }
            Status status = fb.Layout(footArea);
            if (status.IsIncomplete())
            {
                return false;
            }
            else
            {
                area?.setMaxHeight(area.getMaxHeight() - footArea.GetHeight() + oldHeight);

                if (bac.getFootnoteState() == 0)
                {
                    Area ar = bac.getMainReferenceArea();
                    DecreaseMaxHeight(ar, footArea.GetHeight() - oldHeight);
                    footArea.YPosition = basePos + footArea.GetHeight();
                }
            }
        }
        catch (FonetException)
        {
            return false;
        }
        return true;
    }

    protected static void DecreaseMaxHeight(Area ar, int change)
    {
        ar.setMaxHeight(ar.getMaxHeight() - change);

        foreach (object obj in ar.Children)
        {
            if (obj is Area childArea)
            {
                DecreaseMaxHeight(childArea, change);
            }
        }
    }
}