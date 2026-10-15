using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Farm.Tests
{
    // Every villager (found from the assets, so new ones are checked too) shows the same person in the dialogue portrait and in the world:
    // each picture is the villager's own file (never npc_generic or another villager's), the four facings and the poses are all present,
    // and no two villagers share a picture. A playtester once saw "a man with short dark hair" in the dialogue and a woman with red
    // pigtails in the world: Felix's front and back sprites were a different drawing from his portrait and side views.
    public class NpcImageConsistencyTests
    {
        static readonly string[] Facings = { "down", "up", "left", "right" };
        static readonly Vector2Int[] FacingVectors = { Vector2Int.down, Vector2Int.up, Vector2Int.left, Vector2Int.right };

        static List<NpcDefinition> Villagers() =>
            AssetDatabase.FindAssets("t:NpcDefinition", new[] { "Assets/_Project/Data/Npcs" })
                .Select(g => AssetDatabase.LoadAssetAtPath<NpcDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(n => n != null).ToList();

        static string FileOf(Sprite s) => Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(s));

        [Test]
        public void TheRoster_IsNotEmpty_AndHasTheTwelveVillagers()
        {
            var ids = Villagers().Select(n => n.Id).ToList();
            foreach (var id in new[] { "wren", "hazel", "bram", "tilda", "juno", "piper", "marcus", "odalys", "felix", "dorian", "elara", "ione" })
                CollectionAssert.Contains(ids, id);
        }

        [Test]
        public void EveryVillager_UsesTheirOwnPortrait_AndTheirOwnFourFacings()
        {
            foreach (var npc in Villagers())
            {
                Assert.IsNotNull(npc.Portrait, $"{npc.Id}: no portrait");
                Assert.AreEqual($"ui_portrait_{npc.Id}", FileOf(npc.Portrait), $"{npc.Id}: portrait is another file");
                for (var i = 0; i < Facings.Length; i++)
                {
                    var s = npc.SpriteFor(FacingVectors[i]);
                    Assert.IsNotNull(s, $"{npc.Id}: no {Facings[i]} sprite");
                    Assert.AreEqual($"npc_{npc.Id}_idle_{Facings[i]}", FileOf(s), $"{npc.Id}: the {Facings[i]} sprite is the wrong file (a missing one falls back to the front, a generic one is never right)");
                }
            }
        }

        [Test]
        public void EveryVillager_UsesTheirOwnPoses_AndExpressions()
        {
            foreach (var npc in Villagers())
            {
                foreach (var pose in NpcDefinition.PoseNames)
                {
                    var s = npc.PoseFor(pose);
                    Assert.IsNotNull(s, $"{npc.Id}: no {pose} pose");
                    Assert.AreEqual($"npc_{npc.Id}_pose_{pose}", FileOf(s), $"{npc.Id}: {pose} pose is another file");
                }
                foreach (var expression in NpcDefinition.ExpressionNames)
                {
                    var s = npc.PortraitFor(expression);
                    Assert.IsNotNull(s, $"{npc.Id}: no {expression} portrait");
                    var file = FileOf(s);
                    Assert.IsTrue(file == $"ui_portrait_{npc.Id}" || file == $"ui_portrait_{npc.Id}_{expression}", $"{npc.Id}: {expression} portrait is {file}");
                }
            }
        }

        [Test]
        public void NoTwoVillagers_ShareAPicture()
        {
            var seen = new Dictionary<string, string>();
            foreach (var npc in Villagers())
            {
                var sprites = new List<Sprite> { npc.Portrait };
                sprites.AddRange(FacingVectors.Select(npc.SpriteFor));
                sprites.AddRange(NpcDefinition.PoseNames.Select(npc.PoseFor));
                foreach (var s in sprites)
                {
                    var path = AssetDatabase.GetAssetPath(s);
                    var key = System.Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(File.ReadAllBytes(path)));
                    if (seen.TryGetValue(key, out var other)) Assert.AreEqual(other, npc.Id, $"{npc.Id} and {other} share the picture {path}");
                    else seen[key] = npc.Id;
                }
            }
        }

        // The front, back and side drawings must show the same clothes: they share a good part of their colours. (Old Felix art shared well under
        // half, but so do some honest sheets, so this is a floor that catches only a plainly different drawing.)
        [Test]
        public void EveryVillager_FrontAndSideViews_ShareTheirColours()
        {
            foreach (var npc in Villagers())
            {
                var front = Palette(npc.SpriteFor(Vector2Int.down));
                foreach (var f in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up })
                    Assert.GreaterOrEqual(Overlap(front, Palette(npc.SpriteFor(f))), 0.3f, $"{npc.Id}: the {f} picture looks like another person than the front one");
            }
        }

        static Dictionary<int, float> Palette(Sprite s)
        {
            var tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, File.ReadAllBytes(AssetDatabase.GetAssetPath(s)));
            var counts = new Dictionary<int, float>();
            var total = 0f;
            foreach (var c in tex.GetPixels32())
            {
                if (c.a < 128) continue;
                var key = (c.r / 48) * 100 + (c.g / 48) * 10 + c.b / 48;
                counts[key] = counts.TryGetValue(key, out var n) ? n + 1 : 1;
                total++;
            }
            Object.DestroyImmediate(tex);
            foreach (var k in counts.Keys.ToList()) counts[k] /= Mathf.Max(1f, total);
            return counts;
        }

        static float Overlap(Dictionary<int, float> a, Dictionary<int, float> b) =>
            a.Where(kv => b.ContainsKey(kv.Key)).Sum(kv => Mathf.Min(kv.Value, b[kv.Key]));
    }
}
