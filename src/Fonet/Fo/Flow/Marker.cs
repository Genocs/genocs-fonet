using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;


internal class Marker : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Marker(parent, props));

    private string markerClassName;

    private Area registryArea;

    private bool isFirst;

    private bool isLast;



    public Marker(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:marker";

        this.markerClassName = this.Properties.GetProperty("marker-class-name").GetString();
        _textState = _propertyManager.getTextDecoration(parent);

        try
        {
            parent.AddMarker(this.markerClassName);
        }
        catch (FonetException)
        {
        }
    }

    public override Status Layout(Area area)
    {
        this.registryArea = area;
        area.Page?.registerMarker(this);
        return new Status(Status.OK);
    }

    public Status LayoutMarker(Area area)
    {
        if (this._marker == MarkerStart)
        {
            this._marker = 0;
        }

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];

            Status status;
            if ((status = fo.Layout(area)).IsIncomplete())
            {
                this._marker = i;
                return status;
            }
        }

        return new Status(Status.OK);
    }

    public string GetMarkerClassName()
    {
        return markerClassName;
    }

    public Area GetRegistryArea()
    {
        return registryArea;
    }

    public void releaseRegistryArea()
    {
        isFirst = registryArea.IsFirst;
        isLast = registryArea.IsLast;
        registryArea = null;
    }

    public void resetMarker()
    {
        if (registryArea != null)
        {
            Page page = registryArea.Page;
            if (page != null)
            {
                page.unregisterMarker(this);
            }
        }
    }

    public void resetMarkerContent()
    {
        base.ResetMarker();
    }

    public override bool MayPrecedeMarker()
    {
        return true;
    }
}