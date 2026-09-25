using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace RetroVoxel
{
    public class RetroVoxelInfo : GH_AssemblyInfo
    {
        public override string Name => "RetroVoxel";
        // Same 16x16 logo as the category icon — shown in Grasshopper's loaded-libraries list
        public override Bitmap Icon => Properties.Resources.RVX_RetroVoxelLogo;
        public override string Description => "Computational design and physical fabrication toolkit for retro gaming pixel art, sprite sheets, and mosaic geometry.";
        public override Guid Id => new Guid("1c8a0532-3f5b-41c8-b260-d4bc65474428");
        public override string AuthorName => "Shandon Herft";
        public override string AuthorContact => "shandonherft@gmail.com";
        // Single source of truth is the .csproj <Version> — never a second hardcoded string here
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString(3);
    }

    public class RetroVoxelCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("RetroVoxel", Properties.Resources.RVX_RetroVoxelLogo);
            Instances.ComponentServer.AddCategorySymbolName("RetroVoxel", 'R');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
