using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class TableCaption : FObjMixed
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList) =>
            new TableCaption(parent, propertyList);
    }

    new public static FObj.Maker GetMaker() => new Maker();

    protected TableCaption(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        _name = "fo:table-caption";
    }

    public override Status Layout(Area area)
    {
        if (!_propertyManager.IsVisible())
        {
            return new Status(Status.OK);
        }

        if (_marker == MarkerStart)
        {
            _marker = 0;
        }

        int align = _properties.GetProperty("text-align").GetEnum();
        int lineHeight = _properties.GetProperty("line-height").GetLength().MValue();
        MarginProps mProps = _propertyManager.GetMarginProps();

        BlockArea blockArea = new BlockArea(
            _propertyManager.GetFontState(area.getFontInfo()),
            area.getAllocationWidth(),
            area.spaceLeft(),
            mProps.marginLeft,
            mProps.marginRight,
            0,
            align,
            align,
            lineHeight);
        blockArea.setParent(area);
        blockArea.setPage(area.getPage());
        area.addChild(blockArea);

        Status status = base.Layout(blockArea);
        if (status.IsIncomplete())
        {
            return status;
        }

        blockArea.end();
        return new Status(Status.OK);
    }
}
