using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Float : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Float(parent, props));

    protected Float(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:float";
    }

    public override Status Layout(Area area)
    {
        if (!_propertyManager.IsVisible())
        {
            return new Status(Status.OK);
        }

        if (area is not ColumnArea column)
        {
            return base.Layout(area);
        }

        int floatSide = _propertyManager.GetFloatAlign();
        if (floatSide == FloatAlign.NONE)
        {
            floatSide = FloatAlign.LEFT;
        }

        int anchorY = column.getContentHeight();
        int columnWidth = column.getAllocationWidth();
        int remainingHeight = column.spaceLeft();

        var sideFloatArea = new SideFloatArea(
            _propertyManager.GetFontState(area.GetFontInfo()),
            column.XPosition,
            column.YPosition - anchorY,
            columnWidth,
            remainingHeight,
            floatSide)
        {
            Page = area.Page,
            ZIndex = _propertyManager.GetZIndex(),
        };

        sideFloatArea.setIDReferences(area.GetIDReferences());

        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i] is not FONode child)
            {
                continue;
            }

            Status status = child.Layout(sideFloatArea);
            if (status.IsIncomplete())
            {
                return status;
            }
        }

        int floatWidth = MeasureContentWidth(sideFloatArea);
        int floatHeight = sideFloatArea.getContentHeight();

        if (floatWidth <= 0 || floatHeight <= 0)
        {
            return new Status(Status.OK);
        }

        sideFloatArea.SetContentWidth(floatWidth);
        sideFloatArea.FinalizePosition(column.XPosition, columnWidth);

        column.RegisterSideFloat(floatSide, anchorY, floatWidth, floatHeight, sideFloatArea);
        column.AddChild(sideFloatArea);

        return new Status(Status.OK);
    }

    private static int MeasureContentWidth(SideFloatArea sideFloatArea)
    {
        int maxWidth = 0;

        foreach (object child in sideFloatArea.Children)
        {
            if (child is Area childArea)
            {
                maxWidth = Math.Max(maxWidth, childArea.getContentWidth());
            }
        }

        return maxWidth;
    }
}
