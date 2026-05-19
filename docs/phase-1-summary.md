# Phase 1 Summary — Font Pipeline

Completed June 2026.

## What Was Implemented

### Core Infrastructure

| Component | File | Purpose |
|-----------|------|---------|
| `FontTableAccess` | `Pdf/Gdi/FontTableAccess.cs` | Read TrueType/OpenType tables from font files without GDI or `OpenStream` |
| `CmapReader` | `Pdf/Gdi/CmapReader.cs` | Parse cmap tables (formats 0, 4, 12) for glyph mapping and Unicode coverage |
| `NameTableParser` | `Pdf/Gdi/FontTableAccess.cs` | Extract family/PostScript names from name table |

### LibWrapper Replacement

`LibWrapper` now delegates to real implementations via registered `GdiDeviceContent` handles:

- `GetFontData` → `FontTableAccess` (file-based table reads)
- `GetGlyphIndices` → `FontManager.GetGlyphIndices` (cmap + SkiaSharp fallback)
- `GetTextFace` → `FontTableAccess.ReadFamilyName`

Dead code removed: `EnumFontFamilies`, `EnumFontFamiliesEx`.

### FontManager Enhancements

- **Glyph mapping** via `CmapReader` with `SKFont` fallback
- **File path tracking** for private and system fonts
- **Thread-safe caches** (`ConcurrentDictionary`) for parallel test execution
- **System font discovery** with path caching and filename matching

### Unicode Range Detection

`GdiUnicodeRanges` now builds ranges from cmap coverage instead of hardcoded Latin blocks. `UnicodeRange.MapCharacter` maps per-character via `FontManager.GetGlyphIndex`.

### Font Metrics

`GdiFontMetrics` now computes:

- `StemV` — from OS/2 avgCharWidth or cap height
- `AverageWidth` — from OS/2 table
- `MaxWidth` — from hmtx table scan

### Font Subsetting

- `FontSubset` skips optional tables (`cvt`, `prep`, `fpgm`) when absent
- `OS2Table`, `NameTable`, `PostTable` `Write()` preserve raw table bytes

### Tests Added

`FontPipelineTests.cs` (4 tests):

| Test | Validates |
|------|-----------|
| `NunitoGlyphMapping_ReturnsNonZeroForLatinCharacters` | cmap glyph mapping |
| `FontTableAccess_ReadsHeadTableFromNunito` | Table directory parsing |
| `EmbeddedNunitoPdf_ContainsFontDescriptorMetrics` | Full font embedding |
| `SubsetNunitoPdf_ContainsEmbeddedFontProgram` | Font subsetting |

**Total test count:** 9 tests × 3 TFMs = 27 passing runs.

## Known Remaining Gaps

| Gap | Priority | Notes |
|-----|----------|-------|
| CJK validation | Medium | cmap format 12 supported; no CJK font fixture test yet |
| `Gdi*` type rename | Low | Deferred to Phase 2 |
| PostScript name font discovery | Low | Filename matching only; name table matching removed for performance |
| System font on Linux/macOS CI | Medium | Needs CI run to validate |

## Architecture After Phase 1

```
TrueTypeFont / Type2CIDFont
  └── GdiFont.CreateDesignFont
        └── GdiFontMetrics
              ├── FontManager.GetFontData (file bytes)
              ├── FontTableAccess.ReadTable (per-table)
              ├── CmapReader (glyph mapping)
              └── FontFileReader (metrics parsing)

LibWrapper (thin shim)
  └── GdiDeviceContent registry → FontManager / FontTableAccess
```
