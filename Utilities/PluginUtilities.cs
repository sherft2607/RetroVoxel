using System;
using System.Collections.Generic;
using System.Linq;

namespace RetroVoxel
{
    public static class PluginUtilities
    {
        // Shared enum helper — used by any preset component backed by an enum
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        internal static readonly string TabName = "RetroVoxel";

        // Subcategory constants — one per subcategory, in display order.
        // Zero-padded two-digit numeric prefixes in the display string (Gate 2 resolution) so
        // PluginUtilities.TabName's ribbon sort never breaks once double-digit subcategories exist —
        // the exact issue seen on the PokeData dogfood run with unpadded "1.", "2." ... "10." labels.
        internal static readonly string CategoryAAIngest      = "01. Ingest";
        internal static readonly string CategoryABColor       = "02. Color";
        internal static readonly string CategoryACGrid        = "03. Grid";
        internal static readonly string CategoryADFabrication = "04. Fabrication";
        internal static readonly string CategoryAEMosaic      = "05. Mosaic";
        internal static readonly string CategoryAFPresets     = "06. Presets";
        internal static readonly string CategoryAGUtilities   = "07. Utilities";
    }
}
