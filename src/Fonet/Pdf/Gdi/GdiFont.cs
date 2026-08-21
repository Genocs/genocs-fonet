using SkiaSharp;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Cross-platform font wrapper using SkiaSharp instead of Windows GDI handles
/// </summary>
public class GdiFont
{
    private SKTypeface? _typeface;
    private readonly string _faceName;
    private readonly int _height;

    /// <summary>
    /// Class constructor
    /// </summary>
    /// <param name="typeface">SkiaSharp typeface object</param>
    /// <param name="faceName">Face name of the font</param>
    /// <param name="height">Font height</param>
    public GdiFont(SKTypeface typeface, string faceName, int height)
    {
        _typeface = typeface;
        _faceName = faceName;
        _height = height;
    }

    /// <summary>
    /// Constructor for backward compatibility with handle-only creation
    /// </summary>
    public GdiFont(string faceName, int height)
    {
        _faceName = faceName;
        _height = height;

        // Try to load typeface by name
        _typeface = FontManager.Instance.LoadTypeface(faceName, false, false);
    }

    /// <summary>
    /// Class destructor
    /// </summary>
    ~GdiFont()
    {
        Dispose(false);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Typefaces are owned and cached by FontManager; do not dispose here.
        _typeface = null!;
    }

    /// <summary>
    /// Creates a font based on the supplied typeface name and size.
    /// </summary>
    /// <param name="faceName">The typeface name of a font.</param>
    /// <param name="height">
    /// The height, in logical units, of the font's character 
    /// cell or character.
    /// </param>
    /// <param name="bold">Whether the font should be bold</param>
    /// <param name="italic">Whether the font should be italic</param>
    /// <returns>A new GdiFont instance</returns>
    public static GdiFont CreateFont(string faceName, int height, bool bold, bool italic)
    {
        var fontManager = FontManager.Instance;
        var typeface = fontManager.LoadTypeface(faceName, bold, italic) ?? throw new ArgumentException($"Font '{faceName}' not found on system.");

        return new GdiFont(typeface, faceName, height);
    }

    /// <summary>
    /// Creates a font whose height is equal to the negative value
    /// of the EM Square
    /// </summary>
    /// <param name="faceName">The typeface name of a font.</param>
    /// <returns>A new GdiFont instance</returns>
    public static GdiFont CreateDesignFont(string faceName, bool bold, bool italic, GdiDeviceContent dc)
    {
        // Create a temporary font at standard size
        GdiFont tempFont = GdiFont.CreateFont(faceName, 2048, bold, italic);
        dc.SelectFont(tempFont);
        GdiFontMetrics metrics = tempFont.GetMetrics(dc);
        tempFont.Dispose();

        return CreateFont(faceName, -Math.Abs(metrics.EmSquare), bold, italic);
    }

    /// <summary>
    /// Gets the metrics for this font
    /// </summary>
    public GdiFontMetrics GetMetrics(GdiDeviceContent dc)
    {
        return new GdiFontMetrics(dc, this);
    }

    /// <summary>
    /// Gets the face name
    /// </summary>
    public string FaceName { get { return _faceName; } }

    /// <summary>
    /// Gets the font height
    /// </summary>
    public int Height { get { return _height; } }

    /// <summary>
    /// Gets the SkiaSharp typeface
    /// </summary>
    internal SKTypeface? Typeface { get { return _typeface; } }
}