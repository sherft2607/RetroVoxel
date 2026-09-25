# Sprite Sheet Splitter

Splits a sprite sheet into a tree of individual frame images and per-frame pixel colors.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Image Path (IP) | Text | Yes | `""` | Path to the sprite sheet image |
| Frame Width (FW) | Integer | Yes | `16` | Width of a single frame in pixels |
| Frame Height (FH) | Integer | Yes | `16` | Height of a single frame in pixels |

## Outputs

| Name | Type | Description |
|---|---|---|
| Frame Image Path (FIP) | Text tree | One extracted frame image path per branch `{frameIndex}` |
| Frame Index (FI) | Integer tree | Sequential frame index per branch |
| Pixel Colors (PC) | Colour tree | Rows of pixels per frame, path `{frameIndex, row}` |

## API endpoint

None — local file read only.

## Notes

- Frames are read left-to-right, top-to-bottom, based on `Frame Width` / `Frame Height` dividing the
  sheet's pixel dimensions.
- Extracted frame PNGs are written to `%TEMP%\RetroVoxel\frames\`.

## Related

- Feeds [Palette Quantizer](../02-color/Palette-Quantizer.md) via `Pixel Colors`.
