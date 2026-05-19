using System;
using System.IO;
using System.Text;
using Genocs.Fonet;
using Genocs.Fonet.Render.Pdf;
using SkiaSharp;

var fontsDir = Path.GetFullPath(@"src\tests\Genocs.Fonet.Tests\fonts");
var template = Path.GetFullPath(@"src\tests\Genocs.Fonet.Tests\templates\NunitoFontTest.fo");

// Scenario A: Nunito NOT registered at all (simulates "font file exists locally but not added")
RunScenario("A: No registration, font-family=Nunito", FontType.Embed, template);

// Scenario B: lowercase family name with registration
var lowerTemplate = Path.Combine(Path.GetTempPath(), "nunito-lower.fo");
File.WriteAllText(lowerTemplate, File.ReadAllText(template).Replace("font-family=\"Nunito\"", "font-family=\"nunito\""));
RunScenario("B: registered but font-family=nunito (lowercase)", FontType.Embed, lowerTemplate, "Nunito-Regular.ttf");

// Scenario C: comma-separated font list
var listTemplate = Path.Combine(Path.GetTempPath(), "nunito-list.fo");
File.WriteAllText(listTemplate, File.ReadAllText(template).Replace("font-family=\"Nunito\"", "font-family=\"Nunito, sans-serif\""));
RunScenario("C: registered but font-family='Nunito, sans-serif'", FontType.Embed, listTemplate, "Nunito-Regular.ttf");

// Scenario D: which SKFontManager families would fail the LocateSystemFont filename heuristic?
Console.WriteLine("=== D: system families whose file cannot be located by filename heuristic ===");
var winFonts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
var localFonts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts");
var systemFiles = Directory.GetFiles(winFonts).Select(f => Path.GetFileNameWithoutExtension(f).ToLowerInvariant()).ToArray();
int notLocatable = 0, userOnly = 0;
foreach (var fam in SKFontManager.Default.FontFamilies.Distinct().OrderBy(f => f))
{
    var norm = fam.ToLowerInvariant().Replace(" ", "");
    bool inSystem = systemFiles.Any(f => f.Contains(norm));
    bool inUser = Directory.Exists(localFonts) &&
                  Directory.GetFiles(localFonts).Any(f => Path.GetFileNameWithoutExtension(f).ToLowerInvariant().Contains(norm));
    if (!inSystem)
    {
        notLocatable++;
        if (inUser) { userOnly++; }
        Console.WriteLine($"  NOT LOCATABLE: '{fam}'{(inUser ? "  (per-user font dir only)" : string.Empty)}");
    }
}
Console.WriteLine($"  total not locatable: {notLocatable} (of which per-user installs: {userOnly})");
Console.WriteLine();

// Scenario E: render with a system family whose file is NOT locatable -> embedding path
var families = SKFontManager.Default.FontFamilies.Distinct().ToArray();
var broken = families.FirstOrDefault(fam =>
{
    var norm = fam.ToLowerInvariant().Replace(" ", "");
    return !systemFiles.Any(f => f.Contains(norm));
});
if (broken != null)
{
    var brokenTemplate = Path.Combine(Path.GetTempPath(), "broken-family.fo");
    File.WriteAllText(brokenTemplate, File.ReadAllText(template).Replace("font-family=\"Nunito\"", $"font-family=\"{broken}\""));
    RunScenario($"E: system family '{broken}' (file not locatable) Embed", FontType.Embed, brokenTemplate);
    RunScenario($"E2: system family '{broken}' (file not locatable) Link", FontType.Link, brokenTemplate);
}

void RunScenario(string label, FontType fontType, string foPath, params string[] fonts)
{
    Console.WriteLine("=== " + label + " ===");
    var warnings = new System.Collections.Generic.List<string>();
    var driver = FonetDriver.Make();
    driver.OnWarning += (_, e) => warnings.Add(e.GetMessage());
    var options = new PdfRendererOptions { FontType = fontType };
    foreach (var f in fonts)
    {
        try { options.AddPrivateFont(new FileInfo(Path.Combine(fontsDir, f))); }
        catch (Exception ex) { Console.WriteLine($"  Register '{f}' FAILED: {ex.Message}"); }
    }
    driver.Options = options;

    var outPath = Path.Combine(Path.GetTempPath(), $"diag-{Guid.NewGuid():N}.pdf");
    try
    {
        using var input = File.OpenRead(foPath);
        using var outStream = File.Create(outPath);
        driver.Render(input, outStream);
        Console.WriteLine("  Render OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine("  Render EXCEPTION: " + ex.Message + (ex.InnerException != null ? " | inner: " + ex.InnerException.Message : ""));
    }

    foreach (var w in warnings.Distinct())
    {
        Console.WriteLine("  WARN: " + w);
    }

    if (File.Exists(outPath))
    {
        var text = Encoding.Latin1.GetString(File.ReadAllBytes(outPath));
        foreach (System.Text.RegularExpressions.Match m in
                 System.Text.RegularExpressions.Regex.Matches(text, @"/BaseFont\s*/([#\w,+ -]+)"))
        {
            Console.WriteLine("  BaseFont: " + m.Groups[1].Value.Trim());
        }
        File.Delete(outPath);
    }

    Console.WriteLine();
}
