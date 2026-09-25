# Lego Stud Plate

Builds a watertight Lego-compatible stud plate mesh from a Pixel Grid JSON — one base plate plus
one stud cylinder per non-void pixel.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pixel Grid JSON (PGJ) | Text | Yes | — | Grid from [Pixel Grid Builder](../03-grid/Pixel-Grid-Builder.md) |
| Stud Size (SS) | Number | No | `8.0` | Plate cell pitch; stud proportions scale from it |

## Outputs

| Name | Type | Description |
|---|---|---|
| Stud Plate Mesh (SPM) | Mesh | Watertight base plate with one stud per cell |

## API endpoint

None — local geometry generation only.

## Notes

- A void cell in the grid produces no stud at that position, but the base plate always spans the
  full grid bounds.

## Related

- Shares the `Pixel Grid JSON` input with [Perler Bead Grid](Perler-Bead-Grid.md) and
  [LED Matrix Mapper](LED-Matrix-Mapper.md) — interchangeable outputs from the same grid.
