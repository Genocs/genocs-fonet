# Assessment Overview

## Purpose

Genocs.Fonet is a .NET port of **Fonet**, an XSL-FO (Extensible Stylesheet Language Formatting Objects) formatter that produces PDF output. The original implementation relied on **Windows GDI** for font enumeration, metrics, glyph mapping, and image decoding. This port aims to run on **modern .NET without Windows graphics dependencies**.

This assessment evaluates the current state of that migration and identifies what remains before the library can be used reliably in production.

## Executive Summary

The migration is **underway but incomplete**. The codebase has made meaningful progress:

- Targets cross-platform .NET (`net8.0`, `net9.0`, `net10.0`)
- Removed `System.Drawing` and native GDI P/Invoke
- Integrated **SkiaSharp** for font typeface loading and image decoding
- Preserved the original Fonet architecture (parse → layout → render → custom PDF writer)

However, the library **cannot be considered production-ready**. The most critical gap is the **GDI compatibility shim** (`Pdf/Gdi/`) — it preserves the old API surface but many methods are stubs that return dummy values. Font glyph mapping, Unicode range detection, and font table access through this layer are broken or approximate. Combined with ~90 unimplemented XSL-FO properties and minimal test coverage, real-world FO documents will produce incorrect or incomplete PDFs.

### Readiness Scorecard

| Dimension | Rating | Notes |
|-----------|--------|-------|
| Compilability | ✅ Good | Solution builds; ~3,000+ nullable warnings |
| Cross-platform deps | ✅ Good | Single NuGet dep: SkiaSharp 2.88.9 |
| FO parsing | ✅ Good | Largely platform-agnostic |
| Layout engine | ⚠️ Partial | Core block/inline/table works; many properties stubbed |
| PDF generation | ⚠️ Partial | Custom writer functional for basic output |
| Font handling | ⚠️ Partial | Phase 1 complete for Latin/custom fonts; CJK needs CI validation |
| Image handling | ⚠️ Partial | SkiaSharp works; per-pixel extraction is slow |
| Test coverage | ❌ Poor | 5 smoke tests, no content validation |
| CI / automation | ❌ None | No GitHub Actions or cross-platform CI |
| Documentation | ⚠️ Partial | Root README exists but is partially stale |

## What Works Today

Based on code review and existing smoke tests:

- End-to-end FO → PDF for simple documents (text blocks, basic tables, lists)
- PDF Base-14 fonts (Helvetica, Times, Courier families)
- Custom/private font registration via `PdfRendererOptions.AddPrivateFont`
- Image embedding via `<fo:external-graphic>` (SkiaSharp decode)
- Expression evaluation for common length/number properties (actively being fixed)
- Custom PDF writer (no third-party PDF library dependency)

## What Does Not Work Reliably

- **System TrueType font metrics** when code paths hit `LibWrapper` stubs
- **Glyph index mapping** for embedded/subset fonts (`GetGlyphIndices` returns count but never fills indices)
- **Unicode / CJK text** — hardcoded Latin ranges; `Type2CIDFont` subsetting incomplete
- **Advanced XSL-FO features** — floats, captions, bidi, aural properties, multi-switch, etc.
- **Font subsetting table writes** — `OS2Table`, `NameTable`, `PostTable` throw `NotImplementedException`
- **Cross-platform font discovery** — recursive filesystem scan with filename substring matching
- **PDF encryption** — legacy RC4 only; non-ASCII password encoding incomplete

## Active Development Areas

Recent uncommitted changes (as of assessment date) focus on:

- Nullable/type fixes in `DataTypes/` (Length, Color, PercentLength, etc.)
- Expression engine hardening (`Fo/Expr/Numeric.cs`, Min/Max/Abs functions)
- Property system alignment (`Property.cs`, `LengthProperty.cs`, `NumberProperty.cs`)
- Cross-platform test template expansion (`CrossPlatformTest.fo`)

Direction of travel: stabilizing the type system and expression evaluation — **not yet** removing the GDI shim.

## Stale Documentation Note

The root `README.md` states the solution "does not currently build cleanly" (dated 2026-06-12). That is **no longer accurate** — `BoxPropShorthandParser` exists and the solution compiles. The README should be updated to reflect current build status while documenting remaining functional limitations.

## Recommended Next Steps

See [Migration Plan](./migration-plan.md) for the full phased roadmap. Immediate priorities:

1. **Fix the font pipeline** — replace `LibWrapper` stubs with SkiaSharp-backed implementations
2. **Harden tests** — add PDF structure validation, not just file existence
3. **Triage FO features** — implement properties/elements required by target document templates
4. **Add CI** — build and test on Windows, Linux, and macOS

## Assessment Methodology

This assessment was produced by:

- Static analysis of ~658 source files across `Fo/`, `Layout/`, `Pdf/`, `Render/`, `Image/`, `DataTypes/`
- Grep for `TODO`, `FIXME`, `NotImplementedException`, `ToBeImplemented`, platform conditionals
- Review of test project and FO templates
- Architecture tracing from `FonetDriver.Render()` through layout to `PdfRenderer` and `PdfCreator`
- Comparison of `LibWrapper` against original GDI expectations
