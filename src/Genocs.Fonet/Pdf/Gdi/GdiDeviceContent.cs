using SkiaSharp;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Cross-platform font context wrapper using SkiaSharp.
/// Replaces Windows GDI device context with a platform-agnostic implementation.
/// </summary>
public class GdiDeviceContent : IDisposable
{
    /// <summary>
    /// Dummy device context handle for API compatibility
    /// </summary>
    private IntPtr hDC;

    /// <summary>
    /// Currently selected font typeface
    /// </summary>
    private SKTypeface currentTypeface;

    /// <summary>
    ///     Creates a new device context wrapper
    /// </summary>
    public GdiDeviceContent()
    {
        hDC = new IntPtr(Interlocked.Increment(ref nextHandle));
        LibWrapper.RegisterDeviceContext(this);
    }

    private static int nextHandle = 1;

    /// <summary>
    /// Destructor
    /// </summary>
    ~GdiDeviceContent()
    {
        Dispose(false);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Cleans up resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        LibWrapper.UnregisterDeviceContext(this);
        hDC = IntPtr.Zero;
        currentTypeface = null!;
    }

    /// <summary>
    /// Selects a font into the device context context
    /// </summary>
    /// <param name="font">The font to select</param>
    /// <returns>A handle to the previously selected font</returns>
    public IntPtr SelectFont(GdiFont font)
    {
        // Store the current typeface
        if (font != null)
        {
            var previous = currentTypeface;
            currentTypeface = font.Typeface;
            return previous != null ? new IntPtr(1) : IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    /// <summary>
    /// Gets a handle to the currently selected object
    /// </summary>
    public IntPtr GetCurrentObject(GdiDcObject objectType)
    {
        if (objectType == GdiDcObject.Font && currentTypeface != null)
        {
            return new IntPtr(1); // Dummy handle indicating a font is selected
        }
        return IntPtr.Zero;
    }

    /// <summary>
    /// Returns the dummy device context handle
    /// </summary>
    internal IntPtr Handle { get { return hDC; } }

    /// <summary>
    /// Gets the currently selected typeface
    /// </summary>
    internal SKTypeface CurrentTypeface { get { return currentTypeface; } }
}