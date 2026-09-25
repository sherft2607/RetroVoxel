using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Tags a voxel mesh with its matched filament so multi-material slicers can split by color.
    // v1 simplification: tags the whole mesh with its dominant filament match rather than generating
    // per-color interlocking dovetail geometry — the UserDictionary tag is what a slicer profile
    // or a follow-up split step reads to separate materials.
    public class MultiMaterialInterlockComponent : GH_Component
    {
        public MultiMaterialInterlockComponent()
          : base("Multi-Material Interlock", "INTER",
              "Tags a voxel mesh with its matched filament for multi-material fabrication.",
              PluginUtilities.TabName, PluginUtilities.CategoryADFabrication)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Voxel Mesh", "VM", "Mesh from Voxel Mesh Shell.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filament Match JSON", "FMJ", "One match from Delta-E Filament Matcher.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Interlock Mesh", "IM", "Mesh tagged with its matched filament name.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            Mesh mesh = null;
            string matchJson = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref matchJson);

            // 3. Guard clauses
            if (mesh == null || !mesh.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Voxel Mesh is missing or invalid.");
                return;
            }

            // 4. Logic
            string filamentName = "Unassigned";
            try
            {
                if (!string.IsNullOrWhiteSpace(matchJson))
                {
                    var parsed = FilamentBuilders.Parse(matchJson);
                    if (parsed != null)
                        filamentName = (string)parsed["filamentName"] ?? filamentName;
                }
            }
            catch (Newtonsoft.Json.JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Filament Match JSON was not valid.");
            }

            var tagged = mesh.DuplicateMesh();
            tagged.UserDictionary.Set("FilamentName", filamentName);

            // 5. DA.SetData calls
            DA.SetData(0, tagged);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_MultiMaterialInterlock;
        public override Guid ComponentGuid => new Guid("b6342715-3cc8-47c0-9f87-0735448a961e");
    }
}
