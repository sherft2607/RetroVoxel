using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Matches every color in a quantized Palette to its nearest filament swatch by Delta-E distance.
    public class DeltaEFilamentMatcherComponent : GH_Component
    {
        public DeltaEFilamentMatcherComponent()
          : base("Delta-E Filament Matcher", "MATCH",
              "Matches each palette color to the nearest filament color by Delta-E distance.",
              PluginUtilities.TabName, PluginUtilities.CategoryABColor)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Palette", "PAL", "Palette from Palette Quantizer.", GH_ParamAccess.list);
            pManager.AddTextParameter("Filament Library JSON", "FLJ", "{\"filaments\":[{\"name\":..,\"hex\":..}]}", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Filament Match JSON", "FMJ", "Nearest filament match, one per palette color, same order as Palette.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var palette = new List<GH_Colour>();
            string libraryJson = "";

            // 2. DA.GetData calls
            DA.GetDataList(0, palette);
            DA.GetData(1, ref libraryJson);

            // 3. Guard clauses
            if (palette.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Palette is empty.");
                return;
            }

            // 4. Logic
            var library = new List<(string name, string hex)>();
            try
            {
                if (!string.IsNullOrWhiteSpace(libraryJson))
                {
                    var libObj = JObject.Parse(libraryJson);
                    var filaments = libObj["filaments"] as JArray ?? new JArray();
                    foreach (var f in filaments)
                        library.Add(((string)f["name"] ?? "Unknown", (string)f["hex"] ?? "#000000"));
                }
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Filament Library JSON was not valid — using an empty library.");
            }
            if (library.Count == 0) library.Add(("Generic White", "#FFFFFF"));

            var matches = new List<string>();
            foreach (var ghColor in palette)
            {
                Color c = ghColor.Value;
                string hex = "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
                matches.Add(FilamentBuilders.FindNearestFilament(hex, library));
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, matches);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_DeltaEFilamentMatcher;
        public override System.Guid ComponentGuid => new System.Guid("425b5e90-b7c6-47eb-becd-cc6cc75ef10b");
    }
}
