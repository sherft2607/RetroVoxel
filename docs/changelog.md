# Changelog

## 1.0.0 — 2026-09-25

Initial release. [GitHub Release](https://github.com/sherft2607/RetroVoxel/releases/tag/v1.0.0)

- 18 components across 6 tabs: Ingest, Color, Grid, Fabrication, Mosaic, Presets.
- RetroAchievements Web API integration (read-only): console catalog, game metadata, achievement
  badges, game-level artwork (Icon/Title/Ingame/Box Art), and name-based game lookup (Find Game).
- Local sprite sheet ingestion and per-frame splitting.
- Palette quantization, Delta-E filament matching, and a Filament Library Loader (your own real
  filament colors from a file on disk — never a baked-in preset).
- Shared Pixel Grid JSON contract feeding voxel, Lego, Perler bead, and LED matrix output geometry.
- BIM Data Bridge: achievement metadata tagged directly onto fabricated meshes, both as geometry
  UserDictionary and as bake-time Rhino UserText.
- Platform verification: Rhino 7 Windows and Rhino 8 Windows live-tested; Rhino 8 Mac is
  compile-verified only — seeking community verification.
