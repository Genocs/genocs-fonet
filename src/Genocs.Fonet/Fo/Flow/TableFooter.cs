namespace Genocs.Fonet.Fo.Flow
{
    internal class TableFooter : AbstractTableBody
    {
        public static FObj.Maker CreateMaker()
            => FObj.Maker.For((parent, props) => new TableFooter(parent, props));

        public override int GetYPosition()
        {
            return areaContainer.GetCurrentYPosition() - spaceBefore;
        }

        public override void SetYPosition(int value)
        {
            areaContainer.YPosition = value + 2 * spaceBefore;
        }


        public TableFooter(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            Name = "fo:table-footer";
        }
    }
}