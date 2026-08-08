# Genocs.Fonet (genocs-fonet)

## Purpose

Genocs.Fonet is a .NET port of **Fonet**, an XSL-FO (Extensible Stylesheet Language Formatting Objects) formatter that produces PDF output. The original implementation relied on **Windows GDI** for font enumeration, metrics, glyph mapping, and image decoding. This port aims to run on **modern .NET without Windows graphics dependencies**.


![Genocs.Fonet](./assets/banner.png)

Genocs.Fonet is an XSL-FO → PDF library (a fork/derivative of “Fonet”) with a work-in-progress focus on **cross-platform** font and image handling.

For migration status, known issues, and the phased roadmap see the [docs/](./docs/) folder.

## Status

| Area | Status |
|------|--------|
| Build | ✅ Compiles on `net8.0`, `net9.0`, `net10.0` |
| CI | ✅ GitHub Actions (build, test, pack) |
| Font pipeline | ✅ Glyph mapping, subsetting, metrics (Phase 1) |
| Quality | ✅ Image spans, encoding, namespaces (Phase 2) |
| FO completeness | 🔄 Tier 1 batch done; ~87 properties still stubbed (Phase 3) |
| Tests | ✅ 22 tests with PDF structure validation on net8/9/10 |
| Production use | ⚠️ Not recommended yet — FO coverage gaps; CJK needs validation |

## What the cross-platform solution is

The main idea is to remove Windows-only GDI/font/image dependencies and replace them with **SkiaSharp**:

- **Images**: image format detection and pixel extraction uses `SkiaSharp` (`SKCodec`, `SKBitmap`) rather than `System.Drawing` / Windows APIs. See `src/Genocs.Fonet/Image/ApocImage.cs`.
- **Fonts**: font enumeration and typeface loading uses `SKFontManager.Default` + `SKTypeface` rather than GDI handles. See `src/Genocs.Fonet/Pdf/Gdi/FontManager.cs`, `src/Genocs.Fonet/Pdf/Gdi/GdiFont.cs`, `src/Genocs.Fonet/Pdf/Gdi/GdiFontEnumerator.cs`.
- **Private/custom fonts**: font files can be registered via `PdfRendererOptions.AddPrivateFont(...)`, which also registers the file into the Skia-based `FontManager`. See `src/Genocs.Fonet/Pdf/Gdi/GdiPrivateFontCollection.cs`.

## How the pipeline works (high level)

- Entry point: `Genocs.Fonet.FonetDriver` parses FO input and drives rendering. See `src/Genocs.Fonet/FonetDriver.cs`.
- Rendering: `Fonet.Render.Pdf.PdfRenderer` outputs PDF; font setup is driven by `Fonet.Render.Pdf.FontSetup` which enumerates system fonts (via the Skia-backed GDI compatibility layer) and maps FO font triplets to PDF fonts. See `src/Genocs.Fonet/Render/Pdf/FontSetup.cs`.
- Images: `<fo:external-graphic>` is loaded via `Fonet.Image.FonetImageFactory` and stored as a `Fonet.Image.FonetImage` which is embedded as a PDF XObject. See `src/Genocs.Fonet/Image/ApocImageFactory.cs` and `src/Genocs.Fonet/Pdf/PdfCreator.cs`.

## Using a custom font (example: Nunito)

```csharp
using Genocs.Fonet;
using Genocs.Fonet.Render.Pdf;

var driver = FonetDriver.Make();
driver.Options = new PdfRendererOptions();
driver.Options.AddPrivateFont(new FileInfo("fonts/Nunito-Regular.ttf"));
driver.Options.AddPrivateFont(new FileInfo("fonts/Nunito-Bold.ttf"));
```

Then reference it in FO:

```xml
<fo:block font-family="Nunito" font-size="16pt">
  Hello Nunito
</fo:block>
```

The test project carries Nunito font files and FO templates under `src/tests/Genocs.Fonet.Tests/fonts` and `src/tests/Genocs.Fonet.Tests/templates`.

## Build / test

```powershell
dotnet build fonet.slnx -c Release
dotnet test fonet.slnx -c Release
```

## PDF Web API

`src/WebApi` is a Genocs minimal API that builds PDFs via the same `XslFoPdfService` pipeline as the Host console sample. Templates, fonts, and assets are loaded from configurable paths (Docker volumes in compose).

```bash
./scripts/build-images-docker-compose.sh
# POST http://localhost:8080/api/pdf  (see src/WebApi/README.md)
```

## Known limitations

These are actively tracked in [docs/known-issues.md](./docs/known-issues.md) and [docs/migration-plan.md](./docs/migration-plan.md):

1. **~87 unimplemented FO properties** — log warnings and are ignored during layout.
2. **11 unimplemented FO elements** — parse but produce no layout output.
3. **Side-float layout** — `fo:float` renders in flow; `float`/`clear`/`z-index` parse but do not affect layout.
4. **System font discovery** — recursive filesystem scan with filename matching; can be imprecise on Linux/macOS.
5. **CJK / complex scripts** — not validated with fixture tests yet.
6. **PDF encryption** — legacy RC4 only.

## Migration progress

**Phase 0 (stabilize)** — ✅ complete.

**Phase 1 (font pipeline)** — ✅ complete. Glyph mapping, font table access, embedding/subsetting, and font descriptor metrics.

**Phase 2 (quality)** — ✅ complete. Image spans, PDF encoding, namespace consolidation, error reporting.

**Phase 3 (FO completeness)** — 🔄 in progress. Tier 1 properties and table captions done; side-float layout deferred.
