using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Host.Drivers;

public static class SimpleDriver
{
    public static void Run()
    {
        // Resolve assets relative to the executable so the app works from any working directory.
        string baseDirectory = AppContext.BaseDirectory;
        string fontsDirectory = Path.Combine(baseDirectory, "fonts");
        string modelPath = Path.Combine(baseDirectory, "models", "books.xml");
        string templatePath = Path.Combine(baseDirectory, "templates", "NunitoSample.fo");
        string outputPath = Path.Combine(baseDirectory, "NunitoSample.pdf");

        var driver = FonetDriver.Make();
        driver.Options = new PdfRendererOptions
        {
            Title = "Nunito Font Sample",
            Author = "Genocs.Fonet.Host",
            // Embed the font program into the PDF so it renders identically everywhere.
            FontType = FontType.Embed,
        };

        // Private fonts must be registered before Render() is called.
        driver.Options.AddPrivateFont(new FileInfo(Path.Combine(fontsDirectory, "Nunito-Regular.ttf")));
        driver.Options.AddPrivateFont(new FileInfo(Path.Combine(fontsDirectory, "Nunito-Bold.ttf")));
        driver.Options.AddPrivateFont(new FileInfo(Path.Combine(fontsDirectory, "Nunito-Italic.ttf")));

        driver.OnWarning += (_, e) => Console.WriteLine($"[WARN] {e.GetMessage()}");
        driver.OnError += (_, e) => Console.WriteLine($"[ERROR] {e.GetMessage()}");

        driver.ImageHandler = AnalyzeSource;
        driver.CloseOnExit = true;

        // 1. Load the xml model

        Console.WriteLine($"Loading XML model from '{modelPath}'...");

        Console.WriteLine($"XML model loaded from '{modelPath}'...");
        // 2. Transform the xml model to FO using XSLT

        // 3. Render the FO to PDF

        Console.WriteLine($"Rendering '{templatePath}'...");


        using (var input = File.OpenRead(templatePath))
        using (var output = File.Create(outputPath))
        {
            driver.Render(input, output);
        }
    }

    //To leave Fonet default management, simply return null
    static byte[]? AnalyzeSource(string src)
    {
        if (string.IsNullOrWhiteSpace(src))
            return null;

        if (src.StartsWith("url"))
        {
            src = src.Replace("url(", string.Empty)
                     .Replace(")", string.Empty)
                     .Replace("\"", string.Empty)
                     .Replace("'", string.Empty);
        }

        if (!src.StartsWith("data:image") || !src.Contains("base64"))
            return null;

        var base64String = src.Split(',').LastOrDefault();
        if (string.IsNullOrWhiteSpace(base64String))
            return null;

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error: " + ex);
            return null;
        }
    }

}