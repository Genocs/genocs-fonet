using Genocs.Fonet.Pdf.Gdi;
using Genocs.Fonet.Render.Pdf;
using Genocs.Fonet.Tests.Support;
using Genocs.Fonet.UnitTests.Support;
using SkiaSharp;

namespace Genocs.Fonet.UnitTests;

public class FontPipelineTests : PdfTestBase
{
    [Fact]
    public void NunitoGlyphMapping_ReturnsNonZeroForLatinCharacters()
    {
        var fontPath = Path.Combine(FontsDirectory, "Nunito-Regular.ttf");
        var typeface = FontManager.Instance.LoadTypefaceFromFile(fontPath);
        Assert.NotNull(typeface);

        var cmap = FontManager.Instance.GetCmapReader(typeface!);

        Assert.NotNull(cmap);
        Assert.NotEqual(0, cmap!.MapCharacter('A'));
        Assert.NotEqual(0, cmap.MapCharacter('z'));
        Assert.NotEqual(0, cmap.MapCharacter('é'));
    }

    [Fact]
    public void FontTableAccess_ReadsHeadTableFromNunito()
    {
        var fontPath = Path.Combine(FontsDirectory, "Nunito-Regular.ttf");
        var fontData = File.ReadAllBytes(fontPath);
        var headTable = FontTableAccess.ReadTableFromBytes(fontData, "head");

        Assert.NotNull(headTable);
        Assert.True(headTable!.Length >= 54);
    }

    [Fact]
    public void EmbeddedNunitoPdf_ContainsFontDescriptorMetrics()
    {
        var pdfBytes = RenderFo("NunitoFontTest.fo", options =>
        {
            options.FontType = FontType.Embed;
            AddNunitoFonts(options);
        });

        PdfAssertions.AssertValidPdf(pdfBytes);

        var content = System.Text.Encoding.Latin1.GetString(pdfBytes);
        Assert.Contains("/FontDescriptor", content);
        Assert.Contains("/FontFile2", content);
    }

    [Fact]
    public void SubsetNunitoPdf_ContainsEmbeddedFontProgram()
    {
        var pdfBytes = RenderFo("NunitoFontTest.fo", options =>
        {
            options.FontType = FontType.Subset;
            AddNunitoFonts(options);
        });

        PdfAssertions.AssertValidPdf(pdfBytes);

        var content = System.Text.Encoding.Latin1.GetString(pdfBytes);
        Assert.Contains("/FontFile2", content);
        Assert.True(content.Length > 5000, "Subset PDF should contain embedded font program data.");
    }
}
