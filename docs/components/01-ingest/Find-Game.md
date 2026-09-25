# Find Game

Resolves a game name (or substring) to its numeric RetroAchievements Game ID by searching a
console's full game list. RA has no direct "search by title" endpoint — this is the sibling-API
name-lookup pattern, matching a name against `GetGameList` for a given console.
`ButtonComponent` — fires only when its button is pressed.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token (T) | Text | Yes | `""` | RetroAchievements web API key |
| Console ID (CID) | Integer | Yes | `0` | Console ID to search within (e.g. from [RA Console](../06-presets/RA-Console.md)) |
| Name (N) | Text | Yes | `""` | Game title or substring to search for (case-insensitive) |

## Outputs

| Name | Type | Description |
|---|---|---|
| Status (S) | Text | `OK`, or the error from the last search |
| Game ID (GID) | Integer list | Matching Game IDs |
| Game Title (GT) | Text list | Matching game titles, same order as Game ID |

## API endpoint

`GET API_GetGameList.php` — validated at Phase 7-style extension against Console ID 7 (NES).

## Notes

- `GetGameList` returns every game for the console; matching against `Name` happens client-side
  (case-insensitive substring), since RA has no server-side title search.
- Zero matches produces a warning, not an error — `Game ID`/`Game Title` come back empty.
- `RA Game Info`'s `Game ID` input stays numeric-only; wire this component's `Game ID` output into
  it once you've found the ID you want.

## Related

- Feeds [RA Game Info](RA-Game-Info.md).
- Console ID typically comes from [RA Console](../06-presets/RA-Console.md).
