# Risks & Concerns

Technical, operational, and architectural risks for the Genocs.Fonet migration.

**Last updated:** August 2026

## Risk Matrix

| ID | Risk | Likelihood | Impact | Severity | Status |
|----|------|------------|--------|----------|--------|
| R-01 | Font pipeline produces silently wrong PDFs | Low | Critical | **Medium** | ✅ Mitigated — Phase 1 complete; glyph mapping tests pass |
| R-02 | SkiaSharp native crashes on bad font data | Low | High | **Medium** | ✅ Mitigated — file-based reads; crash fixed in Phase 0 |
| R-03 | Incomplete FO coverage breaks consumer templates | High | High | **High** | 🔄 Open — Phase 3 in progress (~87 properties stubbed) |
| R-04 | No CI → regressions go undetected | Low | Medium | **Low** | ✅ Mitigated — GitHub Actions CI in place |
| R-05 | Performance unacceptable for production | Medium | Medium | **Medium** | 🔄 Open — not benchmarked |
| R-06 | Namespace split causes maintenance errors | Low | Low | **Low** | ✅ Resolved — consolidated to `Genocs.Fonet.*` |
| R-07 | Legacy RC4 encryption inadequate | Low | Medium | **Low** | 🔄 Open — document limitations; AES in Phase 4 |
| R-08 | SkiaSharp version lock-in / breaking changes | Low | Medium | **Low** | 🔄 Open — pin version; test on upgrades |
| R-09 | Original Fonet behavior unknown for edge cases | Medium | Medium | **Medium** | 🔄 Open — 22 tests; needs deeper regression |
| R-10 | Single maintainer / bus factor | Medium | High | **High** | 🔄 Open — documentation helps |

---

## Critical Concerns

### 1. Incomplete FO Coverage (Active)

~87 XSL-FO properties and 11 elements remain stubbed. Documents using these features will log warnings and produce incomplete layout. Phase 3 Tier 1 batch is done; side-float layout and Tier 2 i18n are next.

**Recommendation:** Continue template-driven triage per [Migration Plan](./migration-plan.md).

### 2. GDI Shim Naming Debt (Active)

54 files under `Pdf/Gdi/` retain Win32-shaped type names (`GdiFont`, `GdiFontMetrics`) despite SkiaSharp-backed implementations. `LibWrapper` is now a thin device-context registry, but the naming still confuses contributors.

**Recommendation:** Rename `Gdi*` types in Phase 4 (deferred from Phase 1).

### 3. Test Coverage Gap (Active)

22 tests with PDF structure validation provide baseline confidence but cannot detect:

- Wrong rendered text content
- Incorrect layout calculations for stubbed properties
- Cross-platform font rendering differences
- Visual layout regressions

**Recommendation:** Add text extraction validation and visual regression before production release (see [Testing Strategy](./testing-strategy.md)).

---

## Architectural Concerns

### 4. Custom PDF Writer Maintenance Burden

The library implements its own PDF 1.3 writer (~100 files in `Pdf/`). Benefits: no external dependency, full control. Risks:

- PDF spec edge cases may be unhandled
- No community bug fixes from a maintained PDF library
- Encryption, accessibility, PDF/A compliance are all self-implemented

**Open decision:** Continue with custom writer vs. adopt PdfSharp/iText for output generation while keeping Fonet layout.

| Option | Pros | Cons |
|--------|------|------|
| Keep custom writer | No new dep; proven in original Fonet | Maintenance burden; spec gaps |
| Adopt PdfSharp | Community maintained; PDF 2.0 features | Major refactor; API mapping |
| Hybrid | Custom for content streams; library for document structure | Complexity |

**Recommendation:** Keep custom writer for Phase 1–3; evaluate PdfSharp for Phase 4 if PDF spec compliance becomes a requirement.

### 5. No Complex Text Shaping

The library lacks HarfBuzz or equivalent text shaping. This means:

- No ligatures (fi, fl, etc.) unless font handles them automatically
- No correct Arabic/Hebrew joining behavior
- No Indic script reordering
- Kerning is limited to font table pairs

**Impact:** Documents requiring non-Latin scripts or typographic quality will render incorrectly even after font pipeline fixes.

**Recommendation:** Document as known limitation. Evaluate HarfBuzzSharp integration in Phase 4 if i18n is a requirement.

---

## Operational Concerns

### 7. Font Discovery Brittleness

`FontManager.LocateSystemFont` scans filesystem directories with filename substring matching. On Linux:

- Font config may use different directory structures
- Font families may not match filenames (e.g., "DejaVu Sans" in `DejaVuSans.ttf`)
- Recursive scan is slow on first call

**Recommendation:** Use `SKFontManager.MatchFamily` as primary; filesystem scan as last resort with PostScript name validation.

### 8. Nullable Reference Warnings (~326)

Reduced from ~3,000 via `.editorconfig` triage in legacy `Fo/**` and `Layout/**` trees. Remaining warnings are concentrated in `Pdf/**` and `Render/**`.

**Recommendation:** Fix warnings in hot paths incrementally; avoid re-enabling nullable diagnostics in legacy trees until ready.

---

## Security Concerns

### 10. Legacy PDF Encryption

`Pdf/Security/Arc4.cs` implements RC4 stream cipher. RC4 is considered cryptographically broken. If consumers use PDF encryption:

- Documents are not securely protected
- Compliance requirements (GDPR, HIPAA) may not be met

**Recommendation:** Document that encryption is legacy RC4 only. Implement AES-256 (PDF 2.0) in Phase 4 if encryption is a consumer requirement.

### 11. External Resource Loading

`ApocImageFactory` can load images from URIs (HTTP). Risks:

- SSRF if FO templates are user-controlled
- No size limits on downloaded images

**Recommendation:** Add configurable URI allowlist; limit download size. (`HttpClient` with timeout migrated in Phase 2.)

### 12. Unsafe Code Blocks

`AllowUnsafeBlocks` is enabled for TrueType table parsing. Risks:

- Buffer overflows if table offsets are not validated
- Corrupt font files could cause undefined behavior

**Recommendation:** Audit unsafe blocks in `Pdf/Gdi/Font/`; add bounds checking on all table reads.

---

## Dependency Concerns

### 13. SkiaSharp Native Dependency

SkiaSharp bundles native libraries per platform. Concerns:

- Native crash (already observed) bypasses .NET exception handling
- Platform-specific native binary issues (musl vs glibc on Linux)
- Version 2.88.9 may have known issues fixed in later releases

**Recommendation:**
- Pin SkiaSharp version explicitly
- Test on Alpine Linux (musl) if container deployment is expected
- Wrap all SkiaSharp calls in managed try/catch with validation

### 14. Single External Dependency

The library has only one NuGet dependency (SkiaSharp). This is good for supply chain security but means:

- All font and image capability depends on SkiaSharp's feature set
- SkiaSharp API changes require migration effort

---

## Open Decisions

Track decisions needed before or during migration:

| # | Decision | Options | Deadline | Owner |
|---|----------|---------|----------|-------|
| D-01 | Keep or replace custom PDF writer | Keep / PdfSharp / Hybrid | Phase 4 | TBD |
| D-02 | HarfBuzz integration for complex scripts | Yes / No / Later | Phase 4 | TBD |
| D-03 | Target FO spec version | 1.0 / 1.1 / subset | Phase 3 | TBD |
| D-04 | Public API namespace | `Genocs.Fonet` / `Fonet` | Phase 2 | ✅ `Genocs.Fonet.*` |
| D-05 | Minimum supported .NET version | net8.0 / net9.0 | Phase 0 | ✅ net8.0 |
| D-06 | Apache FOP compatibility target | Full / Best-effort / None | Phase 3 | TBD |
| D-07 | NuGet package publishing | Public / Private feed | Phase 4 | TBD |

---

## Concern Resolution Tracking

| Concern | Status | Resolution | Date |
|---------|--------|------------|------|
| Build failures | ✅ Resolved | Solution compiles on net8/9/10 | 2026-06 |
| GDI P/Invoke removal | ✅ Resolved | Replaced with SkiaSharp + file-based font access | 2026-06 |
| Font glyph mapping | ✅ Resolved | `CmapReader` + `FontManager.GetGlyphIndices` | 2026-06 |
| Namespace fragmentation | ✅ Resolved | Consolidated to `Genocs.Fonet.*` | 2026-06 |
| CI pipeline | ✅ Resolved | GitHub Actions workflows | 2026-06 |
| Test coverage | ⚠️ Partial | 22 tests with PDF structure validation | 2026-08 |
| FO feature stubs | 🔄 In progress | Phase 3 Tier 1 done; ~87 properties remain | 2026-08 |
| CJK validation | ❌ Open | No CJK font fixture test | — |

Update this table as concerns are addressed.
