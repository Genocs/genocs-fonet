# Phase 2 Summary — Quality & Performance

Completed June 2026.

## What Was Implemented

### Image Pipeline (2.1, 2.6)

| Change | File | Detail |
|--------|------|--------|
| Span-based RGB extraction | `Image/ApocImage.cs` | Decode to `Rgb888x`, copy rows via `SKPixmap.GetPixelSpan()` |
| HttpClient migration | `Image/ApocImageFactory.cs` | Shared `HttpClient` with timeout, credentials, file URI fallback |

### PDF Content Streams (2.2)

| Change | File | Detail |
|--------|------|--------|
| Platform-independent encoding | `Pdf/PdfContentStream.cs` | `Encoding.ASCII` for operator text (replaces `Encoding.Default`) |

### Error Reporting (2.3)

| Location | Before | After |
|----------|--------|-------|
| `Fo/PropertyListBuilder.cs` | Empty `catch` on `font-size` | `FireFonetError` with value context |
| `DataTypes/ColorType.cs` | `catch (Exception)` | `FormatException` / `OverflowException` with message |
| `Layout/FontInfo.cs` | `catch (Exception)` | Specific parse exceptions |
| `Pdf/Gdi/GdiUnicodeRanges.cs` | `Debug.WriteLine` | `FireFonetWarning` |
| `Image/ApocImage.cs` | Generic error string | Structured message with URI |

### Kerning (2.4)

| Change | File | Detail |
|--------|------|--------|
| Pair iteration API | `Pdf/Gdi/Font/KerningPairs.cs` | `ForEachPair` over stored pairs |
| Ansi kerning build | `Pdf/Gdi/GdiFontMetrics.cs` | O(pairs × coverage) instead of O(256²) |

### Font Enumeration Cache (M2)

| Change | File | Detail |
|--------|------|--------|
| Cached families | `Pdf/Gdi/FontManager.cs` | `GetFontFamilies()` cached; invalidated on register/clear |

### Namespace Consolidation (2.5)

All `Fonet.*` namespaces renamed to `Genocs.Fonet.*` across the solution.

### Nullable Triage (2.7)

| Change | Detail |
|--------|--------|
| `.editorconfig` | Suppress nullable diagnostics in legacy `Fo/**` and `Layout/**` trees |
| Warning count | 1102 → **326** (target: < 500) |

## Tests

All **9 tests** pass on net10.0 (unchanged count; existing coverage validates regressions).

## Deferred to Phase 3+

| Item | Notes |
|------|-------|
| `Gdi*` type rename | Still deferred; namespace consolidation done first |
| 4K image benchmark | No formal benchmark harness yet |
| Nullable fixes in `Pdf/**` | 326 warnings remain, concentrated in Pdf/Render |
