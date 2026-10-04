using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Phase 2 vertical slice (Wren, Hazel, Bram): gift reactions, voices, storylines and the FScript shorthands they use.
    public class Phase2SliceTests
    {
        static readonly string[] Slice = { "wren", "hazel", "bram" };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        // ---- gifts ----

        [Test]
        public void GiftLines_AreOrderedMostSpecificFirst()
        {
            var lines = NpcInteractions.GiftLines("wren", "forage.truffle", "Forage", GiftTaste.Loved, birthday: true, repeat: true);
            CollectionAssert.AreEqual(new[]
            {
                "npc.wren.gift.birthday", "gift.birthday", "npc.wren.gift.repeat", "gift.repeat",
                "npc.wren.gift.item.forage.truffle", "npc.wren.gift.cat.Forage", "npc.wren.gift.loved", "gift.loved",
            }, lines);
        }

        [Test]
        public void GiftLines_DisklikedBirthdayGiftGetsNoBirthdayWarmth()
        {
            var lines = NpcInteractions.GiftLines("bram", "x", "Fish", GiftTaste.Disliked, birthday: true, repeat: false);
            Assert.IsFalse(lines.Any(l => l.Contains("birthday")));
        }

        [Test]
        public void RepeatGifts_HalvePositivePointsOnly()
        {
            Assert.AreEqual(40, FriendshipModel.RepeatGiftPoints(80, true, false));
            Assert.AreEqual(80, FriendshipModel.RepeatGiftPoints(80, false, false));
            Assert.AreEqual(80, FriendshipModel.RepeatGiftPoints(80, true, true), "a birthday gift is never a repeat");
            Assert.AreEqual(-20, FriendshipModel.RepeatGiftPoints(-20, true, false), "a bad gift is not softened by repeating");
        }

        [Test]
        public void SliceItemGiftLines_NameRealItems()
        {
            var ids = new HashSet<string>(UnityEditor.AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_Project/Data" })
                .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDefinition>(UnityEditor.AssetDatabase.GUIDToAssetPath(g))).Where(i => i != null).Select(i => i.Id));
            foreach (var npc in Slice)
                foreach (var dialogue in _story.Dialogues.Select(x => x.Id).Where(d => d.StartsWith($"npc.{npc}.gift.item.")))
                {
                    var item = dialogue.Substring($"npc.{npc}.gift.item.".Length);
                    Assert.IsTrue(ids.Contains(item), $"{dialogue}: no item '{item}'");
                }
        }

        [Test]
        public void SliceVillagers_ReactToAbsurdGifts()
        {
            foreach (var npc in Slice)
                Assert.GreaterOrEqual(_story.Dialogues.Count(d => d.Id.StartsWith($"npc.{npc}.gift.item.")), 15, npc);
        }

        // ---- voices ----

        [Test]
        public void Voices_AreDeterministicAndDistinct()
        {
            var wren = VoiceProfiles.For("wren");
            var hazel = VoiceProfiles.For("hazel");
            var bram = VoiceProfiles.For("bram");
            CollectionAssert.AreEqual(VoiceSynth.Blip(wren, 3), VoiceSynth.Blip(wren, 3));
            Assert.AreNotEqual(wren.Pitch, hazel.Pitch);
            Assert.AreNotEqual(hazel.Pitch, bram.Pitch);
            Assert.Less(bram.Pitch, wren.Pitch);
            Assert.AreEqual(VoiceProfiles.For("tilda").Pitch, VoiceProfiles.For("tilda").Pitch);
        }

        [Test]
        public void Blips_AreAudibleAndBounded()
        {
            foreach (var id in new[] { "wren", "hazel", "bram", "tilda", "juno" })
            {
                var samples = VoiceSynth.Blip(VoiceProfiles.For(id), 1);
                Assert.Greater(samples.Length, 500, id);
                Assert.Greater(samples.Max(Mathf_Abs), 0.05f, id);
                Assert.LessOrEqual(samples.Max(Mathf_Abs), 1f, id);
            }
        }

        static float Mathf_Abs(float f) => f < 0 ? -f : f;

        [Test]
        public void SignatureSounds_Exist()
        {
            foreach (var id in Slice) Assert.Greater(VoiceSynth.Signature(id).Length, 1000, id);
        }

        [Test]
        public void ShouldBlip_FiresOncePerGroupOfCharacters()
        {
            Assert.IsFalse(VoiceSynth.ShouldBlip(0, 1, 2));
            Assert.IsTrue(VoiceSynth.ShouldBlip(1, 2, 2));
            Assert.IsFalse(VoiceSynth.ShouldBlip(2, 3, 2));
            Assert.IsFalse(VoiceSynth.ShouldBlip(5, 5, 2));
            Assert.IsFalse(VoiceSynth.ShouldBlip(0, 10, 0));
        }

        // ---- storylines ----

        static List<StorylineDefinition> Pool() => new List<StorylineDefinition>
        {
            new StorylineDefinition { Id = "a" }, new StorylineDefinition { Id = "b" }, new StorylineDefinition { Id = "c" },
            new StorylineDefinition { Id = "d" }, new StorylineDefinition { Id = "e", Weight = 0f },
        };

        [Test]
        public void StorylineDraw_IsDeterministicWithoutRepeatsAndSkipsZeroWeight()
        {
            var first = Storylines.Draw(Pool(), 1234, 3);
            CollectionAssert.AreEqual(first, Storylines.Draw(Pool(), 1234, 3));
            Assert.AreEqual(3, first.Distinct().Count());
            Assert.IsFalse(first.Contains("e"));
            Assert.AreEqual(4, Storylines.Draw(Pool(), 7, 10).Count, "never more than the pool holds");
        }

        [Test]
        public void StorylineDraw_DiffersBetweenSeeds()
        {
            var sets = Enumerable.Range(1, 40).Select(s => string.Join(",", Storylines.Draw(Pool(), s * 101, 2))).Distinct().Count();
            Assert.Greater(sets, 2);
        }

        // ---- FScript shorthands ----

        [Test]
        public void Say_MakesAOneLineDialogue()
        {
            var r = FScript.Compile("say npc.wren.gift.loved wren: Oh, you menace.", "t");
            Assert.IsEmpty(r.Errors, string.Join("\n", r.Errors));
            Assert.AreEqual(1, r.Dialogues.Count);
            Assert.AreEqual("wren", r.Dialogues[0].Nodes[0].Speaker);
        }

        [Test]
        public void AddTo_WithInlineText_MakesEntriesAndDialogues()
        {
            var r = FScript.Compile("addto npc.wren.talk\n0 season:spring => wren.t1 cat=seasonal expr=happy sfx=signature :: Hello.\n", "t");
            Assert.IsEmpty(r.Errors, string.Join("\n", r.Errors));
            Assert.AreEqual(1, r.SetAdditions.Single().Entries.Count);
            var node = r.Dialogues.Single().Nodes[0];
            Assert.AreEqual("happy", node.Expression);
            Assert.AreEqual("signature", node.Sfx);
        }

        [Test]
        public void AddTo_InlineTextNeedsATalkSet()
        {
            var r = FScript.Compile("addto npc.wren.other\n0 ~ => wren.t1 :: Hello.\n", "t");
            Assert.IsNotEmpty(r.Errors);
        }

        [Test]
        public void SliceTalk_HasDepthAndEveryLineExists()
        {
            foreach (var npc in Slice)
            {
                var set = _story.Set($"npc.{npc}.talk");
                Assert.GreaterOrEqual(set.Entries.Count, 120, npc);
                foreach (var e in set.Entries) Assert.IsNotNull(_story.Dialogue(e.Dialogue), $"{npc}: {e.Dialogue}");
            }
        }

        // ---- scenes, letters, reactions, storylines ----

        [Test]
        public void EachSliceVillager_HasFourHeartEventsAndAFriendDayScene()
        {
            foreach (var npc in Slice)
            {
                foreach (var n in new[] { 4, 6, 8, 10 })
                {
                    var ev = _story.Event($"{npc}_heart{n}");
                    Assert.IsNotNull(ev, $"{npc} heart {n}");
                    Assert.IsTrue(Memories.IsMemory(ev), $"{npc} heart {n} is a memory");
                    StringAssert.Contains($"hearts:{npc}>={n}", ev.Condition);
                }
                Assert.IsNotNull(_story.Event($"{npc}_friend"), npc + " friend-day scene");
            }
        }

        [Test]
        public void HeartEvents_ChainOnThePreviousOne()
        {
            foreach (var npc in Slice)
            {
                StringAssert.Contains($"flag:event.{npc}_heart2", _story.Event($"{npc}_heart4").Condition);
                StringAssert.Contains($"flag:event.{npc}_heart5", _story.Event($"{npc}_heart6").Condition);
                StringAssert.Contains($"flag:event.{npc}_heart6", _story.Event($"{npc}_heart8").Condition);
                StringAssert.Contains($"flag:event.{npc}_heart8", _story.Event($"{npc}_heart10").Condition);
            }
        }

        [Test]
        public void ChoiceCallbacks_RefertoChoicesThatExist()
        {
            var table = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var choiceFlags = new HashSet<string>();
            foreach (var d in _story.Dialogues)
                foreach (var node in d.Nodes)
                    foreach (var c in node.Choices ?? new List<DialogueChoice>())
                        foreach (var e in c.Effects ?? new List<string>()) if (e.StartsWith("flag:choice.")) choiceFlags.Add(e.Substring("flag:choice.".Length));
            var used = System.Text.RegularExpressions.Regex.Matches(string.Join(" ", table.Values), @"\{if choice:([\w.]+)\}");
            Assert.Greater(used.Count, 0);
            foreach (System.Text.RegularExpressions.Match m in used) Assert.IsTrue(choiceFlags.Contains(m.Groups[1].Value), "callback to choice " + m.Groups[1].Value);
        }

        [Test]
        public void SliceLetters_HaveTextAndAKnownSender()
        {
            var table = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var npc in Slice)
            {
                var mine = _story.Letters.Where(l => l.Sender == npc && (l.Id.EndsWith("_bday") || l.Id.EndsWith("_thanks"))).ToList();
                Assert.AreEqual(2, mine.Count, npc);
                foreach (var l in mine) { Assert.IsTrue(table.ContainsKey(l.SubjectKey), l.Id); Assert.IsTrue(table.ContainsKey(l.BodyKey), l.Id); }
            }
        }

        [Test]
        public void SliceReactions_ExistForEachVillager()
        {
            foreach (var npc in Slice)
                Assert.GreaterOrEqual(_story.Reactions.Count(r => r.Npcs == npc), 4, npc);
        }

        [Test]
        public void ShippedStorylines_AreFiveAndAreDrawnFour()
        {
            var all = _story.Storylines.ToList();
            Assert.GreaterOrEqual(all.Count, 5);
            Assert.AreEqual(Storylines.PerGame, Storylines.Draw(all, 99, Storylines.PerGame).Count);
        }
    }
}
