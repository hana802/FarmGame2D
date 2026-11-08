# Farm Plot State Art Design

## Goal

Replace the prototype solid-color farm plots with five readable illustrated states: untilled, tilled, planted, watered, and harvest-ready. The art should use the supplied farm-stage image as a state-readability reference while matching this project's bright grass and warm, polished farming-game palette.

## Design

Each soil state owns one square transparent PNG under `Assets/Resources/Art/World/FarmPlots`. All tiles share a centered orthographic footprint and rounded organic edge. Their distinguishing cues are intentionally redundant: surface color, furrow treatment, and crop detail all change, so the states remain recognizable at gameplay scale.

`FarmPlotArtCatalog` is the single mapping between the farming domain's `SoilState` and Unity resource paths. It loads and caches textures, constructs sprites with a consistent one-unit footprint, and keeps resource lookup out of the view. `FarmPlotView` asks the catalog for the current state sprite whenever the session changes. If an asset is unavailable, the existing state-color palette remains as a safe visual fallback.

## State Mapping

- `Empty`: grassy, stony, irregular ground that has not been cultivated.
- `Tilled`: dry orange-brown soil with clear parallel furrows.
- `Planted`: dry furrows with evenly spaced seed marks.
- `Watered`: dark wet soil with moisture highlights and young green sprouts.
- `Ready`: dense golden wheat rows ready to harvest.

## Verification

An editor test loads the sprite for every enum value. The existing play-mode smoke test verifies that the main scene still boots and the farm plots are constructed. A Unity batch-mode import/build check catches C# compilation and resource-import errors.
