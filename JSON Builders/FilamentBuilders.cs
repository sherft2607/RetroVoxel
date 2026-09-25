using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    public static class FilamentBuilders
    {
        #region Private helpers

        private static (double L, double a, double b) HexToLab(string hex)
        {
            hex = (hex ?? "#000000").TrimStart('#');
            if (hex.Length < 6) hex = "000000";
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            double Lin(double c)
            {
                c /= 255.0;
                return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            double rl = Lin(r), gl = Lin(g), bl = Lin(b);
            double x = rl * 0.4124 + gl * 0.3576 + bl * 0.1805;
            double y = rl * 0.2126 + gl * 0.7152 + bl * 0.0722;
            double z = rl * 0.0193 + gl * 0.1192 + bl * 0.9505;

            double xn = 0.95047, yn = 1.0, zn = 1.08883;
            double Fx(double t) => t > 0.008856 ? Math.Pow(t, 1.0 / 3.0) : (7.787 * t) + (16.0 / 116.0);

            double fx = Fx(x / xn), fy = Fx(y / yn), fz = Fx(z / zn);
            double L = (116.0 * fy) - 16.0;
            double a = 500.0 * (fx - fy);
            double bb = 200.0 * (fy - fz);
            return (L, a, bb);
        }

        // CIE76 Delta-E — sufficient for filament-swatch matching; CIEDE2000 is not needed at this scale
        private static double DeltaE(string hexA, string hexB)
        {
            var (l1, a1, b1) = HexToLab(hexA);
            var (l2, a2, b2) = HexToLab(hexB);
            return Math.Sqrt(Math.Pow(l1 - l2, 2) + Math.Pow(a1 - a2, 2) + Math.Pow(b1 - b2, 2));
        }

        #endregion

        #region Filament match

        // filament match
        public static string FindNearestFilament(string colorHex, IList<(string name, string hex)> library)
        {
            colorHex = colorHex ?? "#000000";
            string bestName = "Unknown";
            string bestHex = "#000000";
            double bestDelta = double.MaxValue;

            foreach (var (name, hex) in library ?? new List<(string name, string hex)>())
            {
                double delta = DeltaE(colorHex, hex);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    bestName = name;
                    bestHex = hex;
                }
            }

            var match = new JObject
            {
                ["sourceColor"] = colorHex,
                ["filamentName"] = bestName,
                ["filamentColor"] = bestHex,
                ["deltaE"] = Math.Round(bestDelta, 2)
            };
            return match.ToString(Formatting.None);
        }

        #endregion

        #region Parse helpers

        public static JObject Parse(string json)
            => string.IsNullOrWhiteSpace(json) ? null : JObject.Parse(json);

        #endregion
    }
}
