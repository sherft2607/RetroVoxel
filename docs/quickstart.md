# Quickstart

1. **Get a RetroAchievements web API key** — see [authentication.md](authentication.md).
2. **Drop RA Console** (06. Presets tab) on the canvas, paste your key into `Token`, press
   **Check Key**.
3. **Confirm connectivity** — `Status` outputs `OK` and `Console ID` / `Console Name` populate as
   parallel lists.
4. **Try a first fetch** — drop **RA Game Info** (01. Ingest), wire the same `Token`, set `Game ID`
   to `1446` (Super Mario Bros.), press **Fetch Game**. `Status` reports `OK` and `Game JSON` /
   `Achievement JSON` populate.
5. From there, wire into **RA Badges** to download achievement badges, or start from
   **Local Sprite Loader** for your own artwork — see [workflows.md](workflows.md) for the full
   set of paths through the plugin.
