using System.Collections;
using System.Globalization;
using System.Text;
using System.Xml;
using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Image;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Layout.Inline;
using Genocs.Fonet.Pdf;
using Genocs.Fonet.Pdf.Gdi;
using Genocs.Fonet.Render.Pdf.Fonts;

namespace Genocs.Fonet.Render.Pdf;

internal sealed class PdfRenderer
{
    /// <summary>
    /// The current vertical position in Millipoints from bottom.
    /// </summary>
    private int _currentYPosition;

    /// <summary>
    /// The current horizontal position in Millipoints from left.
    /// </summary>
    private int _currentXPosition;

    /// <summary>
    /// The horizontal position of the current area container.
    /// </summary>
    private int _currentAreaContainerXPosition;

    /// <summary>
    /// The PDF Document being created.
    /// </summary>
    private PdfCreator? _pdfDoc;

    /// <summary>
    /// The /Resources object of the PDF document being created.
    /// </summary>
    private PdfResources? _pdfResources;

    /// <summary>
    /// The current stream to Add PDF commands to.
    /// </summary>
    private PdfContentStream? _currentStream;

    /// <summary>
    /// The current annotation list to Add annotations to.
    /// </summary>
    private PdfAnnotList? _currentAnnotList;

    /// <summary>
    /// The current page to Add annotations to.
    /// </summary>
    private PdfPage? _currentPage;

    private float currentLetterSpacing = Single.NaN;

    private float currentWordSpacing = Single.NaN;

    /// <summary>
    /// True if a TJ command is left to be written.
    /// </summary>
    private bool textOpen = false;

    /// <summary>
    /// The previous Y coordinate of the last word written.
    /// </summary>
    /// <remarks>
    /// Used to decide if we can draw the next word on the same line.
    /// </remarks>
    private int prevWordY = 0;

    /// <summary>
    /// The previous X coordinate of the last word written.
    /// </summary>
    /// <remarks>
    /// Used to calculate how much space between two words.
    /// </remarks>
    private int prevWordX = 0;

    /// <summary>
    /// The width of the previous word.
    /// </summary>
    /// <remarks>
    /// Used to calculate space between.
    /// </remarks>
    private int prevWordWidth = 0;

    /// <summary>
    /// Reusable word area string buffer to reduce memory usage.
    /// </summary>
    /// <remarks>
    /// TODO: remove use of this.
    /// </remarks>
    private StringBuilder _wordAreaPDF = new StringBuilder();

    /// <summary>
    /// User specified rendering options.
    /// </summary>
    private PdfRendererOptions? _options;

    /// <summary>
    /// The current (internal) font name.
    /// </summary>
    private string? _currentFontName;

    /// <summary>
    /// The current font size in Millipoints.
    /// </summary>
    private int currentFontSize;

    /// <summary>
    /// The current color/gradient to fill shapes with.
    /// </summary>
    private PdfColor? _currentFill;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevUnderlineXEndPos;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevUnderlineYEndPos;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevUnderlineSize;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private PdfColor? _prevUnderlineColor;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevOverlineXEndPos;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevOverlineYEndPos;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevOverlineSize;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private PdfColor prevOverlineColor;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevLineThroughXEndPos;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevLineThroughYEndPos;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private int prevLineThroughSize;

    /// <summary>
    /// Previous values used for text-decoration drawing.
    /// </summary>
    private PdfColor prevLineThroughColor;

    /// <summary>
    /// Provides triplet to font resolution.
    /// </summary>
    private FontInfo? _fontInfo;

    /// <summary>
    /// Handles adding base 14 and all system fonts.
    /// </summary>
    private FontSetup _fontSetup;

    /// <summary>
    /// The IDReferences for this document.
    /// </summary>
    private IDReferences? idReferences;

    private readonly Dictionary<string, PdfName> _svgFillOpacityStates = new();

    /// <summary>
    /// Create the PDF renderer.
    /// </summary>
    internal PdfRenderer(Stream stream)
    {
        _pdfDoc = new PdfCreator(stream);
    }

    /// <summary>
    /// Assigns renderer options to this PdfRenderer.
    /// </summary>
    /// <remarks>
    /// This property will only accept an instance of the PdfRendererOptions class.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// If <i>value</i> is not an instance of PdfRendererOptions.
    /// </exception>
    public PdfRendererOptions Options
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value is not PdfRendererOptions)
            {
                throw new ArgumentException("Options must be an instance of PdfRendererOptions");
            }

            // Guaranteed to work because of above check
            _options = value as PdfRendererOptions;
        }
    }

    public void StartRenderer()
    {
        if (_options != null)
        {
            _pdfDoc.SetOptions(_options);
        }

        _pdfDoc.outputHeader();
    }

    public void StopRenderer()
    {
        _fontSetup.AddToResources(new PdfFontCreator(_pdfDoc), _pdfDoc.getResources());
        _pdfDoc.outputTrailer();

        _pdfDoc = null;
        _pdfResources = null;
        _currentStream = null;
        _currentAnnotList = null;
        _currentPage = null;

        idReferences = null;
        _currentFontName = string.Empty;
        _currentFill = null;
        _prevUnderlineColor = null;
        prevOverlineColor = null;
        prevLineThroughColor = null;
        _fontSetup = null;
        _fontInfo = null;
    }

    /// <summary>
    /// </summary>
    /// <param name="fontInfo"></param>
    public void SetupFontInfo(FontInfo fontInfo)
    {
        _fontInfo = fontInfo;
        _fontSetup = new FontSetup(fontInfo, (_options == null) ? FontType.Link : _options.FontType);
    }

    public void RenderSpanArea(SpanArea area)
    {
        foreach (Box b in area.Children)
        {
            b.Render(this); // column areas
        }
    }

    public void RenderBodyAreaContainer(BodyAreaContainer area)
    {
        int saveY = this._currentYPosition;
        int saveX = this._currentAreaContainerXPosition;

        if (area.getPosition() == Position.ABSOLUTE)
        {
            // Y position is computed assuming positive Y axis, adjust for negative postscript one
            this._currentYPosition = area.GetYPosition();
            this._currentAreaContainerXPosition = area.getXPosition();
        }
        else if (area.getPosition() == Position.RELATIVE)
        {
            this._currentYPosition -= area.GetYPosition();
            this._currentAreaContainerXPosition += area.getXPosition();
        }

        this._currentXPosition = this._currentAreaContainerXPosition;
        int rx = this._currentAreaContainerXPosition;
        int ry = this._currentYPosition;

        int w = area.getAllocationWidth();
        int h = area.getMaxHeight();

        DoBackground(area, rx, ry, w, h);

        // floats & footnotes stuff
        RenderAreaContainer(area.getBeforeFloatReferenceArea());
        RenderAreaContainer(area.getFootnoteReferenceArea());

        // main reference area
        foreach (Box b in area.getMainReferenceArea().Children)
        {
            b.Render(this); // span areas
        }

        if (area.getPosition() != Position.STATIC)
        {
            this._currentYPosition = saveY;
            this._currentAreaContainerXPosition = saveX;
        }
        else
        {
            this._currentYPosition -= area.GetHeight();
        }
    }

    public void RenderAreaContainer(AreaContainer area)
    {
        int saveY = this._currentYPosition;
        int saveX = this._currentAreaContainerXPosition;

        if (area.getPosition() == Position.ABSOLUTE)
        {
            // XPosition and YPosition give the content rectangle position
            this._currentYPosition = area.YPosition;
            this._currentAreaContainerXPosition = area.XPosition;
        }
        else if (area.getPosition() == Position.RELATIVE)
        {
            this._currentYPosition -= area.YPosition;
            this._currentAreaContainerXPosition += area.XPosition;
        }
        else if (area.getPosition() == Position.STATIC)
        {
            this._currentYPosition -= area.getPaddingTop()
                + area.getBorderTopWidth();
        }

        this._currentXPosition = this._currentAreaContainerXPosition;
        DoFrame(area);

        Area.RenderChildrenInZOrder(area.Children, this);

        // Restore previous origin
        this._currentYPosition = saveY;
        this._currentAreaContainerXPosition = saveX;
        if (area.getPosition() == Position.STATIC)
        {
            this._currentYPosition -= area.GetHeight();
        }
    }

    public void RenderBlockArea(BlockArea area)
    {
        // KLease: Temporary test to fix block positioning
        // Offset ypos by padding and border widths
        _currentYPosition -= (area.getPaddingTop() + area.getBorderTopWidth());
        DoFrame(area);
        Area.RenderChildrenInZOrder(area.Children, this);
        _currentYPosition -= (area.getPaddingBottom() + area.getBorderBottomWidth());
    }

    public void RenderLineArea(LineArea area)
    {
        int rx = this._currentAreaContainerXPosition + area.getStartIndent();
        int ry = this._currentYPosition;
        int w = area.getContentWidth();
        int h = area.GetHeight();

        this._currentYPosition -= area.getPlacementOffset();
        this._currentXPosition = rx;

        int bl = this._currentYPosition;

        foreach (Box b in area.Children)
        {
            if (b is InlineArea)
            {
                InlineArea ia = (InlineArea)b;
                _currentYPosition = ry - ia.OffsetY;
            }
            else
            {
                _currentYPosition = ry - area.getPlacementOffset();
            }

            b.Render(this);
        }

        this._currentYPosition = ry - h;
        this._currentXPosition = rx;
    }

    /**
    * Add a line to the current stream
    *
    * @param x1 the start x location in _millipoints
    * @param y1 the start y location in _millipoints
    * @param x2 the end x location in _millipoints
    * @param y2 the end y location in _millipoints
    * @param th the thickness in _millipoints
    * @param r the red component
    * @param g the green component
    * @param b the blue component
    */

    private void AddLine(int x1, int y1, int x2, int y2, int th, PdfColor stroke)
    {
        CloseText();

        _currentStream.Write("ET\nq\n" + stroke.getColorSpaceOut(false)
            + PdfNumber.DoubleOut(x1 / 1000f) + " " + PdfNumber.DoubleOut(y1 / 1000f) + " m "
            + PdfNumber.DoubleOut(x2 / 1000f) + " " + PdfNumber.DoubleOut(y2 / 1000f) + " l "
            + PdfNumber.DoubleOut(th / 1000f) + " w S\n" + "Q\nBT\n");
    }

    /**
    * Add a line to the current stream
    *
    * @param x1 the start x location in _millipoints
    * @param y1 the start y location in _millipoints
    * @param x2 the end x location in _millipoints
    * @param y2 the end y location in _millipoints
    * @param th the thickness in _millipoints
    * @param rs the rule style
    * @param r the red component
    * @param g the green component
    * @param b the blue component
    */

    private void AddLine(int x1, int y1, int x2, int y2, int th, int rs, PdfColor stroke)
    {
        CloseText();
        _currentStream.Write("ET\nq\n" + stroke.getColorSpaceOut(false)
            + SetRuleStylePattern(rs) + PdfNumber.DoubleOut(x1 / 1000f) + " "
            + PdfNumber.DoubleOut(y1 / 1000f) + " m " + PdfNumber.DoubleOut(x2 / 1000f) + " "
            + PdfNumber.DoubleOut(y2 / 1000f) + " l " + PdfNumber.DoubleOut(th / 1000f) + " w S\n"
            + "Q\nBT\n");
    }

    /**
    * Add a rectangle to the current stream
    *
    * @param x the x position of left edge in _millipoints
    * @param y the y position of top edge in _millipoints
    * @param w the width in _millipoints
    * @param h the height in _millipoints
    * @param stroke the stroke color/gradient
    */

    private void AddRect(int x, int y, int w, int h, PdfColor stroke)
    {
        CloseText();
        _currentStream.Write("ET\nq\n" + stroke.getColorSpaceOut(false)
            + PdfNumber.DoubleOut(x / 1000f) + " " + PdfNumber.DoubleOut(y / 1000f) + " "
            + PdfNumber.DoubleOut(w / 1000f) + " " + PdfNumber.DoubleOut(h / 1000f) + " re s\n"
            + "Q\nBT\n");
    }

    /**
    * Add a filled rectangle to the current stream
    *
    * @param x the x position of left edge in _millipoints
    * @param y the y position of top edge in _millipoints
    * @param w the width in _millipoints
    * @param h the height in _millipoints
    * @param fill the fill color/gradient
    * @param stroke the stroke color/gradient
    */

    private void AddRect(int x, int y, int w, int h, PdfColor stroke, PdfColor fill)
    {
        CloseText();
        _currentStream.Write("ET\nq\n" + fill.getColorSpaceOut(true)
            + stroke.getColorSpaceOut(false) + PdfNumber.DoubleOut(x / 1000f)
            + " " + PdfNumber.DoubleOut(y / 1000f) + " " + PdfNumber.DoubleOut(w / 1000f) + " "
            + PdfNumber.DoubleOut(h / 1000f) + " re b\n" + "Q\nBT\n");
    }

    /**
    * Add a filled rectangle to the current stream
    *
    * @param x the x position of left edge in _millipoints
    * @param y the y position of top edge in _millipoints
    * @param w the width in _millipoints
    * @param h the height in _millipoints
    * @param fill the fill color/gradient
    */

    private void AddFilledRect(int x, int y, int w, int h, PdfColor fill)
    {
        CloseText();
        _currentStream.Write("ET\nq\n" + fill.getColorSpaceOut(true)
            + PdfNumber.DoubleOut(x / 1000f) + " " + PdfNumber.DoubleOut(y / 1000f) + " "
            + PdfNumber.DoubleOut(w / 1000f) + " " + PdfNumber.DoubleOut(h / 1000f) + " re f\n"
            + "Q\nBT\n");
    }

    /**
    * Render image area to PDF
    *
    * @param area the image area to Render
    */

    public void RenderImageArea(ImageArea area)
    {
        int x = this._currentXPosition + area.getXOffset();
        int y = this._currentYPosition;
        int w = area.getContentWidth();
        int h = area.GetHeight();

        this._currentYPosition -= h;

        FonetImage img = area.getImage();

        PdfXObject xobj = this._pdfDoc.AddImage(img);
        CloseText();

        _currentStream.Write("ET\nq\n" + PdfNumber.DoubleOut(((float)w) / 1000f) + " 0 0 "
            + PdfNumber.DoubleOut(((float)h) / 1000f) + " "
            + PdfNumber.DoubleOut(((float)x) / 1000f) + " "
            + PdfNumber.DoubleOut(((float)(y - h)) / 1000f) + " cm\n" + "/" + xobj.Name.Name
            + " Do\nQ\nBT\n");

        this._currentXPosition += area.getContentWidth();
    }

    /**
    * Render a foreign object area
    */

    public void RenderForeignObjectArea(ForeignObjectArea area)
    {
        // if necessary need to scale and align the content
        _currentXPosition = this._currentXPosition + area.getXOffset();

        // TODO: why was this here? this.currentYPosition = this.currentYPosition;
        switch (area.getAlign())
        {
            case TextAlign.START:
                break;
            case TextAlign.END:
                break;
            case TextAlign.CENTER:
            case TextAlign.JUSTIFY:
                break;
        }

        switch (area.getVerticalAlign())
        {
            case VerticalAlign.BASELINE:
                break;
            case VerticalAlign.MIDDLE:
                break;
            case VerticalAlign.SUB:
                break;
            case VerticalAlign.SUPER:
                break;
            case VerticalAlign.TEXT_TOP:
                break;
            case VerticalAlign.TEXT_BOTTOM:
                break;
            case VerticalAlign.TOP:
                break;
            case VerticalAlign.BOTTOM:
                break;
        }

        CloseText();

        // in general the content will not be text
        _currentStream.Write("ET\n");

        // align and scale
        _currentStream.Write("q\n");
        switch (area.scalingMethod())
        {
            case Scaling.UNIFORM:
                break;
            case Scaling.NON_UNIFORM:
                break;
        }

        // if the overflow is auto (default), scroll or visible
        // then the contents should not be clipped, since this
        // is considered a printing medium.
        switch (area.getOverflow())
        {
            case Overflow.VISIBLE:
            case Overflow.SCROLL:
            case Overflow.AUTO:
                break;
            case Overflow.HIDDEN:
                break;
        }

        Area? foreignObject = area.getObject();
        if (foreignObject != null)
        {
            foreignObject.Render(this);
        }
        else
        {
            XmlDocument? svgDoc = area.getSvgDocument();
            if (svgDoc != null)
            {
                RenderInlineSvg(area, svgDoc);
            }
        }

        _currentStream.Write("Q\n");
        _currentStream.Write("BT\n");

        _currentXPosition += area.getEffectiveWidth();

        // this.currentYPosition -= area.getEffectiveHeight();
    }

    private void RenderInlineSvg(ForeignObjectArea area, XmlDocument svgDoc)
    {
        XmlElement? root = svgDoc.DocumentElement;
        if (root == null || !root.LocalName.Equals("svg", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        float targetWidthPt = area.getEffectiveWidth() / 1000f;
        float targetHeightPt = area.getEffectiveHeight() / 1000f;
        if (targetWidthPt <= 0f || targetHeightPt <= 0f)
        {
            return;
        }

        if (!TryGetSvgViewport(root, out float viewportWidth, out float viewportHeight)
            || viewportWidth <= 0f
            || viewportHeight <= 0f)
        {
            viewportWidth = targetWidthPt;
            viewportHeight = targetHeightPt;
        }

        float offsetX = _currentXPosition / 1000f;
        float offsetY = (_currentYPosition - area.getEffectiveHeight()) / 1000f;
        float scaleX = targetWidthPt / viewportWidth;
        float scaleY = targetHeightPt / viewportHeight;

        _currentStream.Write("q\n");
        _currentStream.Write("1 0 0 1 " + PdfNumber.DoubleOut(offsetX) + " " + PdfNumber.DoubleOut(offsetY) + " cm\n");
        _currentStream.Write(PdfNumber.DoubleOut(scaleX) + " 0 0 " + PdfNumber.DoubleOut(scaleY) + " 0 0 cm\n");

        SvgStyle baseStyle = SvgStyle.Default;
        foreach (XmlNode childNode in root.ChildNodes)
        {
            if (childNode is XmlElement childElement)
            {
                RenderSvgElement(childElement, viewportHeight, baseStyle);
            }
        }

        _currentStream.Write("Q\n");
    }

    private void RenderSvgElement(XmlElement element, float viewportHeight, SvgStyle inheritedStyle)
    {
        SvgStyle style = MergeSvgStyle(inheritedStyle, element);

        string localName = element.LocalName.ToLowerInvariant();
        switch (localName)
        {
            case "g":
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child is XmlElement childElement)
                    {
                        RenderSvgElement(childElement, viewportHeight, style);
                    }
                }

                break;
            case "rect":
                RenderSvgRect(element, viewportHeight, style);
                break;
            case "circle":
                RenderSvgCircle(element, viewportHeight, style);
                break;
            case "line":
                RenderSvgLine(element, viewportHeight, style);
                break;
            case "path":
                RenderSvgPath(element, viewportHeight, style);
                break;
            default:
                break;
        }
    }

    private void RenderSvgRect(XmlElement element, float viewportHeight, SvgStyle style)
    {
        if (!TryGetFloatAttribute(element, "x", out float x)) x = 0f;
        if (!TryGetFloatAttribute(element, "y", out float y)) y = 0f;
        if (!TryGetFloatAttribute(element, "width", out float width) || width <= 0f) return;
        if (!TryGetFloatAttribute(element, "height", out float height) || height <= 0f) return;

        float pdfY = viewportHeight - y - height;

        EmitSvgPaint(style, (stream, hasFill, hasStroke) =>
        {
            stream.Write(PdfNumber.DoubleOut(x) + " "
                + PdfNumber.DoubleOut(pdfY) + " "
                + PdfNumber.DoubleOut(width) + " "
                + PdfNumber.DoubleOut(height) + " re ");
            stream.Write(GetPaintOperator(hasFill, hasStroke) + "\n");
        });
    }

    private void RenderSvgCircle(XmlElement element, float viewportHeight, SvgStyle style)
    {
        if (!TryGetFloatAttribute(element, "cx", out float cx)) return;
        if (!TryGetFloatAttribute(element, "cy", out float cy)) return;
        if (!TryGetFloatAttribute(element, "r", out float r) || r <= 0f) return;

        float centerY = viewportHeight - cy;
        const float k = 0.552284749831f;
        float c = r * k;

        EmitSvgPaint(style, (stream, hasFill, hasStroke) =>
        {
            stream.Write(PdfNumber.DoubleOut(cx + r) + " " + PdfNumber.DoubleOut(centerY) + " m\n");
            stream.Write(PdfNumber.DoubleOut(cx + r) + " " + PdfNumber.DoubleOut(centerY + c) + " "
                + PdfNumber.DoubleOut(cx + c) + " " + PdfNumber.DoubleOut(centerY + r) + " "
                + PdfNumber.DoubleOut(cx) + " " + PdfNumber.DoubleOut(centerY + r) + " c\n");
            stream.Write(PdfNumber.DoubleOut(cx - c) + " " + PdfNumber.DoubleOut(centerY + r) + " "
                + PdfNumber.DoubleOut(cx - r) + " " + PdfNumber.DoubleOut(centerY + c) + " "
                + PdfNumber.DoubleOut(cx - r) + " " + PdfNumber.DoubleOut(centerY) + " c\n");
            stream.Write(PdfNumber.DoubleOut(cx - r) + " " + PdfNumber.DoubleOut(centerY - c) + " "
                + PdfNumber.DoubleOut(cx - c) + " " + PdfNumber.DoubleOut(centerY - r) + " "
                + PdfNumber.DoubleOut(cx) + " " + PdfNumber.DoubleOut(centerY - r) + " c\n");
            stream.Write(PdfNumber.DoubleOut(cx + c) + " " + PdfNumber.DoubleOut(centerY - r) + " "
                + PdfNumber.DoubleOut(cx + r) + " " + PdfNumber.DoubleOut(centerY - c) + " "
                + PdfNumber.DoubleOut(cx + r) + " " + PdfNumber.DoubleOut(centerY) + " c\n");
            stream.Write(GetPaintOperator(hasFill, hasStroke) + "\n");
        });
    }

    private void RenderSvgLine(XmlElement element, float viewportHeight, SvgStyle style)
    {
        if (!TryGetFloatAttribute(element, "x1", out float x1)) return;
        if (!TryGetFloatAttribute(element, "y1", out float y1)) return;
        if (!TryGetFloatAttribute(element, "x2", out float x2)) return;
        if (!TryGetFloatAttribute(element, "y2", out float y2)) return;

        float py1 = viewportHeight - y1;
        float py2 = viewportHeight - y2;

        EmitSvgPaint(style with { Fill = null }, (stream, hasFill, hasStroke) =>
        {
            stream.Write(PdfNumber.DoubleOut(x1) + " " + PdfNumber.DoubleOut(py1) + " m\n");
            stream.Write(PdfNumber.DoubleOut(x2) + " " + PdfNumber.DoubleOut(py2) + " l\n");
            stream.Write("S\n");
        });
    }

    private void RenderSvgPath(XmlElement element, float viewportHeight, SvgStyle style)
    {
        string d = element.GetAttribute("d");
        if (string.IsNullOrWhiteSpace(d))
        {
            return;
        }

        bool evenOdd = IsEvenOddFill(element);

        EmitSvgPaint(style, (stream, hasFill, hasStroke) =>
        {
            if (!TryWriteSvgPathData(stream, d, viewportHeight))
            {
                return;
            }

            stream.Write(GetPaintOperator(hasFill, hasStroke, evenOdd) + "\n");
        });
    }

    private void EmitSvgPaint(SvgStyle style, Action<PdfContentStream, bool, bool> emitPath)
    {
        bool hasFill = style.Fill != null && style.FillOpacity > 0f;
        bool hasStroke = style.Stroke != null;

        if (!hasFill && !hasStroke)
        {
            return;
        }

        _currentStream.Write("q\n");
        if (hasFill)
        {
            PdfColor fill = style.Fill!;
            _currentStream.Write(fill.getColorSpaceOut(true));

            if (style.FillOpacity < 1f)
            {
                PdfName alphaStateName = GetOrCreateSvgFillOpacityState(style.FillOpacity);
                _currentStream.Write("/" + alphaStateName.Name + " gs\n");
            }
        }

        if (hasStroke)
        {
            PdfColor stroke = style.Stroke!;
            _currentStream.Write(stroke.getColorSpaceOut(false));
            _currentStream.Write(PdfNumber.DoubleOut(style.StrokeWidth <= 0f ? 1f : style.StrokeWidth) + " w\n");
        }

        emitPath(_currentStream, hasFill, hasStroke);
        _currentStream.Write("Q\n");
    }

    private static SvgStyle MergeSvgStyle(SvgStyle inherited, XmlElement element)
    {
        SvgStyle style = inherited;

        string fillAttr = element.GetAttribute("fill");
        if (!string.IsNullOrWhiteSpace(fillAttr))
        {
            style = style with { Fill = ParseSvgColor(fillAttr) };
        }

        string strokeAttr = element.GetAttribute("stroke");
        if (!string.IsNullOrWhiteSpace(strokeAttr))
        {
            style = style with { Stroke = ParseSvgColor(strokeAttr) };
        }

        string strokeWidthAttr = element.GetAttribute("stroke-width");
        if (TryParseSvgNumber(strokeWidthAttr, out float strokeWidth))
        {
            style = style with { StrokeWidth = strokeWidth };
        }

        string fillOpacityAttr = element.GetAttribute("fill-opacity");
        if (TryParseSvgOpacity(fillOpacityAttr, out float fillOpacity))
        {
            style = style with { FillOpacity = fillOpacity };
        }

        string styleAttr = element.GetAttribute("style");
        if (!string.IsNullOrWhiteSpace(styleAttr))
        {
            foreach (string declaration in styleAttr.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pair = declaration.Split(':', 2, StringSplitOptions.TrimEntries);
                if (pair.Length != 2)
                {
                    continue;
                }

                switch (pair[0])
                {
                    case "fill":
                        style = style with { Fill = ParseSvgColor(pair[1]) };
                        break;
                    case "stroke":
                        style = style with { Stroke = ParseSvgColor(pair[1]) };
                        break;
                    case "stroke-width":
                        if (TryParseSvgNumber(pair[1], out float styledWidth))
                        {
                            style = style with { StrokeWidth = styledWidth };
                        }

                        break;
                    case "fill-opacity":
                        if (TryParseSvgOpacity(pair[1], out float styledOpacity))
                        {
                            style = style with { FillOpacity = styledOpacity };
                        }
                        break;
                }
            }
        }

        return style;
    }

    private static string GetPaintOperator(bool hasFill, bool hasStroke, bool evenOdd = false)
    {
        if (hasFill && hasStroke)
        {
            return evenOdd ? "B*" : "B";
        }

        if (hasFill)
        {
            return evenOdd ? "f*" : "f";
        }

        return "S";
    }

    private static bool IsEvenOddFill(XmlElement element)
    {
        string fillRule = element.GetAttribute("fill-rule");
        if (fillRule.Equals("evenodd", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string style = element.GetAttribute("style");
        if (string.IsNullOrWhiteSpace(style))
        {
            return false;
        }

        foreach (string declaration in style.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pair = declaration.Split(':', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                continue;
            }

            if (pair[0].Equals("fill-rule", StringComparison.OrdinalIgnoreCase)
                && pair[1].Equals("evenodd", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryWriteSvgPathData(PdfContentStream stream, string d, float viewportHeight)
    {
        int index = 0;
        char command = '\0';
        char previousCommand = '\0';

        float currentX = 0f;
        float currentY = 0f;
        float subpathStartX = 0f;
        float subpathStartY = 0f;

        bool hasCurrentPoint = false;
        bool hasLastCubicControl = false;
        bool hasLastQuadControl = false;
        float lastCubicControlX = 0f;
        float lastCubicControlY = 0f;
        float lastQuadControlX = 0f;
        float lastQuadControlY = 0f;

        bool wroteAnySegment = false;

        while (true)
        {
            SkipSvgSeparators(d, ref index);
            if (index >= d.Length)
            {
                break;
            }

            if (IsSvgPathCommandLetter(d[index]))
            {
                command = d[index++];
            }
            else if (command == '\0')
            {
                return false;
            }

            switch (command)
            {
                case 'M':
                case 'm':
                    {
                        bool relative = command == 'm';
                        if (!TryReadSvgNumber(d, ref index, out float x)
                            || !TryReadSvgNumber(d, ref index, out float y))
                        {
                            return wroteAnySegment;
                        }

                        if (relative && hasCurrentPoint)
                        {
                            x += currentX;
                            y += currentY;
                        }

                        WritePdfMoveTo(stream, x, y, viewportHeight);
                        wroteAnySegment = true;

                        currentX = x;
                        currentY = y;
                        subpathStartX = x;
                        subpathStartY = y;
                        hasCurrentPoint = true;
                        hasLastCubicControl = false;
                        hasLastQuadControl = false;

                        while (TryReadSvgNumber(d, ref index, out float x2)
                            && TryReadSvgNumber(d, ref index, out float y2))
                        {
                            if (relative)
                            {
                                x2 += currentX;
                                y2 += currentY;
                            }

                            WritePdfLineTo(stream, x2, y2, viewportHeight);
                            wroteAnySegment = true;
                            currentX = x2;
                            currentY = y2;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'L':
                case 'l':
                    {
                        bool relative = command == 'l';
                        while (TryReadSvgNumber(d, ref index, out float x)
                            && TryReadSvgNumber(d, ref index, out float y))
                        {
                            if (relative)
                            {
                                x += currentX;
                                y += currentY;
                            }

                            WritePdfLineTo(stream, x, y, viewportHeight);
                            wroteAnySegment = true;
                            currentX = x;
                            currentY = y;
                            hasCurrentPoint = true;
                            hasLastCubicControl = false;
                            hasLastQuadControl = false;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'H':
                case 'h':
                    {
                        bool relative = command == 'h';
                        while (TryReadSvgNumber(d, ref index, out float x))
                        {
                            if (relative)
                            {
                                x += currentX;
                            }

                            WritePdfLineTo(stream, x, currentY, viewportHeight);
                            wroteAnySegment = true;
                            currentX = x;
                            hasCurrentPoint = true;
                            hasLastCubicControl = false;
                            hasLastQuadControl = false;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'V':
                case 'v':
                    {
                        bool relative = command == 'v';
                        while (TryReadSvgNumber(d, ref index, out float y))
                        {
                            if (relative)
                            {
                                y += currentY;
                            }

                            WritePdfLineTo(stream, currentX, y, viewportHeight);
                            wroteAnySegment = true;
                            currentY = y;
                            hasCurrentPoint = true;
                            hasLastCubicControl = false;
                            hasLastQuadControl = false;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'C':
                case 'c':
                    {
                        bool relative = command == 'c';
                        while (TryReadSvgNumber(d, ref index, out float x1)
                            && TryReadSvgNumber(d, ref index, out float y1)
                            && TryReadSvgNumber(d, ref index, out float x2)
                            && TryReadSvgNumber(d, ref index, out float y2)
                            && TryReadSvgNumber(d, ref index, out float x)
                            && TryReadSvgNumber(d, ref index, out float y))
                        {
                            if (relative)
                            {
                                x1 += currentX;
                                y1 += currentY;
                                x2 += currentX;
                                y2 += currentY;
                                x += currentX;
                                y += currentY;
                            }

                            WritePdfCubicTo(stream, x1, y1, x2, y2, x, y, viewportHeight);
                            wroteAnySegment = true;
                            currentX = x;
                            currentY = y;
                            lastCubicControlX = x2;
                            lastCubicControlY = y2;
                            hasCurrentPoint = true;
                            hasLastCubicControl = true;
                            hasLastQuadControl = false;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'S':
                case 's':
                    {
                        bool relative = command == 's';
                        while (TryReadSvgNumber(d, ref index, out float x2)
                            && TryReadSvgNumber(d, ref index, out float y2)
                            && TryReadSvgNumber(d, ref index, out float x)
                            && TryReadSvgNumber(d, ref index, out float y))
                        {
                            float x1;
                            float y1;
                            if (IsCubicCommand(previousCommand) && hasLastCubicControl)
                            {
                                x1 = 2f * currentX - lastCubicControlX;
                                y1 = 2f * currentY - lastCubicControlY;
                            }
                            else
                            {
                                x1 = currentX;
                                y1 = currentY;
                            }

                            if (relative)
                            {
                                x2 += currentX;
                                y2 += currentY;
                                x += currentX;
                                y += currentY;
                            }

                            WritePdfCubicTo(stream, x1, y1, x2, y2, x, y, viewportHeight);
                            wroteAnySegment = true;
                            currentX = x;
                            currentY = y;
                            lastCubicControlX = x2;
                            lastCubicControlY = y2;
                            hasCurrentPoint = true;
                            hasLastCubicControl = true;
                            hasLastQuadControl = false;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'Q':
                case 'q':
                    {
                        bool relative = command == 'q';
                        while (TryReadSvgNumber(d, ref index, out float qx)
                            && TryReadSvgNumber(d, ref index, out float qy)
                            && TryReadSvgNumber(d, ref index, out float x)
                            && TryReadSvgNumber(d, ref index, out float y))
                        {
                            if (relative)
                            {
                                qx += currentX;
                                qy += currentY;
                                x += currentX;
                                y += currentY;
                            }

                            QuadraticToCubic(currentX, currentY, qx, qy, x, y,
                                out float c1x, out float c1y, out float c2x, out float c2y);
                            WritePdfCubicTo(stream, c1x, c1y, c2x, c2y, x, y, viewportHeight);

                            wroteAnySegment = true;
                            currentX = x;
                            currentY = y;
                            lastQuadControlX = qx;
                            lastQuadControlY = qy;
                            hasCurrentPoint = true;
                            hasLastCubicControl = false;
                            hasLastQuadControl = true;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'T':
                case 't':
                    {
                        bool relative = command == 't';
                        while (TryReadSvgNumber(d, ref index, out float x)
                            && TryReadSvgNumber(d, ref index, out float y))
                        {
                            if (relative)
                            {
                                x += currentX;
                                y += currentY;
                            }

                            float qx;
                            float qy;
                            if (IsQuadraticCommand(previousCommand) && hasLastQuadControl)
                            {
                                qx = 2f * currentX - lastQuadControlX;
                                qy = 2f * currentY - lastQuadControlY;
                            }
                            else
                            {
                                qx = currentX;
                                qy = currentY;
                            }

                            QuadraticToCubic(currentX, currentY, qx, qy, x, y,
                                out float c1x, out float c1y, out float c2x, out float c2y);
                            WritePdfCubicTo(stream, c1x, c1y, c2x, c2y, x, y, viewportHeight);

                            wroteAnySegment = true;
                            currentX = x;
                            currentY = y;
                            lastQuadControlX = qx;
                            lastQuadControlY = qy;
                            hasCurrentPoint = true;
                            hasLastCubicControl = false;
                            hasLastQuadControl = true;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'A':
                case 'a':
                    {
                        bool relative = command == 'a';
                        while (TryReadSvgNumber(d, ref index, out float rx)
                            && TryReadSvgNumber(d, ref index, out float ry)
                            && TryReadSvgNumber(d, ref index, out float angle)
                            && TryReadSvgNumber(d, ref index, out float largeArc)
                            && TryReadSvgNumber(d, ref index, out float sweep)
                            && TryReadSvgNumber(d, ref index, out float x)
                            && TryReadSvgNumber(d, ref index, out float y))
                        {
                            if (relative)
                            {
                                x += currentX;
                                y += currentY;
                            }

                            bool wroteArc = WriteSvgArcAsCubics(
                                stream,
                                currentX,
                                currentY,
                                rx,
                                ry,
                                angle,
                                Math.Abs(largeArc) > 0.5f,
                                Math.Abs(sweep) > 0.5f,
                                x,
                                y,
                                viewportHeight);

                            if (wroteArc)
                            {
                                wroteAnySegment = true;
                            }

                            currentX = x;
                            currentY = y;
                            hasCurrentPoint = true;
                            hasLastCubicControl = false;
                            hasLastQuadControl = false;
                        }

                        previousCommand = command;
                        continue;
                    }

                case 'Z':
                case 'z':
                    stream.Write("h\n");
                    wroteAnySegment = true;
                    currentX = subpathStartX;
                    currentY = subpathStartY;
                    hasCurrentPoint = true;
                    hasLastCubicControl = false;
                    hasLastQuadControl = false;
                    previousCommand = command;
                    continue;

                default:
                    return wroteAnySegment;
            }
        }

        return wroteAnySegment;
    }

    private static bool IsSvgPathCommandLetter(char c)
    {
        return c switch
        {
            'M' or 'm' or 'L' or 'l' or 'H' or 'h' or 'V' or 'v' or 'C' or 'c' or 'S' or 's' or 'Q' or 'q' or 'T' or 't' or 'A' or 'a' or 'Z' or 'z' => true,
            _ => false
        };
    }

    private static void SkipSvgSeparators(string data, ref int index)
    {
        while (index < data.Length)
        {
            char ch = data[index];
            if (char.IsWhiteSpace(ch) || ch == ',')
            {
                index++;
                continue;
            }

            break;
        }
    }

    private static bool TryReadSvgNumber(string data, ref int index, out float value)
    {
        value = 0f;
        SkipSvgSeparators(data, ref index);
        if (index >= data.Length)
        {
            return false;
        }

        int start = index;

        if (data[index] == '+' || data[index] == '-')
        {
            index++;
        }

        bool hasDigits = false;
        while (index < data.Length && char.IsDigit(data[index]))
        {
            hasDigits = true;
            index++;
        }

        if (index < data.Length && data[index] == '.')
        {
            index++;
            while (index < data.Length && char.IsDigit(data[index]))
            {
                hasDigits = true;
                index++;
            }
        }

        if (!hasDigits)
        {
            index = start;
            return false;
        }

        if (index < data.Length && (data[index] == 'e' || data[index] == 'E'))
        {
            int exponentStart = index;
            index++;
            if (index < data.Length && (data[index] == '+' || data[index] == '-'))
            {
                index++;
            }

            bool hasExponentDigits = false;
            while (index < data.Length && char.IsDigit(data[index]))
            {
                hasExponentDigits = true;
                index++;
            }

            if (!hasExponentDigits)
            {
                index = exponentStart;
            }
        }

        string token = data[start..index];
        return float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void WritePdfMoveTo(PdfContentStream stream, float x, float y, float viewportHeight)
    {
        stream.Write(PdfNumber.DoubleOut(x) + " " + PdfNumber.DoubleOut(viewportHeight - y) + " m\n");
    }

    private static void WritePdfLineTo(PdfContentStream stream, float x, float y, float viewportHeight)
    {
        stream.Write(PdfNumber.DoubleOut(x) + " " + PdfNumber.DoubleOut(viewportHeight - y) + " l\n");
    }

    private static void WritePdfCubicTo(PdfContentStream stream,
        float x1,
        float y1,
        float x2,
        float y2,
        float x,
        float y,
        float viewportHeight)
    {
        stream.Write(PdfNumber.DoubleOut(x1) + " " + PdfNumber.DoubleOut(viewportHeight - y1) + " "
            + PdfNumber.DoubleOut(x2) + " " + PdfNumber.DoubleOut(viewportHeight - y2) + " "
            + PdfNumber.DoubleOut(x) + " " + PdfNumber.DoubleOut(viewportHeight - y) + " c\n");
    }

    private static void QuadraticToCubic(float x0,
        float y0,
        float qx,
        float qy,
        float x,
        float y,
        out float c1x,
        out float c1y,
        out float c2x,
        out float c2y)
    {
        c1x = x0 + (2f / 3f) * (qx - x0);
        c1y = y0 + (2f / 3f) * (qy - y0);
        c2x = x + (2f / 3f) * (qx - x);
        c2y = y + (2f / 3f) * (qy - y);
    }

    private static bool WriteSvgArcAsCubics(PdfContentStream stream,
        float x1,
        float y1,
        float rx,
        float ry,
        float xAxisRotationDegrees,
        bool largeArc,
        bool sweep,
        float x2,
        float y2,
        float viewportHeight)
    {
        if (Math.Abs(x2 - x1) < 1e-6f && Math.Abs(y2 - y1) < 1e-6f)
        {
            return false;
        }

        rx = Math.Abs(rx);
        ry = Math.Abs(ry);
        if (rx < 1e-6f || ry < 1e-6f)
        {
            WritePdfLineTo(stream, x2, y2, viewportHeight);
            return true;
        }

        double phi = xAxisRotationDegrees * Math.PI / 180.0;
        double cosPhi = Math.Cos(phi);
        double sinPhi = Math.Sin(phi);

        double dx2 = (x1 - x2) / 2.0;
        double dy2 = (y1 - y2) / 2.0;

        double x1Prime = cosPhi * dx2 + sinPhi * dy2;
        double y1Prime = -sinPhi * dx2 + cosPhi * dy2;

        double rx2 = rx * rx;
        double ry2 = ry * ry;
        double x1Prime2 = x1Prime * x1Prime;
        double y1Prime2 = y1Prime * y1Prime;

        double lambda = x1Prime2 / rx2 + y1Prime2 / ry2;
        if (lambda > 1.0)
        {
            double scale = Math.Sqrt(lambda);
            rx *= (float)scale;
            ry *= (float)scale;
            rx2 = rx * rx;
            ry2 = ry * ry;
        }

        double numerator = rx2 * ry2 - rx2 * y1Prime2 - ry2 * x1Prime2;
        double denominator = rx2 * y1Prime2 + ry2 * x1Prime2;
        if (Math.Abs(denominator) < 1e-12)
        {
            WritePdfLineTo(stream, x2, y2, viewportHeight);
            return true;
        }

        double factor = Math.Sqrt(Math.Max(0.0, numerator / denominator));
        if (largeArc == sweep)
        {
            factor = -factor;
        }

        double cxPrime = factor * (rx * y1Prime / ry);
        double cyPrime = factor * (-ry * x1Prime / rx);

        double cx = cosPhi * cxPrime - sinPhi * cyPrime + (x1 + x2) / 2.0;
        double cy = sinPhi * cxPrime + cosPhi * cyPrime + (y1 + y2) / 2.0;

        double ux = (x1Prime - cxPrime) / rx;
        double uy = (y1Prime - cyPrime) / ry;
        double vx = (-x1Prime - cxPrime) / rx;
        double vy = (-y1Prime - cyPrime) / ry;

        double startAngle = Math.Atan2(uy, ux);
        double sweepAngle = Math.Atan2(ux * vy - uy * vx, ux * vx + uy * vy);
        if (!sweep && sweepAngle > 0)
        {
            sweepAngle -= 2.0 * Math.PI;
        }
        else if (sweep && sweepAngle < 0)
        {
            sweepAngle += 2.0 * Math.PI;
        }

        int segments = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweepAngle) / (Math.PI / 2.0)));
        double delta = sweepAngle / segments;

        for (int i = 0; i < segments; i++)
        {
            double a0 = startAngle + i * delta;
            double a1 = a0 + delta;
            ArcSegmentToCubic(
                cx,
                cy,
                rx,
                ry,
                cosPhi,
                sinPhi,
                a0,
                a1,
                out float c1x,
                out float c1y,
                out float c2x,
                out float c2y,
                out float ex,
                out float ey);

            WritePdfCubicTo(stream, c1x, c1y, c2x, c2y, ex, ey, viewportHeight);
        }

        return true;
    }

    private static void ArcSegmentToCubic(double cx,
        double cy,
        double rx,
        double ry,
        double cosPhi,
        double sinPhi,
        double a0,
        double a1,
        out float c1x,
        out float c1y,
        out float c2x,
        out float c2y,
        out float ex,
        out float ey)
    {
        double t = (4.0 / 3.0) * Math.Tan((a1 - a0) / 4.0);

        double cosA0 = Math.Cos(a0);
        double sinA0 = Math.Sin(a0);
        double cosA1 = Math.Cos(a1);
        double sinA1 = Math.Sin(a1);

        double p1x = cosA0 - t * sinA0;
        double p1y = sinA0 + t * cosA0;
        double p2x = cosA1 + t * sinA1;
        double p2y = sinA1 - t * cosA1;

        MapArcPoint(cx, cy, rx, ry, cosPhi, sinPhi, p1x, p1y, out c1x, out c1y);
        MapArcPoint(cx, cy, rx, ry, cosPhi, sinPhi, p2x, p2y, out c2x, out c2y);
        MapArcPoint(cx, cy, rx, ry, cosPhi, sinPhi, cosA1, sinA1, out ex, out ey);
    }

    private static void MapArcPoint(double cx,
        double cy,
        double rx,
        double ry,
        double cosPhi,
        double sinPhi,
        double ux,
        double uy,
        out float x,
        out float y)
    {
        double ellipseX = rx * ux;
        double ellipseY = ry * uy;

        x = (float)(cx + cosPhi * ellipseX - sinPhi * ellipseY);
        y = (float)(cy + sinPhi * ellipseX + cosPhi * ellipseY);
    }

    private static bool IsCubicCommand(char command)
        => command is 'C' or 'c' or 'S' or 's';

    private static bool IsQuadraticCommand(char command)
        => command is 'Q' or 'q' or 'T' or 't';

    private static bool TryGetSvgViewport(XmlElement root, out float width, out float height)
    {
        width = 0f;
        height = 0f;

        bool hasWidth = TryParseSvgNumber(root.GetAttribute("width"), out width);
        bool hasHeight = TryParseSvgNumber(root.GetAttribute("height"), out height);
        if (hasWidth && hasHeight && width > 0f && height > 0f)
        {
            return true;
        }

        string viewBox = root.GetAttribute("viewBox");
        if (string.IsNullOrWhiteSpace(viewBox))
        {
            return false;
        }

        string[] values = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float vbWidth)
            || !float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float vbHeight)
            || vbWidth <= 0f
            || vbHeight <= 0f)
        {
            return false;
        }

        if (!hasWidth)
        {
            width = vbWidth;
        }

        if (!hasHeight)
        {
            height = vbHeight;
        }

        return width > 0f && height > 0f;
    }

    private static bool TryGetFloatAttribute(XmlElement element, string attributeName, out float value)
        => TryParseSvgNumber(element.GetAttribute(attributeName), out value);

    private static bool TryParseSvgNumber(string value, out float result)
    {
        result = 0f;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        int index = 0;
        while (index < trimmed.Length && (char.IsDigit(trimmed[index]) || trimmed[index] == '.' || trimmed[index] == '-' || trimmed[index] == '+'))
        {
            index++;
        }

        if (index == 0)
        {
            return false;
        }

        string number = trimmed[..index];
        if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
        {
            return false;
        }

        result = parsed;
        return true;
    }

    private static PdfColor? ParseSvgColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!normalized.StartsWith("#", StringComparison.Ordinal))
        {
            return null;
        }

        string hex = normalized[1..];
        if (hex.Length == 3)
        {
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
        }

        if (hex.Length != 6)
        {
            return null;
        }

        if (!int.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r)
            || !int.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g)
            || !int.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
        {
            return null;
        }

        return new PdfColor(r, g, b);
    }

    private PdfName GetOrCreateSvgFillOpacityState(float fillOpacity)
    {
        float alpha = Math.Clamp(fillOpacity, 0f, 1f);
        string cacheKey = alpha.ToString("0.###", CultureInfo.InvariantCulture);
        if (_svgFillOpacityStates.TryGetValue(cacheKey, out PdfName? existing))
        {
            return existing;
        }

        if (_pdfDoc == null || _pdfResources == null)
        {
            throw new InvalidOperationException("PDF document resources are not initialized.");
        }

        PdfName stateName = new("GSFO" + _svgFillOpacityStates.Count);
        PdfDictionary extGState = new(_pdfDoc.Doc.NextObjectId());
        extGState[PdfName.Names.Type] = new PdfName("ExtGState");
        extGState[new PdfName("ca")] = new PdfNumeric((decimal)alpha);
        extGState[new PdfName("CA")] = new PdfNumeric(1m);

        _pdfDoc.AddObject(extGState);
        _pdfResources.AddExtGState(stateName, extGState.GetReference());
        _svgFillOpacityStates[cacheKey] = stateName;

        return stateName;
    }

    private static bool TryParseSvgOpacity(string value, out float result)
    {
        result = 1f;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        bool percent = trimmed.EndsWith("%", StringComparison.Ordinal);
        string numericPart = percent ? trimmed[..^1] : trimmed;

        if (!TryParseSvgNumber(numericPart, out float parsed))
        {
            return false;
        }

        if (percent)
        {
            parsed /= 100f;
        }

        result = Math.Clamp(parsed, 0f, 1f);
        return true;
    }

    private readonly record struct SvgStyle(PdfColor? Fill, PdfColor? Stroke, float StrokeWidth, float FillOpacity)
    {
        public static SvgStyle Default => new(new PdfColor(0, 0, 0), null, 1f, 1f);
    }

    /**
    * Render inline area to PDF
    *
    * @param area inline area to Render
    */

    public void RenderWordArea(WordArea area)
    {
        // TODO: I don't understand why we are locking the private member
        // _wordAreaPDF.  Maybe this string buffer was originally static? (MG)
        lock (_wordAreaPDF)
        {
            StringBuilder pdf = _wordAreaPDF;
            pdf.Length = 0;

            GdiKerningPairs? kerning = null;
            bool kerningAvailable = false;

            // If no options are supplied, by default we do not enable kerning
            if (_options != null && _options.Kerning)
            {
                kerning = area.FontState.Kerning;
                if (kerning != null && (kerning.Count > 0))
                {
                    kerningAvailable = true;
                }
            }

            String name = area.FontState.FontName;
            int size = area.FontState.FontSize;

            // This assumes that *all* CIDFonts use a /ToUnicode mapping
            Font font = (Font)area.FontState.FontInfo.GetFontByName(name);
            bool useMultiByte = font.MultiByteFont;

            string startText = useMultiByte ? "<" : "(";
            string endText = useMultiByte ? "> " : ") ";

            if ((!name.Equals(this._currentFontName)) || (size != this.currentFontSize))
            {
                CloseText();

                this._currentFontName = name;
                this.currentFontSize = size;
                pdf = pdf.Append("/" + name + " " +
                    PdfNumber.DoubleOut(size / 1000f) + " Tf\n");
            }

            // Do letter spacing (must be outside of [...] TJ]
            float letterspacing = ((float)area.FontState.LetterSpacing) / 1000f;
            if (letterspacing != this.currentLetterSpacing)
            {
                this.currentLetterSpacing = letterspacing;
                CloseText();
                pdf.Append(PdfNumber.DoubleOut(letterspacing));
                pdf.Append(" Tc\n");
            }

            float wordspacing = ((float)area.FontState.WordSpacing) / 1000f;
            if (wordspacing != this.currentWordSpacing)
            {
                this.currentWordSpacing = wordspacing;
                CloseText();
                pdf.Append(PdfNumber.DoubleOut(wordspacing));
                pdf.Append(" Tw\n");
            }

            PdfColor? areaColor = this._currentFill;

            if (areaColor == null || areaColor.Red != (double)area.Red || areaColor.Green != (double)area.Green || areaColor.Blue != (double)area.Blue)
            {
                areaColor = new PdfColor((double)area.Red, (double)area.Green, (double)area.Blue);

                CloseText();
                this._currentFill = areaColor;
                pdf.Append(this._currentFill.getColorSpaceOut(true));
            }

            int rx = this._currentXPosition;
            int bl = this._currentYPosition;

            AddWordLines(area, rx, bl, size, areaColor);

            if (!textOpen || bl != prevWordY)
            {
                CloseText();

                pdf.Append("1 0 0 1 " + PdfNumber.DoubleOut(rx / 1000f) +
                    " " + PdfNumber.DoubleOut(bl / 1000f) + " Tm [" + startText);
                prevWordY = bl;
                textOpen = true;
            }
            else
            {
                // express the space between words in thousandths of an em
                int space = prevWordX - rx + prevWordWidth;
                float emDiff = (float)space / (float)currentFontSize * 1000f;

                // this prevents a problem in Acrobat Reader where large
                // numbers cause text to disappear or default to a limit
                if (emDiff < -33000)
                {
                    CloseText();

                    pdf.Append("1 0 0 1 " + PdfNumber.DoubleOut(rx / 1000f) +
                        " " + PdfNumber.DoubleOut(bl / 1000f) + " Tm [" + startText);
                    textOpen = true;
                }
                else
                {
                    pdf.Append(PdfNumber.DoubleOut(emDiff));
                    pdf.Append(" ");
                    pdf.Append(startText);
                }
            }

            prevWordWidth = area.getContentWidth();
            prevWordX = rx;

            string s;
            if (area.getPageNumberID() != null)
            {
                // This text is a page number, so resolve it
                s = idReferences.SetPageNumber(area.getPageNumberID());
                if (s == null)
                {
                    s = String.Empty;
                }
            }
            else
            {
                s = area.getText();
            }

            int wordLength = s.Length;
            for (int index = 0; index < wordLength; index++)
            {
                ushort ch = area.FontState.MapCharacter(s[index]);

                if (!useMultiByte)
                {
                    if (ch > 127)
                    {
                        pdf.Append("\\");
                        pdf.Append(Convert.ToString((int)ch, 8));

                    }
                    else
                    {
                        switch (ch)
                        {
                            case '(':
                            case ')':
                            case '\\':
                                pdf.Append("\\");
                                break;
                        }

                        pdf.Append((char)ch);
                    }
                }
                else
                {
                    pdf.Append(PdfRenderer.GetUnicodeString(ch));
                }

                if (kerningAvailable && (index + 1) < wordLength)
                {
                    ushort ch2 = area.FontState.MapCharacter(s[index + 1]);
                    PdfRenderer.AddKerning(pdf, ch, ch2, kerning, startText, endText);
                }

            }

            pdf.Append(endText);

            _currentStream.Write(pdf.ToString());

            this._currentXPosition += area.getContentWidth();

        }
    }

    /// <summary>
    /// Convert a char to a multibyte hex representation.
    /// </summary>
    private static string GetUnicodeString(ushort c)
    {
        StringBuilder sb = new(4);

        byte[] uniBytes = Encoding.BigEndianUnicode.GetBytes([(char)c]);

        foreach (byte b in uniBytes)
        {
            string hexString = Convert.ToString(b, 16);
            if (hexString.Length == 1)
            {
                sb.Append("0");
            }

            sb.Append(hexString);
        }

        return sb.ToString();

    }

    /**
    * Checks to see if we have some text rendering commands open
    * still and writes out the TJ command to the stream if we do
    */

    private void CloseText()
    {
        if (textOpen)
        {
            _currentStream.Write("] TJ\n");
            textOpen = false;
            prevWordX = 0;
            prevWordY = 0;
        }
    }

    private static void AddKerning(StringBuilder buf, ushort leftIndex, ushort rightIndex,
                            GdiKerningPairs kerning, string startText, string endText)
    {
        if (kerning.HasPair(leftIndex, rightIndex))
        {
            int width = kerning[leftIndex, rightIndex];
            buf.Append(endText).Append(-width).Append(' ').Append(startText);
        }
    }

    public void Render(Page page)
    {
        this.idReferences = page.getIDReferences();
        this._pdfResources = _pdfDoc.getResources();
        this._pdfDoc.SetIdReferences(idReferences);
        this.RenderPage(page);
        this._pdfDoc.output();
    }

    /// <summary>
    ///  Render page into PDF.
    /// </summary>
    /// <param name="page">Page to render.</param>
    /// <exception cref="InvalidOperationException">Thrown if the PDF document is not initialized.</exception>
    public void RenderPage(Page page)
    {
        BodyAreaContainer body;
        AreaContainer before, after, start, end;

        if (_pdfDoc == null)
        {
            throw new InvalidOperationException("PDF document is not initialized.");
        }

        _currentStream = _pdfDoc.MakeContentStream();
        body = page.getBody();
        before = page.getBefore();
        after = page.getAfter();
        start = page.getStart();
        end = page.getEnd();

        _currentFontName = "";
        this.currentFontSize = 0;
        this.currentLetterSpacing = Single.NaN;
        this.currentWordSpacing = Single.NaN;
        this.currentWordSpacing = Single.NaN;

        _currentStream.Write("BT\n");

        // Paint start (watermark) first so overflowing overlays stay behind body content.
        if (start != null)
        {
            RenderAreaContainer(start);
        }

        RenderBodyAreaContainer(body);

        if (before != null)
        {
            RenderAreaContainer(before);
        }

        if (after != null)
        {
            RenderAreaContainer(after);
        }

        if (end != null)
        {
            RenderAreaContainer(end);
        }
        CloseText();

        // Bug fix for issue 1823
        this.currentLetterSpacing = Single.NaN;
        this.currentWordSpacing = Single.NaN;

        float w = page.getWidth();
        float h = page.GetHeight();
        _currentStream.Write("ET\n");

        _currentPage = this._pdfDoc.MakePage(
            this._pdfResources, _currentStream,
            Convert.ToInt32(Math.Round(w / 1000)),
            Convert.ToInt32(Math.Round(h / 1000)), page);

        if (page.hasLinks() || _currentAnnotList != null)
        {
            _currentAnnotList ??= _pdfDoc.MakeAnnotList();

            _currentPage.SetAnnotList(_currentAnnotList);

            ArrayList lsets = page.getLinkSets();
            foreach (LinkSet linkSet in lsets)
            {
                linkSet.align();
                String dest = linkSet.getDest();
                int linkType = linkSet.getLinkType();
                ArrayList rsets = linkSet.getRects();

                foreach (LinkedRectangle lrect in rsets)
                {
                    _currentAnnotList.Add(_pdfDoc.makeLink(lrect.getRectangle(), dest, linkType).GetReference());
                }
            }

            _currentAnnotList = null;
        }
        else
        {
            // just to be on the safe side
            _currentAnnotList = null;
        }

        // ensures that color is properly reset for blocks that carry over pages
        this._currentFill = null;
    }

    /**
    * defines a string containing dashArray and dashPhase for the rule style
    */

    private static string SetRuleStylePattern(int style)
    {
        return style switch
        {
            RuleStyle.SOLID => "[] 0 d ",
            RuleStyle.DASHED => "[3 3] 0 d ",
            RuleStyle.DOTTED => "[1 3] 0 d ",
            RuleStyle.DOUBLE => "[] 0 d ",
            _ => "[] 0 d ",
        };
    }

    private void DoFrame(Area area)
    {
        int w, h;
        int rx = this._currentAreaContainerXPosition;
        w = area.getContentWidth();
        if (area is BlockArea)
        {
            rx += ((BlockArea)area).getStartIndent();
        }

        h = area.getContentHeight();
        int ry = this._currentYPosition;

        rx = rx - area.getPaddingLeft();
        ry = ry + area.getPaddingTop();
        w = w + area.getPaddingLeft() + area.getPaddingRight();
        h = h + area.getPaddingTop() + area.getPaddingBottom();

        DoBackground(area, rx, ry, w, h);

        BorderAndPadding bp = area.GetBorderAndPadding();

        int left = area.getBorderLeftWidth();
        int right = area.getBorderRightWidth();
        int top = area.getBorderTopWidth();
        int bottom = area.getBorderBottomWidth();

        // If style is solid, use filled rectangles
        if (top != 0)
        {
            AddFilledRect(rx, ry, w, top, new PdfColor(bp.GetBorderColor(BorderAndPadding.TOP)));
        }

        if (left != 0)
        {
            AddFilledRect(rx - left, ry - h - bottom, left, h + top + bottom, new PdfColor(bp.GetBorderColor(BorderAndPadding.LEFT)));
        }

        if (right != 0)
        {
            AddFilledRect(rx + w, ry - h - bottom, right, h + top + bottom, new PdfColor(bp.GetBorderColor(BorderAndPadding.RIGHT)));
        }

        if (bottom != 0)
        {
            AddFilledRect(rx, ry - h - bottom, w, bottom, new PdfColor(bp.GetBorderColor(BorderAndPadding.BOTTOM)));
        }
    }

    /// <summary>
    /// Renders an area's background.
    /// </summary>
    /// <param name="area">The area whose background is to be rendered.</param>
    /// <param name="x">The x position of the left edge in _millipoints.</param>
    /// <param name="y">The y position of top edge in _millipoints.</param>
    /// <param name="w">The width in _millipoints.</param>
    /// <param name="h">The height in _millipoints.</param>
    private void DoBackground(Area area, int x, int y, int w, int h)
    {
        if (h == 0 || w == 0)
        {
            return;
        }

        BackgroundProps? props = area.Background;
        if (props == null)
        {
            return;
        }

        if (props.Color.Alpha == 0)
        {
            AddFilledRect(x, y, w, -h, new PdfColor(props.Color));
        }

        if (props.backImage != null)
        {
            int imgW = props.backImage.Width * 1000;
            int imgH = props.backImage.Height * 1000;

            int dx = x;
            int dy = y;
            int endX = x + w;
            int endY = y - h;
            int clipW = w % imgW;
            int clipH = h % imgH;

            bool repeatX = true;
            bool repeatY = true;

            switch (props.backRepeat)
            {
                case BackgroundRepeat.REPEAT:
                    break;
                case BackgroundRepeat.REPEAT_X:
                    repeatY = false;
                    break;
                case BackgroundRepeat.REPEAT_Y:
                    repeatX = false;
                    break;
                case BackgroundRepeat.NO_REPEAT:
                    repeatX = false;
                    repeatY = false;
                    break;
                case BackgroundRepeat.INHERIT:
                    break;
                default:
                    FonetDriver.ActiveDriver?.FireFonetWarning("Ignoring invalid background-repeat property");
                    break;
            }

            // Looping through rows
            while (dy > endY)
            {
                // Looping through cols
                while (dx < endX)
                {
                    if (dx + imgW <= endX)
                    {
                        // no x clipping
                        if (dy - imgH >= endY)
                        {
                            // no x clipping, no y clipping
                            DrawImageScaled(dx, dy, imgW, imgH, props.backImage);
                        }
                        else
                        {
                            // no x clipping, y clipping
                            DrawImageClipped(dx, dy, 0, 0, imgW, clipH, props.backImage);
                        }
                    }
                    else
                    {
                        // x clipping
                        if (dy - imgH >= endY)
                        {
                            // x clipping, no y clipping 
                            DrawImageClipped(dx, dy, 0, 0, clipW, imgH, props.backImage);
                        }
                        else
                        {
                            // x clipping, y clipping
                            DrawImageClipped(dx, dy, 0, 0, clipW, clipH, props.backImage);
                        }
                    }

                    if (repeatX)
                    {
                        dx += imgW;
                    }
                    else
                    {
                        break;
                    }
                }

                dx = x;

                if (repeatY)
                {
                    dy -= imgH;
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Renders an image, rendered at the image's intrinsic size.
    /// This by default calls drawImageScaled() with the image's
    /// intrinsic width and height, but implementations may
    /// override this method if it can provide a more efficient solution.
    /// </summary>
    /// <param name="x">The x position of left edge in _millipoints.</param>
    /// <param name="y">The y position of top edge in _millipoints.</param>
    /// <param name="image">The image to be rendered.</param>
    private void DrawImage(int x, int y, FonetImage image)
    {
        int w = image.Width * 1000;
        int h = image.Height * 1000;
        DrawImageScaled(x, y, w, h, image);
    }

    /// <summary>
    /// Renders an image, scaling it to the given width and height.
    /// If the scaled width and height is the same intrinsic size
    /// of the image, the image is not scaled.
    /// </summary>
    /// <param name="x">The x position of left edge in _millipoints.</param>
    /// <param name="y">The y position of top edge in _millipoints.</param>
    /// <param name="w">The width in _millipoints.</param>
    /// <param name="h">The height in _millipoints.</param>
    /// <param name="image">The image to be rendered.</param>
    private void DrawImageScaled(int x, int y, int w, int h, FonetImage image)
    {
        PdfXObject xobj = _pdfDoc.AddImage(image);
        CloseText();

        _currentStream.Write("ET\nq\n" + PdfNumber.DoubleOut(((float)w) / 1000f) + " 0 0 "
            + PdfNumber.DoubleOut(((float)h) / 1000f) + " "
            + PdfNumber.DoubleOut(((float)x) / 1000f) + " "
            + PdfNumber.DoubleOut(((float)(y - h)) / 1000f) + " cm\n" + "/" + xobj.Name.Name
            + " Do\nQ\nBT\n");
    }

    /// <summary>
    /// Renders an image, clipping it as specified.
    /// </summary>
    /// <param name="x">The x position of left edge in _millipoints.</param>
    /// <param name="y">The y position of top edge in _millipoints.</param>
    /// <param name="clipX">The left edge of the clip in _millipoints.</param>
    /// <param name="clipY">The top edge of the clip in _millipoints.</param>
    /// <param name="clipW">The clip width in _millipoints.</param>
    /// <param name="clipH">The clip height in _millipoints.</param>
    /// <param name="image">The image to be rendered.</param>
    private void DrawImageClipped(int x, int y, int clipX, int clipY, int clipW, int clipH, FonetImage image)
    {
        float cx1 = ((float)x) / 1000f;
        float cy1 = ((float)y - clipH) / 1000f;

        float cx2 = ((float)x + clipW) / 1000f;
        float cy2 = ((float)y) / 1000f;

        int imgX = x - clipX;
        int imgY = y - clipY;

        int imgW = image.Width * 1000;
        int imgH = image.Height * 1000;

        PdfXObject xobj = this._pdfDoc.AddImage(image);
        CloseText();

        _currentStream.Write("ET\nq\n" +
            // clipping
            PdfNumber.DoubleOut(cx1) + " " + PdfNumber.DoubleOut(cy1) + " m\n" +
            PdfNumber.DoubleOut(cx2) + " " + PdfNumber.DoubleOut(cy1) + " l\n" +
            PdfNumber.DoubleOut(cx2) + " " + PdfNumber.DoubleOut(cy2) + " l\n" +
            PdfNumber.DoubleOut(cx1) + " " + PdfNumber.DoubleOut(cy2) + " l\n" +
            "W\n" +
            "n\n" +
            // image matrix
            PdfNumber.DoubleOut(((float)imgW) / 1000f) + " 0 0 " +
            PdfNumber.DoubleOut(((float)imgH) / 1000f) + " " +
            PdfNumber.DoubleOut(((float)imgX) / 1000f) + " " +
            PdfNumber.DoubleOut(((float)imgY - imgH) / 1000f) + " cm\n" +
            "s\n" +
            // the image itself
            "/" + xobj.Name.Name + " Do\nQ\nBT\n");
    }


    /**
     * Render display space
     *
     * @param space the display space to Render
     */

    public void RenderDisplaySpace(DisplaySpace space)
    {
        int d = space.getSize();
        this._currentYPosition -= d;
    }

    private void AddWordLines(WordArea area, int rx, int bl, int size, PdfColor theAreaColor)
    {
        if (area.getUnderlined())
        {
            int yPos = bl - size / 10;
            AddLine(rx, yPos, rx + area.getContentWidth(), yPos, size / 14, theAreaColor);

            // Save position for underlining a following InlineSpace
            prevUnderlineXEndPos = rx + area.getContentWidth();
            prevUnderlineYEndPos = yPos;
            prevUnderlineSize = size / 14;
            _prevUnderlineColor = theAreaColor;
        }

        if (area.getOverlined())
        {
            int yPos = bl + area.FontState.Ascender + size / 10;
            AddLine(rx, yPos, rx + area.getContentWidth(), yPos, size / 14, theAreaColor);
            prevOverlineXEndPos = rx + area.getContentWidth();
            prevOverlineYEndPos = yPos;
            prevOverlineSize = size / 14;
            prevOverlineColor = theAreaColor;
        }

        if (area.getLineThrough())
        {
            int yPos = bl + area.FontState.Ascender * 3 / 8;
            AddLine(rx, yPos, rx + area.getContentWidth(), yPos, size / 14, theAreaColor);
            prevLineThroughXEndPos = rx + area.getContentWidth();
            prevLineThroughYEndPos = yPos;
            prevLineThroughSize = size / 14;
            prevLineThroughColor = theAreaColor;
        }
    }

    /**
     * Render inline space
     *
     * @param space space to Render
     */

    public void RenderInlineSpace(InlineSpace space)
    {
        this._currentXPosition += space.getSize();
        if (space.getUnderlined())
        {
            if (_prevUnderlineColor != null)
            {
                AddLine(prevUnderlineXEndPos, prevUnderlineYEndPos,
                        prevUnderlineXEndPos + space.getSize(),
                        prevUnderlineYEndPos, prevUnderlineSize,
                        _prevUnderlineColor);

                // Save position for a following InlineSpace
                prevUnderlineXEndPos = prevUnderlineXEndPos + space.getSize();
            }
        }

        if (space.getOverlined())
        {
            if (prevOverlineColor != null)
            {
                AddLine(prevOverlineXEndPos, prevOverlineYEndPos,
                        prevOverlineXEndPos + space.getSize(),
                        prevOverlineYEndPos, prevOverlineSize,
                        prevOverlineColor);
                prevOverlineXEndPos = prevOverlineXEndPos + space.getSize();
            }
        }

        if (space.getLineThrough())
        {
            if (prevLineThroughColor != null)
            {
                AddLine(prevLineThroughXEndPos, prevLineThroughYEndPos,
                        prevLineThroughXEndPos + space.getSize(),
                        prevLineThroughYEndPos, prevLineThroughSize,
                        prevLineThroughColor);
                prevLineThroughXEndPos = prevLineThroughXEndPos + space.getSize();
            }
        }
    }

    /**
     * Render leader area
     *
     * @param area area to Render
     */

    public void RenderLeaderArea(LeaderArea area)
    {
        int rx = this._currentXPosition;
        int ry = this._currentYPosition;
        int w = area.getContentWidth();
        int h = area.GetHeight();
        int th = area.getRuleThickness();
        int st = area.getRuleStyle();

        // checks whether thickness is = 0, because of bug in pdf (or where?),
        // a line with thickness 0 is still displayed
        if (th != 0)
        {
            switch (st)
            {
                case RuleStyle.DOUBLE:
                    AddLine(rx, ry, rx + w, ry, th / 3, st, new PdfColor(area.Red, area.Green, area.Blue));
                    AddLine(rx, ry + (2 * th / 3), rx + w, ry + (2 * th / 3), th / 3, st, new PdfColor(area.Red, area.Green, area.Blue));
                    break;
                case RuleStyle.GROOVE:
                    AddLine(rx, ry, rx + w, ry, th / 2, st, new PdfColor(area.Red, area.Green, area.Blue));
                    AddLine(rx, ry + (th / 2), rx + w, ry + (th / 2), th / 2, st, new PdfColor(255, 255, 255));
                    break;
                case RuleStyle.RIDGE:
                    AddLine(rx, ry, rx + w, ry, th / 2, st, new PdfColor(255, 255, 255));
                    AddLine(rx, ry + (th / 2), rx + w, ry + (th / 2), th / 2, st, new PdfColor(area.Red, area.Green, area.Blue));
                    break;
                default:
                    AddLine(rx, ry, rx + w, ry, th, st, new PdfColor(area.Red, area.Green, area.Blue));
                    break;
            }

            _currentXPosition += area.getContentWidth();
            _currentYPosition += th;
        }
    }
}