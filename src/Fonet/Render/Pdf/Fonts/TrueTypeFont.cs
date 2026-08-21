using Genocs.Fonet.Layout;
using Genocs.Fonet.Pdf;
using Genocs.Fonet.Pdf.Gdi;

namespace Genocs.Fonet.Render.Pdf.Fonts;

/// <summary>
/// Represents a TrueType font program.
/// </summary>
internal class TrueTypeFont : Font, IFontDescriptor
{
    public const string WinAnsiEncoding = "WinAnsiEncoding";

    private readonly CodePointMapping? mapping = CodePointMapping.GetMapping(WinAnsiEncoding);

    /// <summary>
    /// Wrapper around a Win32 HDC.
    /// </summary>
    private GdiDeviceContent? _deviceContext;

    /// <summary>
    /// Provides font metrics using the Win32 Api.
    /// </summary>
    private GdiFontMetrics? _metrics;

    /// <summary>
    /// List of kerning pairs.
    /// </summary>
    private GdiKerningPairs kerning;

    /// <summary>
    /// Maps a glyph index to a PDF width
    /// </summary>
    private int[] widths;

    /// <summary>
    /// 
    /// </summary>
    protected FontProperties _properties;

    /// <summary>
    /// Class constructor
    /// </summary>
    /// <param name="properties"></param>
    public TrueTypeFont(FontProperties properties)
    {
        _properties = properties;
        ObtainFontMetrics();
    }

    /// <summary>
    /// Creates a <see cref="GdiFontMetrics"/> object from <b>baseFontName</b>
    /// </summary>
    private void ObtainFontMetrics()
    {
        _deviceContext = new GdiDeviceContent();
        GdiFont font = GdiFont.CreateDesignFont(_properties.FaceName, _properties.IsBold, _properties.IsItalic, _deviceContext);
        _metrics = font.GetMetrics(_deviceContext);
    }

    public PdfArray Array
    {
        get
        {
            PdfArray widthsArray = new PdfArray();
            widthsArray.AddArray(Widths);

            return widthsArray;
        }
    }

    #region Implementation of Font members

    /// <summary>
    /// Returns <see cref="PdfFontSubTypeEnum.TrueType"/>.
    /// </summary>
    public override PdfFontSubTypeEnum SubType
    {
        get { return PdfFontSubTypeEnum.TrueType; }
    }

    public override string FontName
    {
        get
        {
            // See section 5.5.2 "TrueType fonts" for more details
            if (_properties.IsBoldItalic)
            {
                return String.Format("{0},BoldItalic", _properties.FaceName);
            }
            else if (_properties.IsBold)
            {
                return String.Format("{0},Bold", _properties.FaceName);
            }
            else if (_properties.IsItalic)
            {
                return String.Format("{0},Italic", _properties.FaceName);
            }
            else
            {
                return _properties.FaceName;
            }
        }
    }

    public override PdfFontTypeEnum Type
    {
        get { return PdfFontTypeEnum.TrueType; }
    }

    public override string Encoding
    {
        get { return WinAnsiEncoding; }
    }

    public override IFontDescriptor Descriptor
    {
        get { return this; }
    }

    public override bool MultiByteFont
    {
        get { return false; }
    }

    public override ushort MapCharacter(char c)
    {
        // TrueType fonts only support the Basic and Extended Latin blocks
        if (c > byte.MaxValue)
        {
            return (ushort)FirstChar;
        }

        return mapping?.MapCharacter(c) ?? (ushort)FirstChar;
    }

    public override int Ascender
    {
        get { return _metrics.Ascent; }
    }

    public override int Descender
    {
        get { return _metrics.Descent; }
    }

    public override int CapHeight
    {
        get { return _metrics.CapHeight; }
    }

    public override int FirstChar
    {
        get { return 0; }
    }

    public override int LastChar
    {
        get
        {
            // Only support Latin1 character set
            return 255;
        }
    }

    /// <summary>
    /// See <see cref="Font.GetWidth(ushort)"/>
    /// </summary>
    /// <param name="charIndex">A WinAnsi codepoint.</param>
    /// <returns></returns>
    public override int GetWidth(ushort charIndex)
    {
        EnsureWidthsArray();

        // The widths array is keyed on WinAnsiEncoding codepoint
        return widths[charIndex];
    }

    public override int[] Widths
    {
        get
        {
            EnsureWidthsArray();
            return widths;
        }
    }

    #endregion

    private void EnsureWidthsArray()
    {
        widths ??= _metrics.GetAnsiWidths();
    }

    #region Implementation of IFontDescriptior interface

    public int Flags
    {
        get { return _metrics.Flags; }
    }

    public int[] FontBBox
    {
        get { return _metrics.BoundingBox; }
    }

    public int ItalicAngle
    {
        get { return _metrics.ItalicAngle; }
    }

    public int StemV
    {
        get { return _metrics.StemV; }
    }

    public bool HasKerningInfo
    {
        get
        {
            kerning ??= _metrics.AnsiKerningPairs;
            return (kerning.Count != 0);
        }
    }

    public bool IsEmbeddable
    {
        get { return false; }
    }

    public bool IsSubsettable
    {
        get { return false; }
    }

    public byte[] FontData
    {
        get { return _metrics.GetFontData(); }
    }

    public GdiKerningPairs KerningInfo
    {
        get
        {
            kerning ??= _metrics.AnsiKerningPairs;
            return kerning;
        }
    }

    #endregion
}