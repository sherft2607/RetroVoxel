using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Drawing;

namespace RetroVoxel
{
    // Loads a single local image file and reports its pixel dimensions.
    public class LocalSpriteLoaderComponent : GH_Component
    {
        public LocalSpriteLoaderComponent()
          : base("Local Sprite Loader", "LOAD",
              "Loads a single-frame sprite image from disk and reports its size.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAIngest)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "FP", "Path to the sprite image on disk.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Image Path", "IP", "Same file path, passed through for downstream components.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Image width in pixels.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Height", "H", "Image height in pixels.", GH_ParamAccess.item);
            pManager.AddColourParameter("Pixel Colors", "PC", "One branch per row of pixels.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string filePath = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref filePath);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File not found: " + filePath);
                return;
            }

            // 4. Logic
            int width, height;
            var pixelTree = new GH_Structure<GH_Colour>();
            try
            {
                using (var img = new Bitmap(filePath))
                {
                    width = img.Width;
                    height = img.Height;
                    ImagePixelHelpers.AppendPixelRows(pixelTree, img, new GH_Path());
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read image: " + ex.Message);
                return;
            }

            // 5. DA.SetData calls
            DA.SetData(0, filePath);
            DA.SetData(1, width);
            DA.SetData(2, height);
            DA.SetDataTree(3, pixelTree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_LocalSpriteLoader;
        public override Guid ComponentGuid => new Guid("f0df973b-3051-4683-8947-100a54621065");
    }
}
