using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;

namespace Farm.Tests
{
    // Story props (cat, umbrella, notes, lantern, pumpkin, scarecrow ...): each is a Misc item with a generated icon, never sold, and every
    // `spawn` step in the story data names a real item that has an icon.
    public class StoryPropsTests
    {
        static readonly string[] Props = { "cat", "umbrella", "note", "lantern", "trophy", "pumpkin", "scarecrow", "shell" };

        static ItemDefinition Item(string id) => AssetDatabase.LoadAssetAtPath<ItemDefinition>($"Assets/_Project/Data/Items/{id}.asset");

        [Test]
        public void EveryProp_IsAMiscItem_WithAnIcon_AndNames()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var p in Props)
            {
                var item = Item("prop." + p);
                Assert.IsNotNull(item, p);
                Assert.IsNotNull(item.Icon, p + " icon");
                Assert.AreEqual(ItemCategory.Misc, item.Category, p);
                Assert.AreEqual(0, item.SellPrice, p + " is not for sale");
                Assert.AreEqual(0, item.SoldIn.Count, p);
                Assert.IsTrue(en.ContainsKey($"item.prop.{p}.name") && en.ContainsKey($"item.prop.{p}.desc"), p);
            }
        }

        [Test]
        public void EverySpawnStep_NamesAnItemWithAnIcon()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var spawns = story.Events.SelectMany(e => e.Steps).Where(s => s.Type == "spawn").ToList();
            Assert.GreaterOrEqual(spawns.Count, 12, "the storyline and lantern scenes show props");
            foreach (var s in spawns)
            {
                var guid = AssetDatabase.FindAssets($"{s.Name} t:ItemDefinition", new[] { "Assets/_Project/Data" }).Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<ItemDefinition>).FirstOrDefault(i => i != null && i.Id == s.Name);
                Assert.IsNotNull(guid, "spawned item " + s.Name);
                Assert.IsNotNull(guid.Icon, s.Name + " has an icon");
            }
        }

        [Test]
        public void TheStorylineScenes_ShowTheirProps()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            foreach (var (scene, prop) in new[] { ("story_umbrella", "prop.umbrella"), ("story_cat", "prop.cat"), ("story_scarecrows", "prop.scarecrow"),
                ("story_pumpkin_dorian", "prop.pumpkin"), ("story_notes_juno", "prop.note"), ("festival_winter_lanterns", "prop.lantern") })
                Assert.IsTrue(story.Event(scene).Steps.Any(s => s.Type == "spawn" && s.Name == prop), scene);
        }
    }
}
