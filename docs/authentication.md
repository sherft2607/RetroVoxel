# Authentication

RetroVoxel calls the [RetroAchievements Web API](https://retroachievements.org/API/), which uses a
single web API key passed as a query parameter — never a header.

## Getting a key

1. Sign in at [retroachievements.org](https://retroachievements.org).
2. Open your account settings.
3. Copy your **Web API Key**. Treat it like a password — it is scoped to your account.

## Using it in RetroVoxel

- Every HTTP component (RA Console, RA Game Info) has a `Token` (`T`) input — paste your key there,
  or wire it from one shared Panel so every component uses the same key.
- Components that call the API are `ButtonComponent`s: nothing fires until you press their button
  (e.g. **Check Key**, **Fetch Game**). This avoids accidental repeat calls on every canvas
  recompute.
- A username (`?u=`) parameter also exists on the underlying API for user-scoped endpoints, but no
  v1 RetroVoxel component currently calls one — RetroVoxel's scope is Game + Achievement + Console
  catalog data only.

## Reading the Status output

Every HTTP component reports a `Status` (`S`) output:

| Status | Meaning |
|---|---|
| `OK` | Call succeeded — check `Response` / the parsed outputs |
| `Error: Token is empty.` | No key was supplied |
| `Error: HTTP 401 — token expired or invalid...` | Your key is missing, wrong, or revoked — get a fresh one |
| `Error: HTTP <code> ...` | Any other API error — see the message body for detail |
