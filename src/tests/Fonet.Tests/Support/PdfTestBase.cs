using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.UnitTests.Support;

public abstract class PdfTestBase : IDisposable
{
    protected string TempDirectory { get; }

    protected string TemplatesDirectory { get; }

    protected string FontsDirectory { get; }

    protected PdfTestBase()
    {
        TempDirectory = Path.Combine(Path.GetTempPath(), "genocs-fonet-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(TempDirectory);

        TemplatesDirectory = ResolveTestAssetDirectory("templates");
        FontsDirectory = ResolveTestAssetDirectory("fonts");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        try
        {
            if (Directory.Exists(TempDirectory))
            {
                Directory.Delete(TempDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup for temp output directories.
        }
    }

    protected byte[] RenderFo(string templateFileName, Action<PdfRendererOptions>? configure = null)
    {
        var templatePath = Path.Combine(TemplatesDirectory, templateFileName);
        Assert.True(File.Exists(templatePath), $"Missing FO template '{templatePath}'.");

        var outputPath = Path.Combine(TempDirectory, Path.ChangeExtension(templateFileName, ".pdf"));
        var driver = FonetDriver.Make();
        driver.Options = new PdfRendererOptions();
        configure?.Invoke(driver.Options);

        using (var input = File.OpenRead(templatePath))
        using (var output = File.Create(outputPath))
        {
            driver.Render(input, output);
        }

        return File.ReadAllBytes(outputPath);
    }

    protected void AddNunitoFonts(PdfRendererOptions options)
    {
        AddFonts(options, "Nunito-Regular.ttf", "Nunito-Bold.ttf", "Nunito-Italic.ttf");
    }

    protected void AddAllBundledFonts(PdfRendererOptions options)
    {
        foreach (var fontFile in Directory.GetFiles(FontsDirectory, "*.ttf", SearchOption.TopDirectoryOnly))
        {
            options.AddPrivateFont(new FileInfo(fontFile));
        }
    }

    protected void AddFonts(PdfRendererOptions options, params string[] fontFileNames)
    {
        foreach (var fontFileName in fontFileNames)
        {
            var fontPath = Path.Combine(FontsDirectory, fontFileName);
            Assert.True(File.Exists(fontPath), $"Missing font file '{fontPath}'.");
            options.AddPrivateFont(new FileInfo(fontPath));
        }
    }

    private static string ResolveTestAssetDirectory(string folderName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, folderName),
            Path.Combine(Directory.GetCurrentDirectory(), folderName),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", folderName))
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException($"Unable to locate test asset directory '{folderName}'.");
    }
}
