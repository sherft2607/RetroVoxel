using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Downloads one of a game's four artwork images (Icon/Title/Ingame/BoxArt) and its pixel
    // colors — the game-level counterpart to RA Badges, so a whole game's artwork can be
    // voxelized/mosaicked the same way an achievement badge can, not just achievements.
    public class RAGameImagesComponent : GH_Component
    {
        private static readonly RetroVoxelClient _downloader = new RetroVoxelClient("");

        public RAGameImagesComponent()
          : base("RA Game Images", "GIMG",
              "Downloads a game's Icon, Title, Ingame, or Box Art image and its pixel colors.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAIngest)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Game JSON", "GJ", "Game metadata from RA Game Info.", GH_ParamAccess.item);
            pManager.AddTextParameter("Image Type", "IT", "One of: Icon, Title, Ingame, BoxArt.", GH_ParamAccess.item, "Icon");
            pManager.AddIntegerParameter("Max Dimension", "MD", "Downsamples the image so its longer side is at most this many pixels before reading colors (0 = no resize). Box art and title screens are much higher resolution than badges — voxelizing every literal pixel of a 300-600px image can hang or crash Rhino, so this defaults on.", GH_ParamAccess.item, 64);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Image Path", "IP", "Downloaded image path.", GH_ParamAccess.item);
            pManager.AddColourParameter("Pixel Colors", "PC", "One branch per row of pixels.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Status", "S", "OK, or the download error.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string gameJson = "";
            string imageType = "Icon";
            int maxDimension = 64;

            // 2. DA.GetData calls
            DA.GetData(0, ref gameJson);
            DA.GetData(1, ref imageType);
            DA.GetData(2, ref maxDimension);

            // 3. Guard clauses
            if (string.IsNullOrWhiteSpace(gameJson))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Game JSON is empty.");
                return;
            }
            string field = ResolveField(imageType);
            if (field == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Image Type must be one of: Icon, Title, Ingame, BoxArt.");
                return;
            }

            // 4. Logic
            string relativePath;
            try
            {
                var game = JObject.Parse(gameJson);
                relativePath = (string)game[field] ?? "";
            }
            catch (JsonException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Game JSON was not valid: " + ex.Message);
                return;
            }
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Game JSON has no " + field + " field.");
                return;
            }

            string url = "https://i.retroachievements.org" + relativePath;
            var tempDir = Path.Combine(Path.GetTempPath(), "RetroVoxel", "gameimages");
            Directory.CreateDirectory(tempDir);
            string destPath = Path.Combine(tempDir, Path.GetFileName(relativePath));

            var (ok, resultPath, err) = Task.Run(() => _downloader.DownloadImageToFileAsync(url, destPath)).GetAwaiter().GetResult();
            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(2, "Error: " + err);
                return;
            }

            var pixelTree = new GH_Structure<GH_Colour>();
            string outputPath = resultPath;
            try
            {
                using (var original = new Bitmap(resultPath))
                {
                    if (maxDimension > 0 && Math.Max(original.Width, original.Height) > maxDimension)
                    {
                        using (var resized = Downsample(original, maxDimension))
                        {
                            outputPath = Path.Combine(Path.GetDirectoryName(resultPath),
                                Path.GetFileNameWithoutExtension(resultPath) + "_" + maxDimension + "px.png");
                            resized.Save(outputPath, ImageFormat.Png);
                            ImagePixelHelpers.AppendPixelRows(pixelTree, resized, new GH_Path());
                        }
                    }
                    else
                    {
                        ImagePixelHelpers.AppendPixelRows(pixelTree, original, new GH_Path());
                    }
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Downloaded but could not read pixels: " + ex.Message);
            }

            // 5. DA.SetData calls
            DA.SetData(0, outputPath);
            DA.SetDataTree(1, pixelTree);
            DA.SetData(2, "OK");
        }

        private static Bitmap Downsample(Bitmap source, int maxDimension)
        {
            double scale = (double)maxDimension / Math.Max(source.Width, source.Height);
            int newWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
            int newHeight = Math.Max(1, (int)Math.Round(source.Height * scale));

            var resized = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, newWidth, newHeight);
            }
            return resized;
        }

        private static string ResolveField(string imageType)
        {
            switch ((imageType ?? "").Trim().ToLowerInvariant())
            {
                case "icon": return "ImageIcon";
                case "title": return "ImageTitle";
                case "ingame": return "ImageIngame";
                case "boxart": return "ImageBoxArt";
                default: return null;
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_RAGameImages;
        public override Guid ComponentGuid => new Guid("0624fb1d-6ba7-41ae-b0b6-9fbcfa51a0e7");
    }
}
