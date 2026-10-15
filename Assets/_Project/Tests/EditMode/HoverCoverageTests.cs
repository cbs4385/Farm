using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-15: bushes and logs showed no name, and not every icon said what it was. These are the registries that keep it that way: every clearable node and every picture put on a map
    // has a name for the hover label, and every key the labels use exists. (The pictures on the interface are walked by UiTipCoverageFlowTests, which opens the real screens.)
    public class HoverCoverageTests
    {
        static readonly Dictionary<string, string> En = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));

        static IEnumerable<ResourceNodeDefinition> Nodes() => AssetDatabase.FindAssets("t:ResourceNodeDefinition", new[] { "Assets/_Project/Data" })
            .Select(g => AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(n => n != null);

        [Test]
        public void EveryNodeAToolClears_HasAName()
        {
            var nodes = Nodes().ToList();
            Assert.GreaterOrEqual(nodes.Count, 30);
            foreach (var node in nodes.Where(NodeLabels.NamesItself))
            {
                Assert.IsTrue(En.TryGetValue(node.NameKey, out var name) && !string.IsNullOrWhiteSpace(name), node.NameKey + " is missing from en.json");
                Assert.LessOrEqual(name.Length, 40, node.NameKey + " is a short name");
            }
            foreach (var wild in nodes.Where(n => n.Tool == ToolType.None))
                Assert.IsTrue(En.ContainsKey("item." + wild.DropItemId + ".name"), wild.Id + " is named after what it gives");
        }

        [Test]
        public void TheNodesPeopleAsked_BushLogStumpRockWeedTree_AreAllNamed()
        {
            foreach (var id in new[] { "bush", "log", "stump", "rock", "boulder", "weed", "tree", "sapling" })
            {
                var def = Nodes().First(n => n.Id == id);
                Assert.IsTrue(NodeLabels.NamesItself(def), id);
                Assert.IsNotNull(NodeLabels.For(def), id);
            }
        }

        [Test]
        public void ApickedPlant_IsNotGivenAToolNodeName()
        {
            foreach (var picked in Nodes().Where(n => n.Id.EndsWith(ForageDefaults.PickedSuffix)))
                Assert.IsNull(NodeLabels.For(picked), picked.Id);
        }

        // Every key a label names in the source (a string literal like "uitip.hud.gold" or "hover.prop.stall") has text.
        [Test]
        public void EveryLabelKeyInTheSource_ExistsInTheLanguageFile()
        {
            var literal = new Regex("\"((?:uitip|hover)\\.[a-z0-9_]+(?:\\.[a-z0-9_]+)*)\"");
            var missing = new List<string>();
            foreach (var file in Directory.GetFiles("Assets/_Project/Scripts", "*.cs", SearchOption.AllDirectories))
                foreach (Match m in literal.Matches(File.ReadAllText(file)))
                    if (!En.ContainsKey(m.Groups[1].Value)) missing.Add(m.Groups[1].Value + " (" + Path.GetFileName(file) + ")");
            Assert.IsEmpty(missing, string.Join("\n", missing.Distinct()));
        }

        [Test]
        public void TheUiTipLabels_AreShort()
        {
            foreach (var pair in En.Where(p => p.Key.StartsWith("uitip.") || p.Key.StartsWith("node.")))
            {
                Assert.LessOrEqual(pair.Value.Length, 50, pair.Key);
                Assert.IsFalse(pair.Value.Contains("!"), pair.Key);
            }
        }

        [Test]
        public void ANameThatChanges_IsReadWhenItIsShown_AndTheMarksTellApartDecorationFromLabels()
        {
            var go = new GameObject("tip");
            try
            {
                var name = "first";
                var tip = UiTip.Add(go, () => name);
                Assert.AreEqual(UiTip.Kind.Label, tip.Mode);
                Assert.AreEqual("first", tip.Current);
                name = "second";
                Assert.AreEqual("second", tip.Current);
                var child = new GameObject("child");
                child.transform.SetParent(go.transform);
                Assert.AreSame(tip, UiTip.For(child.transform), "a picture inside a named slot is named by it");
                UiTip.Skip(go);
                Assert.AreEqual(UiTip.Kind.Decoration, tip.Mode);
                Assert.IsNull(tip.Current);
                UiTip.HasOwn(go);
                Assert.AreEqual(UiTip.Kind.OwnTooltip, tip.Mode);
                Assert.AreSame(tip, go.GetComponent<UiTip>(), "marking again never doubles the component");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ThePlainHotbarLabel_NamesTheQualityOfWhatYouHold()
        {
            Assert.AreEqual(SlotBadges.NameWithQuality("Parsnip", 2), SlotBadges.NameWithQuality("Parsnip", 2));
            StringAssert.Contains("Parsnip", SlotBadges.NameWithQuality("Parsnip", 2));
            Assert.AreNotEqual("Parsnip", SlotBadges.NameWithQuality("Parsnip", 2), "a better quality is part of the name");
            Assert.AreEqual("Parsnip", SlotBadges.NameWithQuality("Parsnip", 0));
        }

        [Test]
        public void TheMineEntrance_NamesItself_AndWarnsOfWildlife()
        {
            StringAssert.Contains("CAUTION: DANGEROUS WILDLIFE", En["hover.mine.caution"]);
            StringAssert.Contains("{0}", En["hover.prop.mine_entrance"]);
            StringAssert.StartsWith("Mine entrance", En["hover.prop.mine_entrance"], "the name comes first, the caution on its own line");
            var go = new GameObject("door");
            try
            {
                go.AddComponent<BoxCollider2D>();
                var warp = go.AddComponent<Warp>();
                warp.TargetMap = MapIds.Mine;
                StringAssert.EndsWith("CAUTION: DANGEROUS WILDLIFE</color>", warp.HoverLabel, "the stairway down says it too");
                warp.TargetMap = MapIds.Forest;
                StringAssert.DoesNotContain("CAUTION", warp.HoverLabel, "only the way into the mine warns");
            }
            finally { Object.DestroyImmediate(go); }
        }

        // Pictures that are only a floor covering: nobody points at them.
        static readonly HashSet<string> FlatDecoration = new HashSet<string> { "obj_rug", "obj_fest_floor" };

        static bool Named(Component c)
        {
            if (c.GetComponentInParent<HoverNote>(true) is HoverNote note && !string.IsNullOrEmpty(note.Key)) return true;
            if (c.GetComponentInParent<Warp>(true) != null) return true;
            return c.GetComponentsInParent<MonoBehaviour>(true).Any(b => b is IInteractable);
        }

        // Every picture a map builder puts on a map (a prop, a piece of furniture, a tree, a rock, a building) names itself. Fails with the scene and the object.
        [Test]
        public void EveryObjectPlacedOnAMap_HasAHoverName()
        {
            var bad = new List<string>();
            foreach (var path in Directory.GetFiles("Assets/_Project/Scenes", "*.unity"))
            {
                var scene = Path.GetFileNameWithoutExtension(path);
                if (scene == "Bootstrap" || scene == "MainMenu" || scene == "PixelPerfectTest") continue;
                EditorSceneManager.OpenScene(path.Replace('\\', '/'), OpenSceneMode.Single);
                foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
                {
                    var sprite = sr.sprite != null ? sr.sprite.name : null;
                    if (sprite == null || !(sprite.StartsWith("obj_") || sprite.StartsWith("prop_"))) continue;
                    if (FlatDecoration.Contains(sprite) || Named(sr)) continue;
                    bad.Add(scene + ": " + sr.gameObject.name + " (" + sprite + ")");
                }
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Assert.IsEmpty(bad, "unnamed on hover:\n" + string.Join("\n", bad));
        }
    }
}
