using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The automatable lines of the release checklist (docs/03-ImplementationPlan.md): things that must stay true on every
    // build, so a release candidate cannot silently regress them.
    public class ReleaseChecklistTests
    {
        static GameDatabase Db()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder)) db.Merge(pack);
            return db;
        }

        [TearDown]
        public void TearDown()
        {
            Conditions.ClearCustomForTests();
            Resources.Load<GameDatabase>(GameDatabase.ResourcePath).ClearMergedPacks();
        }

        [Test]
        public void NoHorrorItem_IsSoldByAnyShop()
        {
            var horror = MythosData.Crops.SelectMany(c => new[] { $"seed.{c.Id}", $"crop.{c.Id}" }).Concat(MythosData.Relics).ToList();
            var db = Db();
            foreach (var id in horror)
            {
                Assert.IsTrue(db.TryGetItem(id, out var item), id);
                Assert.IsEmpty(item.SoldIn, $"{id} must not be sold anywhere");
            }
            foreach (var item in db.AllItems)
                if (item.SoldIn.Count > 0) CollectionAssert.DoesNotContain(horror, item.Id);
        }

        [Test]
        public void EveryEnding_IsReachableFromData()
        {
            var story = StoryContent.LoadFromResources();
            MythosContent.Load(story);
            foreach (var ending in new[] { "awakened", "sealed", "joined", "ignored" })
            {
                var reachedBy = story.Events.Where(e => e.Steps.Any(st => st.Effects != null && st.Effects.Contains("ending:" + ending))).ToList();
                Assert.IsNotEmpty(reachedBy, $"no event ends the game with '{ending}'");
            }
            // The two player-chosen endings are offered by the altar's dialogue; the god's awakening is run by the step hook.
            var altar = story.Dialogue("mythos.altar");
            var altarEffects = altar.Nodes.SelectMany(n => n.Choices).SelectMany(c => c.Effects ?? new System.Collections.Generic.List<string>()).ToList();
            CollectionAssert.Contains(altarEffects, "event:mythos_ending_resist");
            CollectionAssert.Contains(altarEffects, "event:mythos_ending_join");
            Assert.IsNotNull(story.Event("mythos_ending_awakening"));
        }

        [Test]
        public void TheVersion_IsSemantic_AndTheReleaseFilesExist()
        {
            var settings = File.ReadAllText("ProjectSettings/ProjectSettings.asset");
            var match = Regex.Match(settings, @"bundleVersion:\s*(\S+)");
            Assert.IsTrue(match.Success);
            StringAssert.IsMatch(@"^\d+\.\d+\.\d+$", match.Groups[1].Value);
            foreach (var file in new[] { "Steam/app_build.vdf", "Steam/depot_windows.vdf", "Steam/depot_linux.vdf", "Steam/upload.sh",
                                         "docs/RELEASE.md", "docs/store/STORE_PAGE.md", "docs/store/EULA.md", "docs/store/PRIVACY.md", "CHANGELOG.md" })
                Assert.IsTrue(File.Exists(file), file);
        }

        [Test]
        public void TheStoreDescription_MentionsTheIntensitySetting()
        {
            var text = File.ReadAllText("docs/store/STORE_PAGE.md");
            StringAssert.Contains("Horror intensity", text);
            StringAssert.Contains("Off", text);
        }

        [Test]
        public void ThePlayer_CannotRunWithoutSteam()
        {
            Assert.AreEqual("none", Farm.Platform.PlatformServices.Current.Name);
        }
    }
}
