<!-- pr:begin header -->
<img src="RVX_Plugin_Icon.png" alt="RetroVoxel" width="96" align="left" style="margin-right:16px">

# RetroVoxel
> Grasshopper plugin for the RetroAchievements Web API — by Shandon Herft
>
> Built with the [PleaseREST](https://github.com/shandonherft/PleaseREST) Claude Code plugin — an AI pipeline that turns a REST API reference into a Grasshopper plugin.

Computational design and physical fabrication toolkit that turns retro gaming pixel art, sprite sheets, and RetroAchievements game badges into voxel meshes, mosaic layouts, and laser-cut fabrication geometry.

<br clear="left">

## What you can do with it

- **Voxelize a game's full achievement badge set in one batch** — fetch every badge for a game and turn it into individually-sized, self-documenting voxel plaques, with achievement metadata tagged directly onto each mesh.
- **Extrude a sprite sheet frame by frame** — split a local sprite sheet and vary voxel height per frame parametrically, instead of tuning each frame by hand in an image editor.
- **Match a quantized palette to real filament colors** — sweep palette size and filament library across a whole tree of sprites at once, previewing the closest physical match before committing to a print.
- **Turn any tabular color grid into mosaic geometry** — a hand-authored grid, a spreadsheet import, or sprite-derived pixel data all become Lego stud plates, Perler bead layouts, or LED matrix wiring, interchangeably from the same shared grid.
- **Size a laser-cut standoff stack to real box art** — scale panel dimensions to a game's actual box-art aspect ratio instead of eyeballing it.
- **Voxelize a game's own artwork, not just its achievements** — download and fabricate a game's icon, title screen, in-game screenshot, or box art directly, the same way an achievement badge can be voxelized.

Component chains for each of these are in [docs/workflows.md](docs/workflows.md); worked examples are in [docs/examples/](docs/examples/).

**Under the hood**

- The RetroAchievements API is read-only in this plugin (no write/create/update/delete) — every HTTP component is a `ButtonComponent` that fires only on a literal button press, never on canvas recompute.
- Every Fabrication/Mosaic component consumes the same `Pixel Grid JSON` contract from Pixel Grid Builder, so they're interchangeable outputs from one shared grid — no per-component payload shape to relearn.
<!-- pr:end header -->

<!-- pr:begin install -->
## Installation

**Food4Rhino:** `(pending listing)`
**Yak:** `_PackageManager` → search `retrovoxel`

**Rhino 8 note:** the Rhino 8 packages are built for .NET 7 and load only when Rhino runs on the .NET Core runtime (its default). If the RetroVoxel tab is missing, run `SetDotNetRuntime` in Rhino, choose **.NET Core**, and restart.

**From source:** `dotnet build RetroVoxel.csproj`, then drop the `.gha` for your Rhino version into Grasshopper's `Libraries` folder (Rhino 7: `bin/Debug/net48`; Rhino 8 Windows: `bin/Debug/net7.0-windows`; Rhino 8 Mac: `bin/Debug/net7.0`) and restart Rhino — or point Grasshopper's developer-folder setting at the build folder. Close Rhino before rebuilding; it locks the `.gha`.
<!-- pr:end install -->

<!-- pr:begin quickstart -->
## Quick start

1. Get your RetroAchievements web API key from your account settings — see [docs/authentication.md](docs/authentication.md).
2. Drop **RA Console** onto the canvas and plug your key into `Token` — `Status` confirms the connection.
3. Drop **RA Game Info**, set `Game ID` to `1446` (Super Mario Bros.), press **Fetch Game**.
4. Wire `Achievement JSON` into **RA Badges** to download achievement badges and their pixel colors.
5. Feed pixel colors into **Pixel Grid Builder**, then into **Voxel Mesh Shell** or any Mosaic component.
6. Wire the resulting mesh and `Achievement JSON` into **Achievement Attribute Bridge** to tag each plaque.

Full walkthrough: [docs/quickstart.md](docs/quickstart.md). Common wirings as component chains: [docs/workflows.md](docs/workflows.md). Reference for every component: [docs/index.md](docs/index.md).
<!-- pr:end quickstart -->

<!-- pr:begin components -->
## Components

| Tab | Component | Inputs | Outputs | Description |
|---|---|---|---|---|
| 01. Ingest | Local Sprite Loader | FP | IP, W, H, PC | Loads a single local image and reports its size and pixel colors |
| 01. Ingest | Sprite Sheet Splitter | IP, FW, FH | FIP, FI, PC | Splits a sprite sheet into per-frame images and pixel colors |
| 01. Ingest | RA Game Info | T, GID | S, GJ, AJ | Fetches a game's metadata and achievement list |
| 01. Ingest | RA Badges | AJ | BIP, BRID, AT, PC, S | Downloads each achievement's badge image, readable title, and pixel colors |
| 01. Ingest | Find Game | T, CID, N | S, GID, GT | Resolves a game name/substring to its numeric Game ID |
| 01. Ingest | RA Game Images | GJ, IT, MD | IP, PC, S | Downloads a game's Icon/Title/Ingame/Box Art image (downsampled) and pixel colors |
| 02. Color | Palette Quantizer | PC, MC | QC, PAL, CI | Quantizes pixel colors down to a target color count |
| 02. Color | Delta-E Filament Matcher | PAL, FLJ | FMJ | Matches each palette color to the nearest filament color |
| 02. Color | Filament Library Loader | FP | FLJ | Loads your own filament library JSON file from disk |
| 03. Grid | Pixel Grid Builder | RV, P, TH | PGJ | Builds the shared Pixel Grid JSON from rows of pixel data |
| 04. Fabrication | Voxel Mesh Shell | PGJ, VS, EH | VM | Extrudes a Pixel Grid into a merged voxel shell mesh |
| 04. Fabrication | Multi-Material Interlock | VM, FMJ | IM | Tags a voxel mesh with its matched filament |
| 04. Fabrication | Laser Standoff Stack | AR, SC, PT | SG | Builds an aspect-ratio-scaled laser-cut standoff stack |
| 04. Fabrication | Achievement Attribute Bridge | VM, AJ | TM | Writes achievement metadata into a voxel mesh's UserDictionary |
| 05. Mosaic | Lego Stud Plate | PGJ, SS | SPM | Builds a watertight Lego-compatible stud plate mesh |
| 05. Mosaic | Perler Bead Grid | PGJ, BS | BGM | Builds a pegboard-style bead layout mesh |
| 05. Mosaic | LED Matrix Mapper | PGJ, LP | LAJ, LPOS | Maps a grid to LED positions and serpentine wiring order |
| 06. Presets | RA Console | T | S, R, CID, CN | Validates the API key and lists every console |
<!-- pr:end components -->

<!-- pr:begin requirements -->
## Requirements

- Rhino 7 or 8 (Windows or Mac)
- A RetroAchievements account
- Web API key (from your RetroAchievements account settings) — no OAuth scopes; the key is a single account-level credential

## Documentation

- [docs/index.md](docs/index.md) — overview and every component by tab
- [docs/quickstart.md](docs/quickstart.md) — first fetch in 6 steps
- [docs/workflows.md](docs/workflows.md) — common wirings as component chains
- [docs/authentication.md](docs/authentication.md) — getting and using your API key
- [docs/examples/](docs/examples/) — worked examples
- [docs/changelog.md](docs/changelog.md)

## Development

```
dotnet build RetroVoxel.csproj                          # net48, net7.0-windows, net7.0
dotnet test  RetroVoxel.Tests/RetroVoxel.Tests.csproj   # builder logic, no Rhino needed
```

This plugin was generated with the **PleaseREST** Claude Code plugin (`please-rest` v1.0.0); `CLAUDE.md` holds the build state and design notes.
<!-- pr:end requirements -->

<!-- pr:begin license -->
## License

RetroVoxel is released under the MIT License — Copyright (c) 2026 Shandon Herft. See [LICENSE](LICENSE).
<!-- pr:end license -->
