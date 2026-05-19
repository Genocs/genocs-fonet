using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ListItemBody : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new ListItemBody(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public ListItemBody(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:list-item-body";
    }

    public override Status Layout(Area area)
    {
        if (this._marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            this._marker = 0;
            string id = this._properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FObj fo = (FObj)_children[i];

            Status status;
            if ((status = fo.Layout(area)).IsIncomplete())
            {
                this._marker = i;
                if ((i == 0) && (status.GetCode() == Status.AREA_FULL_NONE))
                {
                    return new Status(Status.AREA_FULL_NONE);
                }
                else
                {
                    return new Status(Status.AREA_FULL_SOME);
                }
            }
        }
        return new Status(Status.OK);
    }
}