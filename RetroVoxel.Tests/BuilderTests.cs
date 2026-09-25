using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RetroVoxel.Tests
{
    public class BuilderTests
    {
        public static IEnumerable<object[]> PixelGridCases()
        {
            yield return new object[] { new List<IList<string>> { new List<string> { "#FF0000", "#00FF00" } }, 1, 2 };
            yield return new object[] { new List<IList<string>> { new List<string> { "#FF0000" }, new List<string> { "#0000FF", "#FFFFFF" } }, 2, 2 };
        }

        [Theory]
        [MemberData(nameof(PixelGridCases))]
        public void BuildPixelGrid_EmitsExpectedShape(List<IList<string>> rows, int expectedRows, int expectedCols)
        {
            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(rows));

            Assert.True(json.ContainsKey("bounds"));
            Assert.True(json.ContainsKey("pitch"));
            Assert.True(json.ContainsKey("thickness"));
            Assert.True(json.ContainsKey("palette"));
            Assert.True(json.ContainsKey("indices"));
            Assert.Equal(expectedRows, (int)json["bounds"]["rows"]);
            Assert.Equal(expectedCols, (int)json["bounds"]["cols"]);
        }

        [Fact]
        public void BuildPixelGrid_NullRows_DoesNotThrow()
        {
            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(null));
            Assert.Equal(0, (int)json["bounds"]["rows"]);
        }

        [Fact]
        public void BuildPixelGrid_DedupesPalette_InFirstSeenOrder()
        {
            var rows = new List<IList<string>>
            {
                new List<string> { "#FF0000", "#00FF00", "#FF0000" }
            };
            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(rows));
            var palette = (JArray)json["palette"];

            Assert.Equal(2, palette.Count); // deduped: red, green
            Assert.Equal("#FF0000", (string)palette[0]);
            Assert.Equal("#00FF00", (string)palette[1]);

            var indexRow = (JArray)((JArray)json["indices"])[0];
            Assert.Equal(0, (int)indexRow[0]);
            Assert.Equal(1, (int)indexRow[1]);
            Assert.Equal(0, (int)indexRow[2]); // repeats the red palette entry
        }

        [Fact]
        public void BuildPixelGrid_VoidCell_GetsIndexNegativeOne_AndIsExcludedFromPalette()
        {
            var rows = new List<IList<string>> { new List<string> { "#FF0000", "", "VOID" } };
            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(rows));

            var palette = (JArray)json["palette"];
            Assert.Single(palette); // only the red cell

            var indexRow = (JArray)((JArray)json["indices"])[0];
            Assert.Equal(0, (int)indexRow[0]);
            Assert.Equal(-1, (int)indexRow[1]);
            Assert.Equal(-1, (int)indexRow[2]);
        }

        [Fact]
        public void BuildPixelGrid_RaggedRow_PadsWithLastCellValue()
        {
            var rows = new List<IList<string>>
            {
                new List<string> { "#FF0000", "#00FF00", "#0000FF" },
                new List<string> { "#FFFFFF" }
            };
            var json = JObject.Parse(PixelGridBuilders.BuildPixelGrid(rows));
            var indexRow = (JArray)((JArray)json["indices"])[1];

            Assert.Equal(3, indexRow.Count); // padded, not truncated
            var palette = (JArray)json["palette"];
            string paddedHex = (string)palette[(int)indexRow[2]];
            Assert.Equal("#FFFFFF", paddedHex); // padded with the last known cell value
        }

        [Fact]
        public void CellAt_OutOfRangeOrVoid_ReturnsNull_NoThrow()
        {
            var grid = PixelGridBuilders.Parse(PixelGridBuilders.BuildPixelGrid(
                new List<IList<string>> { new List<string> { "#123456", "" } }));

            Assert.Equal("#123456", PixelGridBuilders.CellAt(grid, 0, 0));
            Assert.Null(PixelGridBuilders.CellAt(grid, 0, 1));  // void cell
            Assert.Null(PixelGridBuilders.CellAt(grid, 5, 5));  // out of range
            Assert.Null(PixelGridBuilders.CellAt(null, 0, 0));
        }

        public static IEnumerable<object[]> FilamentLibraries()
        {
            yield return new object[]
            {
                "#FF0000",
                new List<(string name, string hex)> { ("Red PLA", "#FF0000"), ("Blue PLA", "#0000FF") },
                "Red PLA"
            };
            yield return new object[]
            {
                "#010101",
                new List<(string name, string hex)> { ("Black PLA", "#000000"), ("White PLA", "#FFFFFF") },
                "Black PLA"
            };
        }

        [Theory]
        [MemberData(nameof(FilamentLibraries))]
        public void FindNearestFilament_PicksClosestMatch(string colorHex, List<(string name, string hex)> library, string expectedName)
        {
            var json = JObject.Parse(FilamentBuilders.FindNearestFilament(colorHex, library));

            Assert.True(json.ContainsKey("sourceColor"));
            Assert.True(json.ContainsKey("filamentName"));
            Assert.True(json.ContainsKey("filamentColor"));
            Assert.True(json.ContainsKey("deltaE"));
            Assert.Equal(expectedName, (string)json["filamentName"]);
        }

        [Fact]
        public void FindNearestFilament_EmptyLibrary_DoesNotThrow()
        {
            var json = JObject.Parse(FilamentBuilders.FindNearestFilament("#ABCDEF", new List<(string name, string hex)>()));
            Assert.Equal("Unknown", (string)json["filamentName"]);
        }
    }
}
