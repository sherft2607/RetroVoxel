using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;

namespace RetroVoxel
{
    // Shared by every image-ingesting component (Local Sprite Loader, Sprite Sheet Splitter,
    // RA Badges) so each one exposes the same Pixel Colors DataTree shape: one row per branch,
    // nested under any caller-supplied prefix path (e.g. per-frame, per-badge).
    internal static class ImagePixelHelpers
    {
        public static void AppendPixelRows(GH_Structure<GH_Colour> tree, Bitmap bmp, GH_Path prefix)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                var rowPath = prefix.AppendElement(y);
                for (int x = 0; x < bmp.Width; x++)
                    tree.Append(new GH_Colour(bmp.GetPixel(x, y)), rowPath);
            }
        }
    }
}
