using System.Linq;
using Farm.Mythos;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Spec WP7 to WP9: the Keepers' reactions, the unaware villagers' dread lines, and Edmund's threads. Everything here belongs to the layer: it is gated on `horror:1`
    // (so at intensity 0 nothing can be heard), and each conversation sets a flag its own condition rules out, so that it plays once.
    public class EdmundThreadsTests
    {
        static JObject Load(string name) => JObject.Parse(Resources.Load<TextAsset>("Mythos/" + name).text);

        static readonly string[] Files = { "mythos_reactions", "mythos_threads" };

        [Test]
        public void TheCellarDoor_IsLockedWithoutTheKey_EarlyWithoutTheLoreOrTheWood_AndOpensWithBoth()
        {
            Assert.AreEqual(EdmundThreads.CellarResult.Locked, EdmundThreads.Resolve(false, 10, true));
            Assert.AreEqual(EdmundThreads.CellarResult.Early, EdmundThreads.Resolve(true, EdmundThreads.TruthLore - 1, true), "not enough lore");
            Assert.AreEqual(EdmundThreads.CellarResult.Early, EdmundThreads.Resolve(true, 10, false), "the wood has not been seen");
            Assert.AreEqual(EdmundThreads.CellarResult.Opens, EdmundThreads.Resolve(true, EdmundThreads.TruthLore, true));
        }

        [Test]
        public void EveryReactionAndThreadLine_IsGatedOnTheLayer_AndPlaysOnce()
        {
            foreach (var file in Files)
            {
                var root = Load(file);
                var dialogues = ((JArray)root["dialogues"]).ToDictionary(d => (string)d["id"], d => d);
                Assert.IsNotEmpty(dialogues, file);
                foreach (var set in (JArray)root["setEntries"])
                    foreach (var entry in (JArray)set["entries"])
                    {
                        var condition = (string)entry["condition"];
                        var id = (string)entry["dialogue"];
                        StringAssert.StartsWith("horror:1", condition, id + " must only be heard with the layer on");
                        Assert.IsTrue(dialogues.ContainsKey(id), id);
                        var effects = ((JArray)dialogues[id]["nodes"][0]["effects"]).Select(e => (string)e).ToList();
                        var ruledOut = effects.Where(e => e.StartsWith("flag:")).Select(e => "!" + e).Any(f => condition.Contains(f));
                        Assert.IsTrue(ruledOut, id + " sets a flag its own condition rules out");
                    }
            }
        }

        [Test]
        public void TheUnawareSix_EachHaveTwoDreadLines_AtTwoThresholds()
        {
            var root = Load("mythos_reactions");
            foreach (var npc in new[] { "bram", "juno", "piper", "felix", "elara", "ione" })
            {
                var lines = ((JArray)root["setEntries"]).SelectMany(s => ((JArray)s["entries"]).Select(e => (set: (string)s["set"], cond: (string)e["condition"])))
                    .Where(x => x.set == $"npc.{npc}.talk" && x.cond.Contains("var:dread>=")).ToList();
                Assert.AreEqual(2, lines.Count, npc);
                Assert.IsTrue(lines.Any(l => l.cond.Contains("var:dread>=30")) && lines.Any(l => l.cond.Contains("var:dread>=60")), npc);
            }
        }

        [Test]
        public void TheFiveKeepers_EachHaveTwoReactionsToTheOfferingBeingTaken_TheSecondAfterTheFirst()
        {
            var root = Load("mythos_reactions");
            foreach (var npc in MythosCast.Keepers)
            {
                var conds = ((JArray)root["setEntries"]).Where(s => (string)s["set"] == $"npc.{npc}.talk").SelectMany(s => ((JArray)s["entries"]).Select(e => (string)e["condition"]))
                    .Where(c => c.Contains("snatch")).ToList();
                Assert.AreEqual(2, conds.Count, npc);
                Assert.IsTrue(conds.Any(c => c.Contains("flag:mythos.snatched") && !c.Contains($"flag:react.{npc}.snatch.1 &&")), npc + ": the first reaction needs the offering taken");
                Assert.IsTrue(conds.Any(c => c.Contains($"flag:react.{npc}.snatch.1 &&")), npc + ": the second needs the first");
            }
        }

        [Test]
        public void TheUncleLetters_AreFoundNotPosted()
        {
            var letters = ((JArray)Load("mythos_story")["letters"]).ToDictionary(l => (string)l["id"], l => (string)l["condition"]);
            StringAssert.Contains("flag:edmund.stove_note", letters[EdmundThreads.NoteLetter]);
            StringAssert.Contains("flag:edmund.box_opened", letters[EdmundThreads.TruthLetter]);
            StringAssert.Contains("flag:woods.open", letters[EdmundThreads.TruthLetter]);
        }

        [Test]
        public void TheOfferingBeingTaken_IsRemembered()
        {
            var events = (JArray)Load("mythos_story")["events"];
            var ritual = events.First(e => (string)e["id"] == "mythos_ritual_witness");
            var steps = ((JArray)ritual["steps"]).ToList();
            var label = steps.FindIndex(s => (string)s["type"] == "label" && (string)s["label"] == "snatched");
            Assert.GreaterOrEqual(label, 0);
            Assert.AreEqual("effects", (string)steps[label + 1]["type"]);
            CollectionAssert.Contains(((JArray)steps[label + 1]["effects"]).Select(e => (string)e).ToList(), "flag:mythos.snatched");
        }
    }
}
