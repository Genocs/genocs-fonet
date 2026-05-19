using System.Text;
using System.Text.RegularExpressions;

namespace Genocs.Fonet.Tests.Support;

internal static partial class PdfAssertions
{
    private const int MinimumPdfSizeBytes = 100;

    public static void AssertValidPdf(byte[] pdfBytes)
    {
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > MinimumPdfSizeBytes, $"PDF is too small ({pdfBytes.Length} bytes).");

        var header = Encoding.ASCII.GetString(pdfBytes, 0, Math.Min(5, pdfBytes.Length));
        Assert.Equal("%PDF-", header);

        var content = Encoding.Latin1.GetString(pdfBytes);
        Assert.Contains("%%EOF", content);
    }

    public static void AssertValidPdfFile(string path)
    {
        Assert.True(File.Exists(path), $"Expected PDF file at '{path}'.");
        AssertValidPdf(File.ReadAllBytes(path));
    }

    public static void AssertPdfPageCount(byte[] pdfBytes, int expectedPages)
    {
        AssertValidPdf(pdfBytes);

        var content = Encoding.Latin1.GetString(pdfBytes);
        var pageCount = PageObjectRegex().Matches(content).Count;
        Assert.Equal(expectedPages, pageCount);
    }

    public static void AssertPdfPageCountAtLeast(byte[] pdfBytes, int minimumPages)
    {
        AssertValidPdf(pdfBytes);

        var content = Encoding.Latin1.GetString(pdfBytes);
        var pageCount = PageObjectRegex().Matches(content).Count;
        Assert.True(pageCount >= minimumPages, $"Expected at least {minimumPages} page(s), found {pageCount}.");
    }

    [GeneratedRegex(@"/Type\s*/Page(?!\w)")]
    private static partial Regex PageObjectRegex();
}
