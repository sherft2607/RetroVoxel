#r "Grasshopper"
#r "RhinoCommon"

// RetroVoxel — additional demo canvas generator (covers the components Demo1/Demo2 didn't)
//
// Run in Rhino 8's Script Editor (C# mode) with RetroVoxel loaded in Grasshopper.
// Builds 4 more canvases into demos/:
//   Demo3_SpriteToVoxel.gh     — Local Sprite Loader -> Palette Quantizer -> Pixel Grid Builder -> Voxel Mesh Shell
//   Demo4_FilamentInterlock.gh — Local Sprite Loader -> Palette Quantizer -> [Filament Library Loader ->] Delta-E Filament Matcher -> Multi-Material Interlock
//                                (+ the same Quantized Colors -> Pixel Grid Builder -> Voxel Mesh Shell feeding the mesh side)
//   Demo5_BoxArtStandoff.gh    — RA Game Info -> Laser Standoff Stack
//   Demo6_SpriteSheetSplit.gh  — Local Sprite Loader -> Sprite Sheet Splitter (outputs shown on Panels, no further consumption)
//
// EDIT THIS before running: set a real PNG path for the sprite/badge image demos.
// A convenient source: any file in %TEMP%\RetroVoxel\badges\ after running Demo1 once.

using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino;

public static class RetroVoxelMoreDemoBuilder
{
    // ---- EDIT THIS ----
    const string SamplePngPath = @"C:\Users\sherft\AppData\Local\Temp\RetroVoxel\badges\321909.png";
    const string SampleFilamentLibraryPath = @"C:\Users\sherft\Downloads\RetroVoxel\RetroVoxel\demos\sample-filament-library.json";

    static readonly Guid LOCAL_SPRITE_LOADER = new Guid("f0df973b-3051-4683-8947-100a54621065");
    static readonly Guid SPRITE_SPLITTER     = new Guid("c5d49729-657a-4ebf-82b1-eaeacab5ce06");
    static readonly Guid PALETTE_QUANTIZER   = new Guid("1efd7553-08d7-417e-b75d-6264611daa4a");
    static readonly Guid DELTA_E_MATCHER     = new Guid("425b5e90-b7c6-47eb-becd-cc6cc75ef10b");
    static readonly Guid FILAMENT_LIB_LOADER = new Guid("9182c791-51e4-4f8e-9275-e7f8dbd1f4ef");
    static readonly Guid PIXEL_GRID          = new Guid("42b5226c-2c77-46f6-bc69-8b08d1c7e674");
    static readonly Guid VOXEL_SHELL         = new Guid("ed89abbc-b3fa-4d6d-93da-aa31b529eb5b");
    static readonly Guid MULTI_MATERIAL      = new Guid("b6342715-3cc8-47c0-9f87-0735448a961e");
    static readonly Guid RA_GAME_INFO        = new Guid("6cbd3079-4923-450c-aafa-1c0266250142");
    static readonly Guid LASER_STANDOFF      = new Guid("880277de-51e3-4b3f-a76d-1851342b9d95");

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

    // GH_Panel/GH_NumberSlider ARE IGH_Param objects themselves — wire the object directly,
    // never ".Params.Output[0]".
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

    static void Wire(IGH_Param source, IGH_Param target) => target.AddSource(source);

    static void Save(GH_Document doc, string path)
    {
        var io = new GH_DocumentIO(doc);
        if (!io.SaveQuiet(path))
            throw new InvalidOperationException("Failed to save " + path);
        RhinoApp.WriteLine("Saved: " + path);
    }

    // ---- Demo 3: Sprite to Voxel ----
    public static void BuildDemo3(string outputPath)
    {
        var doc = new GH_Document();

        var filePathPanel = NewPanel(doc, SamplePngPath, 0, 0);
        var loader = NewComponent(LOCAL_SPRITE_LOADER);
        Place(doc, loader, 250, 0);
        Wire(filePathPanel, loader.Params.Input[0]); // File Path

        var maxColorsPanel = NewPanel(doc, "16", 500, -60);
        var quant = NewComponent(PALETTE_QUANTIZER);
        Place(doc, quant, 550, 0);
        // loader outputs: 0=Image Path, 1=Width, 2=Height, 3=Pixel Colors
        Wire(loader.Params.Output[3], quant.Params.Input[0]); // Pixel Colors
        Wire(maxColorsPanel, quant.Params.Input[1]);          // Max Colors

        var pitchPanel = NewPanel(doc, "2.0", 800, -60);
        var thicknessPanel = NewPanel(doc, "1.0", 800, 0);
        var grid = NewComponent(PIXEL_GRID);
        Place(doc, grid, 850, 0);
        // quant outputs: 0=Quantized Colors, 1=Palette, 2=Color Indices
        Wire(quant.Params.Output[0], grid.Params.Input[0]); // Quantized Colors -> Rows
        Wire(pitchPanel, grid.Params.Input[1]);
        Wire(thicknessPanel, grid.Params.Input[2]);

        var voxelSizePanel = NewPanel(doc, "2.0", 1100, 60);
        var voxel = NewComponent(VOXEL_SHELL);
        Place(doc, voxel, 1150, 0);
        Wire(grid.Params.Output[0], voxel.Params.Input[0]);
        Wire(voxelSizePanel, voxel.Params.Input[1]);

        doc.NewSolution(false);
        Save(doc, outputPath);
    }

    // ---- Demo 4: Filament match to interlock ----
    public static void BuildDemo4(string outputPath)
    {
        var doc = new GH_Document();

        var filePathPanel = NewPanel(doc, SamplePngPath, 0, 0);
        var loader = NewComponent(LOCAL_SPRITE_LOADER);
        Place(doc, loader, 250, 0);
        Wire(filePathPanel, loader.Params.Input[0]);

        var maxColorsPanel = NewPanel(doc, "8", 500, -60);
        var quant = NewComponent(PALETTE_QUANTIZER);
        Place(doc, quant, 550, 0);
        Wire(loader.Params.Output[3], quant.Params.Input[0]);
        Wire(maxColorsPanel, quant.Params.Input[1]);

        var libraryPathPanel = NewPanel(doc, SampleFilamentLibraryPath, 650, -180);
        var libraryLoader = NewComponent(FILAMENT_LIB_LOADER);
        Place(doc, libraryLoader, 750, -150);
        Wire(libraryPathPanel, libraryLoader.Params.Input[0]); // File Path

        var deltaE = NewComponent(DELTA_E_MATCHER);
        Place(doc, deltaE, 950, -60);
        Wire(quant.Params.Output[1], deltaE.Params.Input[0]);        // Palette
        Wire(libraryLoader.Params.Output[0], deltaE.Params.Input[1]); // Filament Library JSON

        var pitchPanel = NewPanel(doc, "2.0", 800, 60);
        var thicknessPanel = NewPanel(doc, "1.0", 800, 120);
        var grid = NewComponent(PIXEL_GRID);
        Place(doc, grid, 850, 90);
        Wire(quant.Params.Output[0], grid.Params.Input[0]); // Quantized Colors -> Rows
        Wire(pitchPanel, grid.Params.Input[1]);
        Wire(thicknessPanel, grid.Params.Input[2]);

        var voxelSizePanel = NewPanel(doc, "2.0", 1100, 150);
        var voxel = NewComponent(VOXEL_SHELL);
        Place(doc, voxel, 1150, 90);
        Wire(grid.Params.Output[0], voxel.Params.Input[0]);
        Wire(voxelSizePanel, voxel.Params.Input[1]);

        var interlock = NewComponent(MULTI_MATERIAL);
        Place(doc, interlock, 1400, 0);
        Wire(voxel.Params.Output[0], interlock.Params.Input[0]);  // Voxel Mesh
        Wire(deltaE.Params.Output[0], interlock.Params.Input[1]); // Filament Match JSON

        doc.NewSolution(false);
        Save(doc, outputPath);
    }

    // ---- Demo 5: Box art standoff ----
    public static void BuildDemo5(string outputPath)
    {
        var doc = new GH_Document();

        var tokenPanel = NewPanel(doc, "<PASTE_TOKEN>", 0, 0);
        var gameIdSlider = NewSlider(doc, 1, 20000, 1446, 0, 60);
        var gameInfo = NewComponent(RA_GAME_INFO);
        Place(doc, gameInfo, 200, 0);
        Wire(tokenPanel, gameInfo.Params.Input[0]);
        Wire(gameIdSlider, gameInfo.Params.Input[1]);

        var aspectRatioPanel = NewPanel(doc, "0.7", 450, -60);
        var stackCountPanel = NewPanel(doc, "4", 450, 0);
        var panelThicknessPanel = NewPanel(doc, "3.0", 450, 60);

        var standoff = NewComponent(LASER_STANDOFF);
        Place(doc, standoff, 550, 0);
        Wire(aspectRatioPanel, standoff.Params.Input[0]);
        Wire(stackCountPanel, standoff.Params.Input[1]);
        Wire(panelThicknessPanel, standoff.Params.Input[2]);
        // Aspect Ratio is typed by hand here (no automated box-art-dimension extraction in v1) —
        // gameInfo is left on canvas so you can read Game JSON's ImageBoxArt and compute it yourself.

        doc.NewSolution(false);
        Save(doc, outputPath);
    }

    // ---- Demo 6: Sprite sheet split (standalone — outputs just shown on Panels) ----
    public static void BuildDemo6(string outputPath)
    {
        var doc = new GH_Document();

        var filePathPanel = NewPanel(doc, SamplePngPath, 0, 0);
        var loader = NewComponent(LOCAL_SPRITE_LOADER);
        Place(doc, loader, 250, 0);
        Wire(filePathPanel, loader.Params.Input[0]);

        var frameWidthPanel = NewPanel(doc, "16", 500, -60);
        var frameHeightPanel = NewPanel(doc, "16", 500, 0);
        var splitter = NewComponent(SPRITE_SPLITTER);
        Place(doc, splitter, 550, 0);
        Wire(loader.Params.Output[0], splitter.Params.Input[0]); // Image Path
        Wire(frameWidthPanel, splitter.Params.Input[1]);
        Wire(frameHeightPanel, splitter.Params.Input[2]);

        // View-only panels for each output — nothing consumes them further; open the panels in
        // GH and inspect the branch structure directly (right-click -> "Draw Paths").
        var framePathView = NewPanel(doc, "", 850, -60);
        Wire(splitter.Params.Output[0], framePathView);
        var frameIndexView = NewPanel(doc, "", 850, 0);
        Wire(splitter.Params.Output[1], frameIndexView);
        var pixelColorsView = NewPanel(doc, "", 850, 60);
        Wire(splitter.Params.Output[2], pixelColorsView);

        doc.NewSolution(false);
        Save(doc, outputPath);
    }
}

// ---- Entry point ----
string demosDir = @"C:\Users\sherft\Downloads\RetroVoxel\RetroVoxel\demos";
RetroVoxelMoreDemoBuilder.BuildDemo3(System.IO.Path.Combine(demosDir, "Demo3_SpriteToVoxel.gh"));
RetroVoxelMoreDemoBuilder.BuildDemo4(System.IO.Path.Combine(demosDir, "Demo4_FilamentInterlock.gh"));
RetroVoxelMoreDemoBuilder.BuildDemo5(System.IO.Path.Combine(demosDir, "Demo5_BoxArtStandoff.gh"));
RetroVoxelMoreDemoBuilder.BuildDemo6(System.IO.Path.Combine(demosDir, "Demo6_SpriteSheetSplit.gh"));
RhinoApp.WriteLine("Done. Edit SamplePngPath to a real PNG before opening Demo3/4/6, then open all 4 and verify.");
