# Game badges to voxel

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** pull every achievement badge for a game and turn the whole set into individually-sized,
self-documenting voxel plaques in one operation.

## Components (wiring order)

1. [RA Game Info](../components/01-ingest/RA-Game-Info.md) (`GAME`)
2. [RA Badges](../components/01-ingest/RA-Badges.md) (`BADGE`)
3. [Pixel Grid Builder](../components/03-grid/Pixel-Grid-Builder.md) (`GRID`)
4. [Voxel Mesh Shell](../components/04-fabrication/Voxel-Mesh-Shell.md) (`VOX`)
5. [Achievement Attribute Bridge](../components/04-fabrication/Achievement-Attribute-Bridge.md) (`BRIDGE`)

## Inputs to set

- `RA Game Info.Token` → your API key
- `RA Game Info.Game ID` → `1446` (Super Mario Bros., validated seed)

## Expected result

Each achievement's badge is downloaded, voxelized, and tagged with its Title/Points/Description in
the mesh's Rhino UserDictionary.

See [demos/README.md](../../demos/README.md#2-game-badges-to-voxel) for the full manual build spec,
including the tree-nesting note for `RA Badges.Pixel Colors`.
