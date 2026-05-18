using Fonet;

namespace Genocs.Fonet.Tests
{
    public class PdfBuilderUnitTests
    {
        [Fact]
        public void BuildPdfTest()
        {
            // Arrange
            FonetDriver driver = FonetDriver.Make();

            // Read from ./templates/StarWarsMovies.fo
            string fofFilename = "./templates/StarWarsMovies.fo";
            string pdfOutputFilename = "./templates/StarWarsMovies.pdf";

            Stream inputStream = new FileStream(fofFilename, FileMode.Open, FileAccess.Read);
            Stream outputStream = new FileStream(pdfOutputFilename, FileMode.Create);

            // Act
            driver.Render(inputStream, outputStream);
            // Assert
            Assert.True(File.Exists(pdfOutputFilename));

        }

        [Fact]
        public void ScaleToFitPropertyTest()
        {
            // Arrange
            FonetDriver driver = FonetDriver.Make();

            // Read from ./templates/ScaleToFitTest.fo
            string fofFilename = "./templates/ScaleToFitTest.fo";
            string pdfOutputFilename = "./templates/ScaleToFitTest.pdf";

            Stream inputStream = new FileStream(fofFilename, FileMode.Open, FileAccess.Read);
            Stream outputStream = new FileStream(pdfOutputFilename, FileMode.Create);

            // Act - This should not throw an exception about "scale-to-fit" not being recognized
            driver.Render(inputStream, outputStream);

            // Assert
            Assert.True(File.Exists(pdfOutputFilename));
        }
    }
}
