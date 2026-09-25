# Voxel Mesh Shell

Extrudes every cell of a Pixel Grid JSON into a voxel box, merged into one shell mesh.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pixel Grid JSON (PGJ) | Text | Yes | — | Grid from [Pixel Grid Builder](../03-grid/Pixel-Grid-Builder.md) |
| Voxel Size (VS) | Number | No | `2.0` | Edge length of one voxel cell |
| Extrusion Height (EH) | Number list | No | — | Optional per-cell height, row-major order; last value repeats |

## Outputs

| Name | Type | Description |
|---|---|---|
| Voxel Mesh (VM) | Mesh | Merged voxel shell mesh |

## API endpoint

None — local geometry generation only.

## Notes

- A void cell (no palette entry at that position) is skipped unless `Extrusion Height` is supplied,
  in which case every position gets a box (useful when height alone should drive the silhouette).

## Related

- Feeds [Multi-Material Interlock](Multi-Material-Interlock.md) and
  [Achievement Attribute Bridge](Achievement-Attribute-Bridge.md).
