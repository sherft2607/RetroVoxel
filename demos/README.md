# RetroVoxel demos — manual build spec

No Grasshopper MCP bridge is wired into this session (no `MCPConnect`-registered server was
available), so Phase 12's live build + auto-fix loop was skipped per the pipeline's graceful
degradation path. Build these 5 canvases by hand in Rhino 8/7 + Grasshopper, test them live, and
report back at Gate 6. Component names/nicknames/params below match the Gate 6 contract revision:
every Ingest component now emits a **Pixel Colors** tree, **Palette Quantizer** consumes/produces
colors directly (not per-image JSON), and **Pixel Grid Builder**'s **Rows** input accepts either a
color/hex tree or comma-separated hex row strings.

---

## 1. Connection

**Goal:** confirm the RA web API key works and the console catalog loads.

**Components:**
- **RA Console** (`CONSOLE`) — 06. Presets

**Wiring:** none — single component.

**Panel setup:**
- `Token` (`T`) → a Panel wired in, text set to `<PASTE_TOKEN>`

**Steps:** paste your real RA web API key over `<PASTE_TOKEN>`, press the **Check Key** button.

**Expected result:** `Status` (`S`) = `OK`; `Console ID` (`CID`) and `Console Name` (`CN`) populate
as parallel lists (Console ID 7 should show `Name = "NES/Famicom"`).

---

## 2. Game badges to voxel

**Goal:** the "Game badge set → voxel plaque batch" workflow — fetch a game's achievements,
download each badge (now returned as a ready-to-use Pixel Colors tree, no separate image-sampling
step needed), voxelize them, and tag each mesh with its achievement metadata.

**Components (wiring order):**
1. **RA Game Info** (`GAME`) — 01. Ingest
2. **RA Badges** (`BADGE`) — 01. Ingest
3. **Pixel Grid Builder** (`GRID`) — 03. Grid
4. **Voxel Mesh Shell** (`VOX`) — 04. Fabrication
5. **Achievement Attribute Bridge** (`BRIDGE`) — 04. Fabrication

**Wiring:**
- `RA Game Info.Achievement JSON` → `RA Badges.Achievement JSON`
- `RA Badges.Pixel Colors` → `Pixel Grid Builder.Rows` — `Pixel Colors` is a nested tree, path
  `{badgeIndex, row}`; use a native GH **Path Mapper** (`{A;B} → {B}`, or graft on `A`) to select one
  badge's rows at a time into `Rows`, since `Pixel Grid Builder` builds one grid per call
- `RA Game Info.Achievement JSON` → `Achievement Attribute Bridge.Achievement JSON`
- `Pixel Grid Builder.Pixel Grid JSON` → `Voxel Mesh Shell.Pixel Grid JSON`
- `Voxel Mesh Shell.Voxel Mesh` → `Achievement Attribute Bridge.Voxel Mesh`

**Panel setup:**
- `RA Game Info.Token` (`T`) → Panel, `<PASTE_TOKEN>`
- `RA Game Info.Game ID` (`GID`) → Panel or slider, `1446` (Super Mario Bros., validated seed)
- `Pixel Grid Builder.Pitch` (`P`) → Panel, `2.0`
- `Pixel Grid Builder.Thickness` (`TH`) → Panel, `1.0`
- `Voxel Mesh Shell.Voxel Size` (`VS`) → Panel, `2.0`

**Steps:** press **Fetch Game**. Once achievements populate, the rest solves automatically.

**Expected result:** `RA Game Info.Status` = `OK`; `RA Badges` downloads badge PNGs to
`%TEMP%\RetroVoxel\badges\` and emits their pixel colors directly (no manual bitmap sampling);
`Achievement Attribute Bridge.Tagged Mesh` carries `Title`/`Points`/`Description`/`BadgeName` in its
Rhino UserDictionary (inspect via a native GH `RhinoScript` panel reading `mesh.UserDictionary`).

---

## 3. Sprite sheet extrusion

**Goal:** the "Local sprite sheet → per-frame voxel extrusion" workflow.

**Components (wiring order):**
1. **Local Sprite Loader** (`LOAD`) — 01. Ingest
2. **Sprite Sheet Splitter** (`SPLIT`) — 01. Ingest
3. **Palette Quantizer** (`QUANT`) — 02. Color
4. **Pixel Grid Builder** (`GRID`) — 03. Grid
5. **Voxel Mesh Shell** (`VOX`) — 04. Fabrication

**Wiring:**
- `Local Sprite Loader.Image Path` → `Sprite Sheet Splitter.Image Path`
- `Sprite Sheet Splitter.Pixel Colors` → `Palette Quantizer.Pixel Colors` (tree shape preserved:
  `{frameIndex, row}`)
- `Palette Quantizer.Quantized Colors` → `Pixel Grid Builder.Rows` (select one frame's branches via
  a Path Mapper, same as demo 2)
- `Pixel Grid Builder.Pixel Grid JSON` → `Voxel Mesh Shell.Pixel Grid JSON`

**Panel setup:**
- `Local Sprite Loader.File Path` → Panel, path to any local sprite sheet PNG
- `Sprite Sheet Splitter.Frame Width` (`FW`) / `Frame Height` (`FH`) → Panels matching the sheet's
  grid (e.g. `16`, `16`)
- `Palette Quantizer.Max Colors` (`MC`) → Panel, `16`

**Expected result:** `Sprite Sheet Splitter` emits `Pixel Colors` nested one level deeper than
`Frame Image Path`/`Frame Index` (adds a row index under each frame); `Palette Quantizer` emits
`Quantized Colors` in the exact same tree shape as its `Pixel Colors` input, plus a flat `Palette`
list and a matching `Color Indices` tree (`-1` for any transparent pixel in the sprite).

---

## 4. Tabular grid mosaic

**Goal:** the cross-platform tabular-to-platform benchmark — a hand-authored color grid becomes
stud/bead/LED placement. This demo also exercises `Pixel Grid Builder`'s **second** Rows form.

**Components (wiring order):**
1. **Pixel Grid Builder** (`GRID`) — 03. Grid
2. **Lego Stud Plate** (`LEGO`) — 05. Mosaic
3. **Perler Bead Grid** (`PERLER`) — 05. Mosaic
4. **LED Matrix Mapper** (`LED`) — 05. Mosaic

**Wiring:**
- `Pixel Grid Builder.Pixel Grid JSON` → each of `Lego Stud Plate.Pixel Grid JSON`,
  `Perler Bead Grid.Pixel Grid JSON`, `LED Matrix Mapper.Pixel Grid JSON`

**Panel setup — pick ONE of these two forms for `Rows` (`RV`):**
- **Form A (color/hex tree):** a native GH Data Tree of Colour params, one branch `{y}` per row,
  multiple Colour items per branch (one per cell)
- **Form B (comma-separated hex strings):** a list of 4 Panels, each containing one row as a single
  comma-separated string, e.g. `"#FF0000, #00FF00, #0000FF, #FFFFFF"` — one panel per row, each
  wired as a separate branch into `Rows`

Also set:
- `Pixel Grid Builder.Pitch` (`P`) → Panel, `2.0`
- `Pixel Grid Builder.Thickness` (`TH`) → Panel, `1.0`
- `Lego Stud Plate.Stud Size` (`SS`) → Panel, `8.0`
- `Perler Bead Grid.Bead Size` (`BS`) → Panel, `5.0`
- `LED Matrix Mapper.LED Pitch` (`LP`) → Panel, `10.0`

**Expected result:** all three Mosaic components consume the identical `Pixel Grid JSON` output
interchangeably (its `palette` is deduplicated, `indices` reference it, `-1` = a void cell if you
leave any row entry blank); `Lego Stud Plate`/`Perler Bead Grid` emit watertight meshes,
`LED Matrix Mapper` emits `LED Address JSON` in serpentine (boustrophedon) order plus matching
`LED Position` points.

---

## 5. Box art standoff

**Goal:** the "Box-art-scaled laser standoff stack" workflow.

**Components (wiring order):**
1. **RA Game Info** (`GAME`) — 01. Ingest
2. **Laser Standoff Stack** (`STACK`) — 04. Fabrication

**Wiring:** none direct — `RA Game Info.Game JSON`'s `ImageBoxArt` dimensions inform the
`Aspect Ratio` input manually for this demo (no automated aspect-ratio extraction component exists
in v1 — compute it from the box art image's pixel dimensions and type the ratio in).

**Panel setup:**
- `RA Game Info.Token` → Panel, `<PASTE_TOKEN>`
- `RA Game Info.Game ID` → Panel, `1446`
- `Laser Standoff Stack.Aspect Ratio` (`AR`) → Panel, e.g. `0.7` (typical box-art portrait ratio)
- `Laser Standoff Stack.Stack Count` (`SC`) → Panel, `4`
- `Laser Standoff Stack.Panel Thickness` (`PT`) → Panel, `3.0`

**Expected result:** `Laser Standoff Stack.Standoff Geometry` (`SG`) emits 4 flat rectangular panel
Breps, evenly spaced along Z, each sized to the given aspect ratio.

---

## Reporting back at Gate 6

For each demo: confirm it solves without error balloons and matches its "Expected result" above, or
report the exact component + error. `RA Console` and `RA Game Info` are `ButtonComponent`s — you
must literally press their button in the live canvas; nothing auto-fires.
