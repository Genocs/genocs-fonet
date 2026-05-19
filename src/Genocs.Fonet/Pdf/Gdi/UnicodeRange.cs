namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
///     Class that represents a unicode character range backed by cmap coverage.
/// </summary>
internal class UnicodeRange
{
    private readonly GdiDeviceContent dc;
    private readonly ushort start;
    private readonly ushort end;

    public UnicodeRange(GdiDeviceContent dc, ushort start, ushort end)
    {
        if (start > end)
        {
            throw new ArgumentException("start cannot be greater than end");
        }

        this.dc = dc;
        this.start = start;
        this.end = end;
    }

    public ushort MapCharacter(char c)
    {
        if (dc.CurrentTypeface == null)
        {
            return 0;
        }

        return FontManager.Instance.GetGlyphIndex(dc.CurrentTypeface, c);
    }

    public ushort Start => start;

    public ushort End => end;
}
