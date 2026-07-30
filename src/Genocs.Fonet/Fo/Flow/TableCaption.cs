using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class TableCaption : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new TableCaption(parent, props));

    protected TableCaption(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:table-caption";
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

        int align = Properties.GetProperty("text-align").GetEnum();
        int lineHeight = Properties.GetProperty("line-height").GetLength().MValue();
        MarginProps mProps = _propertyManager.GetMarginProps();

        BlockArea blockArea = new(
                                    _propertyManager.GetFontState(area.GetFontInfo()),
                                    area.getAllocationWidth(),
                                    area.spaceLeft(),
                                    mProps.marginLeft,
                                    mProps.marginRight,
                                    0,
                                    align,
                                    align,
                                    lineHeight,
                                    area);

        blockArea.Page = area.Page;
        area.AddChild(blockArea);

        Status status = base.Layout(blockArea);
        if (status.IsIncomplete())
        {
            return status;
        }

        blockArea.end();
        return new Status(Status.OK);
    }
}
