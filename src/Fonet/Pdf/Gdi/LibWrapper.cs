using Genocs.Fonet.Pdf.Gdi.Structures;
using System.Collections.Concurrent;
using System.Text;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Cross-platform wrapper for font operations. Replaces Windows GDI P/Invoke calls
/// with SkiaSharp-based implementations that work on all platforms.
/// </summary>
internal sealed class LibWrapper
{
    private static readonly ConcurrentDictionary<IntPtr, WeakReference<GdiDeviceContent>> DeviceContexts = new();

    internal static void RegisterDeviceContext(GdiDeviceContent dc)
    {
        DeviceContexts[dc.Handle] = new WeakReference<GdiDeviceContent>(dc);
    }

    internal static void UnregisterDeviceContext(GdiDeviceContent dc)
    {
        DeviceContexts.TryRemove(dc.Handle, out _);
    }

    internal static int AddFontResourceEx(string lpszFilename, uint fl, int pdv) =>
        File.Exists(lpszFilename) ? 1 : 0;

    internal static int GetTextFace(IntPtr hdc, int nCount, StringBuilder lpFaceName)
    {
        if (lpFaceName == null || nCount <= 0 || !TryGetDeviceContent(hdc, out var dc) || dc.CurrentTypeface == null)
        {
            return 0;
        }

        string? faceName = FontTableAccess.ReadFamilyName(dc.CurrentTypeface) ?? dc.CurrentTypeface.FamilyName ?? string.Empty;
        lpFaceName.Clear();
        lpFaceName.Append(faceName);
        return Math.Min(faceName.Length, nCount - 1);
    }

    private static bool TryGetDeviceContent(IntPtr hdc, out GdiDeviceContent dc)
    {
        dc = null!;
        if (!DeviceContexts.TryGetValue(hdc, out var weakReference) || !weakReference.TryGetTarget(out var target))
        {
            return false;
        }

        dc = target;
        return true;
    }
}

internal delegate int FontEnumDelegate(ref EnumLogFont lpelf, ref NewTextMetric lpntm, uint fontType, int lParam);
