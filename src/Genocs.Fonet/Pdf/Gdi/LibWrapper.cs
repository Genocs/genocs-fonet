using Genocs.Fonet.Pdf.Gdi.Structures;
using System.Collections.Concurrent;
using System.Text;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
///     Cross-platform wrapper for font operations. Replaces Windows GDI P/Invoke calls
///     with SkiaSharp-based implementations that work on all platforms.
/// </summary>
internal sealed class LibWrapper
{
    private static readonly ConcurrentDictionary<IntPtr, WeakReference<GdiDeviceContent>> DeviceContexts = new();
    private static IntPtr dummyDC = new(1);

    internal static void RegisterDeviceContext(GdiDeviceContent dc)
    {
        DeviceContexts[dc.Handle] = new WeakReference<GdiDeviceContent>(dc);
    }

    internal static void UnregisterDeviceContext(GdiDeviceContent dc)
    {
        DeviceContexts.TryRemove(dc.Handle, out _);
    }

    internal static IntPtr GetDC(IntPtr hWnd) => dummyDC;

    internal static uint GetFontData(IntPtr hdc, uint dwTable, uint dwOffset, byte[]? lpvBuffer, uint cbData)
    {
        if (!TryGetDeviceContent(hdc, out var dc) || dc.CurrentTypeface == null)
        {
            return 0;
        }

        var tableData = dwTable == 0
            ? FontTableAccess.ReadFullFont(dc.CurrentTypeface)
            : FontTableAccess.ReadTable(dc.CurrentTypeface, dwTable);

        if (tableData == null || tableData.Length == 0)
        {
            return 0;
        }

        if (lpvBuffer == null || cbData == 0)
        {
            return (uint)tableData.Length;
        }

        if (dwOffset >= tableData.Length)
        {
            return (uint)GdiFontMetrics.GDI_ERROR;
        }

        int bytesToCopy = (int)Math.Min(cbData, tableData.Length - dwOffset);
        Buffer.BlockCopy(tableData, (int)dwOffset, lpvBuffer, 0, bytesToCopy);
        return (uint)bytesToCopy;
    }

    internal static int AddFontResourceEx(string lpszFilename, uint fl, int pdv) =>
        File.Exists(lpszFilename) ? 1 : 0;

    internal static bool RemoveFontResourceEx(string lpFileName, uint fl, int pdv) => true;

    internal static IntPtr CreateFontIndirect(LogFont lplf) => new(Guid.NewGuid().GetHashCode());

    internal static uint GetGlyphIndices(IntPtr hdc, string lpstr, int c, ushort[] pgi, uint fl)
    {
        if (pgi == null || c <= 0 || string.IsNullOrEmpty(lpstr))
        {
            return 0;
        }

        if (!TryGetDeviceContent(hdc, out var dc) || dc.CurrentTypeface == null)
        {
            return 0;
        }

        var text = lpstr.Length <= c ? lpstr : lpstr[..c];
        var indices = FontManager.Instance.GetGlyphIndices(text, dc.CurrentTypeface);
        int count = Math.Min(c, Math.Min(indices.Length, pgi.Length));
        for (int i = 0; i < count; i++)
        {
            pgi[i] = indices[i];
        }

        return (uint)count;
    }

    internal static uint GetFontUnicodeRanges(IntPtr hdc, GlyphSet lpgs) => 0;

    internal static IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj) => IntPtr.Zero;

    internal static IntPtr DeleteObject(IntPtr hgdiobj) => IntPtr.Zero;

    internal static IntPtr GetCurrentObject(IntPtr hdc, GdiDcObject uObjectType) => IntPtr.Zero;

    internal static int GetTextFace(IntPtr hdc, int nCount, StringBuilder lpFaceName)
    {
        if (lpFaceName == null || nCount <= 0 || !TryGetDeviceContent(hdc, out var dc) || dc.CurrentTypeface == null)
        {
            return 0;
        }

        var faceName = FontTableAccess.ReadFamilyName(dc.CurrentTypeface) ?? dc.CurrentTypeface.FamilyName ?? string.Empty;
        lpFaceName.Clear();
        lpFaceName.Append(faceName);
        return Math.Min(faceName.Length, nCount - 1);
    }

    internal static bool DeleteDC(IntPtr hdc) => true;

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
