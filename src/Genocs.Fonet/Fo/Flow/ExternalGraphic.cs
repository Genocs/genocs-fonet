using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Image;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ExternalGraphic : FObj
{
    private int breakAfter = 0;
    private int breakBefore = 0;
    private int _align;
    private int startIndent;
    private int endIndent;
    private int spaceBefore;
    private int spaceAfter;
    private string src;
    private int height;
    private int width;
    private string id;
    private ImageArea imageArea;

    public ExternalGraphic(FObj parent, PropertyList propertyList) : base(parent, propertyList)
    {
        _name = "fo:external-graphic";
    }

    public override Status Layout(Area area)
    {
        if (_marker == MarkerStart)
        {
            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            _align = _properties.GetProperty("text-align").GetEnum();
            startIndent = _properties.GetProperty("start-indent").GetLength().MValue();
            endIndent = _properties.GetProperty("end-indent").GetLength().MValue();

            spaceBefore = _properties.GetProperty("space-before.optimum").GetLength().MValue();
            spaceAfter = _properties.GetProperty("space-after.optimum").GetLength().MValue();

            width = _properties.GetProperty("width").GetLength().MValue();
            height = _properties.GetProperty("height").GetLength().MValue();

            src = _properties.GetProperty("src").GetString();
            id = _properties.GetProperty("id").GetString();

            area.GetIDReferences().CreateID(id);
            _marker = 0;
        }

        try
        {
            FonetImage img = FonetImageFactory.Make(src);
            if ((width == 0) || (height == 0))
            {
                double imgWidth = img.Width;
                double imgHeight = img.Height;

                if ((width == 0) && (height == 0))
                {
                    width = (int)((imgWidth * 1000d));
                    height = (int)((imgHeight * 1000d));
                }
                else if (height == 0)
                {
                    height = (int)((imgHeight * ((double)width)) / imgWidth);
                }
                else if (width == 0)
                {
                    width = (int)((imgWidth * ((double)height)) / imgHeight);
                }
            }

            double ratio = (double)width / (double)height;

            Length maxWidth = this._properties.GetProperty("max-width").GetLength();
            Length maxHeight = this._properties.GetProperty("max-height").GetLength();

            if (maxWidth != null && width > maxWidth.MValue())
            {
                width = maxWidth.MValue();
                height = (int)(((double)width) / ratio);
            }
            if (maxHeight != null && height > maxHeight.MValue())
            {
                height = maxHeight.MValue();
                width = (int)(ratio * ((double)height));
            }

            int areaWidth = area.getAllocationWidth() - startIndent - endIndent;
            int pageHeight = area.getPage().getBody().getMaxHeight() - spaceBefore;

            if (height > pageHeight)
            {
                height = pageHeight;
                width = (int)(ratio * ((double)height));
            }
            if (width > areaWidth)
            {
                width = areaWidth;
                height = (int)(((double)width) / ratio);
            }

            if (area.spaceLeft() < (height + spaceBefore))
            {
                return new Status(Status.AREA_FULL_NONE);
            }

            this.imageArea =
                new ImageArea(_propertyManager.GetFontState(area.getFontInfo()), img,
                              area.getAllocationWidth(), width, height,
                              startIndent, endIndent, _align);

            if ((spaceBefore != 0) && (this._marker == 0))
            {
                area.addDisplaySpace(spaceBefore);
            }

            if (_marker == 0)
            {
                area.GetIDReferences().ConfigureID(id, area);
            }

            imageArea.start();
            imageArea.end();

            if (spaceAfter != 0)
            {
                area.addDisplaySpace(spaceAfter);
            }
            if (breakBefore == BreakBefore.PAGE
                || ((spaceBefore + imageArea.GetHeight())
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

            if (area is BlockArea)
            {
                BlockArea ba = (BlockArea)area;
                LineArea la = ba.getCurrentLineArea();
                if (la == null)
                {
                    return new Status(Status.AREA_FULL_NONE);
                }
                la.addPending();
                if (imageArea.getContentWidth() > la.getRemainingWidth())
                {
                    la = ba.createNextLineArea();
                    if (la == null)
                    {
                        return new Status(Status.AREA_FULL_NONE);
                    }
                }
                la.addInlineArea(imageArea, GetLinkSet());
            }
            else
            {
                area.addChild(imageArea);
                area.increaseHeight(imageArea.getContentHeight());
            }
            imageArea.setPage(area.getPage());

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

        }
        catch (FonetImageException imgex)
        {
            FonetDriver.ActiveDriver.FireFonetError($"Error while creating area : {imgex.Message}");
        }

        return new Status(Status.OK);
    }

    new public static FObj.Maker GetMaker()
    {
        return new Maker();
    }

    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new ExternalGraphic(parent, propertyList);
        }
    }
}