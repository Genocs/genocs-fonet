using Genocs.Fonet.XsltTransformer.Transformers;
using System.Text.Json;

namespace Genocs.Fonet.Host.Drivers;

public static class XsltDriver
{
    private static JsonSerializerOptions jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Run()
    {
        // Resolve assets relative to the executable so the app works from any working directory.
        string baseDirectory = AppContext.BaseDirectory;
        string modelPath = Path.Combine(baseDirectory, "models", "books.json");
        string templatePath = Path.Combine(baseDirectory, "templates", "books.xslt");
        string outputPath = Path.Combine(baseDirectory, "books.pdf");

        XslFoPdfService xslFoPdfService = new();

        Stream pdfDocument = xslFoPdfService.Print(
            document: PrintableDocument(modelPath)!,
            templateName: templatePath,
            resourcesName: null,
            fontsDirectory: "fonts",
            countryId: null);

        // Write the output stream to a file
        using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        pdfDocument.CopyTo(fileStream);
    }

    private static IPrintableDocument? PrintableDocument(string modelPath)
    {
        // Read from the json file and deserialize it into the BookList property
        string json = File.ReadAllText(modelPath);
        return JsonSerializer.Deserialize<Books>(json, jsonSerializerOptions);
    }

    public class Books : IPrintableDocument
    {
        public string? DocumentName { get; set; }
        public List<Book>? BookList { get; set; }
        public string ToXml()
        {
            return ObjectXmlSerializer.SerializeObjectToXmlFormattedString(this);
        }
    }

    public class Book
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
    }
}