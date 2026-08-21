using System.Collections;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Custom collection that maintains a list of Unicode ranges 
/// a font supports and the glyph indices of each character.
/// Cross-platform implementation using cmap table parsing.
/// </summary>
public class GdiUnicodeRanges
{
    private static readonly IComparer SearchComparer = new UnicodeRangeComparer();

    private UnicodeRange[] unicodeRanges = [];

    public GdiUnicodeRanges(GdiDeviceContent dc)
    {
        LoadRanges(dc);
    }

    public int Count => unicodeRanges.Length;

    private void LoadRanges(GdiDeviceContent dc)
    {
        try
        {
            var typeface = dc.CurrentTypeface;
            if (typeface == null)
            {
                unicodeRanges = [new UnicodeRange(dc, 0x0020, 0x007E)];
                return;
            }

            var cmap = FontManager.Instance.GetCmapReader(typeface);
            if (cmap != null && cmap.Ranges.Count > 0)
            {
                unicodeRanges = cmap.Ranges
                    .Select(range => new UnicodeRange(dc, range.Start, range.End))
                    .ToArray();
                return;
            }

            unicodeRanges = [new UnicodeRange(dc, 0x0020, 0x007E)];
        }
        catch (Exception ex)
        {
            FonetDriver.ActiveDriver?.FireFonetWarning($"Unable to retrieve unicode ranges for font; using Latin-1 fallback: {ex.Message}");
            unicodeRanges = [new UnicodeRange(dc, 0x0020, 0x007E)];
        }
    }

    internal UnicodeRange? GetRange(char c)
    {
        int index = Array.BinarySearch(
            unicodeRanges, 0, unicodeRanges.Length, c, SearchComparer);

        return index < 0 ? null : unicodeRanges[index];
    }

    public ushort MapCharacter(char c)
    {
        UnicodeRange? range = GetRange(c);
        return range == null ? (ushort)0 : range.MapCharacter(c);
    }
}
