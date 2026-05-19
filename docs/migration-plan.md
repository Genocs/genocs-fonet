# Migration Plan

Phased roadmap to complete the Windows/GDI → cross-platform .NET migration.

## Guiding Principles

1. **Fix correctness before features** — A working font pipeline is prerequisite for reliable PDF output
2. **Test-driven migration** — Add regression tests before refactoring critical paths
3. **Incremental replacement** — Replace GDI shim methods one at a time, not big-bang rewrite
4. **Template-driven FO triage** — Implement FO properties/elements based on actual document needs
5. **Minimize API churn** — Keep `FonetDriver` and `PdfRendererOptions` stable for consumers

## Phase Overview

```
Phase 0: Stabilize          Phase 1: Font Pipeline       Phase 2: Quality
(2–3 weeks)                 (4–6 weeks)                  (3–4 weeks)
┌─────────────────┐        ┌─────────────────┐           ┌─────────────────┐
│ CI setup        │        │ LibWrapper impl │           │ FO feature      │
│ Test hardening  │   ──▶  │ Glyph mapping   │    ──▶    │ triage          │
│ Nullable cleanup│        │ Font discovery  │           │ Performance     │
│ README update   │        │ Rename Gdi*     │           │ Namespace unify │
└─────────────────┘        └─────────────────┘           └─────────────────┘
                                                                    │
Phase 4: Hardening                  Phase 3: FO Completeness         │
(ongoing)                           (6–8 weeks)                      ▼
┌─────────────────┐        ┌─────────────────┐           ┌─────────────────┐
│ Security (AES)  │        │ Priority props  │           │ Production      │
│ CJK/complex text│   ◀──  │ Priority elems  │           │ release         │
│ Visual regression│       │ Table/float/bidi│           │                 │
└─────────────────┘        └─────────────────┘           └─────────────────┘
```

---

## Phase 0: Stabilize (Foundation)

**Goal:** Establish a reliable baseline for migration work.

**Duration:** 2–3 weeks

### Tasks

| # | Task | Priority | Success Criteria |
|---|------|----------|------------------|
| 0.1 | Add GitHub Actions CI (Windows + Linux + macOS) | P0 | ✅ `.github/workflows/ci.yml` |
| 0.2 | Fix SkiaSharp crash in `CrossPlatformFontAndImageTest` | P0 | ✅ Stop disposing cached typefaces; file-based `GetFontData` |
| 0.3 | Refactor tests to use temp directories | P1 | ✅ `PdfTestBase` with isolated temp output |
| 0.4 | Add basic PDF validation (file header, page count, size bounds) | P1 | ✅ `PdfAssertions` helper |
| 0.5 | Update root README (build status, limitations) | P2 | ✅ Updated |
| 0.6 | Triage nullable warnings (fix critical CS8602 in hot paths) | P2 | Deferred to Phase 2 |

### Milestone: M0 — Green CI

- [x] `dotnet build` passes on Windows, Linux, macOS (CI workflow added)
- [x] `dotnet test` passes all 5 tests without SkiaSharp crash
- [ ] CI badge in README

---

## Phase 1: Font Pipeline (Critical Path)

**Goal:** Replace GDI shim stubs with real SkiaSharp-backed implementations.

**Duration:** 4–6 weeks

**Depends on:** Phase 0 (M0)

### Tasks

| # | Task | Priority | Addresses | Success Criteria |
|---|------|----------|-----------|------------------|
| 1.1 | Implement `LibWrapper.GetFontData` via file-based `FontTableAccess` | P0 | ✅ Done |
| 1.2 | Implement `LibWrapper.GetGlyphIndices` via `CmapReader` + `SKFont` | P0 | ✅ Done |
| 1.3 | Replace hardcoded Unicode ranges with cmap-based coverage | P0 | ✅ Done |
| 1.4 | Harden `FontManager.GetFontData` (file fallback, thread safety) | P0 | ✅ Done |
| 1.5 | Implement `GdiFontMetrics` TODOs (StemV, AverageWidth, MaxWidth) | P1 | ✅ Done |
| 1.6 | Improve font discovery (path caching, filename matching) | P1 | ✅ Done |
| 1.7 | Implement font table `Write()` methods (OS2, Name, Post) | P1 | ✅ Done (raw byte preservation) |
| 1.8 | Complete `Type2CIDFont` subsetting | P1 | ✅ Done (optional cvt/prep/fpgm tables) |
| 1.9 | Rename `Gdi*` types to `Font*` or `SkiaFont*` | P2 | Deferred to Phase 2 |
| 1.10 | Remove dead `LibWrapper` methods no longer called | P2 | Partial (`EnumFontFamilies*` removed) |

### Milestone: M1 — Reliable Fonts

- [x] Custom font (Nunito) embeds with correct glyphs in PDF
- [x] System font resolves correctly on Windows (filename-based discovery + cache)
- [ ] Basic CJK document renders without glyph-0 fallback (needs CJK font fixture test)
- [x] Font subsetting test passes
- [x] Unit tests for glyph mapping, font metrics, table parsing

### Design Decision: Font Abstraction

Introduce a single `IFontBackend` interface:

```csharp
internal interface IFontBackend
{
    byte[] GetFontTable(uint tableTag);
    ushort[] GetGlyphIndices(ReadOnlySpan<char> text);
    UnicodeRange[] GetUnicodeRanges();
    FontMetrics GetMetrics();
}
```

Implementations:
- `SkiaFontBackend` — primary, uses `SKTypeface`
- `FileFontBackend` — direct TTF/OTF file access for subsetting

Retire `LibWrapper` once all callers use `IFontBackend`.

---

## Phase 2: Quality & Performance

**Goal:** Improve reliability, performance, and maintainability.

**Duration:** 3–4 weeks

**Depends on:** Phase 1 (M1)

### Tasks

| # | Task | Priority | Addresses |
|---|------|----------|-----------|
| 2.1 | Optimize image pixel extraction (`SKPixmap` spans) | P1 | ✅ Done |
| 2.2 | Fix `PdfContentStream` encoding (use PDFDocEncoding/UTF-16BE) | P1 | ✅ Done (ASCII for content-stream operators) |
| 2.3 | Replace swallowed exceptions with structured error reporting | P1 | ✅ Done |
| 2.4 | Optimize kerning lookup (hash map instead of cartesian product) | P2 | ✅ Done |
| 2.5 | Consolidate namespaces (`Fonet.*` → `Genocs.Fonet.*`) | P2 | ✅ Done |
| 2.6 | Migrate `WebRequest` to `HttpClient` in image factory | P2 | ✅ Done |
| 2.7 | Reduce nullable warnings to < 500 | P2 | ✅ Done (326 warnings) |

### Milestone: M2 — Performance Baseline

- [x] Image embedding uses span-based pixel extraction (no per-pixel `GetPixel`)
- [x] Font enumeration cached after first call
- [x] No bare `catch` blocks in font/image hot paths

---

## Phase 3: FO Feature Completeness

**Goal:** Implement XSL-FO properties and elements required by target documents.

**Duration:** 6–8 weeks (ongoing, template-driven)

**Depends on:** Phase 1 (M1)

### Approach

1. **Inventory** — Collect FO templates from consumers; identify which properties/elements are actually used
2. **Prioritize** — Rank by frequency and impact (see [Feature Completeness](./feature-completeness.md))
3. **Implement** — Replace `ToBeImplementedProperty` / `ToBeImplementedElement` stubs with real implementations
4. **Test** — Add FO template + PDF validation test per feature

### Priority Tiers

#### Tier 1 — Common layout (implement first)

| Property/Element | Used For |
|------------------|----------|
| `background-color`, `background-image` | Styling |
| `visibility` | Show/hide content |
| `float`, `clear` | Side-by-side layout |
| `z-index` | Layering |
| `margin` shorthand | Spacing |
| `word-spacing`, `letter-spacing` | Text tuning |
| `table-caption`, `caption-side` | Table titles |

#### Tier 2 — Internationalization

| Property/Element | Used For |
|------------------|----------|
| `direction`, `unicode-bidi` | RTL text |
| `writing-mode` | Vertical text |
| `script`, `language` | Font selection hints |
| `bidi-override` | Explicit direction |

#### Tier 3 — Advanced (implement as needed)

| Property/Element | Used For |
|------------------|----------|
| `multi-switch`, `multi-case` | Conditional content |
| Aural properties (`speak`, `voice-family`, etc.) | Accessibility (low priority for PDF) |
| `color-profile` | ICC color management |

### Milestone: M3 — Template Coverage

- [x] Tier 1 FO template (`Phase3Tier1Test.fo`) renders without property warnings
- [x] Feature matrix updated for Tier 1 batch (see `phase-3-summary.md`)
- [x] Regression test per implemented Tier 1 feature group
- [ ] All consumer FO templates render without warnings (ongoing)

---

## Phase 4: Hardening & Release

**Goal:** Production-ready release.

**Duration:** Ongoing

### Tasks

| # | Task | Priority |
|---|------|----------|
| 4.1 | Modern PDF encryption (AES-256 per PDF 2.0) | P2 |
| 4.2 | Visual regression tests (PDF → image comparison) | P2 |
| 4.3 | NuGet package publishing pipeline | P1 |
| 4.4 | API documentation (XML docs on public surface) | P2 |
| 4.5 | Performance benchmarks (document suite) | P2 |
| 4.6 | Security audit of PDF writer | P2 |

### Milestone: M4 — v1.0 Release

- [ ] All Phase 0–3 milestones complete
- [ ] Consumer acceptance testing passed
- [ ] NuGet package published
- [ ] CHANGELOG documenting breaking changes from original Fonet

---

## Success Criteria (Overall)

| Criterion | Target |
|-----------|--------|
| Build | Clean on net8/9/10, Windows/Linux/macOS |
| Tests | > 50 tests, > 80% coverage on font/PDF paths |
| Fonts | Custom + system fonts work on all platforms |
| FO coverage | All consumer templates render correctly |
| Performance | < 2s for typical 10-page document |
| API stability | `FonetDriver` API unchanged from current |

## Tracking

Update milestone checkboxes in this document as work completes. Link PRs in the [Known Issues](./known-issues.md) tracking table.
