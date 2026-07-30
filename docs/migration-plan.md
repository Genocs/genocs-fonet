# Migration Plan

Phased roadmap for the Windows/GDI → cross-platform .NET migration.

**Last updated:** August 2026

## Current Status

| Phase | Status | Summary |
|-------|--------|---------|
| Phase 0: Stabilize | ✅ Complete | CI, test infrastructure, SkiaSharp crash fix |
| Phase 1: Font Pipeline | ✅ Complete | Glyph mapping, font tables, subsetting, metrics |
| Phase 2: Quality & Performance | ✅ Complete | Image spans, encoding, namespaces, error reporting |
| Phase 3: FO Completeness | 🔄 In progress | Tier 1 initial batch done; float side-placement deferred |
| Phase 4: Hardening & Release | ⏳ Not started | NuGet, AES encryption, visual regression |

See [Phase 1 Summary](./phase-1-summary.md), [Phase 2 Summary](./phase-2-summary.md), and [Phase 3 Summary](./phase-3-summary.md) for implementation details.

## Guiding Principles

1. **Fix correctness before features** — A working font pipeline is prerequisite for reliable PDF output
2. **Test-driven migration** — Add regression tests before refactoring critical paths
3. **Incremental replacement** — Replace GDI shim methods one at a time, not big-bang rewrite
4. **Template-driven FO triage** — Implement FO properties/elements based on actual document needs
5. **Minimize API churn** — Keep `FonetDriver` and `PdfRendererOptions` stable for consumers

## Phase Overview

```
Phase 0: Stabilize ✅       Phase 1: Font Pipeline ✅    Phase 2: Quality ✅
┌─────────────────┐        ┌─────────────────┐          ┌─────────────────┐
│ CI setup        │        │ FontTableAccess │          │ Image spans     │
│ Test hardening  │   ──▶  │ CmapReader      │   ──▶    │ Pdf encoding    │
│ PdfAssertions   │        │ Font metrics    │          │ Namespace unify │
└─────────────────┘        └─────────────────┘          └─────────────────┘
                                                                    │
Phase 4: Hardening ⏳               Phase 3: FO Completeness 🔄     │
┌─────────────────┐        ┌─────────────────┐                     ▼
│ Security (AES)  │        │ Tier 1 props    │          ┌─────────────────┐
│ CJK validation  │   ◀──  │ Tier 1 elements │          │ Production      │
│ Visual regression│       │ Float/bidi/RTL  │          │ release         │
│ NuGet publish   │        │ Consumer triage │          │                 │
└─────────────────┘        └─────────────────┘          └─────────────────┘
```

---

## Phase 0: Stabilize (Foundation) — ✅ Complete

**Goal:** Establish a reliable baseline for migration work.

### Tasks

| # | Task | Priority | Status |
|---|------|----------|--------|
| 0.1 | Add GitHub Actions CI | P0 | ✅ `.github/workflows/build-and-test.yml` |
| 0.2 | Fix SkiaSharp crash in `CrossPlatformFontAndImageTest` | P0 | ✅ File-based `FontTableAccess`; no cached typeface dispose |
| 0.3 | Refactor tests to use temp directories | P1 | ✅ `PdfTestBase` with isolated temp output |
| 0.4 | Add basic PDF validation (header, page count, size bounds) | P1 | ✅ `PdfAssertions` helper |
| 0.5 | Update root README (build status, limitations) | P2 | ✅ Updated |
| 0.6 | Triage nullable warnings (fix critical CS8602 in hot paths) | P2 | Deferred — addressed in Phase 2 via `.editorconfig` triage |

### Milestone: M0 — Green CI ✅

- [x] `dotnet build` passes on Windows, Linux, macOS
- [x] `dotnet test` passes all tests without SkiaSharp crash
- [ ] CI badge in README

---

## Phase 1: Font Pipeline (Critical Path) — ✅ Complete

**Goal:** Replace GDI shim stubs with real SkiaSharp-backed implementations.

See [Phase 1 Summary](./phase-1-summary.md) for full details.

### Tasks

| # | Task | Priority | Status |
|---|------|----------|--------|
| 1.1 | Implement `FontTableAccess` for file-based font table reads | P0 | ✅ Done |
| 1.2 | Implement glyph mapping via `CmapReader` + `FontManager.GetGlyphIndices` | P0 | ✅ Done |
| 1.3 | Replace hardcoded Unicode ranges with cmap-based coverage | P0 | ✅ Done |
| 1.4 | Harden `FontManager.GetFontData` (file fallback, thread safety) | P0 | ✅ Done |
| 1.5 | Implement font descriptor metrics (StemV, AverageWidth, MaxWidth) | P1 | ✅ Done |
| 1.6 | Improve font discovery (path caching, filename matching) | P1 | ✅ Done |
| 1.7 | Implement font table `Write()` methods (OS2, Name, Post) | P1 | ✅ Done (raw byte preservation) |
| 1.8 | Complete `Type2CIDFont` subsetting | P1 | ✅ Done (optional cvt/prep/fpgm tables) |
| 1.9 | Rename `Gdi*` types to `Font*` or `SkiaFont*` | P2 | Deferred |
| 1.10 | Remove dead `LibWrapper` methods | P2 | Partial — `LibWrapper` slimmed to device-context registry only |

### Milestone: M1 — Reliable Fonts ✅ (Latin/custom)

- [x] Custom font (Nunito) embeds with correct glyphs in PDF
- [x] System font resolves correctly on Windows (filename-based discovery + cache)
- [ ] Basic CJK document renders without glyph-0 fallback (needs CJK font fixture test)
- [x] Font subsetting test passes
- [x] Unit tests for glyph mapping, font metrics, table parsing

---

## Phase 2: Quality & Performance — ✅ Complete

**Goal:** Improve reliability, performance, and maintainability.

See [Phase 2 Summary](./phase-2-summary.md) for full details.

### Tasks

| # | Task | Priority | Status |
|---|------|----------|--------|
| 2.1 | Optimize image pixel extraction (`SKPixmap` spans) | P1 | ✅ Done |
| 2.2 | Fix `PdfContentStream` encoding (ASCII for operators) | P1 | ✅ Done |
| 2.3 | Replace swallowed exceptions with structured error reporting | P1 | ✅ Done |
| 2.4 | Optimize kerning lookup (pair iteration instead of cartesian product) | P2 | ✅ Done |
| 2.5 | Consolidate namespaces (`Fonet.*` → `Genocs.Fonet.*`) | P2 | ✅ Done |
| 2.6 | Migrate `WebRequest` to `HttpClient` in image factory | P2 | ✅ Done |
| 2.7 | Reduce nullable warnings to < 500 | P2 | ✅ Done (326 warnings via `.editorconfig` triage) |

### Milestone: M2 — Performance Baseline ✅

- [x] Image embedding uses span-based pixel extraction
- [x] Font enumeration cached after first call
- [x] No bare `catch` blocks in font/image hot paths

---

## Phase 3: FO Feature Completeness — 🔄 In Progress

**Goal:** Implement XSL-FO properties and elements required by target documents.

See [Phase 3 Summary](./phase-3-summary.md) and [Feature Completeness](./feature-completeness.md) for details.

### Approach

1. **Inventory** — Collect FO templates from consumers; identify which properties/elements are actually used
2. **Prioritize** — Rank by frequency and impact
3. **Implement** — Replace `ToBeImplementedProperty` / `ToBeImplementedElement` stubs with real implementations
4. **Test** — Add FO template + PDF validation test per feature

### Tier 1 — Completed (initial batch)

| Property/Element | Status | Notes |
|------------------|--------|-------|
| `visibility` | ✅ Layout integrated | Skipped in layout when hidden |
| `word-spacing` | ✅ Layout integrated | PDF `Tw` operator |
| `margin` (shorthand) | ✅ Done | Resolves via individual margin properties |
| `caption-side` | ✅ Done | Used by `TableAndCaption` |
| `fo:table-caption` | ✅ Done | Lays out caption content |
| `fo:table-and-caption` | ✅ Done | Orders caption/table by `caption-side` |
| `background-color`, `background-image` | ✅ Pre-existing | — |
| `letter-spacing` | ✅ Pre-existing | — |
| `float`, `clear`, `z-index` | ✅ Done | Side-float, clear layout, z-order rendering |
| `fo:float` | ✅ Done | Side placement via `SideFloatArea`; text wraps in `BlockArea` |

### Tier 1 — Complete ✅

All Tier 1 layout properties and elements are implemented (initial scope).

### Tier 2 — Internationalization (next)

| Property/Element | Used For |
|------------------|----------|
| `direction`, `unicode-bidi` | RTL text |
| `writing-mode` | Vertical text |
| `script`, `language` | Font selection hints |
| `fo:bidi-override` | Explicit direction |

### Tier 3 — Advanced (implement as needed)

| Property/Element | Used For |
|------------------|----------|
| `multi-switch`, `multi-case` | Conditional content |
| Aural properties (`speak`, `voice-family`, etc.) | Accessibility (low priority for PDF) |
| `color-profile` | ICC color management |

### Milestone: M3 — Template Coverage

- [x] Tier 1 FO template (`Phase3Tier1Test.fo`) renders without property warnings
- [x] Feature matrix updated for Tier 1 batch
- [x] Regression tests for Tier 1 (7 property tests + 2 feature tests)
- [x] Side-float layout for `fo:float`
- [x] `clear` property layout effect
- [x] `z-index` stacking for overlapping areas
- [ ] All consumer FO templates render without warnings (ongoing)

---

## Phase 4: Hardening & Release — ⏳ Not Started

**Goal:** Production-ready release.

### Tasks

| # | Task | Priority | Status |
|---|------|----------|--------|
| 4.1 | Modern PDF encryption (AES-256 per PDF 2.0) | P2 | Open — legacy RC4 only |
| 4.2 | Visual regression tests (PDF → image comparison) | P2 | Open |
| 4.3 | NuGet package publishing pipeline | P1 | Partial — `.github/workflows/nuget-publish.yml` exists |
| 4.4 | API documentation (XML docs on public surface) | P2 | Open |
| 4.5 | Performance benchmarks (document suite) | P2 | Open |
| 4.6 | Security audit of PDF writer | P2 | Open |
| 4.7 | CJK font validation test + cross-platform system font CI | P1 | Open |
| 4.8 | Rename `Gdi*` types (deferred from Phase 1) | P2 | Open |

### Milestone: M4 — v1.0 Release

- [ ] Phase 3 consumer template coverage complete
- [ ] CJK / complex script validation
- [ ] Consumer acceptance testing passed
- [ ] NuGet package published
- [ ] CHANGELOG documenting breaking changes from original Fonet

---

## Success Criteria (Overall)

| Criterion | Target | Current |
|-----------|--------|---------|
| Build | Clean on net8/9/10, Windows/Linux/macOS | ✅ Builds; CI on Ubuntu |
| Tests | > 50 tests, > 80% coverage on font/PDF paths | ⚠️ 22 tests; PDF structure validation only |
| Fonts | Custom + system fonts work on all platforms | ⚠️ Latin/custom proven; CJK needs validation |
| FO coverage | All consumer templates render correctly | 🔄 Tier 1 batch done; ~87 properties still stubbed |
| Performance | < 2s for typical 10-page document | Not benchmarked |
| API stability | `FonetDriver` API unchanged from current | ✅ Stable |

## Tracking

Update milestone checkboxes in this document as work completes. Link PRs in the [Known Issues](./known-issues.md) tracking table.
