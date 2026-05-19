# Known Issues

Issues identified during the migration assessment, categorized by severity.

**Severity legend:**
- **Critical** — Causes crashes, silent data corruption, or makes core features unusable
- **High** — Significant functional gaps affecting common use cases
- **Medium** — Degraded behavior, performance, or maintainability concerns
- **Low** — Technical debt, cosmetic, or edge-case issues

---

## Critical

### C-01: `LibWrapper.GetFontData` always returns 0

| Field | Value |
|-------|-------|
| File | `src/Genocs.Fonet/Pdf/Gdi/LibWrapper.cs` (lines 26–30) |
| Impact | TrueType table reads fail; font embedding/subsetting broken for code paths using this fallback |
| Affected | `GdiFontMetrics.ReadFont()`, `GdiFontCreator.ReadTableData()` |

```csharp
internal static uint GetFontData(...) {
    // For now, return 0 (GDI_ERROR) to indicate not available
    return 0;
}
```

**Fix:** Route font table access through `SKTypeface.OpenStream()` or direct file reads via `FontManager`.

---

### C-02: `LibWrapper.GetGlyphIndices` does not populate glyph array

| Field | Value |
|-------|-------|
| File | `src/Genocs.Fonet/Pdf/Gdi/LibWrapper.cs` (lines 63–67) |
| Impact | All characters map to glyph index 0 in `UnicodeRange.MapCharacter` |
| Affected | `Type2CIDFont.MapCharacter`, embedded TrueType fonts |

```csharp
internal static uint GetGlyphIndices(IntPtr hdc, string lpstr, int c, ushort[] pgi, uint fl) {
    return (uint)c;  // Never fills pgi[]
}
```

**Fix:** Use `SKTypeface.GetGlyphs()` or parse `cmap` table directly.

---

### C-03: SkiaSharp native crash on font stream open

| Field | Value |
|-------|-------|
| Files | `Pdf/Gdi/FontManager.cs`, `Pdf/Gdi/GdiFontMetrics.cs` |
| Impact | Test host aborts (`0xC0000005`) during `CrossPlatformFontAndImageTest` |
| Stack | `FontManager.GetFontData` → `GdiFontMetrics.ReadFont` → `TrueTypeFont.ObtainFontMetrics` |

**Fix:** Harden `SKTypeface.OpenStream()` usage; add file-based fallback; validate font data before native calls.

---

### C-04: Hardcoded Unicode ranges ignore font coverage

| Field | Value |
|-------|-------|
| File | `src/Genocs.Fonet/Pdf/Gdi/GdiUnicodeRanges.cs` (lines 51–88) |
| Impact | Characters outside fixed Latin/punctuation blocks map to glyph 0 |
| Affected | CJK, Arabic, Cyrillic, and other non-Latin scripts |

**Fix:** Query actual font coverage from `cmap` / OS/2 tables or SkiaSharp APIs.

---

## High

### H-01: GDI compatibility layer is mostly no-ops

| Field | Value |
|-------|-------|
| File | `src/Genocs.Fonet/Pdf/Gdi/LibWrapper.cs` |
| Stubs | `GetDC`, `SelectObject`, `DeleteObject`, `GetCurrentObject`, `GetTextFace`, `GetFontUnicodeRanges` |

The shim preserves API shape but does not implement behavior. Silent degradation is worse than explicit failure.

---

### H-02: ~87 unimplemented XSL-FO properties

| Field | Value |
|-------|-------|
| Pattern | `ToBeImplementedProperty.Maker` in `Fo/Properties/*Maker.cs` |
| Behavior | Logs warning via `FireFonetWarning`, property is no-op |

Includes: backgrounds, floats, bidi, speech/aural, advanced table features, z-index, visibility, etc.

---

### H-03: 13 unimplemented XSL-FO elements

| Field | Value |
|-------|-------|
| Base class | `Fo/ToBeImplementedElement.cs` |
| Elements | `Float`, `MultiSwitch`, `MultiCase`, `MultiToggle`, `MultiProperties`, `MultiPropertySet`, `InlineContainer`, `TableCaption`, `TableAndCaption`, `BidiOverride`, `InitialPropertySet`, `Title`, `Declarations`, `ColorProfile` |

These elements parse but produce no layout output.

---

### H-04: Font table write methods throw `NotImplementedException`

| File | Method |
|------|--------|
| `Pdf/Gdi/Font/Tables/OS2Table.cs:261` | `Write()` |
| `Pdf/Gdi/Font/Tables/NameTable.cs:146` | `Write()` |
| `Pdf/Gdi/Font/Tables/PostTable.cs:112` | `Write()` |

Blocks complete font subsetting/rewrite if these tables are needed in output.

---

### H-05: `from-table-column()` expression unimplemented

| Field | Value |
|-------|-------|
| File | `src/Genocs.Fonet/Fo/Expr/FromTableColumnFunction.cs` |
| Behavior | Throws at runtime |

---

### H-06: Test suite is unreliable and shallow

| Field | Value |
|-------|-------|
| File | `src/tests/Genocs.Fonet.Tests/PdfBuilderUnitTests.cs` |
| Tests | 5 integration tests |
| Assertions | `File.Exists()` only — no PDF content validation |
| Reliability | Native crash aborts full test run |

---

### H-07: GDI path is the only font metrics path

No alternate renderer or font backend exists. All system TrueType fonts flow through `GdiDeviceContent` → `GdiFontMetrics` → `LibWrapper`.

---

## Medium

### M-01: Incomplete font descriptor metrics

| File | Properties |
|------|------------|
| `Pdf/Gdi/GdiFontMetrics.cs` | `StemV`, `AverageWidth`, `MaxWidth` return 0 (TODO stubs) |

Affects PDF `/FontDescriptor` dictionary accuracy.

---

### M-02: `Type2CIDFont` subsetting incomplete

| File | `Render/Pdf/Fonts/Type2CIDFont.cs:15` |
| Note | Class-level TODO; `Type2CIDSubsetFont` exists but parent notes gaps |

---

### M-03: Platform-specific font discovery is brittle

| File | `Pdf/Gdi/FontManager.cs` (lines 261–302) |
| Approach | `RuntimeInformation.IsOSPlatform` + hardcoded font directories + filename `Contains()` matching |
| Risk | Wrong face selection, slow scans on Linux/macOS |

---

### M-04: Swallowed exceptions

| File | Behavior |
|------|----------|
| `Fo/PropertyListBuilder.cs:62` | ~~Empty `catch` on `font-size` parse errors~~ Fixed Phase 2 |
| `Pdf/Gdi/FontManager.cs` | Returns empty on missing file (intentional fallback, not bare catch) |
| `DataTypes/ColorType.cs:57` | ~~`catch (Exception)` → black color~~ Fixed Phase 2 |
| `Layout/FontInfo.cs:83` | ~~`catch (Exception)` → weight defaults to 0~~ Fixed Phase 2 |

---

### M-05: `PdfContentStream` uses platform-dependent encoding

| File | `Pdf/PdfContentStream.cs` |
| Issue | ~~`Encoding.Default` for PDF strings~~ Fixed Phase 2 — uses `Encoding.ASCII` |

---

### M-06: O(n²) kerning pair lookup

| File | `Pdf/Gdi/GdiFontMetrics.cs:411` |
| Issue | ~~65,536-iteration cartesian product for ANSI kerning~~ Fixed Phase 2 — iterates font kerning pairs |

---

### M-07: Image pixel extraction is slow

| File | `Image/ApocImage.cs` |
| Issue | ~~Per-pixel `GetPixel(x, y)` loop~~ Fixed Phase 2 — `SKPixmap.GetPixelSpan()` |

---

### M-08: Namespace split (`Fonet` vs `Genocs.Fonet`)

~~Mixed namespaces across the codebase~~ Fixed Phase 2 — consolidated to `Genocs.Fonet.*`

---

### M-09: ~3,000+ nullable reference warnings

Build succeeds; Phase 2 reduced to **326** warnings via `.editorconfig` triage in legacy Fo/Layout trees.

---

## Low

### L-01: Stale Win32 comments

`TrueTypeFont.cs`, `Type2CIDFont.cs` still reference "Win32 HDC" / "Win32 Api" despite SkiaSharp wrappers.

---

### L-02: `ApocImageFactory` uses obsolete `WebRequest`

~~SYSLIB0014 warning~~ Fixed Phase 2 — migrated to `HttpClient`.

---

### L-03: Legacy RC4 PDF encryption

`Pdf/Security/Arc4.cs` — does not meet modern PDF security expectations (AES per PDF 2.0).

---

### L-04: Miscellaneous TODOs (~20)

Scattered across `PdfRenderer.cs`, `GdiKerningPairs.cs`, `HeaderTable.cs`, `HorizontalHeaderTable.cs`, `ProxyFont.cs`, `SecurityManager.cs`, `PdfObjectId.cs`.

---

### L-05: No CI configuration

No `.github/workflows/` or equivalent for automated build/test on multiple platforms.

---

## Issue Tracking Template

When fixing issues, update this table:

| ID | Status | Assignee | PR/Commit | Notes |
|----|--------|----------|-----------|-------|
| C-01 | Fixed | | | `FontTableAccess` + `LibWrapper.GetFontData` |
| C-02 | Fixed | | | `CmapReader` + `FontManager.GetGlyphIndices` |
| C-03 | Fixed | | | Phase 0/1 — file-based reads, thread-safe caches |
| C-04 | Fixed | | | `GdiUnicodeRanges` uses cmap coverage |
| ... | | | | |
