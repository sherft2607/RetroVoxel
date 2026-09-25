# Tabular grid mosaic

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** take a simple tabular color grid — hand-authored, from a spreadsheet, or sprite-derived —
and turn it into mosaic or voxel geometry (the cross-platform tabular-to-platform benchmark).

## Components (wiring order)

1. [Pixel Grid Builder](../components/03-grid/Pixel-Grid-Builder.md) (`GRID`)
2. [Lego Stud Plate](../components/05-mosaic/Lego-Stud-Plate.md) (`LEGO`)
3. [Perler Bead Grid](../components/05-mosaic/Perler-Bead-Grid.md) (`PERLER`)
4. [LED Matrix Mapper](../components/05-mosaic/LED-Matrix-Mapper.md) (`LED`)

## Inputs to set

- `Pixel Grid Builder.Rows` → either a Colour-tree grid or one comma-separated hex string per row,
  e.g. `"#FF0000, #00FF00, #0000FF, #FFFFFF"`

## Expected result

All three Mosaic components consume the same `Pixel Grid JSON` interchangeably, producing a stud
plate, a bead layout, and LED wiring addresses from one grid.

See [demos/README.md](../../demos/README.md#4-tabular-grid-mosaic) for the full manual build spec,
including both `Rows` input forms.
