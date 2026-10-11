using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The reports of 2026-10-11: advice about picking plants and shells is said by Dorian and Felix, and the shop counter does not promise selling.
    public class PlaytestOct11Tests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => new GameDateTime(1, Season.Spring, 10, 12 * 60);
            public string Weather => "sunny";
            public string MapId => "Village";
            public int Hearts(string npcId) => 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        [TestCase("dorian", "leave a few")]
        [TestCase("felix", "leave some")]
        public void TheForagerAndTheFisherman_GiveTheAdvice_WhenFirstMet_AndAmongTheirTalk(string npc, string words)
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            StringAssert.Contains(words, L.Get($"dlg.{npc}.tip.0").ToLowerInvariant());
            var roster = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Story/npc_roster").text;
            var first = roster.IndexOf($"\"id\": \"{npc}.first\"");
            var next = roster.IndexOf($"\"id\": \"{npc}.chat1\"");
            StringAssert.Contains($"dlg.{npc}.tip.0", roster.Substring(first, next - first), "the introduction ends with it");
            var set = StoryContent.LoadFromResources().Set($"npc.{npc}.talk");
            var w = new World();
            w.Flags.Add("met." + npc);
            var heard = new HashSet<string>();
            for (var seed = 0; seed < 400; seed++) heard.Add(set.Pick(w, seed));
            CollectionAssert.Contains(heard, npc + ".chat4", "and anyone who met them earlier can hear it in ordinary talk");
        }

        [Test]
        public void TheShopCounterLabel_PromisesNoSelling() => StringAssert.DoesNotContain("buy and sell", L.Get("hover.shop"));

        [Test]
        public void AFurnace_SaysWhatItNeeds()
        {
            var recipes = new RecipeCatalog(CraftingDefaults.CreateRecipes());
            var text = CraftingRules.WhatItTakes(Stations.Furnace, recipes, null);
            StringAssert.Contains("5 " + ItemIds.CopperOre, text);
            StringAssert.Contains("1 " + ItemIds.Coal, text);
            var smelt = recipes.ForStation(Stations.Furnace).First();
            StringAssert.Contains(L.Get("machine.and"), CraftingRules.NeedsText(smelt, null));
        }

        [Test]
        public void TheMousePointer_IsHiddenForAPad_AndShownForTheMouse()
        {
            Assert.IsFalse(InputDeviceWatcher.CursorShownFor(InputKind.Gamepad));
            Assert.IsTrue(InputDeviceWatcher.CursorShownFor(InputKind.KeyboardMouse));
        }

        [Test]
        public void TheGame_SaysWhichControlsAreInUse()
        {
            Assert.AreEqual("input.using_kb", InputDeviceWatcher.AnnouncementKey(InputKind.KeyboardMouse));
            Assert.AreEqual("input.using_pad", InputDeviceWatcher.AnnouncementKey(InputKind.Gamepad));
            StringAssert.Contains("controller", L.Get("input.using_pad"));
            StringAssert.Contains("mouse", L.Get("input.using_kb"));
        }
    }
}
