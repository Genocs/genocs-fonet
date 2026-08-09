# Genocs.Fonet.XsltTransformer

![Genocs.Fonet Banner](https://raw.githubusercontent.com/Genocs/genocs-fonet/main/assets/banner.png)

XML → XSLT → XSL-FO → PDF transformation helpers for Genocs applications. Builds on `Genocs.Fonet` to turn printable documents and XSLT templates into PDF streams. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Fonet.XsltTransformer
```

This package depends on `Genocs.Fonet`, `Genocs.Core`, and `Genocs.Logging`.

## Getting Started

Implement `IPrintableDocument` (or `IPayload`) for your model, place XSLT templates where `ResourceManager` can load them, then generate a PDF:

```csharp
using Genocs.Fonet.XsltTransformer.Transformers;
using Microsoft.Extensions.Logging.Abstractions;

IPrintableDocument document = /* your document */;
var pdfService = new XslFoPdfService(NullLogger<XslFoPdfService>.Instance);

using Stream pdf = pdfService.Print(
    document,
    templateName: "invoice.fo",
    resourcesName: "resources.xml",
    fontsDirectory: "fonts",
    countryId: "IT");
```

Or render an already-produced XSL-FO `XmlDocument` directly:

```csharp
using Genocs.Fonet.XsltTransformer.Transformers;

PdfPrinterDriver.MakePdf(xslFoDocument, "output.pdf", fontDir: "fonts");
// or
using Stream pdf = PdfPrinterDriver.MakePdfStream(xslFoDocument, fontDir: "fonts");
```

## Main Entry Points

- `IPdfWriterService` / `XslFoPdfService`
- `IPrintableDocument` / `IPayload`
- `PdfPrinterDriver` (`MakePdf`, `MakePdfStream`)
- `XmlTransformationManager`
- `ObjectXmlSerializer`
- `ResourceManager`
- `XsltExtensions`
- `GlobalSettings`

## Pipeline Overview

1. Serialize or supply an `IPrintableDocument` as XML (`ToXml()`).
2. Load the XSLT template (and optional localized resources) via `ResourceManager`.
3. Transform XML → XSL-FO with `XmlTransformationManager`.
4. Render FO → PDF through `PdfPrinterDriver` / `Genocs.Fonet.FonetDriver`.

When a fonts directory is provided, private fonts are registered and embedded (`FontType.Embed`) so output is portable.

## Configuration Notes

- `XslFoPdfService.Print` requires a non-empty `templateName`.
- `fontsDirectory` is resolved relative to the executing folder when needed (`XsltExtensions.GetExecutingFolder`).
- `countryId` selects localized XSLT/resource variants when present.
- `PdfPrinterDriver` image handling accepts `data:image/...;base64,...` sources used by `fo:external-graphic`.
- Template and localization base paths are controlled through `ResourceManager` / `GlobalSettings` (`XsltFolderPath`, `LocalizationXmlFolderPath`, `DefaultCulture`, `DefaultDateFormat`).

## Package Relationship

| Package | Role |
|---------|------|
| `Genocs.Fonet` | Core XSL-FO → PDF engine |
| `Genocs.Fonet.XsltTransformer` | Opinionated XML/XSLT workflow on top of Genocs.Fonet |

Use `Genocs.Fonet` alone when you already produce XSL-FO. Use this package when templates, localization XML, and document models drive PDF generation.

## Linux / Docker Notes

PDF rendering relies on SkiaSharp native assets from `Genocs.Fonet`. In Docker/Linux hosts, reference `SkiaSharp.NativeAssets.Linux` on the executable project if `libSkiaSharp.so` is missing at runtime.

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-fonet/tree/main/docs
- Repository: https://github.com/Genocs/genocs-fonet

## Release Notes

- Releases: https://github.com/Genocs/genocs-fonet/releases
