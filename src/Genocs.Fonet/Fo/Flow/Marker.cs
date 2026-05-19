namespace Genocs.Fonet.Fo.Flow
{
    using Genocs.Fonet;
    using Genocs.Fonet.Fo;
    using Genocs.Fonet.Layout;

    internal class Marker : FObjMixed
    {
        private string markerClassName;

        private Area registryArea;

        private bool isFirst;

        private bool isLast;

        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new Marker(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        public Marker(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this._name = "fo:marker";

            this.markerClassName =
                this._properties.GetProperty("marker-class-name").GetString();
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
            area.getPage().registerMarker(this);
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
                Page page = registryArea.getPage();
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
}