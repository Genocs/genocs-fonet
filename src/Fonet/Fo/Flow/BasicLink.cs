using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class BasicLink : Inline
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new BasicLink(parent, props));

    public BasicLink(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:basic-link";
    }

    public override Status Layout(Area area)
    {
        string? destination;
        int linkType;

        // TODO: Implement the following properties if needed
        // AccessibilityProps accProps = _propertyManager.GetAccessibilityProps();
        // AuralProps aurProps = _propertyManager.GetAuralProps();
        // BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        // BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        // MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        // RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

        if (!(destination = Properties.GetProperty("internal-destination").GetString()).Equals(""))
        {
            linkType = LinkSet.INTERNAL;
        }
        else if (!(destination = Properties.GetProperty("external-destination").GetString()).Equals(""))
        {
            linkType = LinkSet.EXTERNAL;
        }
        else
        {
            throw new FonetException("internal-destination or external-destination must be specified in basic-link");
        }

        if (_marker == MarkerStart)
        {
            string? id = Properties.GetProperty("id")?.GetString();
            area.GetIDReferences().InitializeID(id, area);
            _marker = 0;
        }

        LinkSet ls = new(destination, area, linkType);

        AreaContainer? ac = area.getNearestAncestorAreaContainer();
        while (ac != null && ac.getPosition() != Position.ABSOLUTE)
        {
            ac = ac.getNearestAncestorAreaContainer();
        }

        ac ??= area.Page?.getBody().getCurrentColumnArea();

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FONode? fo = (FONode?)_children[i] ?? throw new FonetException("Child node is null in BasicLink layout.");
            fo.SetLinkSet(ls);

            Status status;
            if ((status = fo.Layout(area)).IsIncomplete())
            {
                _marker = i;
                return status;
            }
        }

        ls.applyAreaContainerOffsets(ac, area);
        area.Page?.addLinkSet(ls);

        return new Status(Status.OK);
    }
}