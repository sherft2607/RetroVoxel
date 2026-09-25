#r "Grasshopper"
#r "RhinoCommon"

// RetroVoxel — demo canvas generator
//
// Run this in Rhino 8's Script Editor (C# mode) with RetroVoxel already loaded in Grasshopper
// (GrasshopperDeveloperSettings pointed at bin/Debug/net7.0-windows, Rhino restarted, Grasshopper
// opened at least once). Paste this whole file into a new C# script and run it — it builds both
// demo canvases programmatically and saves them into this demos/ folder.
//
// NOTE: this is a hand-written substitute for the automated Grasshopper-MCP-bridge build (Job A of
// pr-rhino-test) since no bridge was available in this session. Component pivots are spaced by eye
// for readability, not laid out by any layout engine — nudge them in the GH canvas afterward if a
// wire looks cramped. Token panels are left as the literal placeholder text "<PASTE_TOKEN>" per the
// pipeline's rule never to save a real token into a committed .gh file.

using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino;

public static class RetroVoxelDemoBuilder
{
    // Component GUIDs — from spec/plugin-spec.json, must match ComponentGuid in each .cs file
    static readonly Guid RA_GAME_INFO   = new Guid("6cbd3079-4923-450c-aafa-1c0266250142");
    static readonly Guid RA_BADGES      = new Guid("3c4c229b-58d9-4e3b-98f6-d0e654999e02");
    static readonly Guid PIXEL_GRID     = new Guid("42b5226c-2c77-46f6-bc69-8b08d1c7e674");
    static readonly Guid VOXEL_SHELL    = new Guid("ed89abbc-b3fa-4d6d-93da-aa31b529eb5b");
    static readonly Guid ACHV_BRIDGE    = new Guid("6f38dc89-01ee-41b3-908b-d8d66e6b7972");
    static readonly Guid LEGO_STUD      = new Guid("524265b8-1249-4554-a262-d469abc82a06");
    static readonly Guid PERLER_BEAD    = new Guid("9a91acb0-5b46-419a-9e83-f7d8f3d73ce4");
    static readonly Guid LED_MATRIX     = new Guid("ec7a7b0f-096e-44f4-8832-86edb8c937eb");

    static IGH_Component NewComponent(Guid guid)
    {
        var obj = Instances.ComponentServer.EmitObject(guid);
        if (obj == null)
            throw new InvalidOperationException("Component not found in the live registry: " + guid +
                " — is RetroVoxel.gha loaded in this Grasshopper session?");
        return (IGH_Component)obj;
    }

    static void Place(GH_Document doc, IGH_DocumentObject obj, float x, float y)
    {
        doc.AddObject(obj, false);
        obj.Attributes.Pivot = new PointF(x, y);
        obj.Attributes.ExpireLayout();
    }

    // GH_Panel and GH_NumberSlider ARE IGH_Param objects themselves (a single-output special
    // object) — they do NOT expose a .Params.Output collection the way a multi-output IGH_Component
    // does. Wire directly with the object itself as the source, never ".Params.Output[0]".
    static GH_Panel NewPanel(GH_Document doc, string text, float x, float y)
    {
        var panel = new GH_Panel();
        panel.UserText = text;
        Place(doc, panel, x, y);
        return panel;
    }

    static GH_NumberSlider NewSlider(GH_Document doc, double min, double max, double value, float x, float y)
    {
        var slider = new GH_NumberSlider();
        slider.Slider.Minimum = (decimal)min;
        slider.Slider.Maximum = (decimal)max;
        slider.SetSliderValue((decimal)value);
        Place(doc, slider, x, y);
        return slider;
    }

    static void Wire(IGH_Param source, IGH_Param target)
    {
        target.AddSource(source);
    }

    static void Save(GH_Document doc, string path)
    {
        var io = new GH_DocumentIO(doc);
        if (!io.SaveQuiet(path))
            throw new InvalidOperationException("Failed to save " + path);
        RhinoApp.WriteLine("Saved: " + path);
    }

    // ---- Demo 1: Game badges to voxel ----
    public static void BuildDemo1(string outputPath)
    {
        var doc = new GH_Document();

        var tokenPanel = NewPanel(doc, "<PASTE_TOKEN>", 0, 0);
        var gameIdSlider = NewSlider(doc, 1, 20000, 1446, 0, 60);

        var gameInfo = NewComponent(RA_GAME_INFO);
        Place(doc, gameInfo, 200, 0);
        Wire(tokenPanel, gameInfo.Params.Input[0]);    // Token
        Wire(gameIdSlider, gameInfo.Params.Input[1]);  // Game ID

        var badges = NewComponent(RA_BADGES);
        Place(doc, badges, 450, 0);
        // gameInfo outputs: 0=Status, 1=Game JSON, 2=Achievement JSON
        Wire(gameInfo.Params.Output[2], badges.Params.Input[0]); // Achievement JSON

        // Pixel Grid Builder consumes one badge's rows at a time — wired directly here to the raw
        // tree; in the live canvas, add a Path Mapper ({A;B} -> {B}) between Badges and Grid if you
        // need to select a single badge's branch (see demos/README.md).
        var pitchPanel = NewPanel(doc, "2.0", 700, -60);
        var thicknessPanel = NewPanel(doc, "1.0", 700, 0);

        var grid = NewComponent(PIXEL_GRID);
        Place(doc, grid, 850, 0);
        // badges outputs: 0=Badge Image Path, 1=Badge Resource ID, 2=Achievement Title, 3=Pixel Colors, 4=Status
        Wire(badges.Params.Output[3], grid.Params.Input[0]);      // Pixel Colors -> Rows
        Wire(pitchPanel, grid.Params.Input[1]);                   // Pitch
        Wire(thicknessPanel, grid.Params.Input[2]);               // Thickness

        var voxelSizePanel = NewPanel(doc, "2.0", 1100, 60);

        var voxel = NewComponent(VOXEL_SHELL);
        Place(doc, voxel, 1150, 0);
        Wire(grid.Params.Output[0], voxel.Params.Input[0]);   // Pixel Grid JSON
        Wire(voxelSizePanel, voxel.Params.Input[1]);          // Voxel Size

        var bridge = NewComponent(ACHV_BRIDGE);
        Place(doc, bridge, 1400, 0);
        Wire(voxel.Params.Output[0], bridge.Params.Input[0]);     // Voxel Mesh
        Wire(gameInfo.Params.Output[2], bridge.Params.Input[1]);  // Achievement JSON (same source as Badges)

        doc.NewSolution(false);
        Save(doc, outputPath);
    }

    // ---- Demo 2: Tabular grid to fabrication & mosaic ----
    public static void BuildDemo2(string outputPath)
    {
        var doc = new GH_Document();

        var row0 = NewPanel(doc, "#FF0000, #00FF00, #0000FF", 0, -90);
        var row1 = NewPanel(doc, "#0000FF, #FFFFFF, #FF0000", 0, -30);
        var row2 = NewPanel(doc, "#00FF00, #FF0000, #FFFFFF", 0, 30);
        var row3 = NewPanel(doc, "#FFFFFF, #0000FF, #00FF00", 0, 90);

        var pitchPanel = NewPanel(doc, "2.0", 0, 150);
        var thicknessPanel = NewPanel(doc, "1.0", 0, 200);

        var grid = NewComponent(PIXEL_GRID);
        Place(doc, grid, 250, 0);
        // Rows is a generic tree input — each panel wired as a separate source becomes its own
        // branch (Grasshopper grafts sequential sources onto sequential branches automatically).
        Wire(row0, grid.Params.Input[0]);
        Wire(row1, grid.Params.Input[0]);
        Wire(row2, grid.Params.Input[0]);
        Wire(row3, grid.Params.Input[0]);
        Wire(pitchPanel, grid.Params.Input[1]);
        Wire(thicknessPanel, grid.Params.Input[2]);

        var studSizePanel = NewPanel(doc, "8.0", 500, -150);
        var lego = NewComponent(LEGO_STUD);
        Place(doc, lego, 550, -100);
        Wire(grid.Params.Output[0], lego.Params.Input[0]);
        Wire(studSizePanel, lego.Params.Input[1]);

        var beadSizePanel = NewPanel(doc, "5.0", 500, 0);
        var perler = NewComponent(PERLER_BEAD);
        Place(doc, perler, 550, 50);
        Wire(grid.Params.Output[0], perler.Params.Input[0]);
        Wire(beadSizePanel, perler.Params.Input[1]);

        var pitchLedPanel = NewPanel(doc, "10.0", 500, 150);
        var led = NewComponent(LED_MATRIX);
        Place(doc, led, 550, 200);
        Wire(grid.Params.Output[0], led.Params.Input[0]);
        Wire(pitchLedPanel, led.Params.Input[1]);

        doc.NewSolution(false);
        Save(doc, outputPath);
    }
}

// ---- Entry point ----
string demosDir = @"C:\Users\sherft\Downloads\RetroVoxel\RetroVoxel\demos";
RetroVoxelDemoBuilder.BuildDemo1(System.IO.Path.Combine(demosDir, "Demo1_BadgesToVoxel.gh"));
RetroVoxelDemoBuilder.BuildDemo2(System.IO.Path.Combine(demosDir, "Demo2_GridFabrication.gh"));
RhinoApp.WriteLine("Done. Open both files in Grasshopper, verify they solve, then report back.");
