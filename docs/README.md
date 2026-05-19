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

## Quick Status (June 2026)

| Area | Status |
|------|--------|
| Build | Compiles on `net8.0` / `net9.0` / `net10.0` (nullable warnings remain) |
| `System.Drawing` / GDI P/Invoke | Removed |
| SkiaSharp integration | Fonts (cmap, table access) and images |
| GDI compatibility layer | Thin shim — real implementations behind `LibWrapper` |
| Test suite | 9 integration tests on net8/9/10 (incl. font pipeline) |
| CI / automation | GitHub Actions (Windows, Linux, macOS) |
| Production readiness | **Not ready** — FO coverage gaps; CJK needs validation |

## How to Use These Docs

1. Start with [Assessment Overview](./assessment-overview.md) for the big picture.
2. Review [Known Issues](./known-issues.md) and [Risks & Concerns](./risks-and-concerns.md) before planning work.
3. Follow [Migration Plan](./migration-plan.md) for phased execution.
4. Update issue tables and milestone checkboxes as work progresses.

## Related Resources

- Root [README.md](../README.md) — build instructions and cross-platform notes (partially stale)
- Test templates: `src/tests/Genocs.Fonet.Tests/templates/`
- Entry point: `src/Genocs.Fonet/FonetDriver.cs`
