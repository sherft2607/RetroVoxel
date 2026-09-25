using System;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Validates the RA web API key and populates the console catalog (GetConsoleIDs).
    // Doubles as the plugin's connectivity check — no separate Auth component (Gate 2).
    public class RAConsolePresetComponent : ButtonComponent
    {
        public RAConsolePresetComponent()
            : base("RA Console", "CONSOLE",
                "Validates the RetroAchievements web API key and lists every console ID and name.",
                PluginUtilities.TabName, PluginUtilities.CategoryAFPresets)
        { }

        public override string ButtonLabel => "Check Key";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "RetroAchievements web API key.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Output 0 = Status, output 1 = Response — ButtonComponent.EmitLastResult relies on these indices.
            pManager.AddTextParameter("Status", "S", "OK, or the error from the last check.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw GetConsoleIDs response JSON.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Console ID", "CID", "Console ID for every system on the site.", GH_ParamAccess.list);
            pManager.AddTextParameter("Console Name", "CN", "Console name matching each Console ID by index.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) { EmitLastResult(DA); return; }

            // 1. Declare variables with defaults
            string token = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref token);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(token))
            {
                LastStatus = "Error: Token is empty.";
                LastResponse = "";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Token is empty.");
                DA.SetData(0, LastStatus);
                DA.SetData(1, LastResponse);
                return;
            }

            // 4. Logic
            var client = new RetroVoxelClient(token);
            var (ok, json, err) = Task.Run(() => client.GetConsoleIdsAsync()).GetAwaiter().GetResult();

            LastStatus = ok ? "OK" : "Error: " + err;
            LastResponse = ok ? json : "";

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(0, LastStatus);
                DA.SetData(1, LastResponse);
                return;
            }

            EmitParsed(DA, json);

            // 5. DA.SetData calls
            DA.SetData(0, LastStatus);
            DA.SetData(1, LastResponse);
        }

        // Re-emit parsed outputs from the cached response on non-triggered solves
        protected override void EmitLastResult(IGH_DataAccess DA)
        {
            base.EmitLastResult(DA);                       // Status (0) + Response (1)
            if (string.IsNullOrEmpty(LastResponse)) return;
            EmitParsed(DA, LastResponse);
        }

        private void EmitParsed(IGH_DataAccess DA, string json)
        {
            try
            {
                var consoles = JArray.Parse(json);
                var ids = new System.Collections.Generic.List<int>();
                var names = new System.Collections.Generic.List<string>();
                foreach (var c in consoles)
                {
                    ids.Add((int)(c["ID"] ?? 0));
                    names.Add((string)(c["Name"] ?? ""));
                }
                DA.SetDataList(2, ids);
                DA.SetDataList(3, names);
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not a JSON array: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_RAConsole;
        public override Guid ComponentGuid => new Guid("7260574b-c1f4-4c00-beb5-5f4e6684ed3d");
    }
}
