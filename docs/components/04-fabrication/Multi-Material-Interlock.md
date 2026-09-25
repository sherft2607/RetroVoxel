# Multi-Material Interlock

Tags a voxel mesh with its matched filament so multi-material slicers can split by color.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Voxel Mesh (VM) | Mesh | Yes | — | Mesh from [Voxel Mesh Shell](Voxel-Mesh-Shell.md) |
| Filament Match JSON (FMJ) | Text | No | `""` | One match from [Delta-E Filament Matcher](../02-color/Delta-E-Filament-Matcher.md) |

## Outputs

| Name | Type | Description |
|---|---|---|
| Interlock Mesh (IM) | Mesh | Mesh tagged with its matched filament name |

## API endpoint

None — local geometry/metadata logic only.

## Notes

- **v1 simplification:** tags the whole mesh with its dominant filament match (`FilamentName` in
  the mesh's Rhino `UserDictionary`) rather than generating per-color interlocking dovetail
  geometry. A slicer profile or a follow-up split step reads that tag to separate materials.

## Related

- Consumes [Voxel Mesh Shell](Voxel-Mesh-Shell.md) and
  [Delta-E Filament Matcher](../02-color/Delta-E-Filament-Matcher.md).
