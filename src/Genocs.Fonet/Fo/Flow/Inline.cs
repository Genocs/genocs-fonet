using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Inline : FObjMixed
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new Inline(parent, props));

    public Inline(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:inline";
        if (parent.Name.Equals("fo:flow"))
        {
            throw new FonetException("inline formatting objects cannot be directly under flow");
        }

        // TODO: Implement the following properties if needed
        // AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        // AuralProps mAurProps = _propertyManager.GetAuralProps();
        // BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        // BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        // MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        // RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

        _textState = _propertyManager.getTextDecoration(parent);
    }

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        FOText ft = new(data, start, length, this);
        ft.setUnderlined(_textState.getUnderlined());
        ft.setOverlined(_textState.getOverlined());
        ft.setLineThrough(_textState.getLineThrough());
        _children.Add(ft);
    }
}