using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RetroVoxel
{
    public static class PixelGridBuilders
    {
        #region Private helpers

        // Longest row defines column count; short rows are padded with the last cell's value
        private static int MaxRowLength(IList<IList<string>> rows)
        {
            int max = 0;
            foreach (var row in rows) if (row.Count > max) max = row.Count;
            return max;
        }

        private static bool IsVoid(string hex)
            => string.IsNullOrEmpty(hex) || hex.Equals("VOID", System.StringComparison.OrdinalIgnoreCase);

        #endregion

        #region Pixel grid

        // pixel grid
        // Rows carry raw per-cell hex colors (or null/"" /"VOID" for a transparent/void cell).
        // Builds a deduplicated palette (first-seen order) and an index array referencing it —
        // -1 marks a void cell. pitch/thickness are informational metadata only (e.g. a caller's
        // intended voxel/stud pitch and material thickness); consumers may ignore them and use
        // their own size inputs instead.
        public static string BuildPixelGrid(IList<IList<string>> rows, double pitch = 2.0, double thickness = 1.0)
        {
            rows = rows ?? new List<IList<string>>();
            int cols = MaxRowLength(rows);

            var palette = new List<string>();
            var paletteIndex = new Dictionary<string, int>();
            var indices = new JArray();

            foreach (var row in rows)
            {
                var indexRow = new JArray();
                string last = null;
                for (int c = 0; c < cols; c++)
                {
                    string hex = c < row.Count ? row[c] : last;
                    last = hex;

                    if (IsVoid(hex))
                    {
                        indexRow.Add(-1);
                        continue;
                    }

                    if (!paletteIndex.TryGetValue(hex, out int idx))
                    {
                        idx = palette.Count;
                        palette.Add(hex);
                        paletteIndex[hex] = idx;
                    }
                    indexRow.Add(idx);
                }
                indices.Add(indexRow);
            }

            var grid = new JObject
            {
                ["bounds"] = new JObject { ["rows"] = rows.Count, ["cols"] = cols },
                ["pitch"] = pitch,
                ["thickness"] = thickness,
                ["palette"] = new JArray(palette),
                ["indices"] = indices
            };
            return grid.ToString(Formatting.None);
        }

        #endregion

        #region Parse helpers

        public static JObject Parse(string json)
            => string.IsNullOrWhiteSpace(json) ? null : JObject.Parse(json);

        // Safe per-position lookup into a Pixel Grid JSON — resolves indices[row][col] against
        // palette. Returns null for a void cell (-1) or an out-of-range position, so every
        // Fabrication/Mosaic consumer can treat "no color" as "skip this cell" uniformly, even
        // mid-batch, without throwing.
        public static string CellAt(JObject grid, int row, int col)
        {
            if (grid == null) return null;
            var bounds = grid["bounds"] as JObject;
            var indices = grid["indices"] as JArray;
            var palette = grid["palette"] as JArray;
            if (bounds == null || indices == null || palette == null) return null;
            if (row < 0 || row >= indices.Count) return null;

            var indexRow = indices[row] as JArray;
            if (indexRow == null || indexRow.Count == 0) return null;
            int clampedCol = col < 0 ? 0 : (col >= indexRow.Count ? indexRow.Count - 1 : col);

            int idx = (int)indexRow[clampedCol];
            if (idx < 0 || idx >= palette.Count) return null;
            return (string)palette[idx];
        }

        #endregion
    }
}
