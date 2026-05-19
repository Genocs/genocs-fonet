# Feature Completeness

XSL-FO property and element implementation status for migration tracking.

## Summary

| Category | Implemented | Stubbed (`ToBeImplemented`) | Total |
|----------|-------------|----------------------------|-------|
| Properties | ~200+ | ~87 | ~287 |
| Elements | ~40+ | 13 | ~53+ |

Stubbed properties log a warning and are ignored during layout. Stubbed elements parse but produce no output.

## Unimplemented Elements

These extend `ToBeImplementedElement` — they exist in the FO tree but contribute nothing to layout.

| Element | File | Category | Migration Priority |
|---------|------|----------|-------------------|
| `fo:float` | `Fo/Flow/Float.cs` | Layout | **Tier 1** — partial (in-flow render) |
| `fo:table-caption` | `Fo/Flow/TableCaption.cs` | Tables | **Tier 1** — ✅ Done |
| `fo:table-and-caption` | `Fo/Flow/TableAndCaption.cs` | Tables | **Tier 1** — ✅ Done |
| `fo:inline-container` | `Fo/Flow/InlineContainer.cs` | Layout | Tier 2 |
| `fo:bidi-override` | `Fo/Flow/BidiOverride.cs` | i18n | Tier 2 |
| `fo:multi-switch` | `Fo/Flow/MultiSwitch.cs` | Conditional | Tier 3 |
| `fo:multi-case` | `Fo/Flow/MultiCase.cs` | Conditional | Tier 3 |
| `fo:multi-toggle` | `Fo/Flow/MultiToggle.cs` | Conditional | Tier 3 |
| `fo:multi-properties` | `Fo/Flow/MultiProperties.cs` | Conditional | Tier 3 |
| `fo:multi-property-set` | `Fo/Flow/MultiPropertySet.cs` | Conditional | Tier 3 |
| `fo:initial-property-set` | `Fo/Flow/InitialPropertySet.cs` | Properties | Tier 3 |
| `fo:title` | `Fo/Title.cs` | Metadata | Tier 3 |
| `fo:declarations` | `Fo/Declarations.cs` | Metadata | Tier 3 |
| `fo:color-profile` | `Fo/ColorProfile.cs` | Color | Tier 3 |

## Unimplemented Properties

Properties using `ToBeImplementedProperty.Maker`. Grouped by functional area.

### Layout & Positioning (Tier 1)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `float` | `FloatMaker.cs` | Floating content |
| `clear` | `ClearMaker.cs` | Clear floats |
| `z-index` | `ZIndexMaker.cs` | Stacking order |
| `relative-position` | `RelativePositionMaker.cs` | Relative positioning |
| `visibility` | `VisibilityMaker.cs` | Show/hide |
| `clip` | `ClipMaker.cs` | Clipping |
| `margin` (shorthand) | `MarginMaker.cs` | Margin shorthand |
| `size` | `SizeMaker.cs` | Page size shorthand |
| `min-width` | `MinWidthMaker.cs` | Minimum width |
| `min-height` | `MinHeightMaker.cs` | Minimum height |

### Background & Borders (Tier 1)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `background` (shorthand) | `BackgroundMaker.cs` | Background shorthand |
| `background-attachment` | `BackgroundAttachmentMaker.cs` | Fixed/scroll background |
| `background-position` | `BackgroundPositionMaker.cs` | Background position |
| `background-position-horizontal` | `BackgroundPositionHorizontalMaker.cs` | H position |
| `background-position-vertical` | `BackgroundPositionVerticalMaker.cs` | V position |
| `border-spacing` | `BorderSpacingMaker.cs` | Table cell spacing |
| `border-*-precedence` (4) | Various | Border conflict resolution |
| `empty-cells` | `EmptyCellsMaker.cs` | Empty table cell display |
| `caption-side` | `CaptionSideMaker.cs` | Table caption position |

### Text & Typography (Tier 1–2)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `word-spacing` | `WordSpacingMaker.cs` | Word spacing |
| `white-space` | `WhiteSpaceMaker.cs` | Whitespace handling |
| `white-space-treatment` | `WhiteSpaceTreatmentMaker.cs` | Whitespace treatment |
| `linefeed-treatment` | `LinefeedTreatmentMaker.cs` | Line feed handling |
| `text-transform` | `TextTransformMaker.cs` | Uppercase/lowercase |
| `text-shadow` | `TextShadowMaker.cs` | Text shadow |
| `font` (shorthand) | `FontMaker.cs` | Font shorthand |
| `font-stretch` | `FontStretchMaker.cs` | Font stretch |
| `font-size-adjust` | `FontSizeAdjustMaker.cs` | Font size adjust |
| `font-selection-strategy` | `FontSelectionStrategyMaker.cs` | Font selection |
| `script` | `ScriptMaker.cs` | Script system |
| `line-height-shift-adjustment` | `LineHeightShiftAdjustmentMaker.cs` | Line height |
| `line-stacking-strategy` | `LineStackingStrategyMaker.cs` | Line stacking |
| `alignment-baseline` | `AlignmentBaselineMaker.cs` | Baseline alignment |
| `alignment-adjust` | `AlignmentAdjustMaker.cs` | Alignment adjust |
| `dominant-baseline` | `DominantBaselineMaker.cs` | Dominant baseline |
| `text-altitude` | `TextAltitudeMaker.cs` | Text altitude |
| `text-depth` | `TextDepthMaker.cs` | Text depth |
| `suppress-at-line-break` | `SuppressAtLineBreakMaker.cs` | Line break suppression |
| `score-spaces` | `ScoreSpacesMaker.cs` | Space scoring |
| `treat-as-word-space` | `TreatAsWordSpaceMaker.cs` | Word space treatment |

### Internationalization (Tier 2)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `direction` | `DirectionMaker.cs` | Text direction (LTR/RTL) |
| `unicode-bidi` | `UnicodeBidiMaker.cs` | Bidirectional text |
| `glyph-orientation-horizontal` | `GlyphOrientationHorizontalMaker.cs` | Glyph orientation |
| `glyph-orientation-vertical` | `GlyphOrientationVerticalMaker.cs` | Vertical glyphs |
| `reference-orientation` | `ReferenceOrientationMaker.cs` | Reference orientation |

### Tables (Tier 1–2)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `starts-row` | `StartsRowMaker.cs` | Row start |
| `ends-row` | `EndsRowMaker.cs` | Row end |
| `last-line-end-indent` | `LastLineEndIndentMaker.cs` | Last line indent |

### Page Breaks (Tier 2)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `page-break-before` | `PageBreakBeforeMaker.cs` | Page break before |
| `page-break-after` | `PageBreakAfterMaker.cs` | Page break after |
| `page-break-inside` | `PageBreakInsideMaker.cs` | Page break inside |

### Hyphenation (Tier 2)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `hyphenation-keep` | `HyphenationKeepMaker.cs` | Hyphenation keep |
| `hyphenation-ladder-count` | `HyphenationLadderCountMaker.cs` | Hyphenation ladder |

### Links & Destinations (Tier 2)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `show-destination` | `ShowDestinationMaker.cs` | Link display |
| `indicate-destination` | `IndicateDestinationMaker.cs` | Destination indication |
| `destination-placement-offset` | `DestinationPlacementOffsetMaker.cs` | Destination offset |
| `target-processing-context` | `TargetProcessingContextMaker.cs` | Target context |
| `target-presentation-context` | `TargetPresentationContextMaker.cs` | Presentation context |
| `target-stylesheet` | `TargetStylesheetMaker.cs` | Target stylesheet |

### Color (Tier 3)

| Property | Maker File | Impact |
|----------|-----------|--------|
| `color-profile-name` | `ColorProfileNameMaker.cs` | ICC profile |
| `rendering-intent` | `RenderingIntentMaker.cs` | Color rendering intent |

### Aural / Accessibility (Tier 3 — low priority for PDF)

| Property | Maker File |
|----------|-----------|
| `speak` | `SpeakMaker.cs` |
| `speak-header` | `SpeakHeaderMaker.cs` |
| `speak-numeral` | `SpeakNumeralMaker.cs` |
| `speak-punctuation` | `SpeakPunctuationMaker.cs` |
| `speech-rate` | `SpeechRateMaker.cs` |
| `voice-family` | `VoiceFamilyMaker.cs` |
| `volume` | `VolumeMaker.cs` |
| `pitch` | `PitchMaker.cs` |
| `pitch-range` | `PitchRangeMaker.cs` |
| `stress` | `StressMaker.cs` |
| `richness` | `RichnessMaker.cs` |
| `azimuth` | `AzimuthMaker.cs` |
| `elevation` | `ElevationMaker.cs` |
| `cue` | `CueMaker.cs` |
| `cue-before` | `CueBeforeMaker.cs` |
| `cue-after` | `CueAfterMaker.cs` |
| `pause` | `PauseMaker.cs` |
| `pause-before` | `PauseBeforeMaker.cs` |
| `pause-after` | `PauseAfterMaker.cs` |
| `play-during` | `PlayDuringMaker.cs` |

### Multi-property / Conditional (Tier 3)

| Property | Maker File |
|----------|-----------|
| `switch-to` | `SwitchToMaker.cs` |
| `case-name` | `CaseNameMaker.cs` |
| `case-title` | `CaseTitleMaker.cs` |
| `active-state` | `ActiveStateMaker.cs` |
| `starting-state` | `StartingStateMaker.cs` |
| `auto-restore` | `AutoRestoreMaker.cs` |
| `content-type` | `ContentTypeMaker.cs` |
| `media-usage` | `MediaUsageMaker.cs` |
| `scaling-method` | `ScalingMethodMaker.cs` |
| `xml:lang` | `XMLLangMaker.cs` |

## Unimplemented Expression Functions

| Function | File | Status |
|----------|------|--------|
| `from-table-column()` | `Fo/Expr/FromTableColumnFunction.cs` | Throws at runtime |

## Implemented Features (Confirmed Working)

Based on existing tests and code review:

| Feature | Evidence |
|---------|----------|
| Basic blocks and inlines | `StarWarsMovies.fo` test |
| Font family, size, weight, style | Nunito tests |
| Custom/private fonts | `AddPrivateFont` API |
| `scale-to-fit` | `ScaleToFitTest.fo` |
| External graphics (images) | `CrossPlatformTest.fo` |
| Base-14 PDF fonts | `Render/Pdf/Fonts/Base14Font.cs` |
| Tables (basic) | StarWars template |
| Lists | StarWars template |
| Page masters and sequences | All templates |
| Internal links (basic) | Code exists in `PdfRenderer` |
| Keep properties | `DataTypes/Keep.cs` implemented |
| Borders (individual sides) | Implemented makers (not shorthand) |
| Padding (individual sides) | Implemented makers |

## Feature Triage Process

When a consumer reports a missing feature:

1. **Identify** the property/element from the FO template and warning log
2. **Check** this matrix for current status
3. **Assess** impact — does the document render incorrectly or just miss styling?
4. **Prioritize** using tier system (Tier 1 = implement next)
5. **Implement** property maker or element class with layout logic
6. **Test** — add FO template + PDF validation test
7. **Update** this matrix

## Tracking Template

| Property/Element | Status | Tier | Implemented In | Test |
|-----------------|--------|------|----------------|------|
| `background-color` | ✅ Done | — | `BackgroundColorMaker.cs` | — |
| `float` | ✅ Tier 1 partial | 1 | `FloatMaker.cs` | Enum only; side placement deferred |
| `visibility` | ✅ Done | 1 | `VisibilityMaker.cs` | `Phase3Tier1Test.fo` |
| `margin` (shorthand) | ✅ Done | 1 | `MarginMaker.cs` | `Phase3Tier1Test.fo` |
| `word-spacing` | ✅ Done | 1 | `WordSpacingMaker.cs` | `Phase3Tier1Test.fo` |
| `letter-spacing` | ✅ Done | — | `LetterSpacingMaker.cs` | — |
| `caption-side` | ✅ Done | 1 | `CaptionSideMaker.cs` | `Phase3Tier1Test.fo` |
| `z-index` | ✅ Parse only | 1 | `ZIndexMaker.cs` | — |
| `clear` | ✅ Parse only | 1 | `ClearMaker.cs` | — |

Update this table as features are implemented during Phase 3.
