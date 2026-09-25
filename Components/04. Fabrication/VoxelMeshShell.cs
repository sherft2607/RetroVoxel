using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Extrudes every cell of a Pixel Grid JSON into a voxel box, merged into one shell mesh.
    public class VoxelMeshShellComponent : GH_Component
    {
        public VoxelMeshShellComponent()
          : base("Voxel Mesh Shell", "VOX",
              "Builds a voxel mesh shell from a Pixel Grid JSON, one box per cell.",
              PluginUtilities.TabName, PluginUtilities.CategoryADFabrication)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pixel Grid JSON", "PGJ", "Grid from Pixel Grid Builder.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Voxel Size", "VS", "Edge length of one voxel cell.", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Extrusion Height", "EH", "Optional per-cell height, row-major order; last value repeats.", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Voxel Mesh", "VM", "Merged voxel shell mesh.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string gridJson = "";
            double voxelSize = 2.0;
            var heights = new List<double>();

            // 2. DA.GetData calls
            DA.GetData(0, ref gridJson);
            DA.GetData(1, ref voxelSize);
            DA.GetDataList(2, heights);

            // 3. Guard clauses
            var grid = PixelGridBuilders.Parse(gridJson);
            if (grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pixel Grid JSON is empty or invalid.");
                return;
            }
            if (voxelSize <= 0) voxelSize = 2.0;

            // 4. Logic
            int rows = (int)grid["bounds"]["rows"];
            int cols = (int)grid["bounds"]["cols"];
            var shell = new Mesh();
            double lastHeight = heights.Count > 0 ? heights[heights.Count - 1] : voxelSize;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    string hex = PixelGridBuilders.CellAt(grid, r, c);
                    if (hex == null && heights.Count == 0) continue; // void cell, no explicit height given

                    int index = r * cols + c;
                    double height = index < heights.Count ? heights[index] : lastHeight;
                    if (height <= 0) continue;

                    var box = new Box(Plane.WorldXY,
                        new Interval(c * voxelSize, (c + 1) * voxelSize),
                        new Interval(-(r + 1) * voxelSize, -r * voxelSize),
                        new Interval(0, height));
                    var voxelMesh = Mesh.CreateFromBox(box, 1, 1, 1);

                    // Paint every vertex of this voxel with its source pixel's color so the
                    // merged shell reproduces the badge's colors, not Rhino's default gray.
                    var color = HexToColor(hex);
                    voxelMesh.VertexColors.CreateMonotoneMesh(color);

                    shell.Append(voxelMesh);
                }
            }
            shell.Weld(Math.PI);

            // 5. DA.SetData calls
            DA.SetData(0, shell);
        }

        // Falls back to mid-gray for a void cell that still got a box because Extrusion Height
        // forced one — keeps every appended sub-mesh colored so Mesh.Append never has to clear
        // vertex colors for a count mismatch.
        private static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.Gray;
            hex = hex.TrimStart('#');
            if (hex.Length < 6) return Color.Gray;
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return Color.FromArgb(r, g, b);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_VoxelMeshShell;
        public override Guid ComponentGuid => new Guid("ed89abbc-b3fa-4d6d-93da-aa31b529eb5b");
    }
}
