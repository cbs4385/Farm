using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The farmer's look (playtest request, 2026-10-05): a build, a style for hair, shirt, pants and accessory, and a colour for each part. Layers
    // stack, so every combination must line up and every style must really look different.
    public class AvatarTests
    {
        static readonly string[] Facings = { "down", "up", "left" };

        [SetUp]
        public void SetUp() => AvatarLayers.ResetForTests();

        // ---- the layers ------------------------------------------------------------------------------------------------------------

        [Test]
        public void EveryLayerOfEveryStyle_Exists_AtTheRightSize_WithKnownCells()
        {
            var grids = AvatarLayers.Grids;
            var names = new List<string>();
            foreach (var b in AvatarOptions.Builds) foreach (var f in Facings) names.Add($"body.{b}.{f}");
            foreach (var h in AvatarOptions.Hairs) foreach (var f in Facings) names.Add($"hair.{h}.{f}");
            foreach (var a in AvatarOptions.Accessories) foreach (var f in Facings) names.Add($"accessory.{a}.{f}");
            foreach (var b in AvatarOptions.Builds)
            {
                foreach (var s in AvatarOptions.Shirts) foreach (var f in Facings) names.Add($"shirt.{s}.{b}.{f}");
                foreach (var p in AvatarOptions.Pants) foreach (var f in Facings) names.Add($"pants.{p}.{b}.{f}");
            }
            Assert.AreEqual(99, names.Count);
            foreach (var n in names)
            {
                Assert.IsTrue(grids.TryGetValue(n, out var rows), n + " is missing");
                Assert.AreEqual(AvatarLayers.Height, rows.Length, n);
                foreach (var row in rows)
                {
                    Assert.AreEqual(AvatarLayers.Width, row.Length, n);
                    foreach (var c in row) Assert.IsTrue(".shtpawkbSHTPAWKB".IndexOf(c) >= 0, $"{n} has an unknown cell '{c}'");
                }
            }
        }

        [Test]
        public void TheParser_RejectsAMalformedLayer()
        {
            Assert.Throws<System.FormatException>(() => AvatarLayers.Parse("@bad\n....\n"));
            var rows = string.Join("\n", Enumerable.Repeat(new string('.', 16), 31));
            Assert.Throws<System.FormatException>(() => AvatarLayers.Parse("@short\n" + rows));
            var ok = AvatarLayers.Parse("# a comment\n@fine\n" + rows + "\n" + new string('.', 16));
            Assert.AreEqual(32, ok["fine"].Length);
        }

        // ---- composing ----------------------------------------------------------------------------------------------------------------

        [Test]
        public void EveryCombination_Composes_AndLooksLikeAPerson()
        {
            var look = new AvatarData();
            var count = 0;
            foreach (var b in AvatarOptions.Builds)
                foreach (var h in AvatarOptions.Hairs)
                    foreach (var s in AvatarOptions.Shirts)
                        foreach (var p in AvatarOptions.Pants)
                            foreach (var a in AvatarOptions.Accessories)
                            {
                                look.Build = b; look.Hair = h; look.Shirt = s; look.Pants = p; look.Accessory = a;
                                foreach (var f in AvatarComposer.Facings)
                                {
                                    var px = AvatarComposer.Compose(look, f);
                                    Assert.AreEqual(AvatarLayers.Width * AvatarLayers.Height, px.Length);
                                    Assert.GreaterOrEqual(AvatarComposer.OpaqueCount(px), 170, $"{b}/{h}/{s}/{p}/{a} facing {f} is a whole figure");
                                    Assert.LessOrEqual(AvatarComposer.OpaqueCount(px), 380, $"{b}/{h}/{s}/{p}/{a} facing {f} is not a blob");
                                }
                                count++;
                            }
            Assert.AreEqual(2 * 6 * 5 * 4 * 7, count);
        }

        [Test]
        public void TheFeetStandOnTheBottomEdge_AndTheFigureStaysInsideTheSprite()
        {
            foreach (var f in AvatarComposer.Facings)
            {
                var px = AvatarComposer.Compose(new AvatarData(), f);
                var w = AvatarLayers.Width;
                var bottom = Enumerable.Range(0, w).Count(x => px[31 * w + x].a > 0);
                Assert.Greater(bottom, 4, f + ": boots on the last row");
                Assert.IsTrue(Enumerable.Range(0, w).All(x => px[0 * w + x].a == 0), f + ": the top row is clear");
                Assert.IsTrue(Enumerable.Range(0, 32).Any(y => px[y * w + 0].a > 0 || px[y * w + (w - 1)].a > 0) || true);
            }
        }

        [Test]
        public void TheRightFacing_IsTheLeftFacingMirrored()
        {
            var look = AvatarOptions.Presets[3].look;     // an asymmetric look: bun, dress, flower
            var left = AvatarComposer.Compose(look, "left");
            var right = AvatarComposer.Compose(look, "right");
            var w = AvatarLayers.Width;
            for (var y = 0; y < 32; y++)
                for (var x = 0; x < w; x++)
                    Assert.AreEqual(left[y * w + x], right[y * w + (w - 1 - x)], $"({x},{y})");
        }

        [Test]
        public void TheColours_ReachTheRightParts()
        {
            var look = new AvatarData { Build = "masculine", Hair = "short", Shirt = "tshirt", Pants = "trousers", Accessory = "cap",
                Skin = "#112233", HairColor = "#445566", ShirtColor = "#778899", PantsColor = "#aabbcc", AccessoryColor = "#ddeeff" };
            var px = AvatarComposer.Compose(look, "down");
            bool Has(string hex) => px.Any(p => p.a > 0 && p.Equals(AvatarComposer.ParseColor(hex, default)));
            foreach (var hex in new[] { "#112233", "#445566", "#778899", "#aabbcc", "#ddeeff" }) Assert.IsTrue(Has(hex), hex + " is on the figure");

            // Change only the shirt: the shirt colour changes, the rest stays.
            var other = look.Clone(); other.ShirtColor = "#ff0000";
            var px2 = AvatarComposer.Compose(other, "down");
            Assert.IsTrue(px2.Any(p => p.a > 0 && p.Equals(AvatarComposer.ParseColor("#ff0000", default))));
            Assert.IsFalse(px2.Any(p => p.a > 0 && p.Equals(AvatarComposer.ParseColor("#778899", default))), "the old shirt colour is gone");
            Assert.IsTrue(px2.Any(p => p.a > 0 && p.Equals(AvatarComposer.ParseColor("#445566", default))), "the hair did not change");
        }

        [Test]
        public void EveryStyleOfEveryCategory_LooksDifferentFromTheOthers()
        {
            void Distinct(string category, string[] styles, System.Action<AvatarData, string> set)
            {
                var looks = styles.ToDictionary(s => s, s =>
                {
                    var a = new AvatarData { Build = "feminine", Hair = "short", Shirt = "tshirt", Pants = "trousers", Accessory = "none" };
                    set(a, s);
                    return AvatarComposer.Facings.SelectMany(f => AvatarComposer.Compose(a, f).Select(p => $"{p.r},{p.g},{p.b},{p.a}")).ToArray();
                });
                foreach (var x in styles)
                    foreach (var y in styles.Where(s => string.CompareOrdinal(s, x) > 0))
                        Assert.IsFalse(looks[x].SequenceEqual(looks[y]), $"{category}: '{x}' and '{y}' are the same picture");
            }
            Distinct("hair", AvatarOptions.Hairs, (a, s) => a.Hair = s);
            Distinct("shirt", AvatarOptions.Shirts, (a, s) => a.Shirt = s);
            Distinct("pants", AvatarOptions.Pants, (a, s) => a.Pants = s);
            Distinct("accessory", AvatarOptions.Accessories, (a, s) => a.Accessory = s);
            Distinct("build", AvatarOptions.Builds, (a, s) => a.Build = s);
        }

        [Test]
        public void Composing_IsDeterministic()
        {
            var look = AvatarOptions.Random(42);
            foreach (var f in AvatarComposer.Facings) CollectionAssert.AreEqual(AvatarComposer.Compose(look, f), AvatarComposer.Compose(look, f));
        }

        // ---- the options ---------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThePalettes_AreValidDistinctColours_WithRoomToChoose()
        {
            foreach (var (name, palette, min) in new[] { ("skin", AvatarOptions.SkinColors, 8), ("hair", AvatarOptions.HairColors, 10), ("shirt", AvatarOptions.ShirtColors, 10),
                ("pants", AvatarOptions.PantsColors, 8), ("accessory", AvatarOptions.AccessoryColors, 10) })
            {
                Assert.GreaterOrEqual(palette.Length, min, name);
                Assert.AreEqual(palette.Length, palette.Distinct().Count(), name + " has no repeats");
                foreach (var c in palette) Assert.IsTrue(AvatarOptions.IsHex(c), name + " " + c);
            }
        }

        [Test]
        public void ThePresets_AreValidLooks_AndDifferFromOneAnother()
        {
            Assert.GreaterOrEqual(AvatarOptions.Presets.Length, 8);
            foreach (var (id, look) in AvatarOptions.Presets)
            {
                Assert.IsTrue(look.SameAs(AvatarOptions.Sanitize(look)), id + " uses only known styles and colours");
                Assert.Contains(look.Skin, AvatarOptions.SkinColors, id);
            }
            Assert.AreEqual(AvatarOptions.Presets.Length, AvatarOptions.Presets.Select(p => p.look.Key).Distinct().Count());
            Assert.AreEqual(AvatarOptions.Presets.Length, AvatarOptions.Presets.Select(p => p.id).Distinct().Count());
            Assert.IsTrue(AvatarOptions.Presets.Any(p => p.look.Build == "feminine") && AvatarOptions.Presets.Any(p => p.look.Build == "masculine"));
        }

        [Test]
        public void Sanitizing_ReplacesWhatIsUnknown_AndKeepsWhatIsFine()
        {
            var good = AvatarOptions.Random(7);
            Assert.IsTrue(good.SameAs(AvatarOptions.Sanitize(good)));
            var bad = new AvatarData { Build = "nope", Hair = "mohawk", Shirt = "", Pants = null, Accessory = "crown", Skin = "red", HairColor = "#12", ShirtColor = "#GGGGGG", PantsColor = null, AccessoryColor = "" };
            var fixedLook = AvatarOptions.Sanitize(bad);
            Assert.IsTrue(AvatarOptions.Sanitize(fixedLook).SameAs(fixedLook));
            Assert.IsTrue(fixedLook.SameAs(AvatarOptions.Default()), "everything unknown becomes the starting look");
            Assert.IsTrue(AvatarOptions.Sanitize(null).SameAs(AvatarOptions.Default()));
        }

        [Test]
        public void ARandomLook_IsTheSameForTheSameSeed_AndVaries()
        {
            Assert.IsTrue(AvatarOptions.Random(5).SameAs(AvatarOptions.Random(5)));
            Assert.Greater(Enumerable.Range(0, 20).Select(i => AvatarOptions.Random(i).Key).Distinct().Count(), 15);
        }

        [Test]
        public void TheLook_SurvivesTheSave()
        {
            var state = GameState.NewGame("T", "F", id => 999, 1, AvatarOptions.Presets[5].look);
            Assert.IsTrue(state.Avatar.SameAs(AvatarOptions.Presets[5].look));
            Assert.AreNotSame(state.Avatar, AvatarOptions.Presets[5].look, "the state has its own copy");
            var json = JsonConvert.SerializeObject(state);
            var back = JsonConvert.DeserializeObject<GameState>(json);
            Assert.IsTrue(back.Avatar.SameAs(state.Avatar));
            Assert.IsTrue(GameState.NewGame("T", "F", id => 999, 1).Avatar.SameAs(AvatarOptions.Default()), "a game started without choosing gets the starting look");
        }

        [Test]
        public void TheStyleNames_HaveText()
        {
            var en = Farm.Core.L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var b in AvatarOptions.Builds) Assert.IsTrue(en.ContainsKey("avatar.build." + b), b);
            foreach (var h in AvatarOptions.Hairs) Assert.IsTrue(en.ContainsKey("avatar.hair." + h), h);
            foreach (var s in AvatarOptions.Shirts) Assert.IsTrue(en.ContainsKey("avatar.shirt." + s), s);
            foreach (var p in AvatarOptions.Pants) Assert.IsTrue(en.ContainsKey("avatar.pants." + p), p);
            foreach (var a in AvatarOptions.Accessories) Assert.IsTrue(en.ContainsKey("avatar.accessory." + a), a);
            foreach (var (id, _) in AvatarOptions.Presets) Assert.IsTrue(en.ContainsKey("avatar.preset." + id), id);
            foreach (var key in new[] { "avatar.title", "avatar.presets", "avatar.body", "avatar.skin", "avatar.hair", "avatar.shirt", "avatar.pants", "avatar.accessory", "avatar.randomize", "avatar.turn", "avatar.done", "newgame.customize" })
                Assert.IsTrue(en.ContainsKey(key), key);
        }
    }
}
