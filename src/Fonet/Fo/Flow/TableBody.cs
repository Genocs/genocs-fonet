namespace Genocs.Fonet.Fo.Flow;

internal class TableBody : AbstractTableBody
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new TableBody(parent, props));

    public TableBody(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:table-body";
    }
}