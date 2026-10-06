using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // X-011 / T-044: "at level 0 the game must behave exactly like the base game". The same year is played twice with the same world seed and the
    // same actions, once with no horror layer at all and once with the layer fully installed (its story data, hooks, conditions and effects) but the
    // intensity at off. The complete saved state must be identical.
    public class MythosOffEquivalenceTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [TearDown]
        public void TearDown() => GameSession.HorrorLevelOverride = null;

        static string PlayYear(bool withLayer, int seed, int days, int level = 0)
        {
            GameSession.HorrorLevelOverride = level;
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, NpcDefaults.CreateAll()))
            {
                var s = f.Session;
                s.State.WorldSeed = seed;
                s.Story = StoryContent.LoadFromResources();
                foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder)) s.Db.Merge(pack);
                if (withLayer) MythosModule.Install(s, s.Hooks, f.Bus);
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;       // a talk with no screen to show it logs; the same in both runs
                try
                {
                    s.Db.TryGetCrop("parsnip", out var parsnip);
                    var grid = s.GetGrid(MapIds.Greenhouse);
                    for (var d = 1; d <= days; d++)
                    {
                        // The same farming every day: till, plant, water and harvest a plot.
                        for (var i = 0; i < 24; i++)
                        {
                            int x = i % 6, y = i / 6;
                            grid.Till(x, y);
                            if (grid.TryGetTile(x, y, out var tile) && tile.Crop == null) grid.Plant(x, y, parsnip, s.Clock.Now.Season);
                            grid.Water(x, y);
                            if (grid.TryHarvest(x, y, id => parsnip, out var got)) s.GiveItem(got.ItemId);
                        }
                        // The same chats: every villager is talked to, and a gift is given now and then.
                        foreach (var npc in s.Npcs.All) NpcInteractions.Talk(s, npc);
                        if (d % 5 == 0) s.ChangeGold(d);
                        // Letters are read as they arrive.
                        for (var guard = 0; guard < 5; guard++)
                        {
                            var letter = Mail.Next(s);
                            if (letter == null) break;
                            Mail.Finish(s, letter);
                        }
                        s.EndDay(false);
                    }
                }
                finally { UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false; }
                return JsonConvert.SerializeObject(s.State);
            }
        }

        static string FirstDifference(string a, string b)
        {
            var n = System.Math.Min(a.Length, b.Length);
            var i = 0;
            while (i < n && a[i] == b[i]) i++;
            var from = System.Math.Max(0, i - 80);
            return $"at character {i}: \"...{a.Substring(from, System.Math.Min(160, a.Length - from))}\" versus \"...{b.Substring(from, System.Math.Min(160, b.Length - from))}\"";
        }

        [TestCase(1)]
        [TestCase(2)]
        public void AYearWithTheLayerInstalledButOff_IsIdenticalToAYearWithoutIt(int seed)
        {
            var without = PlayYear(false, seed * 104729 + 3, 4 * 28);
            var with = PlayYear(true, seed * 104729 + 3, 4 * 28);
            Assert.IsNotEmpty(without);
            if (without != with) Assert.Fail("the saved state differs " + FirstDifference(without, with));
        }

        [Test]
        public void TheSameYear_AtFullIntensity_DoesDiffer_SoTheComparisonCanFail()
        {
            var off = PlayYear(true, 104732, 4 * 28, level: 0);
            var full = PlayYear(true, 104732, 4 * 28, level: 2);
            Assert.AreNotEqual(off, full, "the layer changes the saved state when it is on");
        }

        [Test]
        public void TheYear_IsNotTrivial_SoTheComparisonMeansSomething()
        {
            var state = JsonConvert.DeserializeObject<GameState>(PlayYear(false, 424243, 4 * 28));
            Assert.GreaterOrEqual(state.Year, 2, "a full year passed");
            Assert.Greater(state.Npcs.Count, 0, "villagers were talked to");
            Assert.Greater(state.Gold, 500, "gold changed");
            Assert.Greater(state.Maps.Values.Sum(m => m.Tiles.Count), 0, "crops were farmed");
        }
    }
}
