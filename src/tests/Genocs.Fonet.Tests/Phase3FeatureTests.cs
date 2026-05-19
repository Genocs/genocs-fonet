using Genocs.Fonet.Tests.Support;

namespace Genocs.Fonet.Tests;

public class Phase3FeatureTests : PdfTestBase
{
    [Fact]
    public void Phase3Tier1Template_RendersValidPdf()
    {
        var pdfBytes = RenderFo("Phase3Tier1Test.fo", AddNunitoFonts);

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 1);
    }

    [Fact]
    public void ExistingTemplates_StillRenderAfterPhase3Changes()
    {
        var pdfBytes = RenderFo("StarWarsMovies.fo");

        PdfAssertions.AssertValidPdf(pdfBytes);
        PdfAssertions.AssertPdfPageCount(pdfBytes, expectedPages: 11);
    }
}
