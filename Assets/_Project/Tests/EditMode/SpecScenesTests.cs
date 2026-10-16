using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The scenes that close arcs (docs/NPC_INTERCONNECTED_GAMEPLAY_SPEC.md WP2 to WP6): each fires once, sets its own flag, and waits for what it pays off.
    public class SpecScenesTests
    {
        static JObject Scenes => JObject.Parse(Resources.Load<TextAsset>("Story/spec_scenes").text);

        static JObject Event(string id) => (JObject)((JArray)Scenes["events"]).First(e => (string)e["id"] == id);

        static string Condition(string id) => (string)Event(id)["condition"];

        [Test]
        public void EverySpecScene_SetsItsOwnFlagAtTheEnd_AndIsOnce()
        {
            foreach (var e in (JArray)Scenes["events"])
            {
                var id = (string)e["id"];
                Assert.AreNotEqual(false, (bool?)e["once"] ?? true, id + " plays once");
                var steps = ((JArray)e["steps"]).ToList();
                var last = steps.Last(s => (string)s["type"] == "effects");
                CollectionAssert.Contains(((JArray)last["effects"]).Select(x => (string)x).ToList(), "flag:event." + id, id);
                Assert.IsNotNull((string)e["titleKey"], id + " is a memory with a title");
            }
        }

        [Test]
        public void TheHallDance_CannotHappenBeforeTheRoofIsDone_OrWithoutWrensWineAndPipersSong()
        {
            var c = Condition("hall_dance");
            foreach (var needed in new[] { "flag:hall.restored", "flag:hall.roof", "quest:wren_full_house=done", "flag:event.piper_heart10", "flag:event.juno_heart10" })
                StringAssert.Contains(needed, c);
            var participants = ((JArray)Event("hall_dance")["steps"]).Where(s => (string)s["type"] == "place").Select(s => (string)s["actor"]).Where(a => a != "player").Distinct().OrderBy(a => a).ToList();
            CollectionAssert.AreEqual(new[] { "bram", "juno", "marcus", "piper", "tilda", "wren" }, participants, "only the six are placed");
        }

        [Test]
        public void NextDoor_NeedsBothHeartTenScenes_AndReadingNightNeedsTheYes()
        {
            var c = Condition("next_door");
            StringAssert.Contains("flag:event.juno_heart10", c);
            StringAssert.Contains("flag:event.bram_heart10", c);
            var r = Condition("reading_night");
            StringAssert.Contains("flag:choice.reading.yes", r);
            StringAssert.Contains("flag:event.hazel_heart10", r);
        }

        [Test]
        public void TheBoatTrip_HasThreeDestinations_AndTheDecorStandsOnlyWhileItCanHappen()
        {
            Assert.AreEqual(Farm.Editor.MapBuilder.PastThePointCondition, Condition("past_the_point"), "the dressing of the beach and the scene use the same words");
            var steps = ((JArray)Event("past_the_point")["steps"]).Select(s => (string)s["text"]).Where(t => t != null).ToList();
            foreach (var k in new[] { "headland", "rocks", "horizon" }) CollectionAssert.Contains(steps, "event.past_the_point." + k);
            StringAssert.Contains("!flag:event.past_the_point", Farm.Editor.MapBuilder.PastThePointCondition, "the boat is gone once the trip is made");
        }

        [Test]
        public void TheTeachersLetter_ArrivesOnlyAfterTheChain_AndSetsTheFlagBramWaitsFor()
        {
            var letter = (JObject)((JArray)Scenes["letters"]).First(l => (string)l["id"] == "bram_teacher");
            StringAssert.Contains("flag:event.bram_heart10", (string)letter["condition"]);
            StringAssert.Contains("heard:bram.goal.a1", (string)letter["condition"]);
            CollectionAssert.Contains(((JArray)letter["effects"]).Select(x => (string)x).ToList(), "flag:bram.teacher_replied");
        }
    }
}
