# Palette Quantizer

Quantizes a Pixel Colors tree down to a target color count, preserving the input tree's exact
shape on every output.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pixel Colors (PC) | Colour tree | Yes | — | Pixel colors, one branch per row (from an Ingest component) |
| Max Colors (MC) | Integer | Yes | `16` | Target number of colors in the quantized palette |

## Outputs

| Name | Type | Description |
|---|---|---|
| Quantized Colors (QC) | Colour tree | Each pixel snapped to its nearest palette color; same tree shape as input |
| Palette (PAL) | Colour list | The distinct colors used by the quantized output |
| Color Indices (CI) | Integer tree | Palette index per pixel; `-1` for a transparent/void pixel; same tree shape as input |

## API endpoint

None — local color-science logic only.

## Notes

- A pixel with alpha `0` is treated as transparent/void: excluded from the palette and given index
  `-1` in `Color Indices`, rather than being quantized to a visible color.
- Uses simple nearest-Euclidean-distance matching against the top-N most frequent input colors —
  not a full median-cut or k-means quantizer.

## Related

- `Palette` feeds [Delta-E Filament Matcher](Delta-E-Filament-Matcher.md).
