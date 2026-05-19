using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class BlockContainer : FObj
{
    private int position;
    private int top;
    private int bottom;
    private int left;
    private int right;
    private int width;
    private int height;
    private int span;
    private AreaContainer areaContainer;

    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new BlockContainer(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    protected BlockContainer(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:block-container";
        this.span = this._properties.GetProperty("span").GetEnum();
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
            this.position = this._properties.GetProperty("position").GetEnum();
            this.top = this._properties.GetProperty("top").GetLength().MValue();
            this.bottom = this._properties.GetProperty("bottom").GetLength().MValue();
            this.left = this._properties.GetProperty("left").GetLength().MValue();
            this.right = this._properties.GetProperty("right").GetLength().MValue();
            this.width = this._properties.GetProperty("width").GetLength().MValue();
            this.height = this._properties.GetProperty("height").GetLength().MValue();
            span = this._properties.GetProperty("span").GetEnum();

            string id = this._properties.GetProperty("id").GetString();
            area.GetIDReferences().InitializeID(id, area);
        }

        AreaContainer container = (AreaContainer)area;
        if ((this.width == 0) && (this.height == 0))
        {
            width = right - left;
            height = bottom - top;
        }

        this.areaContainer =
            new AreaContainer(_propertyManager.GetFontState(container.getFontInfo()),
                              container.getXPosition() + left,
                              container.GetYPosition() - top, width, height,
                              position);

        areaContainer.setPage(area.getPage());
        areaContainer.setBackground(_propertyManager.GetBackgroundProps());
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
        area.addChild(areaContainer);

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
}