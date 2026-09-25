# Filament Library Loader

Loads a filament library JSON file from disk — your own real filament colors, never a baked-in
list. There is no live filament-color API to source a built-in preset from, so this component
keeps that data where it belongs: a file you maintain yourself.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| File Path (FP) | Text | Yes | `""` | Path to a filament library JSON file |

## Outputs

| Name | Type | Description |
|---|---|---|
| Filament Library JSON (FLJ) | Text | `{"filaments":[{"name":..,"hex":..}]}` |

## API endpoint

None — local file read only.

## File format

```json
{
  "filaments": [
    { "name": "Red PLA", "hex": "#FF0000" },
    { "name": "Blue PLA", "hex": "#0000FF" }
  ]
}
```

## Notes

- Errors if the file doesn't exist or isn't valid JSON.
- Warns (but doesn't error) if the JSON has no `"filaments"` array — downstream
  [Delta-E Filament Matcher](Delta-E-Filament-Matcher.md) falls back to a single generic white
  entry in that case.

## Related

- Feeds [Delta-E Filament Matcher](Delta-E-Filament-Matcher.md).
