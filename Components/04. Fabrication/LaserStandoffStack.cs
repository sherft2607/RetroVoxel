using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace RetroVoxel
{
    // Builds a stack of laser-cut panels sized to a box-art aspect ratio, spaced by standoffs.
    public class LaserStandoffStackComponent : GH_Component
    {
        private const double PanelWidth = 100.0;
        private const double StandoffGap = 10.0;

        public LaserStandoffStackComponent()
          : base("Laser Standoff Stack", "STACK",
              "Builds an aspect-ratio-scaled stack of laser-cut panels on standoffs.",
              PluginUtilities.TabName, PluginUtilities.CategoryADFabrication)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Aspect Ratio", "AR", "Width / height ratio (e.g. from a game's box art).", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Stack Count", "SC", "Number of stacked panels.", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("Panel Thickness", "PT", "Thickness of each panel.", GH_ParamAccess.item, 3.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Standoff Geometry", "SG", "One flat panel Brep per stack level.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            double aspectRatio = 1.0;
            int stackCount = 4;
            double panelThickness = 3.0;

            // 2. DA.GetData calls
            DA.GetData(0, ref aspectRatio);
            DA.GetData(1, ref stackCount);
            DA.GetData(2, ref panelThickness);

            // 3. Guard clauses
            if (aspectRatio <= 0) aspectRatio = 1.0;
            if (stackCount <= 0) stackCount = 4;
            if (panelThickness <= 0) panelThickness = 3.0;

            // 4. Logic
            double height = PanelWidth / aspectRatio;
            var panels = new List<Brep>();
            double z = 0;
            for (int i = 0; i < stackCount; i++)
            {
                var rect = new Rectangle3d(new Plane(new Point3d(0, 0, z), Vector3d.ZAxis), PanelWidth, height);
                var panelPlane = Brep.CreatePlanarBreps(rect.ToNurbsCurve(), 0.001);
                if (panelPlane != null) panels.AddRange(panelPlane);
                z += panelThickness + StandoffGap;
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, panels);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_LaserStandoffStack;
        public override Guid ComponentGuid => new Guid("880277de-51e3-4b3f-a76d-1851342b9d95");
    }
}
