# RetroVoxel — Build Todo

> Derived from spec/plugin-spec.json. RA API is read-only (GET, ?y= key, optional ?u= username) —
> no push/write verify steps; "Verify" below means "returns correct data/geometry", not
> "pushed to platform". Phases 1-7 = pipeline Phases 9-12. Phases 8-10 (`[SHIP]`) = pipeline 13-15,
> run only after Gate 6.

```
PHASE 1 — AUTH / CONNECTIVITY
[ ] RetroVoxelClient.cs            [CODE] base HTTP setup, NewRequest() helper, ?y= (+ optional ?u=) query auth
[ ] RetroVoxelInfo.cs               [CODE] GH_AssemblyInfo + GH_AssemblyPriority
[ ] ButtonComponent.cs             [CODE] base class — copy verbatim, namespace only
[ ] PluginUtilities.cs             [CODE] TabName + zero-padded subcategory constants (01.-07.)
[ ] RAConsolePreset.cs             [CODE] Token input -> GetConsoleIDs call; Console ID + Status outputs (doubles as key check, Gate 2)
[ ] RA Console icon                [ASSET] generate_icon -> add_to_resources
[ ] Verify: RA Console returns a valid console list and Status = OK with a real API key

PHASE 2 — SIMPLE UNITS (Ingest + Color, no JSON body — all GET or local)
[ ] LocalSpriteLoader.cs           [CODE] File Path -> Image Path, Width, Height
[ ] Local Sprite Loader icon       [ASSET] generate_icon -> add_to_resources
[ ] SpriteSheetSplitter.cs         [CODE] Image Path + Frame W/H -> tree of Frame Image Path / Frame Index
[ ] Sprite Sheet Splitter icon     [ASSET] generate_icon -> add_to_resources
[ ] RAGameInfo.cs                  [CODE] Token + Game ID -> GetGameExtended call -> Game JSON, Achievement JSON (tree), Status, Response
[ ] RA Game Info icon              [ASSET] generate_icon -> add_to_resources
[ ] RABadges.cs                    [CODE] Achievement JSON (tree) -> resolve BadgeName to i.retroachievements.org/Badge/ image, fetch bitmap -> Badge Image Path, Badge Name (tree)
[ ] RA Badges icon                 [ASSET] generate_icon -> add_to_resources
[ ] PaletteQuantizer.cs            [CODE] Image Path (tree) + Color Count -> Palette JSON, Quantized Image Path (tree)
[ ] Palette Quantizer icon         [ASSET] generate_icon -> add_to_resources
[ ] FilamentBuilders.cs            [CODE] BuildFilamentMatch() — Delta-E nearest-match lookup
[ ] DeltaEFilamentMatcher.cs       [CODE] Palette JSON (tree) + Filament Library JSON -> Filament Match JSON (tree)
[ ] Delta-E Filament Matcher icon  [ASSET] generate_icon -> add_to_resources
[x] FilamentLibraryLoader.cs       [CODE] (post-Gate-6 addition) File Path -> Filament Library JSON, loaded from disk — added so the plugin never hardcodes/bakes-in filament color data
[x] Filament Library Loader icon   [ASSET] generate_icon -> add_to_resources
[ ] Verify: RA Game Info returns real achievement data for a seed game ID; Local Sprite Loader + Splitter round-trip a sample sprite sheet; Palette Quantizer/Filament Matcher produce valid JSON for a sample image

PHASE 3 — AGGREGATOR (tabular-to-platform benchmark)
[ ] PixelGridBuilders.cs           [CODE] BuildPixelGrid() — tree of row color values -> Pixel Grid JSON
[ ] PixelGridBuilder.cs            [CODE] Row Values (tree) -> Pixel Grid JSON (the shared contract feeding all Fabrication/Mosaic components)
[ ] Pixel Grid Builder icon        [ASSET] generate_icon -> add_to_resources
[ ] Verify: a hand-authored tabular RGB grid produces valid Pixel Grid JSON consumed identically by Voxel Mesh Shell, Lego Stud Plate, Perler Bead Grid, and LED Matrix Mapper

PHASE 4 — LEVEL 2 AGGREGATOR
SKIPPED — RA API has no higher-order structure beyond Game -> Achievement; Pixel Grid Builder (Phase 3) is the only aggregator this plugin needs.

PHASE 5 — CUSTOM / AEC FEATURES
[ ] AchievementAttributeBridge.cs  [CODE] Voxel Mesh (tree) + Achievement JSON (tree) -> writes Title/Points/Description/BadgeName to mesh UserDictionary -> Tagged Mesh (tree)
[ ] Achievement Attribute Bridge icon [ASSET] generate_icon -> add_to_resources
[ ] Verify: a fabricated plaque batch's meshes carry correct per-achievement metadata in Rhino's UserDictionary after leaving the GH tree

PHASE 6 — FABRICATION + MOSAIC + PRESETS
[ ] VoxelMeshShell.cs              [CODE] Pixel Grid JSON + Voxel Size (+ optional per-cell Extrusion Height tree) -> Voxel Mesh (tree)
[ ] Voxel Mesh Shell icon          [ASSET] generate_icon -> add_to_resources
[ ] MultiMaterialInterlock.cs      [CODE] Voxel Mesh (tree) + Filament Match JSON (tree) -> Interlock Mesh (tree)
[ ] Multi-Material Interlock icon  [ASSET] generate_icon -> add_to_resources
[ ] LaserStandoffStack.cs          [CODE] Aspect Ratio + Stack Count + Panel Thickness -> Standoff Geometry (list of Brep)
[ ] Laser Standoff Stack icon      [ASSET] generate_icon -> add_to_resources
[ ] LegoStudPlate.cs               [CODE] Pixel Grid JSON + Stud Size -> Stud Plate Mesh (tree, watertight studs)
[ ] Lego Stud Plate icon           [ASSET] generate_icon -> add_to_resources
[ ] PerlerBeadGrid.cs              [CODE] Pixel Grid JSON + Bead Size -> Bead Grid Mesh (tree, pegboard layout + part counts)
[ ] Perler Bead Grid icon          [ASSET] generate_icon -> add_to_resources
[ ] LEDMatrixMapper.cs             [CODE] Pixel Grid JSON + LED Pitch -> LED Address JSON (tree, serpentine wiring order), LED Position (tree)
[ ] LED Matrix Mapper icon         [ASSET] generate_icon -> add_to_resources
[ ] Verify: each Fabrication/Mosaic component consumes the same Pixel Grid JSON interchangeably and produces valid geometry for a sample grid; presets compose cleanly with RA Game Info / RA Badges

PHASE 7 — TESTS
[ ] RetroVoxel.Tests/BuilderTests.cs        [TEST] PixelGridBuilders + FilamentBuilders — one test per builder method, expected JSON shape (pure logic, net8.0)
[ ] RetroVoxel.Tests/BulkAssemblyTests.cs   [TEST] Pixel Grid Builder aggregation — row-tree values -> assembled grid JSON
[ ] dotnet build                   [CODE] all green (net48, net7.0-windows, net7.0)
[ ] dotnet test                    [CODE] all green
[ ] demos/connection.gh             [MCP] RA Console (key check) demo
[ ] demos/game-badges-to-voxel.gh   [MCP] RA Game Info -> RA Badges -> Achievement Attribute Bridge -> Voxel Mesh Shell (badge plaque batch workflow)
[ ] demos/sprite-sheet-extrusion.gh [MCP] Local Sprite Loader -> Sprite Sheet Splitter -> Palette Quantizer -> Voxel Mesh Shell
[ ] demos/tabular-grid-mosaic.gh    [MCP] Pixel Grid Builder -> Lego Stud Plate / Perler Bead Grid / LED Matrix Mapper (tabular benchmark)
[ ] demos/box-art-standoff.gh       [MCP] RA Game Info -> Laser Standoff Stack (aspect-ratio-scaled)
[ ] Live MCP smoke test             [MCP] load .gha, solve each demo, auto-fix on failure, flag human only if no fix
[ ] Verify: all demos pass and demos/ is populated (or degrade gracefully to manual per pr-rhino-test)

PHASE 8 — DOCS                     (pipeline Phase 13 — after Gate 6)
[ ] docs/index.md                  [SHIP] plugin overview + install
[ ] docs/quickstart.md             [SHIP]
[ ] docs/authentication.md         [SHIP] RA API key (?y=) + optional username (?u=) setup
[ ] docs/components/01-ingest/*.md [SHIP] Local Sprite Loader, Sprite Sheet Splitter, RA Game Info, RA Badges
[ ] docs/components/02-color/*.md  [SHIP] Palette Quantizer, Delta-E Filament Matcher
[ ] docs/components/03-grid/*.md   [SHIP] Pixel Grid Builder
[ ] docs/components/04-fabrication/*.md [SHIP] Voxel Mesh Shell, Multi-Material Interlock, Laser Standoff Stack, Achievement Attribute Bridge
[ ] docs/components/05-mosaic/*.md [SHIP] Lego Stud Plate, Perler Bead Grid, LED Matrix Mapper
[ ] docs/components/06-presets/*.md [SHIP] RA Console
[ ] docs/examples/*.md             [SHIP] one per demo workflow (5 demos)
[ ] docs/changelog.md              [SHIP] starts at 1.0.0

PHASE 9 — GITHUB                   (pipeline Phase 14)
[ ] README.md                      [SHIP] may link to docs/
[ ] CLAUDE.md                      [SHIP]
[ ] LICENSE                        [SHIP]
[ ] .gitignore                     [SHIP] (already present — verify dogfood/, .claude/, settings.local.json ignored)

PHASE 10 — PACKAGE                 (pipeline Phase 15)
[ ] dotnet build -c Release x 3    [SHIP] net48 / net7.0-windows / net7.0
[ ] yak build x 3                  [SHIP]
[ ] .github/workflows/publish.yml  [SHIP]
[ ] Verify: .yak files in dist/

[HUMAN GATE 6] Rhino manual test — demos pre-built & pre-checked; open demos/ and confirm live in GH
[HUMAN GATE 7] Cross-platform test — install .yak on Rh7 Win / Rh8 Win / Rh8 Mac
```
