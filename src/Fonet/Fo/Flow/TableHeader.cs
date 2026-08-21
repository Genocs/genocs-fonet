namespace Genocs.Fonet.Fo.Flow;

internal class TableHeader : AbstractTableBody
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new TableHeader(parent, props));

    public TableHeader(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:table-header";
    }
}