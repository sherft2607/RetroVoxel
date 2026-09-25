# Achievement Attribute Bridge

Writes an achievement's Title, Points, Description, and BadgeName into its matching voxel mesh's
`UserDictionary` — a BIM Data Bridge, selected at Gate 3 — so a fabricated plaque batch stays
self-documenting after it leaves the Grasshopper tree structure.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Voxel Mesh (VM) | Mesh | Yes | — | Mesh from [Voxel Mesh Shell](Voxel-Mesh-Shell.md) |
| Achievement JSON (AJ) | Text | No | `""` | Achievement record from [RA Game Info](../01-ingest/RA-Game-Info.md) |

## Outputs

| Name | Type | Description |
|---|---|---|
| Tagged Mesh (TM) | Mesh | Mesh carrying achievement metadata in its `UserDictionary` |

## API endpoint

None — local metadata logic only.

## Notes

- If `Achievement JSON` is missing or invalid, the mesh passes through untagged rather than
  erroring the whole solve.
- Inspect the tag with a native GH `RhinoScript` component reading `mesh.UserDictionary`, or any
  downstream step that reads Rhino user data.

## Related

- Consumes [Voxel Mesh Shell](Voxel-Mesh-Shell.md) and
  [RA Game Info](../01-ingest/RA-Game-Info.md).
