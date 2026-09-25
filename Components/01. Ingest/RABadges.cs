using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Resolves each achievement's BadgeName into a downloaded badge image from the RA media CDN.
    public class RABadgesComponent : GH_Component
    {
        private static readonly RetroVoxelClient _downloader = new RetroVoxelClient("");

        public RABadgesComponent()
          : base("RA Badges", "BADGE",
              "Downloads the achievement badge image for each achievement record.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAIngest)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Achievement JSON", "AJ", "One achievement record per branch (from RA Game Info).", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Badge Image Path", "BIP", "Downloaded badge image path, one per branch.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Badge Resource ID", "BRID", "The numeric badge image ID used to build the CDN URL — not a readable name.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Achievement Title", "AT", "The achievement's readable title, same order/branches as Badge Resource ID.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Pixel Colors", "PC", "Rows of pixels per badge, path {badgeIndex, row}.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Status", "S", "OK, or a summary of any download errors.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var achievementTree = new GH_Structure<GH_String>();

            // 2. DA.GetData calls
            DA.GetDataTree(0, out achievementTree);

            // 3. Guard clauses
            if (achievementTree == null || achievementTree.IsEmpty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Achievement JSON is empty.");
                return;
            }

            // 4. Logic
            var tempDir = Path.Combine(Path.GetTempPath(), "RetroVoxel", "badges");
            Directory.CreateDirectory(tempDir);

            var pathTree = new GH_Structure<GH_String>();
            var badgeIdTree = new GH_Structure<GH_String>();
            var titleTree = new GH_Structure<GH_String>();
            var pixelTree = new GH_Structure<GH_Colour>();
            int errorCount = 0;

            foreach (var path in achievementTree.Paths)
            {
                var branch = achievementTree.get_Branch(path);
                foreach (var item in branch)
                {
                    string json = item.ToString();
                    string badgeName = "";
                    string title = "";
                    try
                    {
                        var achievement = JObject.Parse(json);
                        badgeName = (string)achievement["BadgeName"] ?? "";
                        title = (string)achievement["Title"] ?? "";
                    }
                    catch (JsonException) { /* leave badgeName/title empty — reported below */ }

                    if (string.IsNullOrWhiteSpace(badgeName))
                    {
                        errorCount++;
                        continue;
                    }

                    string url = "https://i.retroachievements.org/Badge/" + badgeName + ".png";
                    string destPath = Path.Combine(tempDir, badgeName + ".png");

                    var (ok, resultPath, err) = Task.Run(() => _downloader.DownloadImageToFileAsync(url, destPath)).GetAwaiter().GetResult();
                    if (!ok)
                    {
                        errorCount++;
                        continue;
                    }

                    pathTree.Append(new GH_String(resultPath), path);
                    badgeIdTree.Append(new GH_String(badgeName), path);
                    titleTree.Append(new GH_String(title), path);

                    try
                    {
                        using (var badgeImg = new Bitmap(resultPath))
                            ImagePixelHelpers.AppendPixelRows(pixelTree, badgeImg, path);
                    }
                    catch (Exception) { /* image unreadable — pixel colors just stay empty for this badge */ }
                }
            }

            string status = errorCount == 0 ? "OK" : "OK — " + errorCount + " badge(s) failed to download";

            // 5. DA.SetData calls
            DA.SetDataTree(0, pathTree);
            DA.SetDataTree(1, badgeIdTree);
            DA.SetDataTree(2, titleTree);
            DA.SetDataTree(3, pixelTree);
            DA.SetData(4, status);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_RABadges;
        public override Guid ComponentGuid => new Guid("3c4c229b-58d9-4e3b-98f6-d0e654999e02");
    }
}
