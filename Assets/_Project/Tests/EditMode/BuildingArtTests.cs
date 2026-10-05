using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest request (2026-10-05): "each building looks the same". Every village building has its own wall, roof, window, sign and roof
    // ornament (tools/art/build_building_art.py), and no two buildings share any of them.
    public class BuildingArtTests
    {
        static readonly string[] Styles = { "general", "blacksmith", "carpenter", "library", "saloon", "clinic", "hall" };
        static readonly string[] Parts = { "wall", "roof", "window", "sign", "roof_top" };

        static string PathOf(string style, string part) => Path.Combine(Application.dataPath, "_Project", "Art", "Placeholders", $"bld_{style}_{part}.png");

        [Test]
        public void EveryBuildingStyle_HasAllFiveSprites()
        {
            foreach (var style in Styles)
                foreach (var part in Parts)
                    Assert.IsTrue(File.Exists(PathOf(style, part)), $"bld_{style}_{part}.png");
        }

        [Test]
        public void NoTwoBuildings_ShareAWall_ARoof_ASign_OrAnOrnament()
        {
            foreach (var part in Parts)
            {
                var hashes = Styles.Select(s => System.Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(File.ReadAllBytes(PathOf(s, part))))).ToList();
                Assert.AreEqual(Styles.Length, hashes.Distinct().Count(), part + " differs between buildings");
            }
        }

        [Test]
        public void TheVillageBuildings_AreStyledByTheMapBuilder()
        {
            var source = File.ReadAllText(Path.Combine(Application.dataPath, "_Project", "Scripts", "Editor", "MapBuilder.cs"));
            foreach (var style in Styles) StringAssert.Contains($"Style = \"{style}\"", source);
        }
    }
}
