using Genocs.Fonet.Fo.Pagination;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo.Flow;

internal class RetrieveMarker : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new RetrieveMarker(parent, props));

    private string _retrieveClassName;

    private int _retrievePosition;

    private int _retrieveBoundary;

    private Marker? _bestMarker;



    public RetrieveMarker(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:retrieve-marker";

        _retrieveClassName = Properties.GetProperty("retrieve-class-name").GetString();
        _retrievePosition = Properties.GetProperty("retrieve-position").GetEnum();
        _retrieveBoundary = Properties.GetProperty("retrieve-boundary").GetEnum();
    }

    public override Status Layout(Area area)
    {
        if (_marker == MarkerStart)
        {
            _marker = 0;
            Page? containingPage = area.Page;
            _bestMarker = SearchPage(containingPage);

            if (_bestMarker != null)
            {
                _bestMarker.resetMarkerContent();
                return _bestMarker.LayoutMarker(area);
            }

            AreaTree areaTree = containingPage.getAreaTree();
            if (_retrieveBoundary == RetrieveBoundary.PAGE_SEQUENCE)
            {
                PageSequence pageSequence = areaTree.GetCurrentPageSequence();
                if (pageSequence == containingPage.PageSequence)
                {
                    return LayoutBestMarker(areaTree.GetCurrentPageSequenceMarkers(), area);
                }

            }
            else if (_retrieveBoundary == RetrieveBoundary.DOCUMENT)
            {
                return LayoutBestMarker(areaTree.GetDocumentMarkers(), area);

            }
            else if (_retrieveBoundary != RetrieveBoundary.PAGE)
            {
                throw new FonetException("Illegal 'retrieve-boundary' value");
            }

        }
        else if (_bestMarker != null)
        {
            return _bestMarker.LayoutMarker(area);
        }

        return new Status(Status.OK);

    }

    private Status LayoutBestMarker(ArrayList markers, Area area)
    {
        if (markers != null)
        {
            for (int i = markers.Count - 1; i >= 0; i--)
            {
                Marker currentMarker = (Marker)markers[i];
                if (currentMarker.GetMarkerClassName().Equals(_retrieveClassName))
                {
                    _bestMarker = currentMarker;
                    _bestMarker.resetMarkerContent();
                    return _bestMarker.LayoutMarker(area);
                }
            }
        }
        return new Status(Status.OK);
    }

    private Marker? SearchPage(Page page)
    {
        ArrayList pageMarkers = page.getMarkers();
        if (pageMarkers.Count == 0)
        {
            return null;
        }

        if (_retrievePosition == RetrievePosition.FIC)
        {
            for (int i = 0; i < pageMarkers.Count; i++)
            {
                Marker currentMarker = (Marker)pageMarkers[i];
                if (currentMarker.GetMarkerClassName().Equals(_retrieveClassName))
                {
                    return currentMarker;
                }
            }
        }
        else if (_retrievePosition == RetrievePosition.FSWP)
        {
            for (int c = 0; c < pageMarkers.Count; c++)
            {
                Marker currentMarker = (Marker)pageMarkers[c];
                if (currentMarker.GetMarkerClassName().Equals(_retrieveClassName))
                {
                    if (currentMarker.GetRegistryArea().IsFirst)
                    {
                        return currentMarker;
                    }
                }
            }
        }
        else if (_retrievePosition == RetrievePosition.LSWP)
        {
            for (int c = pageMarkers.Count - 1; c >= 0; c--)
            {
                Marker currentMarker = (Marker)pageMarkers[c];
                if (currentMarker.GetMarkerClassName().Equals(_retrieveClassName))
                {
                    if (currentMarker.GetRegistryArea().IsFirst)
                    {
                        return currentMarker;
                    }
                }
            }
        }
        else if (_retrievePosition == RetrievePosition.LEWP)
        {
            for (int c = pageMarkers.Count - 1; c >= 0; c--)
            {
                Marker currentMarker = (Marker)pageMarkers[c];
                if (currentMarker.GetMarkerClassName().Equals(_retrieveClassName))
                {
                    if (currentMarker.GetRegistryArea().IsLast)
                    {
                        return currentMarker;
                    }
                }
            }
        }
        else
        {
            throw new FonetException("Illegal 'retrieve-position' value");
        }

        return null;
    }
}