using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace RetroVoxel
{
    // Builds a watertight Lego-compatible stud plate mesh from a Pixel Grid JSON — one base cell
    // plus one stud cylinder per non-background pixel.
    public class LegoStudPlateComponent : GH_Component
    {
        public LegoStudPlateComponent()
          : base("Lego Stud Plate", "LEGO",
              "Builds a watertight Lego-compatible stud plate mesh from a Pixel Grid JSON.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEMosaic)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pixel Grid JSON", "PGJ", "Grid from Pixel Grid Builder.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stud Size", "SS", "Plate cell pitch; stud proportions scale from it.", GH_ParamAccess.item, 8.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Stud Plate Mesh", "SPM", "Watertight base plate with one stud per cell.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string gridJson = "";
            double studSize = 8.0;

            // 2. DA.GetData calls
            DA.GetData(0, ref gridJson);
            DA.GetData(1, ref studSize);

            // 3. Guard clauses
            var grid = PixelGridBuilders.Parse(gridJson);
            if (grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pixel Grid JSON is empty or invalid.");
                return;
            }
            if (studSize <= 0) studSize = 8.0;

            // 4. Logic
            int rows = (int)grid["bounds"]["rows"];
            int cols = (int)grid["bounds"]["cols"];
            double plateThickness = studSize * 0.375;
            double studRadius = studSize * 0.3;
            double studHeight = studSize * 0.2;

            var plate = new Mesh();
            var baseBox = new Box(Plane.WorldXY,
                new Interval(0, cols * studSize), new Interval(-rows * studSize, 0), new Interval(0, plateThickness));
            var baseMesh = Mesh.CreateFromBox(baseBox, 1, 1, 1);
            baseMesh.VertexColors.CreateMonotoneMesh(Color.Gray); // the base is a single shared plate, not per-pixel
            plate.Append(baseMesh);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    string hex = PixelGridBuilders.CellAt(grid, r, c);
                    if (hex == null) continue; // void cell — no stud

                    var center = new Point3d(c * studSize + studSize / 2.0, -(r * studSize + studSize / 2.0), plateThickness);
                    var cylinder = new Cylinder(new Circle(new Plane(center, Vector3d.ZAxis), studRadius), studHeight);
                    var studMesh = Mesh.CreateFromCylinder(cylinder, 1, 16);

                    // Paint each stud with its source pixel's color, matching Voxel Mesh Shell.
                    studMesh.VertexColors.CreateMonotoneMesh(HexToColor(hex));

                    plate.Append(studMesh);
                }
            }
            plate.Weld(Math.PI);

            // 5. DA.SetData calls
            DA.SetData(0, plate);
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

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_LegoStudPlate;
        public override Guid ComponentGuid => new Guid("524265b8-1249-4554-a262-d469abc82a06");
    }
}
