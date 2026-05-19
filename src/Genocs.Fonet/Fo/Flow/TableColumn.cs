using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class TableColumn : FObj
{
    private Length columnWidthPropVal;
    private int columnWidth;
    private int columnOffset;
    private int numColumnsRepeated;
    private int iColumnNumber;
    private bool setup = false;
    private AreaContainer areaContainer;

    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new TableColumn(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public TableColumn(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:table-column";
    }

    public Length GetColumnWidthAsLength()
    {
        return columnWidthPropVal;
    }

    public int GetColumnWidth()
    {
        return columnWidth;
    }

    public void SetColumnWidth(int columnWidth)
    {
        this.columnWidth = columnWidth;
    }

    public int GetColumnNumber()
    {
        return iColumnNumber;
    }

    public int GetNumColumnsRepeated()
    {
        return numColumnsRepeated;
    }

    public void DoSetup(Area area)
    {
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();

        this.iColumnNumber =
            this._properties.GetProperty("column-number").GetNumber().IntValue();

        this.numColumnsRepeated =
            this._properties.GetProperty("number-columns-repeated").GetNumber().IntValue();

        this.columnWidthPropVal =
            this._properties.GetProperty("column-width").GetLength();

        this.columnWidth = columnWidthPropVal.MValue();

        string id = this._properties.GetProperty("id").GetString();
        area.GetIDReferences().InitializeID(id, area);

        setup = true;
    }

    public override Status Layout(Area area)
    {
        if (this._marker == MarkerBreakAfter)
        {
            return new Status(Status.OK);
        }

        if (this._marker == MarkerStart)
        {
            if (!setup)
            {
                DoSetup(area);
            }
        }
        if (columnWidth > 0)
        {
            this.areaContainer =
                new AreaContainer(_propertyManager.GetFontState(area.getFontInfo()),
                                  columnOffset, 0, columnWidth,
                                  area.getContentHeight(), Position.RELATIVE);
            areaContainer.foCreator = this;
            areaContainer.setPage(area.getPage());
            areaContainer.setBorderAndPadding(_propertyManager.GetBorderAndPadding());
            areaContainer.setBackground(_propertyManager.GetBackgroundProps());
            areaContainer.SetHeight(area.GetHeight());
            area.addChild(areaContainer);
        }
        return new Status(Status.OK);
    }

    public void SetColumnOffset(int columnOffset)
    {
        this.columnOffset = columnOffset;
    }

    public void SetHeight(int height)
    {
        if (areaContainer != null)
        {
            areaContainer.setMaxHeight(height);
            areaContainer.SetHeight(height);
        }
    }
}