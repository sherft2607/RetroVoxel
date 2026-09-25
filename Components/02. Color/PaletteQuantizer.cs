using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RetroVoxel
{
    // Quantizes a Pixel Colors tree (any shape — one branch per row, however nested by an upstream
    // ingest component) down to Max Colors, preserving the exact input tree shape on every output.
    // Alpha == 0 pixels are treated as transparent/void and get Color Indices == -1.
    public class PaletteQuantizerComponent : GH_Component
    {
        public PaletteQuantizerComponent()
          : base("Palette Quantizer", "QUANT",
              "Quantizes a Pixel Colors tree down to a target color count.",
              PluginUtilities.TabName, PluginUtilities.CategoryABColor)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Pixel Colors", "PC", "Pixel colors, one branch per row (from an Ingest component).", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Max Colors", "MC", "Target number of colors in the quantized palette.", GH_ParamAccess.item, 16);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Quantized Colors", "QC", "Each pixel snapped to its nearest palette color; same tree shape as input.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Palette", "PAL", "The distinct colors used by the quantized output.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Color Indices", "CI", "Palette index per pixel; -1 for a transparent/void pixel. Same tree shape as input.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            GH_Structure<GH_Colour> pixelTree;
            int maxColors = 16;

            // 2. DA.GetData calls
            DA.GetDataTree(0, out pixelTree);
            DA.GetData(1, ref maxColors);

            // 3. Guard clauses
            if (pixelTree == null || pixelTree.IsEmpty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pixel Colors is empty.");
                return;
            }
            if (maxColors <= 0) maxColors = 16;

            // 4. Logic — build the frequency histogram over every non-transparent pixel
            var counts = new Dictionary<int, int>();
            foreach (var path in pixelTree.Paths)
            {
                foreach (var item in pixelTree.get_Branch(path))
                {
                    Color c = ((GH_Colour)item).Value;
                    if (c.A == 0) continue; // void — excluded from the palette
                    counts[c.ToArgb()] = counts.TryGetValue(c.ToArgb(), out var n) ? n + 1 : 1;
                }
            }

            var palette = counts.OrderByDescending(kv => kv.Value).Take(maxColors)
                .Select(kv => Color.FromArgb(kv.Key)).ToList();
            if (palette.Count == 0) palette.Add(Color.Black);

            var quantizedTree = new GH_Structure<GH_Colour>();
            var indexTree = new GH_Structure<GH_Integer>();

            foreach (var path in pixelTree.Paths)
            {
                foreach (var item in pixelTree.get_Branch(path))
                {
                    Color c = ((GH_Colour)item).Value;
                    if (c.A == 0)
                    {
                        quantizedTree.Append(new GH_Colour(c), path);
                        indexTree.Append(new GH_Integer(-1), path);
                        continue;
                    }

                    int idx = NearestIndex(c, palette);
                    quantizedTree.Append(new GH_Colour(palette[idx]), path);
                    indexTree.Append(new GH_Integer(idx), path);
                }
            }

            // 5. DA.SetData calls
            DA.SetDataTree(0, quantizedTree);
            DA.SetDataList(1, palette.Select(c => new GH_Colour(c)));
            DA.SetDataTree(2, indexTree);
        }

        private static int NearestIndex(Color c, List<Color> palette)
        {
            int best = 0;
            int bestDist = int.MaxValue;
            for (int i = 0; i < palette.Count; i++)
            {
                var p = palette[i];
                int dr = c.R - p.R, dg = c.G - p.G, db = c.B - p.B;
                int dist = dr * dr + dg * dg + db * db;
                if (dist < bestDist) { bestDist = dist; best = i; }
            }
            return best;
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_PaletteQuantizer;
        public override System.Guid ComponentGuid => new System.Guid("1efd7553-08d7-417e-b75d-6264611daa4a");
    }
}
