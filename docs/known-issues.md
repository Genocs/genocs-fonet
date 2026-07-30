# Known Issues

Issues identified during the migration assessment, categorized by severity.

**Last updated:** August 2026

**Severity legend:**
- **Critical** — Causes crashes, silent data corruption, or makes core features unusable
- **High** — Significant functional gaps affecting common use cases
- **Medium** — Degraded behavior, performance, or maintainability concerns
- **Low** — Technical debt, cosmetic, or edge-case issues

---

## Open Issues

### High

#### H-01: ~87 unimplemented XSL-FO properties

| Field | Value |
|-------|-------|
| Pattern | `ToBeImplementedProperty.Maker` in `Fo/Properties/*Maker.cs` |
| Behavior | Logs warning via `FireFonetWarning`, property is no-op |

Includes: background shorthands, bidi, speech/aural, advanced table features, page breaks, etc. See [Feature Completeness](./feature-completeness.md) for the full list.

Phase 3 Tier 1 properties (`visibility`, `word-spacing`, `margin`, `caption-side`) are now implemented. `float`, `clear`, and `z-index` parse but have no layout effect yet.

---

#### H-02: 11 unimplemented XSL-FO elements

| Field | Value |
|-------|-------|
| Base class | `Fo/ToBeImplementedElement.cs` |
| Elements | `InlineContainer`, `BidiOverride`, `MultiSwitch`, `MultiCase`, `MultiToggle`, `MultiProperties`, `MultiPropertySet`, `InitialPropertySet`, `Title`, `Declarations`, `ColorProfile` |

These elements parse but produce no layout output. `Float`, `TableCaption`, and `TableAndCaption` were implemented in Phase 3.

---

#### H-03: `from-table-column()` expression unimplemented

| Field | Value |
|-------|-------|
| File | `src/Genocs.Fonet/Fo/Expr/FromTableColumnFunction.cs` |
| Behavior | Throws at runtime |

---

#### H-04: Side-float layout not implemented

| Field | Value |
|-------|-------|
| Files | `Fo/Flow/Float.cs`, `Fo/Properties/FloatMaker.cs`, `Fo/Properties/ClearMaker.cs` |
| Behavior | `fo:float` renders children in normal flow order; `float`/`clear`/`z-index` properties parse but do not affect layout |

---

#### H-05: CJK / complex script validation missing

| Field | Value |
|-------|-------|
| Files | `Pdf/Gdi/CmapReader.cs`, `Pdf/Gdi/GdiUnicodeRanges.cs` |
| Behavior | cmap format 12 supported in code; no CJK font fixture test or cross-platform CI validation |

---

### Medium

#### M-01: Platform-specific font discovery is brittle

| File | `Pdf/Gdi/FontManager.cs` |
| Approach | `RuntimeInformation.IsOSPlatform` + hardcoded font directories + filename `Contains()` matching |
| Risk | Wrong face selection, slow scans on Linux/macOS |

---

#### M-02: Test coverage is shallow

| Field | Value |
|-------|-------|
| Tests | 22 (11 integration + 4 font pipeline + 7 Phase 3) |
| Assertions | PDF header, page count, size bounds via `PdfAssertions` |
| Gap | No glyph content validation, no visual regression, no negative tests |

---

#### M-03: ~326 nullable reference warnings

Build succeeds; warnings concentrated in `Pdf/**` and `Render/**` after `.editorconfig` triage in legacy `Fo/**` and `Layout/**` trees.

---

#### M-04: `Gdi*` naming retained

Types under `Pdf/Gdi/` retain Win32-shaped names (`GdiFont`, `GdiFontMetrics`, etc.) despite SkiaSharp-backed implementations. Deferred rename creates ongoing confusion for contributors.

---

### Low

#### L-01: Stale Win32 comments

`TrueTypeFont.cs`, `Type2CIDFont.cs` still reference "Win32 HDC" / "Win32 Api" despite SkiaSharp wrappers.

---

#### L-02: Legacy RC4 PDF encryption

`Pdf/Security/Arc4.cs` — does not meet modern PDF security expectations (AES per PDF 2.0).

---

#### L-03: Miscellaneous TODOs (~20)

Scattered across `PdfRenderer.cs`, `GdiKerningPairs.cs`, `HeaderTable.cs`, `HorizontalHeaderTable.cs`, `ProxyFont.cs`, `SecurityManager.cs`, `PdfObjectId.cs`.

---

## Resolved Issues

Issues fixed during Phases 0–2. Kept for historical reference.

| ID | Issue | Fixed In | Resolution |
|----|-------|----------|------------|
| C-01 | `LibWrapper.GetFontData` returned 0 | Phase 1 | `FontTableAccess` file-based table reads |
| C-02 | `LibWrapper.GetGlyphIndices` did not populate array | Phase 1 | `CmapReader` + `FontManager.GetGlyphIndices` |
| C-03 | SkiaSharp native crash on font stream open | Phase 0/1 | File-based reads, thread-safe caches |
| C-04 | Hardcoded Unicode ranges | Phase 1 | `GdiUnicodeRanges` uses cmap coverage |
| H-06 | Font table `Write()` threw `NotImplementedException` | Phase 1 | Raw byte preservation in OS2/Name/Post tables |
| H-07 | Test suite unreliable (native crash) | Phase 0 | SkiaSharp crash fixed; 22 tests pass on net8/9/10 |
| M-05 | `PdfContentStream` used platform-dependent encoding | Phase 2 | Uses `Encoding.ASCII` |
| M-06 | O(n²) kerning pair lookup | Phase 2 | Iterates font kerning pairs |
| M-07 | Image pixel extraction slow (per-pixel `GetPixel`) | Phase 2 | `SKPixmap.GetPixelSpan()` |
| M-08 | Namespace split (`Fonet` vs `Genocs.Fonet`) | Phase 2 | Consolidated to `Genocs.Fonet.*` |
| M-09 | Incomplete font descriptor metrics | Phase 1 | StemV, AverageWidth, MaxWidth from OS/2 and hmtx |
| M-10 | `Type2CIDFont` subsetting incomplete | Phase 1 | Optional cvt/prep/fpgm tables handled |
| L-04 | `ApocImageFactory` used obsolete `WebRequest` | Phase 2 | Migrated to `HttpClient` |
| L-05 | No CI configuration | Phase 0 | `.github/workflows/build-and-test.yml` |

## Issue Tracking Template

When fixing issues, update this table:

| ID | Status | Assignee | PR/Commit | Notes |
|----|--------|----------|-----------|-------|
| H-04 | Open | | | Side-float layout for `fo:float` |
| H-05 | Open | | | CJK font fixture test needed |
| ... | | | | |
