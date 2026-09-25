using Grasshopper.Kernel;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Loads a filament library JSON file from disk — the user's own real filament colors, never
    // a baked-in list (there is no live filament-color API to source one from).
    public class FilamentLibraryLoaderComponent : GH_Component
    {
        public FilamentLibraryLoaderComponent()
          : base("Filament Library Loader", "FLIB",
              "Loads a filament library JSON file from disk.",
              PluginUtilities.TabName, PluginUtilities.CategoryABColor)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "FP", "Path to a filament library JSON file.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Filament Library JSON", "FLJ", "{\"filaments\":[{\"name\":..,\"hex\":..}]}", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string filePath = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref filePath);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File not found: " + filePath);
                return;
            }

            // 4. Logic
            string json;
            try
            {
                json = File.ReadAllText(filePath);
                var parsed = JObject.Parse(json);
                if (parsed["filaments"] == null)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File has no \"filaments\" array — Delta-E Filament Matcher will fall back to a default.");
            }
            catch (JsonException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File is not valid JSON: " + ex.Message);
                return;
            }
            catch (IOException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read file: " + ex.Message);
                return;
            }

            // 5. DA.SetData calls
            DA.SetData(0, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_FilamentLibraryLoader;
        public override Guid ComponentGuid => new Guid("9182c791-51e4-4f8e-9275-e7f8dbd1f4ef");
    }
}
