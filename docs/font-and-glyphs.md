# Font & Glyph Migration

Deep dive on replacing the Windows GDI font pipeline with a cross-platform SkiaSharp implementation.

## Background

The original Fonet library used Windows GDI APIs for:

| GDI API | Purpose in Fonet |
|---------|------------------|
| `CreateFontIndirect` | Create font from LOGFONT struct |
| `SelectObject` / `GetCurrentObject` | Manage active font in device context |
| `GetTextMetrics` / `GetTextFace` | Font metrics and family name |
| `GetFontData` | Read TrueType/OpenType table data |
| `GetGlyphIndices` | Map Unicode code points to glyph IDs |
| `GetFontUnicodeRanges` | Query supported Unicode blocks |
| `EnumFontFamiliesEx` | Enumerate installed system fonts |
| `AddFontResourceEx` | Register private font files |

The migration replaced P/Invoke with `LibWrapper` — a class that preserves the GDI API shape but delegates to SkiaSharp where implemented, and returns stubs elsewhere.

## Current State

### What SkiaSharp Handles

| Concern | Implementation | File |
|---------|----------------|------|
| Font enumeration | `SKFontManager.Default.GetFontFamilies()` | `FontManager.cs` |
| Typeface loading | `SKTypeface.FromFamilyName()`, `FromFile()` | `FontManager.cs`, `GdiFont.cs` |
| Private fonts | `GdiPrivateFontCollection.AddFontFile()` | `GdiPrivateFontCollection.cs` |
| Basic metrics | `SKFontMetrics` (ascent, descent, size) | `GdiFontMetrics.cs` |

### What Is Still Stubbed

| GDI API | `LibWrapper` Behavior | Impact |
|---------|----------------------|--------|
| `GetFontData` | Returns 0 (error) | Cannot read font tables via shim |
| `GetGlyphIndices` | Returns count, empty array | All glyphs map to index 0 |
| `GetFontUnicodeRanges` | Returns 0 | Coverage detection fails |
| `SelectObject` | No-op | DC state not tracked |
| `GetCurrentObject` | Returns `IntPtr.Zero` | Cannot query active font |
| `GetTextFace` | Returns 0 | Face name not available via shim |
| `EnumFontFamiliesEx` | Dummy callback data | Legacy enumeration path broken |

### Parallel Implementation in FontManager

`FontManager` has partial SkiaSharp implementations that bypass `LibWrapper`:

```csharp
// FontManager.cs — placeholder glyph mapping
internal ushort[] GetGlyphIndices(string text)
{
    // Returns sequential indices 0, 1, 2, ... — NOT real glyph IDs
    var indices = new ushort[text.Length];
    for (int i = 0; i < text.Length; i++)
        indices[i] = (ushort)i;
    return indices;
}
```

This is **not wired into** `UnicodeRange.MapCharacter`, which still calls `LibWrapper.GetGlyphIndices`.

## Font Pipeline Call Graph

```
FonetDriver.Render()
  └── StreamRenderer
        └── PdfRenderer.Render(page)
              └── FontSetup (initializes font catalog)
                    └── ProxyFont
                          ├── Base14Font (✅ no GDI)
                          └── TrueTypeFont / Type2CIDFont
                                └── GdiDeviceContent.CreateFont()
                                      └── GdiFontMetrics
                                            ├── FontManager.GetFontData()  ← SkiaSharp
                                            ├── LibWrapper.GetFontData()   ← ❌ stub
                                            ├── LibWrapper.GetGlyphIndices ← ❌ stub
                                            └── GdiUnicodeRanges           ← hardcoded Latin
```

## Migration Strategy

### Step 1: Centralize Font Table Access

Create `FontTableReader` that reads TrueType tables from a single source:

```csharp
internal sealed class FontTableReader
{
    private readonly byte[] _fontData;

    public FontTableReader(SKTypeface typeface)
    {
        using var stream = typeface.OpenStream();
        _fontData = stream.ReadBytes();
    }

    public FontTableReader(string filePath)
    {
        _fontData = File.ReadAllBytes(filePath);
    }

    public byte[] ReadTable(uint tag) { /* parse offset table, return table bytes */ }
    public ushort GetGlyphIndex(int codePoint) { /* parse cmap table */ }
    public UnicodeRange[] GetCoverage() { /* parse OS/2 + cmap */ }
}
```

**Consumers to migrate:**
- `GdiFontMetrics.ReadFont()`
- `GdiFontCreator.ReadTableData()`
- `TrueTypeFont.ObtainFontMetrics()`
- `Type2CIDFont.MapCharacter()`

### Step 2: Implement Glyph Mapping

Replace both `LibWrapper.GetGlyphIndices` and `FontManager.GetGlyphIndices`:

**Option A — SkiaSharp API:**
```csharp
typeface.GetGlyphs(text.AsSpan(), glyphs.AsSpan());
```

**Option B — Direct cmap parsing:**
Parse the `cmap` table from font data. More control, required for subsetting.

**Recommendation:** Use SkiaSharp for runtime mapping; parse cmap directly for subsetting (already partially implemented in `Pdf/Gdi/Font/Tables/`).

### Step 3: Fix Unicode Range Detection

Replace `GdiUnicodeRanges` hardcoded blocks:

```csharp
// Current (broken for non-Latin):
ranges.Add(new UnicodeRange(0x0020, 0x007F));  // Basic Latin
ranges.Add(new UnicodeRange(0x00A0, 0x00FF));  // Latin-1 Supplement
// ...

// Target:
var cmap = fontTableReader.GetCmapTable();
foreach (var range in cmap.GetUnicodeRanges())
    ranges.Add(range);
```

### Step 4: Improve Font Discovery

Current approach (`FontManager.LocateSystemFont`):

```
1. Try SKFontManager.Default.MatchFamily()
2. Fallback: scan OS font directories recursively
3. Match by filename.Contains(familyName)  ← brittle
```

Target approach:

```
1. SKFontManager.Default.MatchFamily(family, weight, width, slant)
2. If no match: query font file PostScript name / full name from name table
3. Cache results in ConcurrentDictionary
4. On Linux: consider fontconfig binding (optional, Phase 4)
```

### Step 5: Complete Font Subsetting

Font subsetting rewrites TrueType tables for embedded fonts. Current gaps:

| Table | Read | Write | Notes |
|-------|------|-------|-------|
| `head` | ✅ | ⚠️ partial | Checksum TODO |
| `hhea` | ✅ | ⚠️ partial | Field validation TODO |
| `hmtx` | ✅ | ✅ | |
| `maxp` | ✅ | ✅ | |
| `cmap` | ✅ | ✅ | |
| `glyf` | ✅ | ✅ | |
| `loca` | ✅ | ❌ | Short/long format TODO |
| `OS/2` | ✅ | ❌ | `NotImplementedException` |
| `name` | ✅ | ❌ | `NotImplementedException` |
| `post` | ✅ | ❌ | `NotImplementedException` |

Priority: implement `Write()` for OS/2, name, and post tables to unblock subsetting.

### Step 6: Retire GDI Shim

Once all callers use `FontTableReader` / `IFontBackend`:

1. Delete `LibWrapper.cs`
2. Delete `Pdf/Gdi/Structures/` (LogFont, TextMetric, GlyphSet, etc.)
3. Rename remaining types:
   - `GdiFont` → `SkiaFont` or `FontHandle`
   - `GdiFontMetrics` → `FontMetricsProvider`
   - `GdiDeviceContent` → `FontContext`
   - `GdiPrivateFontCollection` → `PrivateFontCollection`
4. Move from `Pdf/Gdi/` to `Fonts/` or `Pdf/Fonts/`

## Testing Requirements

| Test | Validates |
|------|-----------|
| `FontTableReader_ReadsHeadTable` | Table parsing from TTF file |
| `GlyphMapping_LatinCharacters` | A–Z map to non-zero glyph IDs |
| `GlyphMapping_NonLatinCharacters` | CJK chars map correctly |
| `FontDiscovery_FindsNunito` | System/custom font resolution |
| `FontSubsetting_ProducesValidPdf` | Subset font embeds and renders |
| `FontMetrics_MatchPdfDescriptor` | StemV, Ascent, Descent values |
| `CrossPlatform_SystemFontWindows` | Windows system font works |
| `CrossPlatform_SystemFontLinux` | Linux system font works |

## Risk: SkiaSharp Native Crashes

`SKTypeface.OpenStream()` can crash with access violations on corrupted or unsupported font data. Mitigations:

1. Validate font file header (`0x00010000` or `OTTO`) before opening
2. Wrap in try/catch with fallback to file-based table reader
3. Cache successfully opened typefaces; don't re-open per glyph request
4. Add test with intentionally corrupt font file to verify graceful failure

## Files to Modify (Phase 1)

| File | Changes |
|------|---------|
| `Pdf/Gdi/LibWrapper.cs` | Implement or delete |
| `Pdf/Gdi/FontManager.cs` | Real glyph mapping, hardened GetFontData |
| `Pdf/Gdi/GdiFontMetrics.cs` | Complete metric TODOs, use FontTableReader |
| `Pdf/Gdi/GdiUnicodeRanges.cs` | cmap-based coverage |
| `Pdf/Gdi/GdiFontCreator.cs` | Route through FontTableReader |
| `Render/Pdf/Fonts/TrueTypeFont.cs` | Remove Win32 comments, use new backend |
| `Render/Pdf/Fonts/Type2CIDFont.cs` | Complete subsetting |
| `Pdf/Gdi/Font/Tables/OS2Table.cs` | Implement Write() |
| `Pdf/Gdi/Font/Tables/NameTable.cs` | Implement Write() |
| `Pdf/Gdi/Font/Tables/PostTable.cs` | Implement Write() |
