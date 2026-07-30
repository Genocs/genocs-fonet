namespace Genocs.Fonet.WebApi.Configuration;

/// <summary>
/// Paths for templates, fonts, and assets. In Docker these map to external volumes.
/// </summary>
public sealed class PdfStorageOptions
{
    public const string SectionName = "pdfStorage";

    /// <summary>Absolute or relative path to XSLT/FO templates.</summary>
    public string TemplatesPath { get; set; } = "data/templates";

    /// <summary>Absolute or relative path to private fonts.</summary>
    public string FontsPath { get; set; } = "data/fonts";

    /// <summary>Absolute or relative path to images and other assets.</summary>
    public string AssetsPath { get; set; } = "data/assets";

    public string ResolveTemplatesPath() => Resolve(TemplatesPath);

    public string ResolveFontsPath() => Resolve(FontsPath);

    public string ResolveAssetsPath() => Resolve(AssetsPath);

    private static string Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("pdfStorage path is not configured.");
        }

        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
    }
}
