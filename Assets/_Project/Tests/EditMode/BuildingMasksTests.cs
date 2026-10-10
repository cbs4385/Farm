using System.IO;
using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Resources/BuildingMasks.json (from tools/art/build_building_masks.py): every building picture has a mask that fits it.
    public class BuildingMasksTests
    {
        static string[] Styles() => Directory.GetFiles(Path.Combine(Application.dataPath, "_Project", "Art", "Placeholders"), "prop_bld_*.png")
            .Select(f => Path.GetFileNameWithoutExtension(f).Substring("prop_bld_".Length)).ToArray();

        [Test]
        public void EveryBuildingPicture_HasAMask_WithBlockingCellsBesideItsDoor()
        {
            foreach (var style in Styles())
            {
                var mask = BuildingMasks.Get(style);
                Assert.IsNotNull(mask, style + " has a mask (run tools/art/build_building_masks.py)");
                Assert.Greater(mask.DoorPx, 0, style);
                Assert.IsTrue(mask.Blocks(-1, 0) || mask.Blocks(1, 0), style + " blocks a cell beside its door");
                Assert.IsFalse(mask.Blocks(0, 0), style + ": the door cell itself is open");
                Assert.IsFalse(mask.SolidCells().Any(c => c.y < 0), style);
            }
        }

        [Test]
        public void AMaskIsNoTallerThanItsPicture()
        {
            foreach (var style in Styles())
            {
                var png = new Texture2D(2, 2);
                png.LoadImage(File.ReadAllBytes(Path.Combine(Application.dataPath, "_Project", "Art", "Placeholders", $"prop_bld_{style}.png")));
                Assert.LessOrEqual(BuildingMasks.Get(style).Rows.Count * 16, png.height, style);
                Object.DestroyImmediate(png);
            }
        }
    }
}
