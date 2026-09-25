using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Maps a Pixel Grid JSON to LED positions and serpentine wiring addresses/colors, matching how
    // a WS2812-style matrix is physically wired (even rows left-to-right, odd rows reversed).
    public class LEDMatrixMapperComponent : GH_Component
    {
        public LEDMatrixMapperComponent()
          : base("LED Matrix Mapper", "LED",
              "Maps a Pixel Grid JSON to LED positions and serpentine wiring order.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEMosaic)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pixel Grid JSON", "PGJ", "Grid from Pixel Grid Builder.", GH_ParamAccess.item);
            pManager.AddNumberParameter("LED Pitch", "LP", "Distance between adjacent LEDs.", GH_ParamAccess.item, 10.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("LED Address JSON", "LAJ", "{index, row, col, color} per LED, in serpentine wiring order.", GH_ParamAccess.list);
            pManager.AddPointParameter("LED Position", "LPOS", "World position per LED, matching LED Address JSON order.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string gridJson = "";
            double pitch = 10.0;

            // 2. DA.GetData calls
            DA.GetData(0, ref gridJson);
            DA.GetData(1, ref pitch);

            // 3. Guard clauses
            var grid = PixelGridBuilders.Parse(gridJson);
            if (grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pixel Grid JSON is empty or invalid.");
                return;
            }
            if (pitch <= 0) pitch = 10.0;

            // 4. Logic
            int rows = (int)grid["bounds"]["rows"];
            int cols = (int)grid["bounds"]["cols"];
            var addresses = new List<string>();
            var positions = new List<Point3d>();
            int index = 0;

            for (int r = 0; r < rows; r++)
            {
                bool reversed = r % 2 == 1;
                for (int i = 0; i < cols; i++)
                {
                    int c = reversed ? (cols - 1 - i) : i;
                    string hex = PixelGridBuilders.CellAt(grid, r, c);

                    var addr = new JObject { ["index"] = index, ["row"] = r, ["col"] = c, ["color"] = hex };
                    addresses.Add(addr.ToString(Formatting.None));
                    positions.Add(new Point3d(c * pitch + pitch / 2.0, -(r * pitch + pitch / 2.0), 0));
                    index++;
                }
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, addresses);
            DA.SetDataList(1, positions);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_LEDMatrixMapper;
        public override Guid ComponentGuid => new Guid("ec7a7b0f-096e-44f4-8832-86edb8c937eb");
    }
}
