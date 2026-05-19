namespace Genocs.Fonet.Fo.Flow
{
    using Genocs.Fonet.Layout;
    using Genocs.Fonet.Fo;

    internal class InlineContainer : ToBeImplementedElement
    {
        new internal class Maker : FObj.Maker
        {
            public override FObj Make(FObj parent, PropertyList propertyList)
            {
                return new InlineContainer(parent, propertyList);
            }
        }

        new public static FObj.Maker GetMaker()
        {
            return new Maker();
        }

        protected InlineContainer(FObj parent, PropertyList propertyList)
            : base(parent, propertyList)
        {
            this._name = "fo:inline-container";

            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();
        }
    }
}