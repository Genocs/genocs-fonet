namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Class that represents a unicode character range backed by cmap coverage.
/// </summary>
internal class UnicodeRange
{
    private readonly GdiDeviceContent _deviceContext;
    public ushort Start { get; }
    public ushort End { get; }

    public UnicodeRange(GdiDeviceContent dc, ushort start, ushort end)
    {
        if (start > end)
        {
            throw new ArgumentException("start cannot be greater than end");
        }

        _deviceContext = dc;
        Start = start;
        End = end;
    }

    public ushort MapCharacter(char c)
    {
        if (_deviceContext.CurrentTypeface == null)
        {
            return 0;
        }

        return FontManager.Instance.GetGlyphIndex(_deviceContext.CurrentTypeface, c);
    }
}
