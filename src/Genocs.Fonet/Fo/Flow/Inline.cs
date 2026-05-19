using Genocs.Fonet.Fo;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class Inline : FObjMixed
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new Inline(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    public Inline(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:inline";
        if (parent.GetName().Equals("fo:flow"))
        {
            throw new FonetException("inline formatting objects cannot"
                + " be directly under flow");
        }

        AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
        AuralProps mAurProps = _propertyManager.GetAuralProps();
        BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
        BackgroundProps bProps = _propertyManager.GetBackgroundProps();
        MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
        RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();
        _textState = _propertyManager.getTextDecoration(parent);
    }

    protected internal override void AddCharacters(char[] data, int start, int length)
    {
        FOText ft = new FOText(data, start, length, this);
        ft.setUnderlined(_textState.getUnderlined());
        ft.setOverlined(_textState.getOverlined());
        ft.setLineThrough(_textState.getLineThrough());
        _children.Add(ft);
    }
}