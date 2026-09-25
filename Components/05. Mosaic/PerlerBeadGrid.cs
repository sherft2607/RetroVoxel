using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace RetroVoxel
{
    // Builds a pegboard-style bead layout mesh from a Pixel Grid JSON — one disc per non-background pixel.
    public class PerlerBeadGridComponent : GH_Component
    {
        public PerlerBeadGridComponent()
          : base("Perler Bead Grid", "PERLER",
              "Builds a pegboard-style bead layout mesh from a Pixel Grid JSON.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEMosaic)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pixel Grid JSON", "PGJ", "Grid from Pixel Grid Builder.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bead Size", "BS", "Peg pitch and bead diameter.", GH_ParamAccess.item, 5.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Bead Grid Mesh", "BGM", "One flat bead disc per placed cell.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string gridJson = "";
            double beadSize = 5.0;

            // 2. DA.GetData calls
            DA.GetData(0, ref gridJson);
            DA.GetData(1, ref beadSize);

            // 3. Guard clauses
            var grid = PixelGridBuilders.Parse(gridJson);
            if (grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pixel Grid JSON is empty or invalid.");
                return;
            }
            if (beadSize <= 0) beadSize = 5.0;

            // 4. Logic
            int rows = (int)grid["bounds"]["rows"];
            int cols = (int)grid["bounds"]["cols"];
            double beadRadius = beadSize * 0.45;
            double beadHeight = beadSize * 0.3;
            int placedCount = 0;

            var beads = new Mesh();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    string hex = PixelGridBuilders.CellAt(grid, r, c);
                    if (hex == null) continue; // void cell — no bead

                    var center = new Point3d(c * beadSize + beadSize / 2.0, -(r * beadSize + beadSize / 2.0), 0);
                    var cylinder = new Cylinder(new Circle(new Plane(center, Vector3d.ZAxis), beadRadius), beadHeight);
                    var beadMesh = Mesh.CreateFromCylinder(cylinder, 1, 12);

                    // Paint each bead with its source pixel's color, matching Voxel Mesh Shell.
                    beadMesh.VertexColors.CreateMonotoneMesh(HexToColor(hex));

                    beads.Append(beadMesh);
                    placedCount++;
                }
            }
            beads.Weld(Math.PI);

            if (placedCount == 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No non-background cells — no beads placed.");

            // 5. DA.SetData calls
            DA.SetData(0, beads);
        }

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

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_PerlerBeadGrid;
        public override Guid ComponentGuid => new Guid("9a91acb0-5b46-419a-9e83-f7d8f3d73ce4");
    }
}
