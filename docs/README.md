# Genocs.Fonet Migration Documentation

This folder tracks the assessment, concerns, and migration plan for porting the legacy Windows/GDI-based Fonet XSL-FO library to a cross-platform .NET implementation.

## Documents

| Document | Purpose |
|----------|---------|
| [Assessment Overview](./assessment-overview.md) | Executive summary, current readiness, and key findings |
| [Architecture](./architecture.md) | Pipeline, module layout, and technology choices |
| [Known Issues](./known-issues.md) | Categorized bugs, stubs, and blockers with severity |
| [Migration Plan](./migration-plan.md) | Phased roadmap, milestones, and success criteria |
| [Font & Glyph Migration](./font-and-glyphs.md) | Deep dive on the GDI shim replacement strategy |
| [Testing Strategy](./testing-strategy.md) | Current coverage, gaps, and recommended test layers |
| [Feature Completeness](./feature-completeness.md) | XSL-FO property and element implementation matrix |
| [Risks & Concerns](./risks-and-concerns.md) | Technical debt, operational risks, and open decisions |
| [Phase 1 Summary](./phase-1-summary.md) | Font pipeline implementation details (completed) |
| [Phase 2 Summary](./phase-2-summary.md) | Quality & performance improvements (completed) |
| [Phase 3 Summary](./phase-3-summary.md) | FO feature completeness — Tier 1 batch (in progress) |

## Quick Status (August 2026)

| Area | Status |
|------|--------|
| Build | Compiles on `net8.0` / `net9.0` / `net10.0` |
| `System.Drawing` / GDI P/Invoke | Removed |
| SkiaSharp integration | Fonts (cmap, table access) and images |
| GDI compatibility layer | Thin shim — font ops via `FontManager` / `FontTableAccess` |
| Font pipeline (Phase 1) | ✅ Complete — glyph mapping, subsetting, metrics |
| Quality & performance (Phase 2) | ✅ Complete — image spans, encoding, namespaces |
| FO completeness (Phase 3) | 🔄 Tier 1 initial batch done; side-float layout deferred |
| Test suite | 22 tests on net8/9/10 with PDF structure validation |
| CI / automation | GitHub Actions (build, test, pack, NuGet publish workflows) |
| Production readiness | **Not ready** — ~87 FO properties still stubbed; CJK needs validation |

## How to Use These Docs

1. Start with [Assessment Overview](./assessment-overview.md) for the big picture.
2. Review [Known Issues](./known-issues.md) and [Risks & Concerns](./risks-and-concerns.md) before planning work.
3. Follow [Migration Plan](./migration-plan.md) for phased execution.
4. Update issue tables and milestone checkboxes as work progresses.

## Related Resources

- Root [README.md](../README.md) — build instructions and cross-platform notes
- Test templates: `src/tests/Genocs.Fonet.Tests/templates/`
- Entry point: `src/Genocs.Fonet/FonetDriver.cs`
