using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;

namespace Farm.Tests
{
    // T-131: every villager has the four pose sprites (wave, sit, shrug, point) and the scenes use poses only by names that exist.
    public class PosesTests
    {
        static readonly string[] All = { "wren", "hazel", "bram", "tilda", "juno", "piper", "marcus", "odalys", "felix", "dorian", "elara", "ione" };

        static NpcDefinition Npc(string id) => AssetDatabase.LoadAssetAtPath<NpcDefinition>($"Assets/_Project/Data/Npcs/{id}.asset");

        [Test]
        public void EveryVillager_HasAllFourPoses()
        {
            foreach (var id in All)
            {
                var npc = Npc(id);
                Assert.IsNotNull(npc, id);
                foreach (var pose in NpcDefinition.PoseNames) Assert.IsNotNull(npc.PoseFor(pose), $"{id} {pose}");
                Assert.IsNull(npc.PoseFor("backflip"), "an unknown pose is simply absent");
            }
        }

        [Test]
        public void ThePoseSprites_AreOnTheirFeetLikeTheIdleSprites_AndTheSameHeight()
        {
            foreach (var id in All)
            {
                var npc = Npc(id);
                var idle = npc.SpriteFor(UnityEngine.Vector2Int.down);
                foreach (var pose in NpcDefinition.PoseNames)
                {
                    var s = npc.PoseFor(pose);
                    Assert.AreEqual(0f, s.pivot.y, 0.01f, $"{id} {pose} pivots at the feet");
                    Assert.AreEqual(idle.rect.height, s.rect.height, $"{id} {pose} height");
                }
            }
        }

        [Test]
        public void EveryAnimStep_UsesAKnownName()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var names = story.Events.SelectMany(e => e.Steps).Where(s => s.Type == "anim").Select(s => s.Name).Distinct().ToList();
            foreach (var n in names) CollectionAssert.Contains(EventSteps.Anims, n);
            Assert.IsTrue(names.Intersect(NpcDefinition.PoseNames).Any(), "scenes use the new poses");
        }
    }
}
