using Genocs.Fonet.Tests.Support;

namespace Genocs.Fonet.Tests;

public class PdfBuilderUnitTests : PdfTestBase
{
    [Fact]
    public void BuildNunitoFontCustomPdfTest()
    {
        var pdfBytes = RenderFo("NunitoFontCustomTest.fo", AddAllBundledFonts);

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 1);
    }

    [Fact]
    public void BuildPdfTest()
    {
        var pdfBytes = RenderFo("StarWarsMovies.fo");

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 11);
    }

    [Fact]
    public void BuildNunitoFontPdfTest()
    {
        var pdfBytes = RenderFo("NunitoFontTest.fo", AddNunitoFonts);

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 1);
    }

    [Fact]
    public void ScaleToFitPropertyTest()
    {
        var pdfBytes = RenderFo("ScaleToFitTest.fo", AddNunitoFonts);

        PdfAssertions.AssertValidPdf(pdfBytes);
    }

    [Fact]
    public void CrossPlatformFontAndImageTest()
    {
        var pdfBytes = RenderFo("CrossPlatformTest.fo", AddNunitoFonts);

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCountAtLeast(pdfBytes, minimumPages: 1);
    }

    [Fact]
    public void WritePdfToDisks_StarWarsMovies()
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(outputDirectory);

        var pdfBytes = RenderFo("StarWarsMovies.fo");
        var outputPath = Path.Combine(outputDirectory, "StarWarsMovies_Test.pdf");

        File.WriteAllBytes(outputPath, pdfBytes);

        Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
        Assert.True(new FileInfo(outputPath).Length > 0, "PDF file is empty");

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 11);
    }

    [Fact]
    public void WritePdfToDisk_NunitoFont()
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(outputDirectory);

        var pdfBytes = RenderFo("NunitoFontTest.fo", AddNunitoFonts);
        var outputPath = Path.Combine(outputDirectory, "NunitoFont_Test.pdf");

        File.WriteAllBytes(outputPath, pdfBytes);

        Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
        Assert.True(new FileInfo(outputPath).Length > 0, "PDF file is empty");

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 1);
    }

    [Fact]
    public void WritePdfToDisk_AllBundledFonts()
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(outputDirectory);

        var pdfBytes = RenderFo("NunitoFontCustomTest.fo", AddAllBundledFonts);
        var outputPath = Path.Combine(outputDirectory, "AllBundledFonts_Test.pdf");

        File.WriteAllBytes(outputPath, pdfBytes);

        Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
        Assert.True(new FileInfo(outputPath).Length > 0, "PDF file is empty");

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 1);
    }

    [Fact]
    public void WritePdfToDisk_ScaleToFit()
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(outputDirectory);

        var pdfBytes = RenderFo("ScaleToFitTest.fo", AddNunitoFonts);
        var outputPath = Path.Combine(outputDirectory, "ScaleToFit_Test.pdf");

        File.WriteAllBytes(outputPath, pdfBytes);

        Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
        Assert.True(new FileInfo(outputPath).Length > 0, "PDF file is empty");

        PdfAssertions.AssertValidPdf(pdfBytes);
    }

    [Fact]
    public void WritePdfToDisk_CrossPlatform()
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        Directory.CreateDirectory(outputDirectory);

        var pdfBytes = RenderFo("CrossPlatformTest.fo", AddNunitoFonts);
        var outputPath = Path.Combine(outputDirectory, "CrossPlatform_Test.pdf");

        File.WriteAllBytes(outputPath, pdfBytes);

        Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
        Assert.True(new FileInfo(outputPath).Length > 0, "PDF file is empty");

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCountAtLeast(pdfBytes, minimumPages: 1);
    }

    [Fact]
    public void WriteAllPdfsToDisk_CompleteSet()
    {
        var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput", "CompleteSet");
        Directory.CreateDirectory(outputDirectory);

        var testCases = new[]
        {
            ("StarWarsMovies.fo", "StarWarsMovies.pdf", (Action<Genocs.Fonet.Render.Pdf.PdfRendererOptions>?)null, 11),
            ("NunitoFontTest.fo", "NunitoFont.pdf", (Action<Genocs.Fonet.Render.Pdf.PdfRendererOptions>?)AddNunitoFonts, 1),
            ("NunitoFontCustomTest.fo", "NunitoFontCustom.pdf", (Action<Genocs.Fonet.Render.Pdf.PdfRendererOptions>?)AddAllBundledFonts, 1),
            ("ScaleToFitTest.fo", "ScaleToFit.pdf", (Action<Genocs.Fonet.Render.Pdf.PdfRendererOptions>?)AddNunitoFonts, 1),
            ("CrossPlatformTest.fo", "CrossPlatform.pdf", (Action<Genocs.Fonet.Render.Pdf.PdfRendererOptions>?)AddNunitoFonts, 2)
        };

        foreach (var (template, outputFile, configure, expectedPages) in testCases)
        {
            var pdfBytes = RenderFo(template, configure);
            var outputPath = Path.Combine(outputDirectory, outputFile);

            File.WriteAllBytes(outputPath, pdfBytes);

            Assert.True(File.Exists(outputPath), $"PDF file was not created at {outputPath}");
            Assert.True(new FileInfo(outputPath).Length > 0, $"PDF file {outputFile} is empty");

            PdfAssertions.AssertValidPdf(pdfBytes);
            PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages);
        }
    }
}
