using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // A playtester was asked twice to choose who falls ill (Hazel's sabotage offer, in the library and again in the forest).
    // Story-band priorities (4 and up) never go on cooldown, so a scene that sets no flag and is not guarded by one is offered on every talk.
    public class MythosScenesPlayOnceTests
    {
        const int StoryBand = 4;

        static JObject Story() => JObject.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Project", "Resources", "Mythos", "mythos_story.json")));

        static IEnumerable<string> FlagsSetBy(JObject story, string dialogueId)
        {
            var dialogue = story["dialogues"].First(d => (string)d["id"] == dialogueId);
            foreach (var node in dialogue["nodes"])
            {
                foreach (var e in node["effects"] ?? new JArray()) yield return (string)e;
                foreach (var c in node["choices"] ?? new JArray())
                    foreach (var e in c["effects"] ?? new JArray()) yield return (string)e;
            }
        }

        [Test]
        public void EveryStoryBandMythosScene_SetsAFlagItsOwnConditionRulesOut()
        {
            var story = Story();
            var loops = new List<string>();
            foreach (var set in story["setEntries"])
                foreach (var entry in set["entries"])
                {
                    if ((int)entry["priority"] < StoryBand) continue;
                    var id = (string)entry["dialogue"];
                    var condition = (string)entry["condition"] ?? string.Empty;
                    var flags = FlagsSetBy(story, id).Where(e => e.StartsWith("flag:")).Select(e => e.Substring(5));
                    var guarded = flags.Any(f => Regex.IsMatch(condition, @"!flag:" + Regex.Escape(f) + @"(?![\w.])"));
                    if (!guarded) loops.Add($"{set["set"]}: {id} (priority {entry["priority"]})");
                }
            Assert.IsEmpty(loops, "these scenes would be offered on every talk: set a flag in the dialogue and test !flag in the entry's condition\n" + string.Join("\n", loops));
        }

        [Test]
        public void HazelsSabotageOffer_IsAskedOnce_WhateverThePlayerAnswers()
        {
            var story = Story();
            var choices = story["dialogues"].First(d => (string)d["id"] == "mythos.hazel.sabotage")["nodes"][0]["choices"];
            Assert.AreEqual(3, choices.Count(), "Tilda, Marcus, or forget it");
            foreach (var c in choices)
                CollectionAssert.Contains(c["effects"]?.Select(e => (string)e).ToList() ?? new List<string>(), "flag:mythos.sabotage.asked", "every answer ends the offer: " + c["text"]);
            var entry = story["setEntries"].SelectMany(s => s["entries"]).First(e => (string)e["dialogue"] == "mythos.hazel.sabotage");
            StringAssert.Contains("!flag:mythos.sabotage.asked", (string)entry["condition"]);
        }
    }
}
