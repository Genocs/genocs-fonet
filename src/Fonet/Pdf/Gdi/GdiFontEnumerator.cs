using System.Collections;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Cross-platform font enumerator using SkiaSharp's FontManager.
/// </summary>
public class GdiFontEnumerator
{
    /// <summary>
    /// Returns a list of font family names sorted in ascending order.
    /// </summary>
    public string[] FamilyNames
    {
        get
        {
            var fontManager = FontManager.Instance;
            var allFamilies = fontManager.GetFontFamilies();
            return allFamilies;
        }
    }

    /// <summary>
    /// Returns a list of font styles associated with <i>familyName</i>.
    /// </summary>
    /// <param name="familyName">The font family name</param>
    /// <returns>Available font styles</returns>
    public FontStyles GetStyles(string familyName)
    {
        var fontManager = FontManager.Instance;
        return fontManager.GetFontStyles(familyName);
    }
}

public class FontStyles
{
    private IDictionary styles = new Hashtable();

    public bool RegularAvailable
    {
        get { return (styles.Contains("Regular") || styles.Contains("Normal")); }
    }

    public bool BoldAvailable
    {
        get { return (styles.Contains("Bold")); }
    }

    public bool ItalicAvailable
    {
        get { return (styles.Contains("Italic")); }
    }

    public bool BoldItalicAvailable
    {
        get { return (styles.Contains("Bold Italic")); }
    }

    internal void AddStyle(string styleName)
    {
        if (!styles.Contains(styleName))
        {
            styles.Add(styleName, String.Empty);
        }
    }

    internal void Clear()
    {
        styles.Clear();
    }

    internal bool Contains(string styleName)
    {
        return styles.Contains(styleName);
    }
}
