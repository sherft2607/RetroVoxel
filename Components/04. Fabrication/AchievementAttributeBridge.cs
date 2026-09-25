using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    // Mesh goo that carries achievement metadata as real Rhino object UserText on bake — visible
    // via _What, the Properties panel, and _UserText, unlike plain geometry UserDictionary.
    public class GH_TaggedMesh : GH_Mesh, IGH_BakeAwareData
    {
        private readonly Dictionary<string, string> _userText;

        public GH_TaggedMesh(Mesh mesh, Dictionary<string, string> userText) : base(mesh)
        {
            _userText = userText ?? new Dictionary<string, string>();
        }

        bool IGH_BakeAwareData.BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            var attributes = att != null ? att.Duplicate() : doc.CreateDefaultAttributes();
            foreach (var kv in _userText)
                attributes.SetUserString(kv.Key, kv.Value);

            obj_guid = doc.Objects.AddMesh(Value, attributes);
            return obj_guid != Guid.Empty;
        }
    }

    // BIM Data Bridge (Gate 3 AEC feature): writes an achievement's Title/Points/Description/
    // BadgeName into its matching voxel mesh's UserDictionary (geometry-level, always present)
    // AND as bake-time UserText (visible in Rhino's Properties panel / _UserText / _What once
    // baked) so a fabricated plaque batch stays self-documenting after leaving the GH tree.
    public class AchievementAttributeBridgeComponent : GH_Component
    {
        public AchievementAttributeBridgeComponent()
          : base("Achievement Attribute Bridge", "BRIDGE",
              "Writes achievement metadata into a voxel mesh's UserDictionary and bake-time UserText.",
              PluginUtilities.TabName, PluginUtilities.CategoryADFabrication)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Voxel Mesh", "VM", "Mesh from Voxel Mesh Shell.", GH_ParamAccess.item);
            pManager.AddTextParameter("Achievement JSON", "AJ", "Achievement record from RA Game Info.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Tagged Mesh", "TM", "Mesh carrying achievement metadata (UserDictionary + bake-time UserText).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            Mesh mesh = null;
            string achievementJson = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref achievementJson);

            // 3. Guard clauses
            if (mesh == null || !mesh.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Voxel Mesh is missing or invalid.");
                return;
            }

            // 4. Logic
            var tagged = mesh.DuplicateMesh();
            var userText = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrWhiteSpace(achievementJson))
                {
                    var achievement = JObject.Parse(achievementJson);
                    string title = (string)achievement["Title"] ?? "";
                    string points = ((int)(achievement["Points"] ?? 0)).ToString();
                    string description = (string)achievement["Description"] ?? "";
                    string badgeName = (string)achievement["BadgeName"] ?? "";

                    tagged.UserDictionary.Set("Title", title);
                    tagged.UserDictionary.Set("Points", int.Parse(points));
                    tagged.UserDictionary.Set("Description", description);
                    tagged.UserDictionary.Set("BadgeName", badgeName);

                    userText["Title"] = title;
                    userText["Points"] = points;
                    userText["Description"] = description;
                    userText["BadgeName"] = badgeName;
                }
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Achievement JSON was not valid — mesh passed through untagged.");
            }

            // 5. DA.SetData calls
            DA.SetData(0, new GH_TaggedMesh(tagged, userText));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RVX_AchievementAttributeBridge;
        public override Guid ComponentGuid => new Guid("6f38dc89-01ee-41b3-908b-d8d66e6b7972");
    }
}
