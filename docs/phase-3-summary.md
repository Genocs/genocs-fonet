# Phase 3 Summary — FO Feature Completeness (Tier 1)

Started June 2026. Tier 1 initial batch complete.

## Tier 1 Properties Implemented

| Property | Maker | Layout / PDF integration |
|----------|-------|--------------------------|
| `visibility` | `VisibilityMaker` | `PropertyManager.IsVisible()`; skipped in `Block`, `FObjMixed` |
| `word-spacing` | `WordSpacingMaker` | `FontState.WordSpacing`; `LineArea`; PDF `Tw` operator |
| `margin` (shorthand) | `MarginMaker` + `GenericMargin` | Resolves via `margin-top/right/bottom/left` |
| `float` | `FloatMaker` | Used by `fo:float` side placement (block-level `float` deferred) |
| `clear` | `ClearMaker` | Layout integrated via `ColumnArea.GetClearOffset` |
| `z-index` | `ZIndexMaker` | Layout integrated; children render in z-order |
| `caption-side` | `CaptionSideMaker` | Used by `TableAndCaption` ordering |

Already working before Phase 3: `background-color`, `background-image`, `letter-spacing`.

## Tier 1 Elements Implemented

| Element | File | Behavior |
|---------|------|----------|
| `fo:table-caption` | `Flow/TableCaption.cs` | Lays out caption content in a `BlockArea` |
| `fo:table-and-caption` | `Flow/TableAndCaption.cs` | Orders caption/table by `caption-side` |
| `fo:float` | `Flow/Float.cs` | Side-float via `SideFloatArea`; left/right placement with text wrap |

## Tests Added

| Test file | Count | Coverage |
|-----------|-------|----------|
| `Phase3FeatureTests.cs` | 4 | Tier 1 FO template + float/clear/z-index tests + regression |
| `Phase3PropertyTests.cs` | 5 | Enum/shorthand property parsing |
| `templates/Phase3Tier1Test.fo` | — | margin, visibility, spacing, table caption |
| `templates/Phase3FloatSideTest.fo` | — | left/right side floats with wrapping text |

**Total tests:** 29 on net10.0 (was 9).

## Deferred (Tier 1 gaps / Tier 2+)

| Item | Notes |
|------|-------|
| `visibility: collapse` | Treated like `hidden` (content skipped) |
| RTL / bidi (Tier 2) | `direction`, `unicode-bidi`, `fo:bidi-override` still stubbed |
| Multi-switch / aural (Tier 3) | Unchanged |

## Next Steps

1. Tier 2 i18n properties (`direction`, `unicode-bidi`)
2. Expand template inventory from consumer documents
