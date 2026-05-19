# Testing Strategy

Current test state, gaps, and recommended testing layers for the migration.

## Current State

### Test Project

| Attribute | Value |
|-----------|-------|
| Project | `src/tests/Genocs.Fonet.Tests/` |
| Framework | xUnit 2.9.3 |
| Target | `net10.0` only (library targets net8/9/10) |
| Test count | 5 |
| Test type | Integration/smoke only |
| Assertion depth | File existence (`Assert.True(File.Exists(...))`) |

### Existing Tests

| Test | Template | What It Exercises |
|------|----------|-------------------|
| `BuildPdfTest` | `StarWarsMovies.fo` | Basic FO → PDF (system fonts) |
| `BuildNunitoFontPdfTest` | `NunitoFontTest.fo` | Custom font via `AddPrivateFont` |
| `BuildNunitoFontCustomPdfTest` | `NunitoFontCustomTest.fo` | Custom font variants |
| `ScaleToFitPropertyTest` | `ScaleToFitTest.fo` | `scale-to-fit` property parsing |
| `CrossPlatformFontAndImageTest` | `CrossPlatformTest.fo` | SkiaSharp fonts + image embedding |

### FO Templates Available

```
src/tests/Genocs.Fonet.Tests/templates/
├── StarWarsMovies.fo
├── NunitoFontTest.fo
├── NunitoFontCustomTest.fo
├── ScaleToFitTest.fo
├── CrossPlatformTest.fo
└── nunito-test.fo (unused in tests)
```

### Problems with Current Tests

1. **No content validation** — A blank PDF file would pass all tests
2. **Side effects** — Tests write PDFs into `templates/` directory
3. **No isolation** — Tests depend on working directory being test project root
4. **Unreliable** — `CrossPlatformFontAndImageTest` can crash test host (SkiaSharp native error)
5. **No negative tests** — Invalid FO, missing fonts, corrupt images not tested
6. **Single TFM** — Tests only run on `net10.0`; library multi-targeting not verified
7. **No CI** — Tests not run automatically on PR/push

## Recommended Testing Layers

```
┌─────────────────────────────────────────────────────────────┐
│  Layer 4: Visual Regression (Phase 4)                     │
│  PDF → rasterize → pixel diff against baseline              │
├─────────────────────────────────────────────────────────────┤
│  Layer 3: Integration Tests (Phase 1–3)                    │
│  FO template → PDF → structural/content validation          │
├─────────────────────────────────────────────────────────────┤
│  Layer 2: Component Tests (Phase 1–2)                      │
│  Font metrics, glyph mapping, table parsing, layout units   │
├─────────────────────────────────────────────────────────────┤
│  Layer 1: Unit Tests (Phase 0–1)                           │
│  DataTypes, expressions, property parsing, PDF object model   │
└─────────────────────────────────────────────────────────────┘
```

### Layer 1: Unit Tests (Phase 0)

Pure logic, no I/O, fast execution.

| Area | Example Tests | Files to Test |
|------|---------------|---------------|
| Length types | `Length.FromString("10pt")` → correct millipoints | `DataTypes/Length.cs`, `PercentLength.cs` |
| Color parsing | `ColorType.Parse("rgb(255,0,0)")` → red | `DataTypes/ColorType.cs` |
| Expressions | `min(10, 20)` → 10 | `Fo/Expr/MinFunction.cs` |
| Property makers | `font-size="12pt"` → FontSizeProperty | `Fo/Properties/FontSizeMaker.cs` |
| PDF objects | PdfArray, PdfDictionary serialization | `Pdf/PdfArray.cs`, `PdfDictionary.cs` |

**Target:** 30+ unit tests, < 5s total runtime.

### Layer 2: Component Tests (Phase 1)

Test subsystems with controlled inputs.

| Area | Example Tests |
|------|---------------|
| Font table reader | Read `head`, `cmap`, `hmtx` from Nunito TTF |
| Glyph mapping | 'A' → glyph ID != 0 for Nunito |
| Font metrics | Ascent/descent within expected range |
| Font discovery | `MatchFamily("Nunito")` returns typeface |
| Image decode | Load PNG/JPEG, verify dimensions and pixel count |
| Layout | Single block produces one line area with correct width |

**Target:** 20+ component tests.

### Layer 3: Integration Tests (Phase 1–3)

End-to-end FO → PDF with validation.

#### PDF Structural Validation (implement in Phase 0)

```csharp
static void AssertValidPdf(string path)
{
    var bytes = File.ReadAllBytes(path);
    Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    Assert.Contains("%%EOF", Encoding.ASCII.GetString(bytes));
    Assert.True(new FileInfo(path).Length > 100);
}

static void AssertPdfHasPages(string path, int expectedPages)
{
    var text = Encoding.Latin1.GetString(File.ReadAllBytes(path));
    var count = Regex.Matches(text, @"/Type\s*/Page[^s]").Count;
    Assert.Equal(expectedPages, count);
}
```

#### Font Embedding Validation

```csharp
static void AssertPdfContainsFont(string path, string fontName)
{
    var text = Encoding.Latin1.GetString(File.ReadAllBytes(path));
    Assert.Contains($"/BaseFont /{fontName}", text);
}
```

#### Text Content Validation (Phase 2+)

Use a PDF text extraction library (e.g., UglyToad.PdfPig) to verify rendered text:

```csharp
using UglyToad.PdfPig;

static void AssertPdfContainsText(string path, string expected)
{
    using var doc = PdfDocument.Open(path);
    var text = string.Join("", doc.GetPages().SelectMany(p => p.GetWords()).Select(w => w.Text));
    Assert.Contains(expected, text);
}
```

#### Test Template Additions (Phase 1–3)

| Template | Tests |
|----------|-------|
| `BasicTable.fo` | Table layout, borders, cell padding |
| `CjkText.fo` | CJK character rendering |
| `RtlText.fo` | Right-to-left text direction |
| `BackgroundColor.fo` | Background color property |
| `ExternalGraphic.fo` | Image embedding (PNG, JPEG) |
| `PageNumbers.fo` | Page number citations |
| `InternalLinks.fo` | `fo:basic-link` with `internal-destination` |
| `MultiPage.fo` | Pagination, page breaks |
| `InvalidFont.fo` | Missing font → graceful error (negative test) |
| `MalformedFo.fo` | Invalid XML → structured exception (negative test) |

### Layer 4: Visual Regression (Phase 4)

For layout-sensitive features where text extraction is insufficient:

1. Render FO → PDF
2. Rasterize PDF pages to PNG (using SkiaSharp or PdfPig + rendering)
3. Compare against committed baseline images
4. Fail on pixel diff above threshold

**Tool options:**
- `Verify` (NuGet) — snapshot testing with diff viewer
- Custom SkiaSharp PDF→bitmap→pixel compare

**Caution:** Visual tests are platform-sensitive (font rendering differences). Prefer structural/text tests where possible.

## Test Infrastructure Improvements

### Phase 0: Immediate

```csharp
public class PdfTestBase
{
    protected string TempDir { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    protected byte[] RenderFo(string foPath, Action<PdfRendererOptions>? configure = null)
    {
        Directory.CreateDirectory(TempDir);
        var driver = FonetDriver.Make();
        driver.Options = new PdfRendererOptions();
        configure?.Invoke(driver.Options);

        var outputPath = Path.Combine(TempDir, "output.pdf");
        using var input = File.OpenRead(foPath);
        using var output = File.Create(outputPath);
        driver.Render(input, output);
        return File.ReadAllBytes(outputPath);
    }
}
```

### CI Matrix (Phase 0)

```yaml
# .github/workflows/ci.yml
strategy:
  matrix:
    os: [windows-latest, ubuntu-latest, macos-latest]
    dotnet: ['8.0.x', '9.0.x', '10.0.x']
```

### Coverage Targets

| Phase | Line Coverage Target | Focus Areas |
|-------|---------------------|-------------|
| Phase 0 | 20% | Smoke tests + CI |
| Phase 1 | 40% | Font pipeline, glyph mapping |
| Phase 2 | 50% | Image, PDF writer |
| Phase 3 | 60% | FO properties by priority |
| Phase 4 | 70% | Overall |

Use `coverlet.collector` (already referenced) with:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Naming Convention

```
{Area}_{Scenario}_{ExpectedResult}

Examples:
FontTableReader_ValidTtf_ReturnsHeadTable
GlyphMapping_LatinText_ReturnsNonZeroIndices
RenderFo_SimpleBlock_ProducesValidPdf
RenderFo_MissingFont_ThrowsFonetException
```

## What NOT to Test

- Every `ToBeImplementedProperty` stub (test when implemented)
- PDF byte-for-byte identity across platforms (font rendering varies)
- Internal private methods directly (test through public API)
- Apache FOP compatibility (out of scope unless explicitly required)
