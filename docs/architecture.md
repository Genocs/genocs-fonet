# Architecture

## High-Level Pipeline

Genocs.Fonet follows the classic **Apache FOP / Fonet** processing model:

```
┌─────────────┐    ┌──────────────┐    ┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│  XSL-FO     │───▶│  FO Object   │───▶│  Area Tree  │───▶│  PDF Render  │───▶│  PDF Output │
│  (.fo XML)  │    │  Tree        │    │  (Layout)   │    │  (Content)   │    │  (Stream)   │
└─────────────┘    └──────────────┘    └─────────────┘    └──────────────┘    └─────────────┘
     Parse              Format              Queue               Write
```

### Stage 1: Parse

| Component | Path | Role |
|-----------|------|------|
| Entry point | `FonetDriver.cs` | Creates renderer, stream handler, tree builder |
| XML parsing | `Fo/FOTreeBuilder.cs` | SAX-style reader → FO object tree |
| Element mapping | `Fo/StandardElementMapping.cs` | Maps XML elements to `FObj` subclasses |
| Property parsing | `Fo/PropertyListBuilder.cs` | Resolves attributes to `Property` objects |

### Stage 2: Layout (Format)

| Component | Path | Role |
|-----------|------|------|
| Page sequencing | `Fo/Pagination/PageSequence.cs` | Drives pagination |
| Area tree | `Layout/*.cs` (~37 files) | Pages, blocks, lines, inline areas |
| Font state | `Layout/FontState.cs`, `Layout/FontInfo.cs` | Active font during layout |

### Stage 3: Render Queue

| Component | Path | Role |
|-----------|------|------|
| Stream renderer | `StreamRenderer.cs` | Queues pages, resolves ID references, handles markers |
| PDF renderer | `Render/Pdf/PdfRenderer.cs` (~1,375 lines) | Area tree → PDF content stream operators |

### Stage 4: PDF Write

| Component | Path | Role |
|-----------|------|------|
| PDF creator | `Pdf/PdfCreator.cs` | Orchestrates document assembly |
| PDF document model | `Pdf/PdfDocument.cs`, `PdfWriter.cs`, `XRefTable.cs` | Custom PDF 1.3 writer |
| Font embedding | `Render/Pdf/FontSetup.cs`, `Render/Pdf/Fonts/*` | Base-14 + TrueType/CID fonts |
| Images | `Image/ApocImageFactory.cs`, `Image/ApocImage.cs` | External graphics → PDF XObjects |
| Security | `Pdf/Security/SecurityManager.cs`, `Arc4.cs` | RC4 encryption (legacy) |

## Module Layout

```
src/Genocs.Fonet/
├── FonetDriver.cs          # Public API entry
├── StreamRenderer.cs       # Render orchestration
├── Fo/                     # XSL-FO object model (~400 files)
│   ├── Flow/               # Block, Inline, Table, List, etc.
│   ├── Pagination/         # Page-sequence, page-masters
│   ├── Properties/         # Property makers (one per FO property)
│   └── Expr/               # XPath-like expression functions
├── Layout/                 # Area tree and layout algorithms
├── Render/Pdf/             # PDF rendering backend
│   └── Fonts/              # Base14, TrueType, Type2CID, ProxyFont
├── Pdf/                    # Custom PDF object model and writer
│   ├── Gdi/                # ⚠️ GDI compatibility shim (SkiaSharp-backed)
│   │   ├── Font/           # TrueType table parsing/subsetting
│   │   └── Structures/     # Legacy Win32 struct shapes
│   ├── Filter/             # Flate, DCT, RunLength compression
│   └── Security/           # Encryption
├── Image/                  # Image loading (SkiaSharp)
├── DataTypes/              # Length, Color, Keep, etc.
├── Apps/                   # FormattingResults API
└── Util/                   # String utilities
```

## Namespace Split (Migration Friction)

The codebase uses **two namespace families**:

| Namespace | Usage |
|-----------|-------|
| `Genocs.Fonet` | Public API (`FonetDriver`, `PdfRendererOptions`) |
| `Fonet.Fo.*` | FO object model, layout |
| `Fonet.Pdf.*` | PDF internals |
| `Fonet.Render.Pdf.*` | PDF renderer |
| `Genocs.Fonet.Pdf.Gdi` | Font compatibility layer |

This split reflects a partial rename during migration. It causes resolution friction (e.g., `FonetDriver` referenced from `Fonet.*` namespaces) and should be consolidated in a later phase.

## Rendering Backend

### PDF Output

The library uses a **custom in-house PDF writer** — not PdfSharp, iText, QuestPDF, or similar. This is pure C# and platform-independent. It generates PDF 1.3-compatible output with:

- Content streams (text, vector graphics, images)
- Font dictionaries (Type1, TrueType, Type0/CID)
- Annotations and links
- Optional RC4 encryption

### Font Pipeline (Critical Path)

```
FontSetup
  └── ProxyFont
        └── TrueTypeFont / Type2CIDFont
              └── GdiDeviceContent
                    └── GdiFontMetrics
                          ├── FontManager (SkiaSharp SKTypeface)
                          └── LibWrapper (⚠️ stubs)
```

- **Base-14 fonts**: Built-in metrics, no GDI dependency — works reliably
- **System/custom TrueType**: Routes through GDI shim → SkiaSharp hybrid
- **CID fonts**: Unicode mapping via `GdiUnicodeRanges` (hardcoded Latin ranges)

### Image Pipeline

```
ApocImageFactory → ApocImage (SkiaSharp SKCodec/SKBitmap) → PdfCreator (XObject embed)
```

Images no longer use `System.Drawing`. The non-JPEG path decodes to `SKBitmap` and extracts pixels row-by-row (slow for large images).

## Technology Stack

| Concern | Technology |
|---------|------------|
| Runtime | .NET 8 / 9 / 10 |
| Font loading | SkiaSharp 2.88.9 (`SKTypeface`, `SKFontManager`) |
| Image decode | SkiaSharp (`SKCodec`, `SKBitmap`) |
| PDF generation | Custom writer (pure C#) |
| Unsafe code | Enabled (`AllowUnsafeBlocks`) for TrueType table parsing |
| Tests | xUnit 2.9.3 |

## What Is NOT in the Architecture

- No alternate render targets (AWT, SVG, Print)
- No HarfBuzz or complex text shaping
- No fontconfig P/Invoke on Linux
- No CI/CD pipeline
- No visual regression testing infrastructure

## Data Flow Example

A minimal FO document:

```xml
<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
  <fo:layout-master-set>...</fo:layout-master-set>
  <fo:page-sequence master-reference="...">
    <fo:flow flow-name="xsl-region-body">
      <fo:block font-family="Helvetica" font-size="12pt">Hello</fo:block>
    </fo:flow>
  </fo:page-sequence>
</fo:root>
```

1. `FOTreeBuilder` parses XML → `Root`, `PageSequence`, `Block` objects
2. `PageSequence.Format()` creates `Page` areas with `BlockArea` children
3. `StreamRenderer` queues the page, resolves any `id` references
4. `PdfRenderer.Render(page)` writes text operators using embedded Helvetica metrics
5. `PdfCreator` assembles objects, writes xref table, outputs to stream
