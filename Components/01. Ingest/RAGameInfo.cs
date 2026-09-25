using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Fetches extended game metadata and its full achievement list (with badge names) from RetroAchievements.
    public class RAGameInfoComponent : ButtonComponent
    {
        public RAGameInfoComponent()
          : base("RA Game Info", "GAME",
              "Fetches a game's metadata and achievement list from RetroAchievements.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAIngest)
        { }

        public override string ButtonLabel => "Fetch Game";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "RetroAchievements web API key.", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("Game ID", "GID", "RetroAchievements game ID.", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Exactly 3 outputs — RA Game Info stops at Game JSON / Achievement JSON. Badge
            // download and Pixel Colors extraction are RA Badges' sole responsibility.
            pManager.AddTextParameter("Status", "S", "OK, or the error from the last fetch.", GH_ParamAccess.item);
            pManager.AddTextParameter("Game JSON", "GJ", "Game metadata (title, console, image paths).", GH_ParamAccess.item);
            pManager.AddTextParameter("Achievement JSON", "AJ", "One achievement record per branch.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) { EmitLastResult(DA); return; }

            // 1. Declare variables with defaults
            string token = "";
            int gameId = 0;

            // 2. DA.GetData calls
            DA.GetData(0, ref token);
            DA.GetData(1, ref gameId);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(token) || gameId <= 0)
            {
                LastStatus = "Error: Token and a valid Game ID are required.";
                LastResponse = "";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, LastStatus);
                DA.SetData(0, LastStatus);
                return;
            }

            // 4. Logic
            var client = new RetroVoxelClient(token);
            var (ok, json, err) = Task.Run(() => client.GetGameExtendedAsync(gameId.ToString())).GetAwaiter().GetResult();

            // LastResponse caches the raw response internally (persisted via ButtonComponent's
            // Write/Read) even though it is no longer exposed as its own output port.
            LastStatus = ok ? "OK" : "Error: " + err;
            LastResponse = ok ? json : "";

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(0, LastStatus);
                return;
            }

            EmitParsed(DA, json);

            // 5. DA.SetData calls
            DA.SetData(0, LastStatus);
        }

        // Overridden (not base.EmitLastResult) — the base assumes output 1 is Response, but here
        // output 1 is Game JSON. Status stays at 0; Game JSON / Achievement JSON are re-derived
        // from the cached raw response.
        protected override void EmitLastResult(IGH_DataAccess DA)
        {
            if (string.IsNullOrEmpty(LastStatus) && string.IsNullOrEmpty(LastResponse))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Press \"" + ButtonLabel + "\" to run.");
                return;
            }
            DA.SetData(0, LastStatus);
            if (string.IsNullOrEmpty(LastResponse)) return;
            EmitParsed(DA, LastResponse);
        }

        private void EmitParsed(IGH_DataAccess DA, string json)
        {
            try
            {
                var game = JObject.Parse(json);
                DA.SetData(1, game.ToString(Formatting.None));

                var achievementTree = new GH_Structure<GH_String>();
                var achievements = game["Achievements"] as JObject;
                if (achievements != null)
                {
                    int i = 0;
                    foreach (var prop in achievements.Properties())
                    {
                        var path = new GH_Path(i);
                        achievementTree.Append(new GH_String(prop.Value.ToString(Formatting.None)), path);
                        i++;
                    }
                }
                DA.SetDataTree(2, achievementTree);
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_RAGameInfo;
        public override Guid ComponentGuid => new Guid("6cbd3079-4923-450c-aafa-1c0266250142");
    }
}
