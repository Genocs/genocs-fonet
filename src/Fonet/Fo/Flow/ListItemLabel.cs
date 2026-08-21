using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ListItemLabel : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new ListItemLabel(parent, props));

    public ListItemLabel(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:list-item-label";
    }

    public override Status Layout(Area area)
    {
        int numChildren = this._children.Count;

        if (numChildren != 1)
        {
            throw new FonetException("list-item-label must have exactly one block in this version of FO.NET");
        }

        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        string id = this.Properties.GetProperty("id").GetString();
        area.GetIDReferences().InitializeID(id, area);

        Block block = (Block)_children[0];

        Status status;
        status = block.Layout(area);
        area.AddDisplaySpace(-block.GetAreaHeight());
        return status;
    }
}