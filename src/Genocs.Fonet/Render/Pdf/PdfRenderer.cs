using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Properties;
using Genocs.Fonet.Image;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Layout.Inline;
using Genocs.Fonet.Pdf;
using Genocs.Fonet.Pdf.Gdi;
using Genocs.Fonet.Render.Pdf.Fonts;
using System.Collections;
using System.Text;

namespace Genocs.Fonet.Render.Pdf;

internal sealed class PdfRenderer
{
    /// <summary>
    /// The current vertical position in _millipoints from bottom.
    /// </summary>
    private int currentYPosition = 0;

    /// <summary>
    /// The current horizontal position in _millipoints from left.
    /// </summary>
    private int currentXPosition = 0;

    /// <summary>
    /// The horizontal position of the current area container.
    /// </summary>
    private int currentAreaContainerXPosition = 0;

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
    private PdfAnnotList? currentAnnotList;

    /// <summary>
    /// The current page to Add annotations to.
    /// </summary>
    private PdfPage? currentPage;

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
    ///     User specified rendering options.
    /// </summary>
    private PdfRendererOptions? _options;

    /// <summary>
    /// The current (internal) font name.
    /// </summary>
    private string? _currentFontName;

    /// <summary>
    /// The current font size in _millipoints.
    /// </summary>
    private int currentFontSize;

    /// <summary>
    /// The current color/gradient to fill shapes with.
    /// </summary>
    private PdfColor? currentFill = null;

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
    private PdfColor prevUnderlineColor;

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
    private FontInfo fontInfo;

    /// <summary>
    /// Handles adding base 14 and all system fonts.
    /// </summary>
    private FontSetup fontSetup;

    /// <summary>
    /// The IDReferences for this document.
    /// </summary>
    private IDReferences? idReferences;

    /// <summary>
    /// Create the PDF renderer.
    /// </summary>
    internal PdfRenderer(Stream stream)
    {
        _pdfDoc = new PdfCreator(stream);
    }

    /// <summary>
    ///     Assigns renderer options to this PdfRenderer
    /// </summary>
    /// <remarks>
    ///     This property will only accept an instance of the PdfRendererOptions class
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     If <i>value</i> is not an instance of PdfRendererOptions
    /// </exception>
    public PdfRendererOptions Options
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!(value is PdfRendererOptions))
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
        fontSetup.AddToResources(new PdfFontCreator(_pdfDoc), _pdfDoc.getResources());
        _pdfDoc.outputTrailer();

        _pdfDoc = null;
        _pdfResources = null;
        _currentStream = null;
        currentAnnotList = null;
        currentPage = null;

        idReferences = null;
        _currentFontName = String.Empty;
        currentFill = null;
        prevUnderlineColor = null;
        prevOverlineColor = null;
        prevLineThroughColor = null;
        fontSetup = null;
        fontInfo = null;
    }

    /// <summary>
    /// </summary>
    /// <param name="fontInfo"></param>
    public void SetupFontInfo(FontInfo fontInfo)
    {
        this.fontInfo = fontInfo;
        this.fontSetup = new FontSetup(
            fontInfo, (_options == null) ? FontType.Link : _options.FontType);
    }

    public void RenderSpanArea(SpanArea area)
    {
        foreach (Box b in area.getChildren())
        {
            b.Render(this); // column areas
        }

    }

    public void RenderBodyAreaContainer(BodyAreaContainer area)
    {
        int saveY = this.currentYPosition;
        int saveX = this.currentAreaContainerXPosition;

        if (area.getPosition() == Position.ABSOLUTE)
        {
            // Y position is computed assuming positive Y axis, adjust for negative postscript one
            this.currentYPosition = area.GetYPosition();
            this.currentAreaContainerXPosition = area.getXPosition();
        }
        else if (area.getPosition() == Position.RELATIVE)
        {
            this.currentYPosition -= area.GetYPosition();
            this.currentAreaContainerXPosition += area.getXPosition();
        }

        this.currentXPosition = this.currentAreaContainerXPosition;
        int rx = this.currentAreaContainerXPosition;
        int ry = this.currentYPosition;

        int w = area.getAllocationWidth();
        int h = area.getMaxHeight();

        DoBackground(area, rx, ry, w, h);

        // floats & footnotes stuff
        RenderAreaContainer(area.getBeforeFloatReferenceArea());
        RenderAreaContainer(area.getFootnoteReferenceArea());

        // main reference area
        foreach (Box b in area.getMainReferenceArea().getChildren())
        {
            b.Render(this); // span areas
        }

        if (area.getPosition() != Position.STATIC)
        {
            this.currentYPosition = saveY;
            this.currentAreaContainerXPosition = saveX;
        }
        else
        {
            this.currentYPosition -= area.GetHeight();
        }

    }

    public void RenderAreaContainer(AreaContainer area)
    {
        int saveY = this.currentYPosition;
        int saveX = this.currentAreaContainerXPosition;

        if (area.getPosition() == Position.ABSOLUTE)
        {
            // XPosition and YPosition give the content rectangle position
            this.currentYPosition = area.GetYPosition();
            this.currentAreaContainerXPosition = area.getXPosition();
        }
        else if (area.getPosition() == Position.RELATIVE)
        {
            this.currentYPosition -= area.GetYPosition();
            this.currentAreaContainerXPosition += area.getXPosition();
        }
        else if (area.getPosition() == Position.STATIC)
        {
            this.currentYPosition -= area.getPaddingTop()
                + area.getBorderTopWidth();
        }

        this.currentXPosition = this.currentAreaContainerXPosition;
        DoFrame(area);

        foreach (Box b in area.getChildren())
        {
            b.Render(this);
        }

        // Restore previous origin
        this.currentYPosition = saveY;
        this.currentAreaContainerXPosition = saveX;
        if (area.getPosition() == Position.STATIC)
        {
            this.currentYPosition -= area.GetHeight();
        }
    }

    public void RenderBlockArea(BlockArea area)
    {
        // KLease: Temporary test to fix block positioning
        // Offset ypos by padding and border widths
        this.currentYPosition -= (area.getPaddingTop()
            + area.getBorderTopWidth());
        DoFrame(area);
        foreach (Box b in area.getChildren())
        {
            b.Render(this);
        }
        this.currentYPosition -= (area.getPaddingBottom()
            + area.getBorderBottomWidth());
    }

    public void RenderLineArea(LineArea area)
    {
        int rx = this.currentAreaContainerXPosition + area.getStartIndent();
        int ry = this.currentYPosition;
        int w = area.getContentWidth();
        int h = area.GetHeight();

        this.currentYPosition -= area.getPlacementOffset();
        this.currentXPosition = rx;

        int bl = this.currentYPosition;

        foreach (Box b in area.getChildren())
        {
            if (b is InlineArea)
            {
                InlineArea ia = (InlineArea)b;
                this.currentYPosition = ry - ia.getYOffset();
            }
            else
            {
                this.currentYPosition = ry - area.getPlacementOffset();
            }
            b.Render(this);
        }

        this.currentYPosition = ry - h;
        this.currentXPosition = rx;
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

    private void AddLine(int x1, int y1, int x2, int y2, int th,
                         PdfColor stroke)
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

    private void AddLine(int x1, int y1, int x2, int y2, int th, int rs,
                         PdfColor stroke)
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

    private void AddRect(int x, int y, int w, int h, PdfColor stroke,
                         PdfColor fill)
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

    private void AddFilledRect(int x, int y, int w, int h,
                               PdfColor fill)
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
        int x = this.currentXPosition + area.getXOffset();
        int y = this.currentYPosition;
        int w = area.getContentWidth();
        int h = area.GetHeight();

        this.currentYPosition -= h;

        FonetImage img = area.getImage();

        PdfXObject xobj = this._pdfDoc.AddImage(img);
        CloseText();

        _currentStream.Write("ET\nq\n" + PdfNumber.DoubleOut(((float)w) / 1000f) + " 0 0 "
            + PdfNumber.DoubleOut(((float)h) / 1000f) + " "
            + PdfNumber.DoubleOut(((float)x) / 1000f) + " "
            + PdfNumber.DoubleOut(((float)(y - h)) / 1000f) + " cm\n" + "/" + xobj.Name.Name
            + " Do\nQ\nBT\n");

        this.currentXPosition += area.getContentWidth();
    }

    /**
    * Render a foreign object area
    */

    public void RenderForeignObjectArea(ForeignObjectArea area)
    {
        // if necessary need to scale and align the content
        this.currentXPosition = this.currentXPosition + area.getXOffset();
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

        area.getObject().Render(this);
        _currentStream.Write("Q\n");
        _currentStream.Write("BT\n");
        this.currentXPosition += area.getEffectiveWidth();
        // this.currentYPosition -= area.getEffectiveHeight();
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

            PdfColor? areaColor = this.currentFill;

            if (areaColor == null || areaColor.Red != (double)area.getRed()
                || areaColor.Green != (double)area.getGreen()
                || areaColor.Blue != (double)area.getBlue())
            {
                areaColor = new PdfColor((double)area.getRed(),
                                         (double)area.getGreen(),
                                         (double)area.getBlue());


                CloseText();
                this.currentFill = areaColor;
                pdf.Append(this.currentFill.getColorSpaceOut(true));
            }


            int rx = this.currentXPosition;
            int bl = this.currentYPosition;

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
                s = idReferences.getPageNumber(area.getPageNumberID());
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

            this.currentXPosition += area.getContentWidth();

        }
    }

    /// <summary>
    /// Convert a char to a multibyte hex representation
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
        this._pdfResources = this._pdfDoc.getResources();
        this._pdfDoc.SetIdReferences(idReferences);
        this.RenderPage(page);
        this._pdfDoc.output();
    }

    /// <summary>
    ///  Render page into PDF
    /// </summary>
    /// <param name="page">Page to render</param>
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

        RenderBodyAreaContainer(body);

        if (before != null)
        {
            RenderAreaContainer(before);
        }

        if (after != null)
        {
            RenderAreaContainer(after);
        }

        if (start != null)
        {
            RenderAreaContainer(start);
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

        currentPage = this._pdfDoc.makePage(
            this._pdfResources, _currentStream,
            Convert.ToInt32(Math.Round(w / 1000)),
            Convert.ToInt32(Math.Round(h / 1000)), page);

        if (page.hasLinks() || currentAnnotList != null)
        {
            currentAnnotList ??= _pdfDoc.MakeAnnotList();

            currentPage.SetAnnotList(currentAnnotList);

            ArrayList lsets = page.getLinkSets();
            foreach (LinkSet linkSet in lsets)
            {
                linkSet.align();
                String dest = linkSet.getDest();
                int linkType = linkSet.getLinkType();
                ArrayList rsets = linkSet.getRects();

                foreach (LinkedRectangle lrect in rsets)
                {
                    currentAnnotList.Add(_pdfDoc.makeLink(lrect.getRectangle(), dest, linkType).GetReference());
                }
            }
            currentAnnotList = null;
        }
        else
        {
            // just to be on the safe side
            currentAnnotList = null;
        }

        // ensures that color is properly reset for blocks that carry over pages
        this.currentFill = null;
    }

    /**
    * defines a string containing dashArray and dashPhase for the rule style
    */

    private string SetRuleStylePattern(int style)
    {
        string rs;
        rs = style switch
        {
            RuleStyle.SOLID => "[] 0 d ",
            RuleStyle.DASHED => "[3 3] 0 d ",
            RuleStyle.DOTTED => "[1 3] 0 d ",
            RuleStyle.DOUBLE => "[] 0 d ",
            _ => "[] 0 d ",
        };
        return rs;
    }

    private void DoFrame(Area area)
    {
        int w, h;
        int rx = this.currentAreaContainerXPosition;
        w = area.getContentWidth();
        if (area is BlockArea)
        {
            rx += ((BlockArea)area).getStartIndent();
        }
        h = area.getContentHeight();
        int ry = this.currentYPosition;

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
            AddFilledRect(rx, ry, w, top,
                          new PdfColor(bp.GetBorderColor(BorderAndPadding.TOP)));
        }
        if (left != 0)
        {
            AddFilledRect(rx - left, ry - h - bottom, left, h + top + bottom,
                          new PdfColor(bp.GetBorderColor(BorderAndPadding.LEFT)));
        }
        if (right != 0)
        {
            AddFilledRect(rx + w, ry - h - bottom, right, h + top + bottom,
                          new PdfColor(bp.GetBorderColor(BorderAndPadding.RIGHT)));
        }
        if (bottom != 0)
        {
            AddFilledRect(rx, ry - h - bottom, w, bottom,
                          new PdfColor(bp.GetBorderColor(BorderAndPadding.BOTTOM)));
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

        BackgroundProps props = area.getBackground();
        if (props == null)
        {
            return;
        }

        if (props.backColor.Alpha == 0)
        {
            AddFilledRect(x, y, w, -h, new PdfColor(props.backColor));
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
                    FonetDriver.ActiveDriver.FireFonetWarning("Ignoring invalid background-repeat property");
                    break;
            }

            while (dy > endY)
            { // looping through rows 
                while (dx < endX)
                { // looping through cols 
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
                } // end looping through cols

                dx = x;

                if (repeatY)
                {
                    dy -= imgH;
                }
                else
                {
                    break;
                }
            } // end looping through rows 
        }
    }

    /// <summary>
    ///     Renders an image, rendered at the image's intrinsic size.
    ///     This by default calls drawImageScaled() with the image's
    ///     intrinsic width and height, but implementations may
    ///     override this method if it can provide a more efficient solution.
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
    ///     Renders an image, scaling it to the given width and height.
    ///     If the scaled width and height is the same intrinsic size 
    ///     of the image, the image is not scaled
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
    ///     Renders an image, clipping it as specified.
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
        this.currentYPosition -= d;
    }

    private void AddWordLines(WordArea area, int rx, int bl, int size,
                              PdfColor theAreaColor)
    {
        if (area.getUnderlined())
        {
            int yPos = bl - size / 10;
            AddLine(rx, yPos, rx + area.getContentWidth(), yPos, size / 14,
                    theAreaColor);
            // save position for underlining a following InlineSpace
            prevUnderlineXEndPos = rx + area.getContentWidth();
            prevUnderlineYEndPos = yPos;
            prevUnderlineSize = size / 14;
            prevUnderlineColor = theAreaColor;
        }

        if (area.getOverlined())
        {
            int yPos = bl + area.FontState.Ascender + size / 10;
            AddLine(rx, yPos, rx + area.getContentWidth(), yPos, size / 14,
                    theAreaColor);
            prevOverlineXEndPos = rx + area.getContentWidth();
            prevOverlineYEndPos = yPos;
            prevOverlineSize = size / 14;
            prevOverlineColor = theAreaColor;
        }

        if (area.getLineThrough())
        {
            int yPos = bl + area.FontState.Ascender * 3 / 8;
            AddLine(rx, yPos, rx + area.getContentWidth(), yPos, size / 14,
                    theAreaColor);
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
        this.currentXPosition += space.getSize();
        if (space.getUnderlined())
        {
            if (prevUnderlineColor != null)
            {
                AddLine(prevUnderlineXEndPos, prevUnderlineYEndPos,
                        prevUnderlineXEndPos + space.getSize(),
                        prevUnderlineYEndPos, prevUnderlineSize,
                        prevUnderlineColor);
                // save position for a following InlineSpace
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
        int rx = this.currentXPosition;
        int ry = this.currentYPosition;
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
                    AddLine(rx, ry, rx + w, ry, th / 3, st,
                            new PdfColor(area.getRed(), area.getGreen(),
                                         area.getBlue()));
                    AddLine(rx, ry + (2 * th / 3), rx + w, ry + (2 * th / 3),
                            th / 3, st,
                            new PdfColor(area.getRed(), area.getGreen(),
                                         area.getBlue()));
                    break;
                case RuleStyle.GROOVE:
                    AddLine(rx, ry, rx + w, ry, th / 2, st,
                            new PdfColor(area.getRed(), area.getGreen(),
                                         area.getBlue()));
                    AddLine(rx, ry + (th / 2), rx + w, ry + (th / 2), th / 2, st,
                            new PdfColor(255, 255, 255));
                    break;
                case RuleStyle.RIDGE:
                    AddLine(rx, ry, rx + w, ry, th / 2, st,
                            new PdfColor(255, 255, 255));
                    AddLine(rx, ry + (th / 2), rx + w, ry + (th / 2), th / 2, st,
                            new PdfColor(area.getRed(), area.getGreen(),
                                         area.getBlue()));
                    break;
                default:
                    AddLine(rx, ry, rx + w, ry, th, st,
                            new PdfColor(area.getRed(), area.getGreen(),
                                         area.getBlue()));
                    break;
            }
            this.currentXPosition += area.getContentWidth();
            this.currentYPosition += th;
        }
    }

}