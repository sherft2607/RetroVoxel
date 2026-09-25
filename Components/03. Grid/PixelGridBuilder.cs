using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RetroVoxel
{
    // Assembles a tree of per-row pixel data into the single shared Pixel Grid JSON contract every
    // Fabrication/Mosaic component consumes. Rows accepts either form:
    //   a) a DataTree of Colour/Text where each branch {y} holds one row's cells, or
    //   b) a flat list of one comma-separated hex string per row (e.g. "#FF0000, #00FF00, #0000FF")
    public class PixelGridBuilderComponent : GH_Component
    {
        public PixelGridBuilderComponent()
          : base("Pixel Grid Builder", "GRID",
              "Builds a Pixel Grid JSON from a tree of rows of pixel colors, or one comma-separated hex row per branch.",
              PluginUtilities.TabName, PluginUtilities.CategoryACGrid)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rows", "RV",
                "One branch per row: either multiple Colour/hex items (one per cell), or a single comma-separated hex string.",
                GH_ParamAccess.tree);
            pManager.AddNumberParameter("Pitch", "P", "Informational cell pitch, stored in the grid JSON.", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Thickness", "TH", "Informational material thickness, stored in the grid JSON.", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Pixel Grid JSON", "PGJ", "The assembled tabular pixel grid (bounds, pitch, thickness, palette, indices).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            GH_Structure<IGH_Goo> rowTree;
            double pitch = 2.0, thickness = 1.0;

            // 2. DA.GetData calls
            DA.GetDataTree(0, out rowTree);
            DA.GetData(1, ref pitch);
            DA.GetData(2, ref thickness);

            // 3. Guard clauses
            if (rowTree == null || rowTree.IsEmpty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Rows is empty.");
                return;
            }

            // 4. Logic
            var rows = new List<IList<string>>();
            foreach (var path in rowTree.Paths)
            {
                var branch = rowTree.get_Branch(path);
                var row = new List<string>();

                if (branch.Count == 1 && TryGetString(branch[0], out string single) && single.Contains(","))
                {
                    // form (b): one comma-separated hex string per row
                    foreach (var part in single.Split(','))
                        row.Add(ToHex(part.Trim()));
                }
                else
                {
                    // form (a): one Colour/hex item per cell
                    foreach (var goo in branch)
                        row.Add(GooToHex(goo));
                }
                rows.Add(row);
            }
            string gridJson = PixelGridBuilders.BuildPixelGrid(rows, pitch, thickness);

            // 5. DA.SetData calls
            DA.SetData(0, gridJson);
        }

        private static bool TryGetString(object item, out string value)
        {
            value = null;
            if (item is IGH_Goo goo) return goo.CastTo(out value);
            return false;
        }

        private static string GooToHex(object item)
        {
            if (item is IGH_Goo goo)
            {
                if (goo.CastTo(out Color c)) return ToHex(c);
                if (goo.CastTo(out string s)) return ToHex(s.Trim());
            }
            return null;
        }

        private static string ToHex(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

        private static string ToHex(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("VOID", StringComparison.OrdinalIgnoreCase)) return null;
            return value.StartsWith("#") ? value : "#" + value;
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_PixelGridBuilder;
        public override Guid ComponentGuid => new Guid("42b5226c-2c77-46f6-bc69-8b08d1c7e674");
    }
}
