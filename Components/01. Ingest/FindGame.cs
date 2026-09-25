using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Resolves a game name (or substring) to its numeric RetroAchievements Game ID by searching
    // GetGameList for a console — RA has no direct "search by title" endpoint, so this is the
    // sibling-API name-lookup pattern (like Find[Resource] over a Drive-style search endpoint).
    public class FindGameComponent : ButtonComponent
    {
        public FindGameComponent()
          : base("Find Game", "FIND",
              "Finds Game IDs matching a name (or substring) within a console's game list.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAIngest)
        { }

        public override string ButtonLabel => "Find Game";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "RetroAchievements web API key.", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("Console ID", "CID", "RetroAchievements console ID (e.g. from RA Console).", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Name", "N", "Game title or substring to search for (case-insensitive).", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "OK, or the error from the last search.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Game ID", "GID", "Matching Game IDs.", GH_ParamAccess.list);
            pManager.AddTextParameter("Game Title", "GT", "Matching game titles, same order as Game ID.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) { EmitLastResult(DA); return; }

            // 1. Declare variables with defaults
            string token = "";
            int consoleId = 0;
            string name = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref token);
            DA.GetData(1, ref consoleId);
            DA.GetData(2, ref name);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(token) || consoleId <= 0 || string.IsNullOrWhiteSpace(name))
            {
                LastStatus = "Error: Token, Console ID, and Name are all required.";
                LastResponse = "";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, LastStatus);
                DA.SetData(0, LastStatus);
                return;
            }

            // 4. Logic
            var client = new RetroVoxelClient(token);
            var (ok, json, err) = Task.Run(() => client.GetGameListAsync(consoleId.ToString())).GetAwaiter().GetResult();

            LastStatus = ok ? "OK" : "Error: " + err;
            LastResponse = ok ? json : "";

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(0, LastStatus);
                return;
            }

            EmitParsed(DA, json, name);

            // 5. DA.SetData calls
            DA.SetData(0, LastStatus);
        }

        // Cache the search term too — a non-triggered re-solve must re-filter the same way.
        private string _lastName = "";

        protected override void EmitLastResult(IGH_DataAccess DA)
        {
            if (string.IsNullOrEmpty(LastStatus) && string.IsNullOrEmpty(LastResponse))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Press \"" + ButtonLabel + "\" to run.");
                return;
            }
            DA.SetData(0, LastStatus);
            if (string.IsNullOrEmpty(LastResponse)) return;
            EmitParsed(DA, LastResponse, _lastName);
        }

        private void EmitParsed(IGH_DataAccess DA, string json, string name)
        {
            _lastName = name;
            try
            {
                var games = JArray.Parse(json);
                var ids = new List<int>();
                var titles = new List<string>();
                foreach (var g in games)
                {
                    string title = (string)g["Title"] ?? "";
                    if (title.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    ids.Add((int)(g["ID"] ?? 0));
                    titles.Add(title);
                }
                DA.SetDataList(1, ids);
                DA.SetDataList(2, titles);

                if (ids.Count == 0)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No games matched \"" + name + "\" on this console.");
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not a JSON array: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_FindGame;
        public override Guid ComponentGuid => new Guid("cb6c3560-dfbf-4438-8c33-c15e871f298e");
    }
}
