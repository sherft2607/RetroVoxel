# Pixel Grid Builder

Builds a Pixel Grid JSON from rows of pixel data — the single shared contract every
Fabrication/Mosaic component consumes. This is RetroVoxel's answer to the cross-platform
"tabular data → platform content" benchmark: any tabular color grid, whatever its source, becomes
fabrication geometry through this one component.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Rows (RV) | Generic (Colour or Text) tree | Yes | — | One branch per row — see **Row formats** below |
| Pitch (P) | Number | No | `2.0` | Informational cell pitch, stored in the grid JSON |
| Thickness (TH) | Number | No | `1.0` | Informational material thickness, stored in the grid JSON |

### Row formats — either one, per branch

1. **Colour/hex tree** — a branch `{y}` holds multiple Colour or hex-text items, one per cell.
2. **Comma-separated hex string** — a branch holds a single string like
   `"#FF0000, #00FF00, #0000FF"`; it's split into that row's cells automatically.

An empty cell, an empty string, or the literal value `VOID` marks a transparent/void cell (index
`-1` in the output, no palette entry).

## Outputs

| Name | Type | Description |
|---|---|---|
| Pixel Grid JSON (PGJ) | Text | `{bounds:{rows,cols}, pitch, thickness, palette:[hex,...], indices:[[idx,...],...]}` |

## API endpoint

None — local aggregation logic only.

## Notes

- The palette is deduplicated in first-seen order — an exact-match dedup, not a lossy quantization
  (use [Palette Quantizer](../02-color/Palette-Quantizer.md) upstream first if you need to reduce
  colors, not just dedupe them).
- A ragged row (fewer cells than the widest row) is padded with its last cell's value, never
  truncated or dropped.

## Related

- Feeds [Voxel Mesh Shell](../04-fabrication/Voxel-Mesh-Shell.md),
  [Lego Stud Plate](../05-mosaic/Lego-Stud-Plate.md),
  [Perler Bead Grid](../05-mosaic/Perler-Bead-Grid.md), and
  [LED Matrix Mapper](../05-mosaic/LED-Matrix-Mapper.md) — all four consume the identical
  `Pixel Grid JSON` interchangeably.
