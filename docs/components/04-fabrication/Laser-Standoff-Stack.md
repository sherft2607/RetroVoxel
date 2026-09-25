# Laser Standoff Stack

Builds a stack of laser-cut panels sized to a box-art aspect ratio, spaced by standoffs.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Aspect Ratio (AR) | Number | No | `1.0` | Width / height ratio (e.g. from a game's box art) |
| Stack Count (SC) | Integer | No | `4` | Number of stacked panels |
| Panel Thickness (PT) | Number | No | `3.0` | Thickness of each panel |

## Outputs

| Name | Type | Description |
|---|---|---|
| Standoff Geometry (SG) | Brep list | One flat panel Brep per stack level |

## API endpoint

`GET API_GetGame.php` (optional, upstream) — `ImageBoxArt`'s pixel dimensions can inform
`Aspect Ratio`, but no automated extraction exists in v1; compute the ratio and type it in.

## Example response (from RA Game Info's underlying data)

```json
{
  "Title": "Sonic the Hedgehog",
  "ConsoleName": "Mega Drive",
  "ImageBoxArt": "/Images/051872.png"
}
```

## Notes

- Panels are 100 units wide by default, scaled to `height = 100 / AspectRatio`, spaced 10 units
  apart plus `Panel Thickness` along Z.

## Related

- Aspect ratio typically sourced from [RA Game Info](../01-ingest/RA-Game-Info.md).
