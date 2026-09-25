# RA Game Images

Downloads one of a game's four artwork images and its pixel colors — the game-level counterpart
to [RA Badges](RA-Badges.md), so a whole game's artwork can be voxelized/mosaicked the same way an
achievement badge can, not just achievements.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Game JSON (GJ) | Text | Yes | — | Game metadata from [RA Game Info](RA-Game-Info.md) |
| Image Type (IT) | Text | No | `"Icon"` | One of: `Icon`, `Title`, `Ingame`, `BoxArt` |
| Max Dimension (MD) | Integer | No | `64` | Downsamples the image so its longer side is at most this many pixels (0 = no resize) |

## Outputs

| Name | Type | Description |
|---|---|---|
| Image Path (IP) | Text | Downloaded image path |
| Pixel Colors (PC) | Colour tree | One branch per row of pixels |
| Status (S) | Text | `OK`, or the download error |

## API endpoint

None — resolves an image path already present in `Game JSON` (`ImageIcon`/`ImageTitle`/
`ImageIngame`/`ImageBoxArt`) to `https://i.retroachievements.org{path}` and downloads it directly
from the RA media CDN (no API key required for the CDN itself).

## Notes

- `Image Type` is case-insensitive; an unrecognized value errors with the valid list.
- Downloaded images are cached to `%TEMP%\RetroVoxel\gameimages\`.
- **Box art and title screens are much higher resolution than badges** (often 300-600px+ vs. a
  badge's ~64px). Voxelizing every literal pixel of a full-resolution box art image builds and
  merges hundreds of thousands of tiny meshes downstream, which can hang or crash Rhino — this is
  why `Max Dimension` defaults to `64` rather than passing the image through at native resolution.

## Related

- Consumes [RA Game Info](RA-Game-Info.md)'s `Game JSON`.
- `Pixel Colors` feeds [Palette Quantizer](../02-color/Palette-Quantizer.md) or
  [Pixel Grid Builder](../03-grid/Pixel-Grid-Builder.md), same as any other Ingest component.
