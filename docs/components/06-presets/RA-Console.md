# RA Console

Validates the RetroAchievements web API key and lists every console ID and name. Doubles as the
plugin's connectivity check — there is no separate Auth component (merged at Gate 2/3).
`ButtonComponent` — fires only when its button is pressed.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token (T) | Text | Yes | `""` | RetroAchievements web API key |

## Outputs

| Name | Type | Description |
|---|---|---|
| Status (S) | Text | `OK`, or the error from the last check |
| Response (R) | Text | Raw `GetConsoleIDs` response JSON |
| Console ID (CID) | Integer list | Console ID for every system on the site |
| Console Name (CN) | Text list | Console name matching each Console ID by index |

## API endpoint

`GET API_GetConsoleIDs.php` — validated at Phase 7.

## Example response

```json
[
  { "ID": 7, "Name": "NES/Famicom", "IconURL": "https://static.retroachievements.org/assets/images/system/nes.png", "Active": true, "IsGameSystem": true }
]
```

## Notes

- Press **Check Key** to fire the call; this is the recommended first component on any RetroVoxel
  canvas to confirm your key works before wiring anything else.

## Related

- See [quickstart.md](../../quickstart.md) and [authentication.md](../../authentication.md).
