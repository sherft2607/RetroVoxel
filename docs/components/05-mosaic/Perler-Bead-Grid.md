# Perler Bead Grid

Builds a pegboard-style bead layout mesh from a Pixel Grid JSON — one disc per non-void pixel.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pixel Grid JSON (PGJ) | Text | Yes | — | Grid from [Pixel Grid Builder](../03-grid/Pixel-Grid-Builder.md) |
| Bead Size (BS) | Number | No | `5.0` | Peg pitch and bead diameter |

## Outputs

| Name | Type | Description |
|---|---|---|
| Bead Grid Mesh (BGM) | Mesh | One flat bead disc per placed cell |

## API endpoint

None — local geometry generation only.

## Notes

- Warns (but doesn't error) if the grid has no non-void cells — no beads are placed.

## Related

- Shares the `Pixel Grid JSON` input with [Lego Stud Plate](Lego-Stud-Plate.md) and
  [LED Matrix Mapper](LED-Matrix-Mapper.md).
