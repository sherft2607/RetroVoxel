# LED Matrix Mapper

Maps a Pixel Grid JSON to LED positions and serpentine (boustrophedon) wiring order, matching how a
WS2812-style matrix is physically wired.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pixel Grid JSON (PGJ) | Text | Yes | — | Grid from [Pixel Grid Builder](../03-grid/Pixel-Grid-Builder.md) |
| LED Pitch (LP) | Number | No | `10.0` | Distance between adjacent LEDs |

## Outputs

| Name | Type | Description |
|---|---|---|
| LED Address JSON (LAJ) | Text list | `{index, row, col, color}` per LED, in serpentine wiring order |
| LED Position (LPOS) | Point list | World position per LED, matching `LED Address JSON` order |

## API endpoint

None — local geometry/addressing logic only.

## Example response

```json
{ "index": 0, "row": 0, "col": 0, "color": "#FF0000" }
```

## Notes

- Even rows wire left-to-right, odd rows right-to-left — the standard serpentine layout for
  physical LED strips.
- A void cell still gets an LED address, with `"color": null` — off, not skipped, so addressing
  stays contiguous.

## Related

- Shares the `Pixel Grid JSON` input with [Lego Stud Plate](Lego-Stud-Plate.md) and
  [Perler Bead Grid](Perler-Bead-Grid.md).
