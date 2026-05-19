using Genocs.Fonet.Pdf.Gdi;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Cross-platform font manager using SkiaSharp for handling font operations
/// without dependency on Windows GDI.
/// </summary>
internal sealed class FontManager
{
    private static readonly Lazy<FontManager> instance = new(() => new FontManager());
    private readonly SKFontManager fontManager;
    private readonly ConcurrentDictionary<string, SKTypeface> typefaces = new();
    private readonly ConcurrentDictionary<SKTypeface, string> typefaceFilePaths = new();
    private readonly ConcurrentDictionary<string, List<SKTypeface>> privateTypefaces = new();
    private readonly ConcurrentDictionary<string, string> systemFontPathCache = new();
    private readonly ConcurrentDictionary<string, CmapReader> cmapCache = new();
    private string[]? cachedFontFamilies;
    private readonly object fontFamiliesLock = new();

    public static FontManager Instance => instance.Value;
    private FontManager()
    {
        fontManager = SKFontManager.Default;
    }

    /// <summary>
    /// Gets all available font family names on the system.
    /// </summary>
    public string[] GetFontFamilies()
    {
        if (cachedFontFamilies != null)
        {
            return cachedFontFamilies;
        }

        lock (fontFamiliesLock)
        {
            if (cachedFontFamilies != null)
            {
                return cachedFontFamilies;
            }

            var families = new SortedSet<string>();
            foreach (var family in fontManager.FontFamilies)
            {
                if (!string.IsNullOrEmpty(family))
                {
                    families.Add(family);
                }
            }

            foreach (var family in privateTypefaces.Keys)
            {
                families.Add(family);
            }

            cachedFontFamilies = families.ToArray();
            return cachedFontFamilies;
        }
    }

    /// <summary>
    /// Gets available styles (Regular, Bold, Italic, Bold Italic) for a font family.
    /// </summary>
    public FontStyles GetFontStyles(string familyName)
    {
        var styles = new FontStyles();
        foreach (var privateTypeface in GetPrivateTypefaces(familyName))
        {
            AddStyle(styles, privateTypeface);
        }

        var typeface = fontManager.MatchFamily(familyName);

        if (typeface != null)
        {
            // SkiaSharp provides styles through typeface weight and slant
            // Test for available styles by attempting to create typefaces
            if (fontManager.MatchFamily(familyName, new SKFontStyle(400, 5, SKFontStyleSlant.Upright)) != null)
            {
                styles.AddStyle("Regular");
            }

            if (fontManager.MatchFamily(familyName, new SKFontStyle(700, 5, SKFontStyleSlant.Upright)) != null)
            {
                styles.AddStyle("Bold");
            }

            if (fontManager.MatchFamily(familyName, new SKFontStyle(400, 5, SKFontStyleSlant.Italic)) != null)
            {
                styles.AddStyle("Italic");
            }

            if (fontManager.MatchFamily(familyName, new SKFontStyle(700, 5, SKFontStyleSlant.Italic)) != null)
            {
                styles.AddStyle("Bold Italic");
            }

            // If no styles were detected, assume at least Regular is available
            var hasAnyStyle = false;
            if (styles.RegularAvailable || styles.BoldAvailable || styles.ItalicAvailable || styles.BoldItalicAvailable)
            {
                hasAnyStyle = true;
            }

            if (!hasAnyStyle)
            {
                styles.AddStyle("Regular");
            }
        }

        return styles;
    }

    /// <summary>
    /// Loads a font typeface by family name and style.
    /// </summary>
    public SKTypeface LoadTypeface(string familyName, bool bold = false, bool italic = false)
    {
        var weight = bold ? 700 : 400;
        var slant = italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        var fontStyle = new SKFontStyle(weight, 5, slant);

        var cacheKey = $"{familyName}_{weight}_{slant}";
        if (typefaces.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var typeface = MatchPrivateTypeface(familyName, bold, italic)
            ?? fontManager.MatchFamily(familyName, fontStyle)
            ?? fontManager.MatchFamily(familyName);
        
        if (typeface != null)
        {
            typefaces[cacheKey] = typeface;
            if (!typefaceFilePaths.ContainsKey(typeface))
            {
                var systemPath = LocateSystemFont(familyName);
                if (!string.IsNullOrEmpty(systemPath))
                {
                    typefaceFilePaths[typeface] = systemPath;
                }
            }
        }

        return typeface;
    }

    /// <summary>
    /// Loads a font from a file path.
    /// </summary>
    public SKTypeface LoadTypefaceFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Font file not found: {filePath}");
        }

        var cacheKey = filePath.ToLowerInvariant();
        if (typefaces.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var typeface = SKTypeface.FromFile(filePath);
        if (typeface != null)
        {
            typefaces[cacheKey] = typeface;
            RememberTypefacePath(typeface, filePath);
        }

        return typeface;
    }

    private void RememberTypefacePath(SKTypeface typeface, string filePath)
    {
        typefaceFilePaths[typeface] = filePath;
        cmapCache.TryRemove(filePath, out _);
    }

    /// <summary>
    ///     Registers a private font file so it can be resolved by family name.
    /// </summary>
    public void RegisterTypefaceFromFile(string filePath)
    {
        var typeface = LoadTypefaceFromFile(filePath);
        if (typeface == null || string.IsNullOrEmpty(typeface.FamilyName))
        {
            throw new ArgumentException($"Unable to load font file: {filePath}", nameof(filePath));
        }

        if (!privateTypefaces.TryGetValue(typeface.FamilyName, out var familyTypefaces))
        {
            familyTypefaces = [];
            privateTypefaces[typeface.FamilyName] = familyTypefaces;
        }

        lock (familyTypefaces)
        {
            if (!familyTypefaces.Contains(typeface))
            {
                familyTypefaces.Add(typeface);
            }
        }

        InvalidateFontFamilyCache();
    }

    private void InvalidateFontFamilyCache()
    {
        lock (fontFamiliesLock)
        {
            cachedFontFamilies = null;
        }
    }

    /// <summary>
    /// Extracts font data (TrueType/OpenType tables) from a typeface.
    /// </summary>
    public byte[] GetFontData(SKTypeface typeface)
    {
        ArgumentNullException.ThrowIfNull(typeface);

        if (TryReadFontFile(typeface, out var fontData))
        {
            return fontData;
        }

        return Array.Empty<byte>();
    }

    /// <summary>
    ///     Reads raw font bytes from a registered file path or system font directory.
    ///     Avoids SKTypeface.OpenStream(), which can crash on some platform/font combinations.
    /// </summary>
    private bool TryReadFontFile(SKTypeface typeface, out byte[] fontData)
    {
        if (typefaceFilePaths.TryGetValue(typeface, out var filePath) && File.Exists(filePath))
        {
            fontData = File.ReadAllBytes(filePath);
            return fontData.Length > 0;
        }

        foreach (var familyTypefaces in privateTypefaces.Values)
        {
            foreach (var privateTypeface in familyTypefaces)
            {
                if (!ReferenceEquals(privateTypeface, typeface))
                {
                    continue;
                }

                if (typefaceFilePaths.TryGetValue(privateTypeface, out filePath) && File.Exists(filePath))
                {
                    fontData = File.ReadAllBytes(filePath);
                    return fontData.Length > 0;
                }
            }
        }

        var systemPath = LocateSystemFont(typeface.FamilyName);
        if (!string.IsNullOrEmpty(systemPath) && File.Exists(systemPath))
        {
            typefaceFilePaths[typeface] = systemPath;
            fontData = File.ReadAllBytes(systemPath);
            return fontData.Length > 0;
        }

        fontData = Array.Empty<byte>();
        return false;
    }

    /// <summary>
    ///     Gets glyph indices for a string of characters.
    /// </summary>
    public ushort[] GetGlyphIndices(string text, SKTypeface typeface)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<ushort>();
        }

        var cmap = GetCmapReader(typeface);
        if (cmap != null)
        {
            var indices = new ushort[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                indices[i] = cmap.MapCharacter(text[i]);
            }

            return indices;
        }

        return GetGlyphIndicesFromSkia(text, typeface);
    }

    public ushort GetGlyphIndex(SKTypeface typeface, int codePoint)
    {
        var cmap = GetCmapReader(typeface);
        if (cmap != null)
        {
            return cmap.MapCharacter(codePoint);
        }

        using var font = new SKFont(typeface);
        var glyphs = new ushort[1];
        font.GetGlyphs(((char)codePoint).ToString(), glyphs);
        return glyphs[0];
    }

    public CmapReader? GetCmapReader(SKTypeface typeface)
    {
        ArgumentNullException.ThrowIfNull(typeface);

        if (TryGetTypefacePath(typeface, out var filePath))
        {
            if (cmapCache.TryGetValue(filePath, out var cached))
            {
                return cached;
            }

            var fontData = File.ReadAllBytes(filePath);
            var cmap = CmapReader.FromFontData(fontData);
            if (cmap != null)
            {
                cmapCache[filePath] = cmap;
            }

            return cmap;
        }

        var data = GetFontData(typeface);
        return data.Length == 0 ? null : CmapReader.FromFontData(data);
    }

    private ushort[] GetGlyphIndicesFromSkia(string text, SKTypeface typeface)
    {
        using var font = new SKFont(typeface);
        var indices = new ushort[text.Length];
        font.GetGlyphs(text, indices);
        return indices;
    }

    private bool TryGetTypefacePath(SKTypeface typeface, out string filePath)
    {
        if (typefaceFilePaths.TryGetValue(typeface, out filePath!))
        {
            return true;
        }

        var located = LocateSystemFont(typeface.FamilyName);
        if (!string.IsNullOrEmpty(located))
        {
            typefaceFilePaths[typeface] = located;
            filePath = located;
            return true;
        }

        filePath = string.Empty;
        return false;
    }


    /// <summary>
    ///     Locates a system font file by family name (cross-platform).
    /// </summary>
    private string? LocateSystemFont(string familyName)
    {
        var cacheKey = familyName.ToLowerInvariant();
        if (systemFontPathCache.TryGetValue(cacheKey, out var cachedPath))
        {
            return cachedPath;
        }

        var fontPaths = GetSystemFontDirectories();
        var normalizedFamily = NormalizeFamilyName(familyName);

        foreach (var basePath in fontPaths)
        {
            if (!Directory.Exists(basePath))
            {
                continue;
            }

            var files = Directory.GetFiles(basePath, "*.ttf", SearchOption.AllDirectories)
                .Union(Directory.GetFiles(basePath, "*.otf", SearchOption.AllDirectories))
                .Union(Directory.GetFiles(basePath, "*.ttc", SearchOption.AllDirectories));

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                if (fileName.Contains(normalizedFamily))
                {
                    systemFontPathCache[cacheKey] = file;
                    return file;
                }
            }
        }

        return null;
    }

    private static List<string> GetSystemFontDirectories()
    {
        var fontPaths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fontPaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fontPaths.AddRange(
            [
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts")
            ]);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            fontPaths.AddRange(
            [
                "/Library/Fonts",
                "/System/Library/Fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")
            ]);
        }

        return fontPaths;
    }

    /// <summary>
    ///     Normalizes a font family name for file matching.
    /// </summary>
    private string NormalizeFamilyName(string familyName)
    {
        return familyName?.ToLowerInvariant().Replace(" ", "") ?? string.Empty;
    }

    /// <summary>
    ///     Clears the typeface cache.
    /// </summary>
    public void ClearCache()
    {
        foreach (var typeface in typefaces.Values)
        {
            typeface?.Dispose();
        }
        typefaces.Clear();
        typefaceFilePaths.Clear();
        privateTypefaces.Clear();
        systemFontPathCache.Clear();
        cmapCache.Clear();
        InvalidateFontFamilyCache();
    }

    private IEnumerable<SKTypeface> GetPrivateTypefaces(string familyName)
    {
        foreach (var entry in privateTypefaces)
        {
            if (string.Equals(entry.Key, familyName, StringComparison.OrdinalIgnoreCase))
            {
                return entry.Value;
            }
        }

        return Array.Empty<SKTypeface>();
    }

    private SKTypeface MatchPrivateTypeface(string familyName, bool bold, bool italic)
    {
        var matches = GetPrivateTypefaces(familyName).ToArray();
        if (matches.Length == 0)
        {
            return null;
        }

        return matches
            .OrderBy(typeface => StyleDistance(typeface, bold, italic))
            .FirstOrDefault();
    }

    private static int StyleDistance(SKTypeface typeface, bool bold, bool italic)
    {
        int distance = Math.Abs(typeface.FontStyle.Weight - (bold ? 700 : 400));
        bool typefaceItalic = typeface.FontStyle.Slant != SKFontStyleSlant.Upright || typeface.IsItalic;
        if (typefaceItalic != italic)
        {
            distance += 1000;
        }

        return distance;
    }

    private static void AddStyle(FontStyles styles, SKTypeface typeface)
    {
        bool bold = typeface.FontStyle.Weight >= 600 || typeface.IsBold;
        bool italic = typeface.FontStyle.Slant != SKFontStyleSlant.Upright || typeface.IsItalic;

        if (bold && italic)
        {
            styles.AddStyle("Bold Italic");
        }
        else if (bold)
        {
            styles.AddStyle("Bold");
        }
        else if (italic)
        {
            styles.AddStyle("Italic");
        }
        else
        {
            styles.AddStyle("Regular");
        }
    }
}
