# Genocs.Fonet

![Genocs.Fonet Banner](https://raw.githubusercontent.com/Genocs/genocs-fonet/main/assets/banner.png)

XSL-FO to PDF library for Genocs applications. A cross-platform .NET port of Fonet that renders Formatting Objects to PDF without Windows GDI dependencies. Supports `net10.0`, `net9.0`, and `net8.0`.

## Installation

```bash
dotnet add package Genocs.Fonet
```

## Getting Started

Use this package when you already have XSL-FO input and need PDF output. Font enumeration, typeface loading, and image decoding are backed by SkiaSharp so the library runs on Windows, Linux, and macOS.

```csharp
using Genocs.Fonet;
using Genocs.Fonet.Render.Pdf;

var driver = FonetDriver.Make();
driver.Options = new PdfRendererOptions
{
    Title = "Sample document",
    Author = "Genocs",
    FontType = FontType.Embed
};

driver.Options.AddPrivateFont(new FileInfo("fonts/Nunito-Regular.ttf"));
driver.Options.AddPrivateFont(new FileInfo("fonts/Nunito-Bold.ttf"));

using var input = File.OpenRead("document.fo");
using var output = File.Create("document.pdf");
driver.Render(input, output);
```

Reference the private font from FO:

```xml
<fo:block font-family="Nunito" font-size="16pt">
  Hello Nunito
</fo:block>
```

You can also render from an `XmlDocument` or `XmlReader`:

```csharp
var doc = new XmlDocument();
doc.Load("document.fo");
driver.Render(doc, output);
```

## Main Entry Points

- `FonetDriver` / `FonetDriver.Make`
- `FonetDriver.Render` (stream, `XmlDocument`, or `XmlReader`)
- `PdfRendererOptions`
- `PdfRendererOptions.AddPrivateFont`
- `FontType` (`Link` or `Embed`)
- `FonetDriver.BaseDirectory` (resolve relative external resources)
- `FonetDriver.ImageHandler` (custom `external-graphic` loading)

## Configuration Notes

- `PdfRendererOptions.FontType` defaults to `FontType.Link`. Prefer `FontType.Embed` when PDFs must render identically across machines.
- Register custom fonts with `AddPrivateFont` before calling `Render`.
- `BaseDirectory` defaults to the current working directory and is used to resolve relative image paths.
- `Timeout` controls HTTP fetches for remote resources (default `100000` ms).
- Optional PDF metadata: `Title`, `Subject`, `Author`, keywords via `AddKeyword`.
- Optional encryption uses legacy RC4 via `OwnerPassword` / `UserPassword` and permission flags (`EnablePrinting`, `EnableModify`, `EnableCopy`, `EnableAdd`).

## Package Relationship

| Package | Role |
|---------|------|
| `Genocs.Fonet` | Core XSL-FO → PDF engine |
| `Genocs.Fonet.XsltTransformer` | Higher-level XML → XSLT → FO → PDF pipeline built on this package |

Install `Genocs.Fonet.XsltTransformer` when you want to transform domain XML with XSLT templates instead of supplying FO directly.

## Linux / Docker Notes

This package references `SkiaSharp` and `SkiaSharp.NativeAssets.Linux`. For self-contained Docker publishes, also reference `SkiaSharp.NativeAssets.Linux` in the host project if native assets are not copied transitively.

## Known Limitations

This library is under active migration. Before production use, review:

- Unimplemented FO properties and elements (stubs log warnings)
- Side-float layout gaps
- System font discovery precision on Linux/macOS
- CJK / complex script validation status
- Legacy RC4-only PDF encryption

Details live in the repository docs linked below.

## Support

- Documentation Portal: https://genocs-blog.netlify.app/
- Documentation: https://github.com/Genocs/genocs-fonet/tree/main/docs
- Repository: https://github.com/Genocs/genocs-fonet

## Release Notes

- Releases: https://github.com/Genocs/genocs-fonet/releases
