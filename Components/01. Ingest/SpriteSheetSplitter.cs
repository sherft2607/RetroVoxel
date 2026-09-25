using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RetroVoxel
{
    // Splits a multi-frame sprite sheet into individual frame images, one branch per frame.
    public class SpriteSheetSplitterComponent : GH_Component
    {
        public SpriteSheetSplitterComponent()
          : base("Sprite Sheet Splitter", "SPLIT",
              "Splits a sprite sheet into a tree of individual frame images.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAIngest)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Image Path", "IP", "Path to the sprite sheet image.", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("Frame Width", "FW", "Width of a single frame in pixels.", GH_ParamAccess.item, 16);
            pManager.AddIntegerParameter("Frame Height", "FH", "Height of a single frame in pixels.", GH_ParamAccess.item, 16);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Frame Image Path", "FIP", "One extracted frame image path per branch.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Frame Index", "FI", "Sequential frame index per branch.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Pixel Colors", "PC", "Rows of pixels per frame, path {frameIndex, row}.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string imagePath = "";
            int frameWidth = 16, frameHeight = 16;

            // 2. DA.GetData calls
            DA.GetData(0, ref imagePath);
            DA.GetData(1, ref frameWidth);
            DA.GetData(2, ref frameHeight);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File not found: " + imagePath);
                return;
            }
            if (frameWidth <= 0 || frameHeight <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Frame Width/Height must be positive.");
                return;
            }

            // 4. Logic
            var pathTree = new GH_Structure<Grasshopper.Kernel.Types.GH_String>();
            var indexTree = new GH_Structure<Grasshopper.Kernel.Types.GH_Integer>();
            var pixelTree = new GH_Structure<Grasshopper.Kernel.Types.GH_Colour>();
            var tempDir = Path.Combine(Path.GetTempPath(), "RetroVoxel", "frames");
            Directory.CreateDirectory(tempDir);

            using (var sheet = new Bitmap(imagePath))
            {
                int cols = Math.Max(1, sheet.Width / frameWidth);
                int rows = Math.Max(1, sheet.Height / frameHeight);
                int frameIndex = 0;

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var rect = new Rectangle(c * frameWidth, r * frameHeight, frameWidth, frameHeight);
                        using (var frame = sheet.Clone(rect, sheet.PixelFormat))
                        {
                            string framePath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(imagePath) + "_" + frameIndex + ".png");
                            frame.Save(framePath, ImageFormat.Png);

                            var path = new GH_Path(frameIndex);
                            pathTree.Append(new Grasshopper.Kernel.Types.GH_String(framePath), path);
                            indexTree.Append(new Grasshopper.Kernel.Types.GH_Integer(frameIndex), path);
                            ImagePixelHelpers.AppendPixelRows(pixelTree, frame, path);
                        }
                        frameIndex++;
                    }
                }
            }

            // 5. DA.SetData calls
            DA.SetDataTree(0, pathTree);
            DA.SetDataTree(1, indexTree);
            DA.SetDataTree(2, pixelTree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_SpriteSheetSplitter;
        public override Guid ComponentGuid => new Guid("c5d49729-657a-4ebf-82b1-eaeacab5ce06");
    }
}
