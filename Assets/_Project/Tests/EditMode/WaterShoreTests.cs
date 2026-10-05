using System.IO;
using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest report (2026-10-05): "the forest pond is made of a series of pond tiles, and does not look like one continuous body of water."
    public class WaterShoreTests
    {
        const string ArtDir = "Assets/_Project/Art/Placeholders/";

        [Test]
        public void OpenWater_UsesRippleVariants_AndTheShoreTilesNameTheirLand()
        {
            System.Func<int, int, bool> lake = (x, y) => x >= 0 && x < 5 && y >= 0 && y < 5;
            Assert.AreEqual(0, WaterShore.Mask(lake, 2, 2), "the middle has no land beside it");
            Assert.AreEqual(1, WaterShore.Mask(lake, 2, 4), "land to the north");
            Assert.AreEqual(1 | 8, WaterShore.Mask(lake, 0, 4), "north-west corner: land north and west");
            Assert.AreEqual(2 | 4, WaterShore.Mask(lake, 4, 0), "south-east corner: land east and south");
            Assert.IsTrue(WaterShore.TileNameAt(lake, 2, 2).StartsWith("tile_water_m0v"));
            Assert.AreEqual("tile_water_m1", WaterShore.TileNameAt(lake, 2, 4));
        }

        [Test]
        public void AnInsideCorner_IsMarkedOnlyWhereTheTwoSidesAreWater()
        {
            System.Func<int, int, bool> bay = (x, y) => !(x == 3 && y == 3);       // water everywhere but one land cell
            Assert.AreEqual(16, WaterShore.Mask(bay, 2, 2), "land only at the north-east diagonal");
            Assert.AreEqual(2, WaterShore.Mask(bay, 2, 3), "beside it, the land is a plain east side");
        }

        [Test]
        public void EveryWayLandCanSurroundACell_HasArt()
        {
            for (var bits = 0; bits < 256; bits++)
            {
                var land = bits;
                // the eight neighbours of (0,0): bit order N E S W NE SE SW NW
                System.Func<int, int, bool> water = (x, y) =>
                {
                    if (x == 0 && y == 0) return true;
                    var i = x == 0 && y == 1 ? 0 : x == 1 && y == 0 ? 1 : x == 0 && y == -1 ? 2 : x == -1 && y == 0 ? 3 :
                            x == 1 && y == 1 ? 4 : x == 1 && y == -1 ? 5 : x == -1 && y == -1 ? 6 : 7;
                    return (land >> i & 1) == 0;
                };
                var name = WaterShore.TileNameAt(water, 0, 0);
                Assert.IsTrue(File.Exists(ArtDir + name + ".png"), name + " exists (neighbourhood " + bits + ")");
            }
            for (var v = 0; v < WaterShore.Variants; v++) Assert.IsTrue(File.Exists(ArtDir + "tile_water_m0v" + v + ".png"));
        }

        [Test]
        public void AnyWaterTile_CountsAsWater_ButGrassDoesNot()
        {
            Assert.IsTrue(WaterShore.IsWaterTile("tile_water"));
            Assert.IsTrue(WaterShore.IsWaterTile("tile_water_m9"));
            Assert.IsFalse(WaterShore.IsWaterTile("tile_grass"));
            Assert.IsFalse(WaterShore.IsWaterTile(null));
            Assert.AreEqual(50, Directory.GetFiles(ArtDir, "tile_water_m*.png").Length, "47 shore tiles and 4 open-water variants less the shared mask 0");
        }
    }
}
