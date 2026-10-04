using System.Linq;
using Farm.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Farm.Tests
{
    // T-130: the slice villagers have a portrait for every expression a line can ask for, and any other face falls back to the base.
    public class ExpressionPortraitTests
    {
        static readonly string[] Slice = { "wren", "hazel", "bram", "tilda", "juno", "piper" };

        static NpcDefinition Npc(string id) => AssetDatabase.LoadAssetAtPath<NpcDefinition>($"Assets/_Project/Data/Npcs/{id}.asset");

        [Test]
        public void SliceVillagers_HaveAllFiveExpressionPortraits_AllDistinct()
        {
            foreach (var id in Slice)
            {
                var npc = Npc(id);
                Assert.IsNotNull(npc, id);
                var sprites = new[] { "happy", "sad", "surprised", "embarrassed", "thinking" }.Select(npc.PortraitFor).ToList();
                foreach (var s in sprites) { Assert.IsNotNull(s, id); Assert.AreNotSame(npc.Portrait, s, id + ": an expression, not the base"); }
                Assert.AreEqual(5, sprites.Distinct().Count(), id + ": five different faces");
            }
        }

        [Test]
        public void NeutralAndUnknownExpressions_FallBackToTheBasePortrait()
        {
            foreach (var id in Slice)
            {
                var npc = Npc(id);
                Assert.AreSame(npc.Portrait, npc.PortraitFor("neutral"), id);
                Assert.AreSame(npc.Portrait, npc.PortraitFor("no-such-face"), id);
                Assert.AreSame(npc.Portrait, npc.PortraitFor(null), id);
            }
        }

        [Test]
        public void ExpressionSprites_AreSquareThirtyTwo_AndListedAsFinalArt()
        {
            var listed = System.IO.File.ReadAllText("Assets/_Project/Art/Placeholders/final_art.txt");
            foreach (var id in Slice)
                foreach (var e in new[] { "happy", "sad", "surprised", "embarrassed", "thinking" })
                {
                    var name = $"ui_portrait_{id}_{e}";
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Placeholders/{name}.png");
                    Assert.IsNotNull(sprite, name);
                    Assert.AreEqual(32f, sprite.rect.width, name);
                    Assert.AreEqual(32f, sprite.rect.height, name);
                    StringAssert.Contains(name, listed, name + " is protected from regeneration");
                }
        }

        [Test]
        public void EveryExpressionNameALineUses_HasAPortraitForTheSliceVillagers()
        {
            // The lines of the slice villagers only ask for the names the game knows.
            var story = Farm.Gameplay.StoryContent.LoadFromResources();
            foreach (var d in story.Dialogues)
                foreach (var n in d.Nodes)
                    if (!string.IsNullOrEmpty(n.Expression))
                        Assert.Contains(n.Expression, NpcDefinition.ExpressionNames, $"{d.Id}/{n.Id}");
        }
    }
}
