using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Image;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo;

internal class PropertyManager(PropertyList pList)
{
    private readonly PropertyList _propertyList = pList;
    private FontState? fontState = null;
    private BorderAndPadding? borderAndPadding = null;
    private HyphenationProps? hyphProps = null;
    private BackgroundProps? bgProps = null;

    private string? saLeft;
    private string? saRight;
    private string? saTop;
    private string? saBottom;

    private static string msgColorFmt = "border-{0}-color";
    private static string msgStyleFmt = "border-{0}-style";
    private static string msgWidthFmt = "border-{0}-width";
    private static string msgPaddingFmt = "padding-{0}";

    private void InitDirections()
    {
        saTop = _propertyList.AbsoluteToRelative(PropertyList.TOP);
        saBottom = _propertyList.AbsoluteToRelative(PropertyList.BOTTOM);
        saLeft = _propertyList.AbsoluteToRelative(PropertyList.LEFT);
        saRight = _propertyList.AbsoluteToRelative(PropertyList.RIGHT);
    }

    public FontState GetFontState(FontInfo fontInfo)
    {
        if (fontState == null)
        {
            string? fontFamily = _propertyList.GetProperty("font-family")?.GetString();
            string? fontStyle = _propertyList.GetProperty("font-style")?.GetString();
            string? fontWeight = _propertyList.GetProperty("font-weight")?.GetString();
            int fontSize = _propertyList.GetProperty("font-size")?.GetLength()?.Millipoints() ?? 0;
            int fontVariant = _propertyList.GetProperty("font-variant")?.GetEnum() ?? 0;

            fontState = new FontState(fontInfo, fontFamily, fontStyle, fontWeight, fontSize, fontVariant);
        }
        return fontState;
    }


    public BorderAndPadding GetBorderAndPadding()
    {
        if (borderAndPadding == null)
        {
            this.borderAndPadding = new BorderAndPadding();
            InitDirections();

            InitBorderInfo(BorderAndPadding.TOP, saTop);
            InitBorderInfo(BorderAndPadding.BOTTOM, saBottom);
            InitBorderInfo(BorderAndPadding.LEFT, saLeft);
            InitBorderInfo(BorderAndPadding.RIGHT, saRight);
        }
        return borderAndPadding;
    }

    private void InitBorderInfo(int whichSide, string saSide)
    {
        borderAndPadding.SetPadding(
            whichSide, _propertyList.GetProperty(String.Format(msgPaddingFmt, saSide)).GetCondLength());
        int style = _propertyList.GetProperty(String.Format(msgStyleFmt, saSide)).GetEnum();
        if (style != Constants.NONE)
        {
            borderAndPadding.SetBorder(whichSide, style,
                                       _propertyList.GetProperty(String.Format(msgWidthFmt, saSide)).GetCondLength(),
                                       _propertyList.GetProperty(String.Format(msgColorFmt, saSide)).GetColorType());
        }
    }

    public HyphenationProps GetHyphenationProps()
    {
        if (hyphProps == null)
        {
            this.hyphProps = new HyphenationProps();
            hyphProps.hyphenate = this._propertyList.GetProperty("hyphenate").GetEnum();
            hyphProps.hyphenationChar =
                this._propertyList.GetProperty("hyphenation-character").GetCharacter();
            hyphProps.hyphenationPushCharacterCount =
                this._propertyList.GetProperty("hyphenation-push-character-count").GetNumber().IntValue();
            hyphProps.hyphenationRemainCharacterCount =
                this._propertyList.GetProperty("hyphenation-remain-character-count").GetNumber().IntValue();
            hyphProps.language = this._propertyList.GetProperty("language").GetString();
            hyphProps.country = this._propertyList.GetProperty("country").GetString();
        }
        return hyphProps;
    }

    public int CheckBreakBefore(Area area)
    {
        if (!(area is ColumnArea))
        {
            switch (_propertyList.GetProperty("break-before").GetEnum())
            {
                case BreakBefore.PAGE:
                    return Status.FORCE_PAGE_BREAK;
                case BreakBefore.ODD_PAGE:
                    return Status.FORCE_PAGE_BREAK_ODD;
                case BreakBefore.EVEN_PAGE:
                    return Status.FORCE_PAGE_BREAK_EVEN;
                case BreakBefore.COLUMN:
                    return Status.FORCE_COLUMN_BREAK;
                default:
                    return Status.OK;
            }
        }
        else
        {
            ColumnArea colArea = (ColumnArea)area;
            switch (_propertyList.GetProperty("break-before").GetEnum())
            {
                case BreakBefore.PAGE:
                    if (!colArea.hasChildren() && (colArea.getColumnIndex() == 1))
                    {
                        return Status.OK;
                    }
                    else
                    {
                        return Status.FORCE_PAGE_BREAK;
                    }
                case BreakBefore.ODD_PAGE:
                    if (!colArea.hasChildren() && (colArea.getColumnIndex() == 1)
                        && (colArea.Page?.getNumber() % 2 != 0))
                    {
                        return Status.OK;
                    }
                    else
                    {
                        return Status.FORCE_PAGE_BREAK_ODD;
                    }
                case BreakBefore.EVEN_PAGE:
                    if (!colArea.hasChildren() && (colArea.getColumnIndex() == 1)
                        && (colArea.Page?.getNumber() % 2 == 0))
                    {
                        return Status.OK;
                    }
                    else
                    {
                        return Status.FORCE_PAGE_BREAK_EVEN;
                    }
                case BreakBefore.COLUMN:
                    if (!area.hasChildren())
                    {
                        return Status.OK;
                    }
                    else
                    {
                        return Status.FORCE_COLUMN_BREAK;
                    }
                default:
                    return Status.OK;
            }
        }
    }

    public int CheckBreakAfter(Area area)
    {
        switch (_propertyList.GetProperty("break-after").GetEnum())
        {
            case BreakAfter.PAGE:
                return Status.FORCE_PAGE_BREAK;
            case BreakAfter.ODD_PAGE:
                return Status.FORCE_PAGE_BREAK_ODD;
            case BreakAfter.EVEN_PAGE:
                return Status.FORCE_PAGE_BREAK_EVEN;
            case BreakAfter.COLUMN:
                return Status.FORCE_COLUMN_BREAK;
            default:
                return Status.OK;
        }
    }

    public MarginProps GetMarginProps()
    {
        MarginProps props = new MarginProps();

        props.marginTop = this._propertyList.GetProperty("margin-top").GetLength().Millipoints();
        props.marginBottom = this._propertyList.GetProperty("margin-bottom").GetLength().Millipoints();
        props.marginLeft = this._propertyList.GetProperty("margin-left").GetLength().Millipoints();
        props.marginRight = this._propertyList.GetProperty("margin-right").GetLength().Millipoints();
        return props;
    }

    public bool IsVisible()
    {
        int visibility = _propertyList.GetProperty("visibility").GetEnum();
        return visibility == Visibility.VISIBLE;
    }

    public int GetFloatAlign()
        => _propertyList.GetProperty("float").GetEnum();

    public int GetClear()
        => _propertyList.GetProperty("clear").GetEnum();

    public int GetZIndex()
    {
        Length? zIndex = _propertyList.GetProperty("z-index")?.GetLength();
        if (zIndex == null || zIndex.IsAuto())
        {
            return 0;
        }

        return zIndex.Millipoints();
    }

    public BackgroundProps GetBackgroundProps()
    {
        if (bgProps == null)
        {
            bgProps = new BackgroundProps();

            bgProps.Color = _propertyList?.GetProperty("background-color")?.GetColorType();

            string src = _propertyList.GetProperty("background-image").GetString();
            if (src == "none")
            {
                bgProps.backImage = null;
            }
            else if (src == "inherit")
            {
                bgProps.backImage = null;
            }
            else
            {
                try
                {
                    bgProps.backImage = FonetImageFactory.Make(src);
                }
                catch (FonetImageException imgex)
                {
                    bgProps.backImage = null;
                    FonetDriver.ActiveDriver?.FireFonetError(imgex.Message);
                }
            }

            bgProps.backRepeat = _propertyList?.GetProperty("background-repeat")?.GetEnum() ?? 0;
        }

        return bgProps;
    }

    public MarginInlineProps GetMarginInlineProps()
    {
        MarginInlineProps props = new MarginInlineProps();
        return props;
    }

    public AccessibilityProps GetAccessibilityProps()
    {
        AccessibilityProps props = new();
        string? sourceDocument = _propertyList.GetProperty("source-document")?.GetString();

        if (!"none".Equals(sourceDocument))
        {
            props.SourceDoc = sourceDocument;
        }

        sourceDocument = _propertyList.GetProperty("role")?.GetString();

        if (!"none".Equals(sourceDocument))
        {
            props.Role = sourceDocument;
        }

        return props;
    }

    public AuralProps GetAuralProps()
        => new();

    public RelativePositionProps GetRelativePositionProps()
    {
        RelativePositionProps props = new RelativePositionProps();
        return props;
    }

    public AbsolutePositionProps GetAbsolutePositionProps()
    {
        AbsolutePositionProps props = new AbsolutePositionProps();
        return props;
    }

    public TextState getTextDecoration(FObj parent)
    {
        TextState? textState = null;
        bool found = false;

        do
        {
            string fname = parent.Name;
            if (fname.Equals("fo:flow") || fname.Equals("fo:static-content"))
            {
                found = true;
            }
            else if (fname.Equals("fo:block") || fname.Equals("fo:inline"))
            {
                FObjMixed fom = (FObjMixed)parent;
                textState = fom.GetTextState();
                found = true;
            }
            parent = parent.Parent;
        } while (!found);

        TextState ts = new TextState();

        if (textState != null)
        {
            ts.setUnderlined(textState.getUnderlined());
            ts.setOverlined(textState.getOverlined());
            ts.setLineThrough(textState.getLineThrough());
        }

        int textDecoration = this._propertyList.GetProperty("text-decoration").GetEnum();

        if (textDecoration == TextDecoration.UNDERLINE)
        {
            ts.setUnderlined(true);
        }
        if (textDecoration == TextDecoration.OVERLINE)
        {
            ts.setOverlined(true);
        }
        if (textDecoration == TextDecoration.LINE_THROUGH)
        {
            ts.setLineThrough(true);
        }
        if (textDecoration == TextDecoration.NO_UNDERLINE)
        {
            ts.setUnderlined(false);
        }
        if (textDecoration == TextDecoration.NO_OVERLINE)
        {
            ts.setOverlined(false);
        }
        if (textDecoration == TextDecoration.NO_LINE_THROUGH)
        {
            ts.setLineThrough(false);
        }

        return ts;
    }
}