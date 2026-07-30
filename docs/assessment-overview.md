# Assessment Overview

## Purpose

Genocs.Fonet is a .NET port of **Fonet**, an XSL-FO (Extensible Stylesheet Language Formatting Objects) formatter that produces PDF output. The original implementation relied on **Windows GDI** for font enumeration, metrics, glyph mapping, and image decoding. This port aims to run on **modern .NET without Windows graphics dependencies**.

This assessment evaluates the current state of that migration and identifies what remains before the library can be used reliably in production.

**Last updated:** August 2026

## Executive Summary

The migration has made substantial progress through Phases 0–2 and the initial Phase 3 Tier 1 batch:

- Targets cross-platform .NET (`net8.0`, `net9.0`, `net10.0`)
- Removed `System.Drawing` and native GDI P/Invoke
- Integrated **SkiaSharp** for font typeface loading and image decoding
- Preserved the original Fonet architecture (parse → layout → render → custom PDF writer)
- Font pipeline complete: glyph mapping via `CmapReader`, file-based font table access, subsetting, and font descriptor metrics
- Quality improvements: span-based image extraction, structured error reporting, namespace consolidation
- CI via GitHub Actions; 22 tests with PDF structure validation on net8/9/10

However, the library **cannot be considered production-ready**. ~87 XSL-FO properties and 11 elements remain stubbed. Side-float layout, RTL/bidi, and CJK text need further work and validation.

### Readiness Scorecard

| Dimension | Rating | Notes |
|-----------|--------|-------|
| Compilability | ✅ Good | Solution builds cleanly on net8/9/10 |
| Cross-platform deps | ✅ Good | Single NuGet dep: SkiaSharp 2.88.9 |
| FO parsing | ✅ Good | Largely platform-agnostic |
| Layout engine | ⚠️ Partial | Core block/inline/table works; ~87 properties stubbed |
| PDF generation | ⚠️ Partial | Custom writer functional for basic output |
| Font handling | ✅ Good (Latin) | Custom + system fonts work; CJK needs validation |
| Image handling | ✅ Good | SkiaSharp with span-based pixel extraction |
| Test coverage | ⚠️ Partial | 22 tests with PDF structure validation; no content/visual checks |
| CI / automation | ✅ Good | GitHub Actions build, test, pack workflows |
| Documentation | ✅ Good | Migration docs updated through Phase 3 |

## What Works Today

Based on code review and the 22 passing tests:

- End-to-end FO → PDF for simple documents (text blocks, basic tables, lists)
- PDF Base-14 fonts (Helvetica, Times, Courier families)
- Custom/private font registration via `PdfRendererOptions.AddPrivateFont`
- Custom font embedding with correct glyph mapping and subsetting (Nunito tests)
- Image embedding via `<fo:external-graphic>` (SkiaSharp decode)
- Expression evaluation for common length/number properties
- Custom PDF writer (no third-party PDF library dependency)
- Phase 3 Tier 1: `visibility`, `word-spacing`, `margin` shorthand, table captions

## What Does Not Work Reliably

- **Side-float layout** — `fo:float` renders in flow; `float`/`clear`/`z-index` parse but do not affect layout
- **Advanced XSL-FO features** — bidi, aural properties, multi-switch, inline-container, etc.
- **Unicode / CJK text** — cmap format 12 supported in code but not validated with CJK font fixtures
- **Cross-platform system font discovery** — recursive filesystem scan with filename substring matching; imprecise on Linux/macOS
- **PDF encryption** — legacy RC4 only; non-ASCII password encoding incomplete
- **~87 stubbed FO properties** — log warnings and are ignored during layout

## Completed Migration Phases

| Phase | Status | Key deliverables |
|-------|--------|------------------|
| Phase 0: Stabilize | ✅ Complete | CI, `PdfTestBase`, `PdfAssertions`, SkiaSharp crash fix |
| Phase 1: Font Pipeline | ✅ Complete | `FontTableAccess`, `CmapReader`, glyph mapping, subsetting, metrics |
| Phase 2: Quality | ✅ Complete | Image spans, PDF encoding, namespaces, error reporting, kerning |
| Phase 3: FO Completeness | 🔄 In progress | Tier 1 properties/elements done; side-float deferred |

See [Migration Plan](./migration-plan.md) and phase summaries for details.

## Recommended Next Steps

1. **Complete Phase 3** — side-float layout, Tier 2 i18n properties, consumer template triage
2. **CJK validation** — add CJK font fixture test and cross-platform CI coverage
3. **Deepen tests** — glyph content validation, visual regression, negative cases
4. **Phase 4 hardening** — NuGet publish, AES encryption, performance benchmarks

## Assessment Methodology

This assessment was produced by:

- Static analysis of ~658 source files across `Fo/`, `Layout/`, `Pdf/`, `Render/`, `Image/`, `DataTypes/`
- Grep for `TODO`, `FIXME`, `NotImplementedException`, `ToBeImplemented`, platform conditionals
- Review of test project (22 tests) and FO templates
- Architecture tracing from `FonetDriver.Render()` through layout to `PdfRenderer` and `PdfCreator`
- Verification against Phase 1–3 implementation summaries
