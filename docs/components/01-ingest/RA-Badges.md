# RA Badges

Resolves each achievement's `BadgeName` into a downloaded badge image and its pixel colors.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Achievement JSON (AJ) | Text tree | Yes | — | One achievement record per branch (from [RA Game Info](RA-Game-Info.md)) |

## Outputs

| Name | Type | Description |
|---|---|---|
| Badge Image Path (BIP) | Text tree | Downloaded badge image path, one per branch |
| Badge Resource ID (BRID) | Text tree | The numeric badge image ID used to build the CDN URL — not a readable name |
| Achievement Title (AT) | Text tree | The achievement's readable title, same branches as Badge Resource ID |
| Pixel Colors (PC) | Colour tree | Rows of pixels per badge, path `{badgeIndex, row}` |
| Status (S) | Text | `OK`, or a summary of any download errors |

## API endpoint

No separate documented endpoint — resolves each `BadgeName` into
`https://i.retroachievements.org/Badge/<BadgeName>.png` and downloads the image directly (no API
key required for the media CDN).

## Notes

- Downloaded badges are cached to `%TEMP%\RetroVoxel\badges\`.
- If a badge fails to download, that branch is skipped and the error is counted in `Status`; other
  badges still complete.

## Related

- `Pixel Colors` feeds [Palette Quantizer](../02-color/Palette-Quantizer.md) or
  [Pixel Grid Builder](../03-grid/Pixel-Grid-Builder.md) directly (select one badge's rows with a
  Path Mapper).
