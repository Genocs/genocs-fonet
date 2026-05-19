# Risks & Concerns

Technical, operational, and architectural risks for the Genocs.Fonet migration.

## Risk Matrix

| ID | Risk | Likelihood | Impact | Severity | Mitigation |
|----|------|------------|--------|----------|------------|
| R-01 | Font pipeline produces silently wrong PDFs | High | Critical | **Critical** | Phase 1 font work; glyph mapping tests |
| R-02 | SkiaSharp native crashes on bad font data | Medium | High | **High** | Validate before OpenStream; graceful fallback |
| R-03 | Incomplete FO coverage breaks consumer templates | High | High | **High** | Template-driven triage (Phase 3) |
| R-04 | No CI → regressions go undetected | High | Medium | **High** | Phase 0 CI setup |
| R-05 | Performance unacceptable for production | Medium | Medium | **Medium** | Benchmarks; image/font optimization |
| R-06 | Namespace split causes maintenance errors | Medium | Low | **Medium** | Phase 2 consolidation |
| R-07 | Legacy RC4 encryption inadequate | Low | Medium | **Low** | Document limitations; AES in Phase 4 |
| R-08 | SkiaSharp version lock-in / breaking changes | Low | Medium | **Low** | Pin version; test on upgrades |
| R-09 | Original Fonet behavior unknown for edge cases | Medium | Medium | **Medium** | Reference Apache FOP; add regression tests |
| R-10 | Single maintainer / bus factor | Medium | High | **High** | Documentation (this folder); clear architecture |

---

## Critical Concerns

### 1. Silent Correctness Failures

The most dangerous aspect of the current migration is that **stubbed GDI methods fail silently**. Unlike a crash or exception, returning glyph index 0 for every character produces a PDF that looks "almost right" for ASCII text (glyph 0 is often `.notdef` or space) but is completely wrong for any non-trivial font embedding.

**Concern:** Consumers may deploy the library thinking it works because simple Latin documents appear fine, then encounter garbled text in production.

**Recommendation:**
- Add diagnostic mode that warns when stub code paths are hit
- Fail fast on glyph index 0 for printable characters
- Document known limitations prominently in NuGet package description

### 2. GDI Shim Technical Debt

Retaining 54 files under `Pdf/Gdi/` with Win32-shaped structs (`LogFont`, `TextMetric`, `GlyphSet`) creates ongoing confusion:

- New contributors assume GDI is still used
- Dual code paths (FontManager vs LibWrapper) diverge
- Refactoring is harder because of indirection through dummy handles

**Recommendation:** Treat GDI shim removal as a hard milestone (M1), not optional cleanup.

### 3. Test Coverage Gap

Five smoke tests with file-existence assertions provide **near-zero confidence** in correctness. The test suite cannot detect:

- Wrong glyph mapping
- Incorrect layout calculations
- Missing FO property effects
- Cross-platform font differences
- PDF structural errors

**Recommendation:** No production release until Layer 2+ tests exist (see [Testing Strategy](./testing-strategy.md)).

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

### 5. Namespace Fragmentation

Three namespace families (`Genocs.Fonet`, `Fonet.Fo`, `Fonet.Pdf`) reflect incomplete migration. This causes:

- `FonetDriver` resolution issues across namespaces
- Confusion about public vs. internal API
- Harder NuGet packaging (what namespace do consumers import?)

**Recommendation:** Consolidate to `Genocs.Fonet.*` in Phase 2; use `[Obsolete]` attributes on old namespaces if needed.

### 6. No Complex Text Shaping

The library lacks HarfBuzz or equivalent text shaping. This means:

- No ligatures (fi, fl, etc.) unless font handles them automatically
- No correct Arabic/Hebrew joining behavior
- No Indic script reordering
- Kerning is limited to font table pairs (with O(n²) lookup)

**Impact:** Documents requiring non-Latin scripts or typographic quality will render incorrectly even after font pipeline fixes.

**Recommendation:** Document as known limitation. Evaluate HarfBuzzSharp integration in Phase 4 if i18n is a requirement.

---

## Operational Concerns

### 7. No CI/CD Pipeline

Without automated builds:

- Cross-platform regressions go undetected
- PRs can merge broken code
- No test coverage tracking over time
- No automated NuGet publishing

**Recommendation:** Phase 0 priority — GitHub Actions with Windows/Linux/macOS matrix.

### 8. Font Discovery Brittleness

`FontManager.LocateSystemFont` scans filesystem directories with filename substring matching. On Linux:

- Font config may use different directory structures
- Font families may not match filenames (e.g., "DejaVu Sans" in `DejaVuSans.ttf`)
- Recursive scan is slow on first call

**Recommendation:** Use `SKFontManager.MatchFamily` as primary; filesystem scan as last resort with PostScript name validation.

### 9. Nullable Reference Warnings (~3,000+)

While not runtime bugs, the volume of nullable warnings indicates:

- Incomplete migration to nullable reference types
- Potential `NullReferenceException` at runtime in unexercised paths
- Noise makes it hard to spot new warnings

**Recommendation:** Fix warnings in hot paths (font, layout, render) first; batch-fix DataTypes/Fo in Phase 2.

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
- No timeout on `WebRequest` (obsolete API)
- No size limits on downloaded images

**Recommendation:** Add configurable URI allowlist; migrate to `HttpClient` with timeout; limit download size.

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
| D-04 | Public API namespace | `Genocs.Fonet` / `Fonet` | Phase 2 | TBD |
| D-05 | Minimum supported .NET version | net8.0 / net9.0 | Phase 0 | TBD |
| D-06 | Apache FOP compatibility target | Full / Best-effort / None | Phase 3 | TBD |
| D-07 | NuGet package publishing | Public / Private feed | Phase 4 | TBD |

---

## Concern Resolution Tracking

| Concern | Status | Resolution | Date |
|---------|--------|------------|------|
| Build failures (README claim) | ✅ Resolved | Solution compiles on net8/9/10 | 2026-06 |
| GDI P/Invoke removal | ✅ Resolved | Replaced with LibWrapper/SkiaSharp | — |
| Font glyph mapping | ❌ Open | Phase 1 | — |
| Test coverage | ❌ Open | Phase 0–2 | — |
| FO feature stubs | ❌ Open | Phase 3 | — |
| CI pipeline | ❌ Open | Phase 0 | — |

Update this table as concerns are addressed.
