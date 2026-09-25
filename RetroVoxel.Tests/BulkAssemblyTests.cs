using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RetroVoxel.Tests
{
    // Pixel Grid Builder is this plugin's aggregator: a tree of rows -> one assembled Pixel Grid JSON
    // (bounds, pitch, thickness, palette, indices). These tests prove the tree -> grid assembly
    // behaves like pr-bulk-data expects of any aggregator: row count preserved, ragged rows handled
    // without throwing, index alignment preserved.
    public class BulkAssemblyTests
    {
        [Fact]
        public void Assemble_RowCountIn_EqualsRowCountOut_InOrder()
        {
            var rows = new List<IList<string>>
            {
                new List<string> { "#FF0000" },
                new List<string> { "#00FF00" },
                new List<string> { "#0000FF" }
            };

            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(rows));

            Assert.Equal(rows.Count, (int)json["bounds"]["rows"]);
            var grid = PixelGridBuilders.Parse(json.ToString());
            Assert.Equal("#FF0000", PixelGridBuilders.CellAt(grid, 0, 0));
            Assert.Equal("#00FF00", PixelGridBuilders.CellAt(grid, 1, 0));
            Assert.Equal("#0000FF", PixelGridBuilders.CellAt(grid, 2, 0));
        }

        [Fact]
        public void Assemble_RaggedRow_NoThrow_RestOfRowIntact()
        {
            var rows = new List<IList<string>>
            {
                new List<string> { "#111111", "#222222", "#333333" },
                new List<string> { "#AAAAAA" } // ragged — fewer cells than the widest row
            };

            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(rows));

            Assert.Equal(2, (int)json["bounds"]["rows"]);
            Assert.Equal(3, (int)json["bounds"]["cols"]); // widest row sets column count — no data dropped
            var indexRow = (JArray)((JArray)json["indices"])[1];
            Assert.Equal(3, indexRow.Count); // ragged row padded, not truncated
        }

        [Fact]
        public void Assemble_EmptyRows_ReturnsZeroByZeroGrid_NoThrow()
        {
            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(new List<IList<string>>()));

            Assert.Equal(0, (int)json["bounds"]["rows"]);
            Assert.Equal(0, (int)json["bounds"]["cols"]);
        }

        [Fact]
        public void Assemble_IndexAlignment_PositionIPreserved()
        {
            var rows = new List<IList<string>>
            {
                new List<string> { "#000001" },
                new List<string> { "#000002" },
                new List<string> { "#000003" }
            };

            var grid = PixelGridBuilders.Parse(PixelGridBuilders.BuildPixelGrid(rows));

            for (int i = 0; i < rows.Count; i++)
                Assert.Equal(rows[i][0], PixelGridBuilders.CellAt(grid, i, 0));
        }

        [Fact]
        public void Assemble_MixedVoidAndColorRow_EachPositionResolvesIndependently()
        {
            var rows = new List<IList<string>> { new List<string> { "#FF0000", "", "#00FF00", "VOID" } };

            var grid = PixelGridBuilders.Parse(PixelGridBuilders.BuildPixelGrid(rows));

            Assert.Equal("#FF0000", PixelGridBuilders.CellAt(grid, 0, 0));
            Assert.Null(PixelGridBuilders.CellAt(grid, 0, 1));
            Assert.Equal("#00FF00", PixelGridBuilders.CellAt(grid, 0, 2));
            Assert.Null(PixelGridBuilders.CellAt(grid, 0, 3));
        }
    }
}
