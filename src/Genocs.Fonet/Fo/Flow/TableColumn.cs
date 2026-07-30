using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class TableColumn : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new TableColumn(parent, props));

    private Length columnWidthPropVal;
    private int columnWidth;
    private int columnOffset;
    private int numColumnsRepeated;
    private int iColumnNumber;
    private bool setup = false;
    private AreaContainer areaContainer;



    public TableColumn(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:table-column";
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

        this.iColumnNumber = this.Properties.GetProperty("column-number").GetNumber().IntValue();

        this.numColumnsRepeated = this.Properties.GetProperty("number-columns-repeated").GetNumber().IntValue();

        this.columnWidthPropVal = this.Properties.GetProperty("column-width").GetLength();

        this.columnWidth = columnWidthPropVal.MValue();

        string id = this.Properties.GetProperty("id").GetString();
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
            this.areaContainer = new AreaContainer(_propertyManager.GetFontState(area.GetFontInfo()),
                                  columnOffset, 0, columnWidth,
                                  area.getContentHeight(),
                                  Position.RELATIVE,
                                  null);

            areaContainer.foCreator = this;
            areaContainer.Page = area.Page;
            areaContainer.setBorderAndPadding(_propertyManager.GetBorderAndPadding());
            areaContainer.setBackground(_propertyManager.GetBackgroundProps());
            areaContainer.SetHeight(area.GetHeight());
            area.AddChild(areaContainer);
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