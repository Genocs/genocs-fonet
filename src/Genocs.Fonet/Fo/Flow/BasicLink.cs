using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class BasicLink : Inline
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new BasicLink(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public BasicLink(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:basic-link";
    }

    public override Status Layout(Area area)
    {
        string destination;
        int linkType;
        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

        if (!(destination = _properties.GetProperty("internal-destination").GetString()).Equals(""))
        {
            linkType = LinkSet.INTERNAL;
        }
        else if (!(destination = _properties.GetProperty("external-destination").GetString()).Equals(""))
        {
            linkType = LinkSet.EXTERNAL;
        }
        else
        {
            throw new FonetException("internal-destination or external-destination must be specified in basic-link");
        }

        if (_marker == MarkerStart)
        {
            string id = _properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
            _marker = 0;
        }

        LinkSet ls = new LinkSet(destination, area, linkType);

        AreaContainer ac = area.getNearestAncestorAreaContainer();
        while (ac != null && ac.getPosition() != Position.ABSOLUTE)
        {
            ac = ac.getNearestAncestorAreaContainer();
        }

        if (ac == null)
        {
            ac = area.getPage().getBody().getCurrentColumnArea();
        }

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];
            fo.SetLinkSet(ls);

            Status status;
            if ((status = fo.Layout(area)).IsIncomplete())
            {
                this._marker = i;
                return status;
            }
        }

        ls.applyAreaContainerOffsets(ac, area);
        area.getPage().addLinkSet(ls);

        return new Status(Status.OK);
    }
}