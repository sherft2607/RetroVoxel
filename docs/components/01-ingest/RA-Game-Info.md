# RA Game Info

Fetches a game's metadata and full achievement list (with badge names) from RetroAchievements.
`ButtonComponent` — fires only when its button is pressed.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token (T) | Text | Yes | `""` | RetroAchievements web API key |
| Game ID (GID) | Integer | Yes | `0` | RetroAchievements game ID |

## Outputs

| Name | Type | Description |
|---|---|---|
| Status (S) | Text | `OK`, or the error from the last fetch |
| Game JSON (GJ) | Text | Game metadata (title, console, image paths) |
| Achievement JSON (AJ) | Text tree | One achievement record per branch |

## API endpoint

`GET API_GetGameExtended.php` — validated against Console ID 7 (NES) / Game ID 1446 (Super Mario
Bros.) at Phase 7.

## Notes

- Each achievement record includes `Title`, `Description`, `Points`, and `BadgeName` — `BadgeName`
  feeds [RA Badges](RA-Badges.md).
- Press the button to fire the call; a non-triggered recompute re-emits the last cached result.

## Related

- [RA Badges](RA-Badges.md) consumes `Achievement JSON`.
- [Achievement Attribute Bridge](../04-fabrication/Achievement-Attribute-Bridge.md) also consumes
  `Achievement JSON` to tag fabricated meshes.
- [Find Game](Find-Game.md) resolves a game name to a `Game ID` for this component, if you don't
  already know the numeric ID.
- [RA Game Images](RA-Game-Images.md) consumes `Game JSON` to download the game's own artwork.
