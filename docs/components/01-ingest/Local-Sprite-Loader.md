# Local Sprite Loader

Loads a single-frame sprite image from disk and reports its size and pixel colors.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| File Path (FP) | Text | Yes | `""` | Path to the sprite image on disk |

## Outputs

| Name | Type | Description |
|---|---|---|
| Image Path (IP) | Text | Same file path, passed through for downstream components |
| Width (W) | Integer | Image width in pixels |
| Height (H) | Integer | Image height in pixels |
| Pixel Colors (PC) | Colour tree | One branch `{row}` per row of pixels |

## API endpoint

None — local file read only.

## Notes

- Errors if the file doesn't exist or isn't a readable image format.

## Related

- Feeds [Sprite Sheet Splitter](Sprite-Sheet-Splitter.md) or directly into
  [Palette Quantizer](../02-color/Palette-Quantizer.md) via `Pixel Colors`.
