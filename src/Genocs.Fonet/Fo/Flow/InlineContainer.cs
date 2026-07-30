namespace Genocs.Fonet.Fo.Flow;

using Genocs.Fonet.Layout;
using Genocs.Fonet.Fo;

internal class InlineContainer : ToBeImplementedElement
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new InlineContainer(parent, props));

    protected InlineContainer(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:inline-container";

        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();
    }
}