# Delta-E Filament Matcher

Matches each palette color to the nearest filament color by CIE76 Delta-E distance.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Palette (PAL) | Colour list | Yes | — | Palette from [Palette Quantizer](Palette-Quantizer.md) |
| Filament Library JSON (FLJ) | Text | No | `""` | `{"filaments":[{"name":..,"hex":..}]}` — from [Filament Library Loader](Filament-Library-Loader.md) |

## Outputs

| Name | Type | Description |
|---|---|---|
| Filament Match JSON (FMJ) | Text list | Nearest filament match, one per palette color, same order as `Palette` |

## API endpoint

None — local color-science logic only.

## Example response

```json
{
  "sourceColor": "#FF0000",
  "filamentName": "Red PLA",
  "filamentColor": "#FF0000",
  "deltaE": 0.0
}
```

## Notes

- If `Filament Library JSON` is empty or invalid, falls back to a single "Generic White" `#FFFFFF`
  entry so the component never errors on a missing library.
- Uses CIE76 Delta-E (Euclidean distance in Lab space) — sufficient for filament-swatch matching at
  this scale; not CIEDE2000.

## Related

- Consumes [Filament Library Loader](Filament-Library-Loader.md)'s output.
- Feeds [Multi-Material Interlock](../04-fabrication/Multi-Material-Interlock.md).
