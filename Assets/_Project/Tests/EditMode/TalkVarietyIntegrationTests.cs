using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-090 wired into the game: talking to a villager uses the variety engine and the memory is saved with the game.
    public class TalkVarietyIntegrationTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        TestSessionFixture Fixture()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null);
            f.Session.Story = StoryContent.LoadFromResources();
            return f;
        }

        static string LastSaid(GameSession s, string npc) => LineMemory.Load(s).LastOf($"npc.{npc}.talk")?.Dialogue;

        [Test]
        public void Talking_RemembersWhatWasSaid_InTheSave_AndRepeatsItWithinTheDay()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var wren = s.Npcs.Get("wren");
                s.SetFlag("met.wren");
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                NpcInteractions.Talk(s, wren);
                var first = LastSaid(s, "wren");
                NpcInteractions.Talk(s, wren);
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;

                Assert.IsNotNull(first);
                Assert.AreEqual(first, LastSaid(s, "wren"), "the same line twice in a day");
                Assert.IsTrue(s.State.ModuleData.ContainsKey(LineMemory.ModuleId), "stored with the save");
                Assert.AreEqual(1, LineMemory.Load(s).TimesHeard("npc.wren.talk", first));
            }
        }

        [Test]
        public void DailyTalks_RotateThroughEveryEligibleLine_BeforeRepeating()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                var wren = s.Npcs.Get("wren");
                s.SetFlag("met.wren");
                var said = new List<string>();
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                for (var day = 3; day < 3 + 8; day++)
                {
                    s.Clock.SetTime(new GameDateTime(1, Season.Fall, day, 20 * 60));
                    NpcInteractions.Talk(s, wren);
                    said.Add(LastSaid(s, "wren"));
                }
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
                // At 0 hearts on a fall evening Wren has five eligible lines (the content still being thin is what the
                // writing phase fixes): all five come before any repeat.
                Assert.GreaterOrEqual(said.Take(5).Distinct().Count(), 5, "first rotation: " + string.Join(", ", said));
            }
        }
    }
}
