using Genocs.Fonet.Pdf.Gdi.Font;
using Genocs.Fonet.Pdf.Gdi.Font.Tables;
using System.Collections.Specialized;
using System.Text;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
///     Class that obtains OutlineTextMetrics for a TrueType font
/// </summary>
/// <example>
/// </example>
public class GdiFontMetrics
{
    public const long GDI_ERROR = 0xFFFFFFFFL;

    private FontFileReader reader;
    private GdiDeviceContent dc;
    private GdiFont currentFont;
    private PdfUnitConverter converter;
    private GdiUnicodeRanges ranges;

    private string faceName;

    private HeaderTable head;
    private PostTable post;
    private HorizontalHeaderTable hhea;
    private HorizontalMetricsTable hmtx;
    private OS2Table os2;
    private KerningTable kern;

    private byte[] data;

    internal GdiFontMetrics(GdiDeviceContent dc, GdiFont currentFont)
    {
        if (dc.Handle == IntPtr.Zero)
        {
            throw new ArgumentNullException("dc", "Handle to device context cannot be null");
        }
        if (dc.GetCurrentObject(GdiDcObject.Font) == IntPtr.Zero)
        {
            throw new ArgumentException("dc", "No font selected into supplied device context");
        }
        this.dc = dc;
        this.currentFont = currentFont;

        // FontFileReader requires the font facename because the font may exist in 
        // a TrueType collection.
        StringBuilder builder = new StringBuilder(255);
        LibWrapper.GetTextFace(dc.Handle, builder.Capacity, builder);
        faceName = builder.Length == 0 ? currentFont.FaceName : builder.ToString();

        ranges = new GdiUnicodeRanges(dc);
        reader = new FontFileReader(new MemoryStream(GetFontData()), faceName);
        converter = new PdfUnitConverter(EmSquare);

        // After we have cached the font data, we can safely delete the resource
        //currentFont.Dispose();
    }

    /// <summary>
    ///     Retrieves the typeface name of the font that is selected into the 
    ///     device context supplied to the GdiFontMetrics constructor. 
    /// </summary>
    public string FaceName
    {
        get { return faceName; }
    }

    /// <summary>
    ///     Specifies the number of logical units defining the x- or y-dimension 
    ///     of the em square for this font.  The common value for EmSquare is 2048.
    /// </summary>
    /// <remarks>
    ///     The number of units in the x- and y-directions are always the same 
    ///     for an em square.) 
    /// </remarks>
    public int EmSquare
    {
        get
        {
            EnsureHeadTable();
            return (int)head.unitsPermEm;
        }
    }

    /// <summary>
    ///     Gets the main italic angle of the font expressed in tenths of 
    ///     a degree counterclockwise from the vertical.
    /// </summary>
    /// <remarks>
    ///     Regular (roman) fonts have a value of zero. Italic fonts typically 
    ///     have a negative italic angle (that is, they lean to the right). 
    /// </remarks>
    public int ItalicAngle
    {
        get
        {
            EnsurePostTable();
            // TODO: Is the italic angle always a whole number?
            return converter.ToPdfUnits((int)post.ItalicAngle);
        }
    }

    /// <summary>
    ///     Specifies the maximum distance _characters in this font extend 
    ///     above the base line. This is the typographic ascent for the font. 
    /// </summary>
    public int Ascent
    {
        get
        {
            EnsureHheaTable();
            return converter.ToPdfUnits(hhea.ascender);
        }
    }

    /// <summary>
    ///     Specifies the maximum distance _characters in this font extend 
    ///     below the base line. This is the typographic descent for the font. 
    /// </summary>
    public int Descent
    {
        get
        {
            EnsureHheaTable();
            return converter.ToPdfUnits(hhea.decender);
        }
    }

    /// <summary>
    ///     Gets the distance between the baseline and the approximate 
    ///     height of uppercase letters.
    /// </summary>
    public int CapHeight
    {
        get
        {
            EnsureOS2Table();
            return converter.ToPdfUnits(os2.CapHeight);
        }
    }

    /// <summary>
    ///     Gets the distance between the baseline and the approximate 
    ///     height of non-ascending lowercase letters.
    /// </summary>
    public int XHeight
    {
        get
        {
            EnsureOS2Table();
            return converter.ToPdfUnits(os2.XHeight);
        }
    }

    /// <summary>
    ///     TODO: The thickness, measured horizontally, of the dominant vertical 
    ///     stems of the glyphs in the font.
    /// </summary>
    public int StemV
    {
        get
        {
            EnsureOS2Table();
            if (os2.AvgCharWidth > 0)
            {
                return converter.ToPdfUnits(Math.Max(50, (int)(os2.AvgCharWidth * 0.7)));
            }

            if (os2.CapHeight > 0)
            {
                return converter.ToPdfUnits(Math.Max(50, os2.CapHeight / 10));
            }

            return converter.ToPdfUnits(80);
        }
    }

    /// <summary>
    ///     Gets the value of the first character defined in the font
    /// </summary>
    public ushort FirstChar
    {
        get
        {
            EnsureOS2Table();
            return os2.FirstChar;
        }
    }

    /// <summary>
    ///     Gets the value of the last character defined in the font
    /// </summary>
    public ushort LastChar
    {
        get
        {
            EnsureOS2Table();
            return os2.LastChar;
        }
    }

    /// <summary>
    ///     Gets the average width of glyphs in a font.
    /// </summary>
    public int AverageWidth
    {
        get
        {
            EnsureOS2Table();
            return converter.ToPdfUnits(os2.AvgCharWidth);
        }
    }

    /// <summary>
    ///     Gets the maximum width of glyphs in a font.
    /// </summary>
    public int MaxWidth
    {
        get
        {
            EnsureHmtxTable();
            int maxAdvance = 0;
            for (int i = 0; i < hmtx.Count; i++)
            {
                maxAdvance = Math.Max(maxAdvance, hmtx[i].AdvanceWidth);
            }

            return converter.ToPdfUnits(maxAdvance);
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the font can be legally embedded 
    ///     within a document.
    /// </summary>
    public bool IsEmbeddable
    {
        get
        {
            EnsureOS2Table();
            return os2.IsEmbeddable;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the font can be legally subsetted.
    /// </summary>
    public bool IsSubsettable
    {
        get
        {
            EnsureOS2Table();
            return os2.IsSubsettable;
        }
    }

    /// <summary>
    ///     Gets the font's bounding box.
    /// </summary>
    /// <remarks>
    ///     This is the smallest rectangle enclosing the shape that would 
    ///     result if all the glyphs of the font were placed with their 
    ///     origins cooincident and then filled.
    /// </remarks>
    public int[] BoundingBox
    {
        get
        {
            EnsureHeadTable();
            return new int[] {
                converter.ToPdfUnits(head.xMin),
                converter.ToPdfUnits(head.yMin),
                converter.ToPdfUnits(head.xMax),
                converter.ToPdfUnits(head.yMax)
            };
        }
    }

    /// <summary>
    ///     Gets a collection of flags defining various characteristics of 
    ///     a font (e.g. serif or sans-serif, symbolic, etc).
    /// </summary>
    public int Flags
    {
        get
        {
            EnsureOS2Table();

            BitVector32 flags = new BitVector32(0);
            flags[1] = os2.IsMonospaced;
            flags[2] = os2.IsSerif;
            flags[8] = os2.IsScript;
            flags[64] = os2.IsItalic;

            // Symbolic and NonSymbolic are mutually exclusive
            if (os2.IsSymbolic)
            {
                flags[4] = true;
            }
            else
            {
                flags[32] = true;
            }

            if (flags.Data == 0)
            {
                // Nonsymbolic is a good default
                return 32;
            }
            else
            {
                return flags.Data;
            }
        }
    }

    /// <summary>
    ///     Gets font metric data for a TrueType font or TrueType collection.
    /// </summary>
    /// <returns></returns>
    public byte[] GetFontData()
    {
        if (data == null)
        {
            try
            {
                byte[] fontBytes = FontManager.Instance.GetFontData(dc.CurrentTypeface);
                if (FontTableAccess.IsTrueTypeCollection(fontBytes))
                {
                    data = ReadFontFromCollection();
                }
                else
                {
                    data = ReadFont();
                }

            }
            catch (Exception e)
            {
                throw new Exception(
                    String.Format("Failed to load data for font {0}", FaceName), e);
            }
        }

        return data;
    }

    private byte[] ReadFontFromCollection()
    {
        GdiFontCreator creator = new GdiFontCreator(dc);
        return creator.Build();
    }

    private byte[] ReadFont()
    {
        var typeface = dc.CurrentTypeface ?? currentFont.Typeface;
        if (typeface == null)
        {
            throw new InvalidOperationException("No font selected into device context");
        }

        byte[] fontData = FontManager.Instance.GetFontData(typeface);
        if (fontData.Length > 0)
        {
            return fontData;
        }

        throw new InvalidOperationException($"Unable to read font data for {FaceName}");
    }

    /// <summary>
    ///     Gets a collection of kerning pairs.
    /// </summary>
    /// <returns></returns>
    public GdiKerningPairs KerningPairs
    {
        get
        {
            if (reader.ContainsTable(TableNames.Kern))
            {
                kern = (KerningTable)reader.GetTable(TableNames.Kern);
                return new GdiKerningPairs(kern.KerningPairs, converter);
            }
            else
            {
                return GdiKerningPairs.Empty;
            }
        }
    }

    /// <summary>
    ///     Gets a collection of kerning pairs for _characters defined in 
    ///     the WinAnsiEncoding scheme only.
    /// </summary>
    /// <returns></returns>
    public GdiKerningPairs AnsiKerningPairs
    {
        get
        {
            if (reader.ContainsTable(TableNames.Kern))
            {
                kern = (KerningTable)reader.GetTable(TableNames.Kern);

                // The kerning pairs obtained from the TrueType font are keyed 
                // on glyph index, whereas the ansi kerning pairs should be keyed 
                // on codepoint value from the WinAnsiEncoding scheme.
                KerningPairs oldPairs = kern.KerningPairs;
                KerningPairs newPairs = new KerningPairs(oldPairs.Length);

                WinAnsiMapping mapping = WinAnsiMapping.Mapping;
                var codepointsByGlyph = new Dictionary<ushort, List<ushort>>();
                for (int i = 0; i < 256; i++)
                {
                    ushort glyph = ranges.MapCharacter((char)i);
                    ushort codepoint = mapping.MapCharacter((char)i);
                    if (!codepointsByGlyph.TryGetValue(glyph, out var list))
                    {
                        list = new List<ushort>();
                        codepointsByGlyph[glyph] = list;
                    }

                    list.Add(codepoint);
                }

                oldPairs.ForEachPair((leftGlyph, rightGlyph, amount) =>
                {
                    if (!codepointsByGlyph.TryGetValue(leftGlyph, out var leftCodepoints) ||
                        !codepointsByGlyph.TryGetValue(rightGlyph, out var rightCodepoints))
                    {
                        return;
                    }

                    foreach (ushort leftCp in leftCodepoints)
                    {
                        foreach (ushort rightCp in rightCodepoints)
                        {
                            newPairs.Add(leftCp, rightCp, amount);
                        }
                    }
                });
                return new GdiKerningPairs(newPairs, converter);

            }
            else
            {
                return GdiKerningPairs.Empty;
            }
        }
    }

    /// <summary>
    ///     Retrieves the widths, in PDF units, of consecutive glyphs.
    /// </summary>
    /// <returns>
    ///     An array of integers whose size is equal to the number of glyphs 
    ///     specified in the 'maxp' table.
    ///     The width at location 0 is the width of glyph with index 0, 
    ///     The width at location 1 is the width of glyph with index 1, 
    ///     etc...
    /// </returns>
    public int[] GetWidths()
    {
        EnsureHmtxTable();

        int[] widths = new int[hmtx.Count];

        // Convert each width to PDF units
        for (int i = 0; i < hmtx.Count; i++)
        {
            widths[i] = converter.ToPdfUnits(hmtx[i].AdvanceWidth);
        }

        return widths;
    }

    /// <summary>
    ///     Returns the width, in PDF units, of consecutive glyphs for the 
    ///     WinAnsiEncoding only.
    /// </summary>
    /// <returns>An array consisting of 256 elements.</returns>
    public int[] GetAnsiWidths()
    {
        EnsureHmtxTable();

        // WinAnsiEncoding consists of 256 _characters
        int[] widths = new int[256];

        // The glyph at position 0 always represents the .notdef glyph
        int missingWidth = converter.ToPdfUnits(hmtx[0].AdvanceWidth);
        for (int c = 0; c < 256; c++)
        {
            widths[c] = missingWidth;
        }

        // Convert a unicode character to a code point value in the 
        // WinAnsiEncoding scheme
        WinAnsiMapping mapping = WinAnsiMapping.Mapping;

        for (int c = 0; c < 256; c++)
        {
            ushort glyphIndex = MapCharacter((char)c);
            ushort codepoint = mapping.MapCharacter((char)c);
            widths[codepoint] = converter.ToPdfUnits(hmtx[glyphIndex].AdvanceWidth);
        }

        return widths;
    }

    /// <summary>
    ///     Translates the supplied character to a glyph index using the 
    ///     currently selected font.
    /// </summary>
    /// <param name="c">A unicode character.</param>
    /// <returns></returns>
    public ushort MapCharacter(char c)
    {
        return ranges.MapCharacter(c);
    }

    private void EnsureHmtxTable()
    {
        if (hmtx == null)
        {
            hmtx = (HorizontalMetricsTable)GetTable(TableNames.Hmtx);
        }
    }

    private void EnsureHheaTable()
    {
        if (hhea == null)
        {
            hhea = (HorizontalHeaderTable)GetTable(TableNames.Hhea);
        }
    }

    private void EnsurePostTable()
    {
        if (post == null)
        {
            post = (PostTable)GetTable(TableNames.Post);
        }
    }

    private void EnsureHeadTable()
    {
        if (head == null)
        {
            head = (HeaderTable)GetTable(TableNames.Head);
        }
    }

    private void EnsureOS2Table()
    {
        if (os2 == null)
        {
            os2 = (OS2Table)GetTable(TableNames.Os2);
        }
    }

    private FontTable GetTable(string name)
    {
        try
        {
            return reader.GetTable(name);
        }
        catch
        {
            throw new Exception(String.Format(
                "Unable to retrieve table {0} from font {1}", name, FaceName));
        }
    }
}
