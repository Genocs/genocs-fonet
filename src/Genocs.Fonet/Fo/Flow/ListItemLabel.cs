using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ListItemLabel : FObj
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new ListItemLabel(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public ListItemLabel(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:list-item-label";
    }

    public override Status Layout(Area area)
    {
        int numChildren = this._children.Count;

        if (numChildren != 1)
        {
            throw new FonetException("list-item-label must have exactly one block in this version of FO.NET");
        }

        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        string id = this._properties.GetProperty("id").GetString();
        area.GetIDReferences().InitializeID(id, area);

        Block block = (Block)_children[0];

        Status status;
        status = block.Layout(area);
        area.addDisplaySpace(-block.GetAreaHeight());
        return status;
    }
}