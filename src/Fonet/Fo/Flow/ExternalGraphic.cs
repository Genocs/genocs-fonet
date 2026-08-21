using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Image;
using Genocs.Fonet.Layout;

namespace Genocs.Fonet.Fo.Flow;

internal class ExternalGraphic : FObj
{
    public static FObj.Maker CreateMaker()
        => FObj.Maker.For((parent, props) => new ExternalGraphic(parent, props));

    private string? _id;
    private string? _src;
    private int _height;
    private int _width;
    private int _breakAfter;
    private int _breakBefore;
    private int _align;
    private int _startIndent;
    private int _endIndent;
    private int _spaceBefore;
    private int _spaceAfter;
    private ImageArea? _imageArea;

    public ExternalGraphic(FObj? parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        Name = "fo:external-graphic";
    }

    public override Status Layout(Area area)
    {
        if (_marker == MarkerStart)
        {
            _src = Properties?.GetProperty("src")?.GetString();

            if (string.IsNullOrWhiteSpace(_src))
            {
                FonetDriver.ActiveDriver?.FireFonetError($"fo:external-graphic cannot have null 'src'. The image will be ignored!");
                return new Status(Status.OK);
            }

            _id = Properties?.GetProperty("id")?.GetString();


            _width = Properties?.GetProperty("width")?.GetLength()?.Millipoints() ?? 0;
            _height = Properties?.GetProperty("height")?.GetLength()?.Millipoints() ?? 0;


            AccessibilityProps mAccProps = _propertyManager.GetAccessibilityProps();
            AuralProps mAurProps = _propertyManager.GetAuralProps();
            BorderAndPadding bap = _propertyManager.GetBorderAndPadding();
            BackgroundProps bProps = _propertyManager.GetBackgroundProps();
            MarginInlineProps mProps = _propertyManager.GetMarginInlineProps();
            RelativePositionProps mRelProps = _propertyManager.GetRelativePositionProps();

            _align = Properties?.GetProperty("text-align")?.GetEnum() ?? 0;
            _startIndent = Properties?.GetProperty("start-indent")?.GetLength()?.Millipoints() ?? 0;
            _endIndent = Properties?.GetProperty("end-indent")?.GetLength()?.Millipoints() ?? 0;

            _spaceBefore = Properties?.GetProperty("space-before.optimum")?.GetLength()?.Millipoints() ?? 0;
            _spaceAfter = Properties?.GetProperty("space-after.optimum")?.GetLength()?.Millipoints() ?? 0;


            area.GetIDReferences().CreateID(_id);
            _marker = 0;
        }

        try
        {
            FonetImage img = FonetImageFactory.Make(_src);
            if ((_width == 0) || (_height == 0))
            {
                double imgWidth = img.Width;
                double imgHeight = img.Height;

                if ((_width == 0) && (_height == 0))
                {
                    _width = (int)(imgWidth * 1000d);
                    _height = (int)(imgHeight * 1000d);
                }
                else if (_height == 0)
                {
                    _height = (int)((imgHeight * ((double)_width)) / imgWidth);
                }
                else if (_width == 0)
                {
                    _width = (int)((imgWidth * ((double)_height)) / imgHeight);
                }
            }

            double ratio = (double)_width / (double)_height;

            Length? maxWidth = Properties?.GetProperty("max-width")?.GetLength();
            Length? maxHeight = Properties?.GetProperty("max-height")?.GetLength();

            if (maxWidth != null && _width > maxWidth.Millipoints())
            {
                _width = maxWidth.Millipoints();
                _height = (int)(((double)_width) / ratio);
            }

            if (maxHeight != null && _height > maxHeight.Millipoints())
            {
                _height = maxHeight.Millipoints();
                _width = (int)(ratio * ((double)_height));
            }

            int areaWidth = area.getAllocationWidth() - _startIndent - _endIndent;
            int pageHeight = area.Page?.getBody().getMaxHeight() - _spaceBefore ?? 0;

            if (_height > pageHeight)
            {
                _height = pageHeight;
                _width = (int)(ratio * ((double)_height));
            }

            if (_width > areaWidth)
            {
                _width = areaWidth;
                _height = (int)(((double)_width) / ratio);
            }

            if (area.spaceLeft() < (_height + _spaceBefore))
            {
                return new Status(Status.AREA_FULL_NONE);
            }

            this._imageArea =
                new ImageArea(_propertyManager.GetFontState(area.GetFontInfo()), img,
                              area.getAllocationWidth(), _width, _height,
                              _startIndent, _endIndent, _align);

            if ((_spaceBefore != 0) && (this._marker == 0))
            {
                area.AddDisplaySpace(_spaceBefore);
            }

            if (_marker == 0)
            {
                area.GetIDReferences().ConfigureID(_id, area);
            }

            _imageArea.start();
            _imageArea.end();

            if (_spaceAfter != 0)
            {
                area.AddDisplaySpace(_spaceAfter);
            }

            if (_breakBefore == BreakBefore.PAGE
                || ((_spaceBefore + _imageArea.GetHeight())
                    > area.spaceLeft()))
            {
                return new Status(Status.FORCE_PAGE_BREAK);
            }

            if (_breakBefore == BreakBefore.ODD_PAGE)
            {
                return new Status(Status.FORCE_PAGE_BREAK_ODD);
            }

            if (_breakBefore == BreakBefore.EVEN_PAGE)
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
                if (_imageArea.getContentWidth() > la.getRemainingWidth())
                {
                    la = ba.createNextLineArea();
                    if (la == null)
                    {
                        return new Status(Status.AREA_FULL_NONE);
                    }
                }

                la.addInlineArea(_imageArea, GetLinkSet());
            }
            else
            {
                area.AddChild(_imageArea);
                area.increaseHeight(_imageArea.getContentHeight());
            }

            _imageArea.Page = area.Page;

            if (_breakAfter == BreakAfter.PAGE)
            {
                this._marker = MarkerBreakAfter;
                return new Status(Status.FORCE_PAGE_BREAK);
            }

            if (_breakAfter == BreakAfter.ODD_PAGE)
            {
                this._marker = MarkerBreakAfter;
                return new Status(Status.FORCE_PAGE_BREAK_ODD);
            }

            if (_breakAfter == BreakAfter.EVEN_PAGE)
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
}