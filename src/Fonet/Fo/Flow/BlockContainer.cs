using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class BlockContainer : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new BlockContainer(parent, props));

    private int position;
    private int top;
    private int bottom;
    private int left;
    private int right;
    private int width;
    private int height;
    private int span;
    private AreaContainer areaContainer;



    protected BlockContainer(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:block-container";
        this.span = this.Properties.GetProperty("span").GetEnum();
    }

    public override Status Layout(Area area)
    {
        if (this._marker == MarkerStart)
        {
            AbsolutePositionProps mAbsProps = _propertyManager.GetAbsolutePositionProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginProps mProps = _propertyManager.GetMarginProps();

            this._marker = 0;
            this.position = ResolvePosition();
            this.top = this.Properties.GetProperty("top").GetLength().Millipoints();
            this.bottom = this.Properties.GetProperty("bottom").GetLength().Millipoints();
            this.left = this.Properties.GetProperty("left").GetLength().Millipoints();
            this.right = this.Properties.GetProperty("right").GetLength().Millipoints();
            this.width = this.Properties.GetProperty("width").GetLength().Millipoints();
            this.height = this.Properties.GetProperty("height").GetLength().Millipoints();
            span = this.Properties.GetProperty("span").GetEnum();

            string id = this.Properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        AreaContainer container = (AreaContainer)area;
        if ((this.width == 0) && (this.height == 0))
        {
            width = right - left;
            height = bottom - top;
        }

        this.areaContainer =
            new AreaContainer(_propertyManager.GetFontState(container.GetFontInfo()),
                              container.XPosition + left,
                              container.YPosition - top, width, height,
                              position,
                              null)
            {
                ZIndex = _propertyManager.GetZIndex(),
            };

        areaContainer.Page = area.Page;
        areaContainer.Background = (_propertyManager.GetBackgroundProps());
        areaContainer.setBorderAndPadding(_propertyManager.GetBorderAndPadding());
        areaContainer.start();

        areaContainer.setAbsoluteHeight(0);
        areaContainer.setIDReferences(area.GetIDReferences());

        int numChildren = this._children.Count;
        for (int i = this._marker; i < numChildren; i++)
        {
            FObj fo = (FObj)_children[i];
            Status status = fo.Layout(areaContainer);
        }

        areaContainer.end();
        if (position == Position.ABSOLUTE)
        {
            areaContainer.SetHeight(height);
        }
        area.AddChild(areaContainer);

        return new Status(Status.OK);
    }

    public override int GetContentWidth()
    {
        if (areaContainer != null)
        {
            return areaContainer.getContentWidth();
        }
        else
        {
            return 0;
        }
    }

    public override bool GeneratesReferenceAreas()
    {
        return true;
    }

    public int GetSpan()
    {
        return this.span;
    }

    private int ResolvePosition()
    {
        int absolutePosition = this.Properties.GetProperty("absolute-position").GetEnum();
        if (absolutePosition == AbsolutePosition.ABSOLUTE)
        {
            return Position.ABSOLUTE;
        }

        if (absolutePosition == AbsolutePosition.FIXED)
        {
            return Position.FIXED;
        }

        return this.Properties.GetProperty("position").GetEnum();
    }
}