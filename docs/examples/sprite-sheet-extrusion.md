# Sprite sheet extrusion

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** split a local sprite sheet into frames and vary voxel extrusion per frame; also covers
quantizing a tree of sprites and previewing the closest real filament match.

## Components (wiring order)

1. [Local Sprite Loader](../components/01-ingest/Local-Sprite-Loader.md) (`LOAD`)
2. [Sprite Sheet Splitter](../components/01-ingest/Sprite-Sheet-Splitter.md) (`SPLIT`)
3. [Palette Quantizer](../components/02-color/Palette-Quantizer.md) (`QUANT`)
4. [Pixel Grid Builder](../components/03-grid/Pixel-Grid-Builder.md) (`GRID`)
5. [Voxel Mesh Shell](../components/04-fabrication/Voxel-Mesh-Shell.md) (`VOX`)

## Inputs to set

- `Local Sprite Loader.File Path` → any local sprite sheet PNG
- `Sprite Sheet Splitter.Frame Width` / `Frame Height` → match the sheet's grid (e.g. `16`, `16`)
- `Palette Quantizer.Max Colors` → `16`

## Expected result

One voxel mesh per frame, with extrusion height controllable per frame via
`Voxel Mesh Shell.Extrusion Height`.

See [demos/README.md](../../demos/README.md#3-sprite-sheet-extrusion) for the full manual build
spec.
