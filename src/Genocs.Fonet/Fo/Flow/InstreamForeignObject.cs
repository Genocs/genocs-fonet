using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Layout.Inline;
using System.Globalization;
using System.Xml;

namespace Genocs.Fonet.Fo.Flow;

internal class InstreamForeignObject : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new InstreamForeignObject(parent, props));

    private int breakBefore;
    private int breakAfter;
    private int scaling;
    private int width;
    private int height;
    private int contwidth;
    private int contheight;
    private bool wauto;
    private bool hauto;
    private bool cwauto;
    private bool chauto;
    private int spaceBefore;
    private int spaceAfter;
    private int startIndent;
    private int endIndent;
    private ForeignObjectArea areaCurrent;

    public InstreamForeignObject(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:instream-foreign-object";
    }

    public override Status Layout(Area area)
    {
        if (this._marker == MarkerBreakAfter)
        {
            return new Status(Status.OK);
        }

        if (this._marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            string id = this.Properties.GetProperty("id").GetString();
            int align = this.Properties.GetProperty("text-align").GetEnum();
            int valign = this.Properties.GetProperty("vertical-align").GetEnum();
            int overflow = this.Properties.GetProperty("overflow").GetEnum();

            this.breakBefore = this.Properties.GetProperty("break-before").GetEnum();
            this.breakAfter = this.Properties.GetProperty("break-after").GetEnum();
            this.width = this.Properties.GetProperty("width").GetLength().MValue();
            this.height = this.Properties.GetProperty("height").GetLength().MValue();
            this.contwidth =
                this.Properties.GetProperty("content-width").GetLength().MValue();
            this.contheight =
                this.Properties.GetProperty("content-height").GetLength().MValue();
            this.wauto = this.Properties.GetProperty("width").GetLength().IsAuto();
            this.hauto = this.Properties.GetProperty("height").GetLength().IsAuto();
            this.cwauto = this.Properties.GetProperty("content-width").GetLength().IsAuto();
            this.chauto =
                this.Properties.GetProperty("content-height").GetLength().IsAuto();

            this.startIndent = this.Properties.GetProperty("start-indent").GetLength().MValue();
            this.endIndent = this.Properties.GetProperty("end-indent").GetLength().MValue();
            this.spaceBefore =
                this.Properties.GetProperty("space-before.optimum").GetLength().MValue();
            this.spaceAfter = this.Properties.GetProperty("space-after.optimum").GetLength().MValue();

            this.scaling = this.Properties.GetProperty("scaling").GetEnum();

            area.GetIDReferences().CreateID(id);
            if (this.areaCurrent == null)
            {
                this.areaCurrent =
                    new ForeignObjectArea(_propertyManager.GetFontState(area.GetFontInfo()),
                                          area.getAllocationWidth());

                this.areaCurrent.start();
                areaCurrent.SetWidth(this.width);
                areaCurrent.SetHeight(this.height);
                areaCurrent.SetContentWidth(this.contwidth);
                areaCurrent.setContentHeight(this.contheight);
                areaCurrent.setScaling(this.scaling);
                areaCurrent.setAlign(align);
                areaCurrent.setVerticalAlign(valign);
                areaCurrent.setOverflow(overflow);
                areaCurrent.setSizeAuto(wauto, hauto);
                areaCurrent.setContentSizeAuto(cwauto, chauto);

                areaCurrent.Page = area.Page;

                int numChildren = this._children.Count;
                if (numChildren > 1)
                {
                    throw new FonetException("Only one child element is allowed in an instream-foreign-object");
                }
                if (this._children.Count > 0)
                {
                    FONode fo = (FONode)_children[0];
                    if (fo is XMLObj xmlObj)
                    {
                        XmlDocument? doc = xmlObj.GetDocument();
                        if (doc != null)
                        {
                            areaCurrent.setSvgDocument(doc);
                            if (TryReadSvgIntrinsicSize(doc, out int intrinsicWidth, out int intrinsicHeight))
                            {
                                areaCurrent.setIntrinsicWidth(intrinsicWidth);
                                areaCurrent.setIntrinsicHeight(intrinsicHeight);
                            }
                        }
                    }

                    Status status;
                    if ((status =
                        fo.Layout(this.areaCurrent)).IsIncomplete())
                    {
                        return status;
                    }
                    this.areaCurrent.end();
                }
            }

            this._marker = 0;

            if (breakBefore == BreakBefore.PAGE
                || ((spaceBefore + areaCurrent.getEffectiveHeight())
                    > area.spaceLeft()))
            {
                return new Status(Status.FORCE_PAGE_BREAK);
            }

            if (breakBefore == BreakBefore.ODD_PAGE)
            {
                return new Status(Status.FORCE_PAGE_BREAK_ODD);
            }

            if (breakBefore == BreakBefore.EVEN_PAGE)
            {
                return new Status(Status.FORCE_PAGE_BREAK_EVEN);
            }
        }

        if (this.areaCurrent == null)
        {
            return new Status(Status.OK);
        }

        if (area is BlockArea)
        {
            BlockArea ba = (BlockArea)area;
            LineArea la = ba.getCurrentLineArea();
            if (la == null)
            {
                return new Status(Status.AREA_FULL_NONE);
            }
            la.addPending();
            if (areaCurrent.getEffectiveWidth() > la.getRemainingWidth())
            {
                la = ba.createNextLineArea();
                if (la == null)
                {
                    return new Status(Status.AREA_FULL_NONE);
                }
            }
            la.addInlineArea(areaCurrent, GetLinkSet());
        }
        else
        {
            area.AddChild(areaCurrent);
            area.increaseHeight(areaCurrent.getEffectiveHeight());
        }

        if (this._isInTableCell)
        {
            startIndent += _forcedStartOffset;
        }

        areaCurrent.setStartIndent(startIndent);
        areaCurrent.Page = area.Page;

        if (breakAfter == BreakAfter.PAGE)
        {
            this._marker = MarkerBreakAfter;
            return new Status(Status.FORCE_PAGE_BREAK);
        }

        if (breakAfter == BreakAfter.ODD_PAGE)
        {
            this._marker = MarkerBreakAfter;
            return new Status(Status.FORCE_PAGE_BREAK_ODD);
        }

        if (breakAfter == BreakAfter.EVEN_PAGE)
        {
            this._marker = MarkerBreakAfter;
            return new Status(Status.FORCE_PAGE_BREAK_EVEN);
        }

        areaCurrent = null;
        return new Status(Status.OK);
    }

    private static bool TryReadSvgIntrinsicSize(XmlDocument doc, out int widthMpt, out int heightMpt)
    {
        widthMpt = 0;
        heightMpt = 0;

        XmlElement? root = doc.DocumentElement;
        if (root == null || !root.LocalName.Equals("svg", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        bool hasWidth = TryParseSvgLengthToMpt(root.GetAttribute("width"), out widthMpt);
        bool hasHeight = TryParseSvgLengthToMpt(root.GetAttribute("height"), out heightMpt);

        if (hasWidth && hasHeight)
        {
            return true;
        }

        string viewBox = root.GetAttribute("viewBox");
        if (string.IsNullOrWhiteSpace(viewBox))
        {
            return false;
        }

        string[] parts = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double vbWidth)
            || !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double vbHeight)
            || vbWidth <= 0
            || vbHeight <= 0)
        {
            return false;
        }

        if (!hasWidth)
        {
            widthMpt = (int)Math.Round(vbWidth * 1000d);
        }

        if (!hasHeight)
        {
            heightMpt = (int)Math.Round(vbHeight * 1000d);
        }

        return widthMpt > 0 && heightMpt > 0;
    }

    private static bool TryParseSvgLengthToMpt(string value, out int mpt)
    {
        mpt = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        int idx = 0;
        while (idx < trimmed.Length && (char.IsDigit(trimmed[idx]) || trimmed[idx] == '.' || trimmed[idx] == '-' || trimmed[idx] == '+'))
        {
            idx++;
        }

        if (idx == 0)
        {
            return false;
        }

        string numberPart = trimmed[..idx];
        string unitPart = trimmed[idx..].Trim().ToLowerInvariant();

        if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
        {
            return false;
        }

        double points = unitPart switch
        {
            "" => number,
            "px" => number,
            "pt" => number,
            "pc" => number * 12d,
            "in" => number * 72d,
            "cm" => number * 72d / 2.54d,
            "mm" => number * 72d / 25.4d,
            _ => double.NaN
        };

        if (double.IsNaN(points) || points <= 0)
        {
            return false;
        }

        mpt = (int)Math.Round(points * 100d);
        return mpt > 0;
    }
}