using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-12 (version 0.5.0): buildings drawn over the player at their doors, no way to ask the carpenter for repairs in conversation, and farm buildings that looked
    // finished before they were built.
    public class PlaytestOct12Tests
    {
        [Test]
        public void ABuilding_IsOverAPlayerBehindIt_AndUnderOneInFrontOrInTheDoorway()
        {
            const float line = 10f;                                   // the top of the door's row
            Assert.IsTrue(BuildingDepth.IsOverPlayer(12.5f, line), "behind the building");
            Assert.IsFalse(BuildingDepth.IsOverPlayer(9.4f, line), "in front of it");
            Assert.IsFalse(BuildingDepth.IsOverPlayer(9.1f, line), "on the door's own cell");
        }

        [Test]
        public void ABuildingOnItsFront_IsDrawnUnderTheVillagersAndThePlayer()
        {
            Assert.Less(BuildingDepth.DefaultFront, 9, "villagers are at 9");
            Assert.Greater(BuildingDepth.DefaultFront, 4, "over what lies on the ground");
        }

        [Test]
        public void AFarmBuilding_IsDerelictUntilTheCarpenterHasBuiltIt()
        {
            foreach (var type in FarmBuildings.Types)
            {
                Assert.IsTrue(DerelictLook.IsDerelict(type, false), type.Id + " waits for the carpenter");
                Assert.IsFalse(DerelictLook.IsDerelict(type, true), type.Id + " is built");
            }
            Assert.IsFalse(DerelictLook.IsDerelict(null, false));
        }

        [Test]
        public void TheBoards_AreOneCellWideAndTwoTall()
        {
            var boards = DerelictLook.Boards();
            Assert.AreEqual(16f, boards.rect.width);
            Assert.AreEqual(32f, boards.rect.height);
            var pixels = boards.texture.GetPixels32();
            Assert.IsTrue(pixels.Any(p => p.a > 0), "something is drawn");
            Assert.IsTrue(pixels.Any(p => p.a == 0), "and the door shows round it");
            TestResets.RunAll();
        }

        [Test]
        public void TheCarpenter_HasAChoiceToAskForBuilding_AndNobodyElseDoes()
        {
            var carpenter = NpcDefinition.Create("marcus", Season.Summer, 9, MapIds.Carpenter, 6, 4, business: "carpenter");
            var other = NpcDefinition.Create("tilda", Season.Spring, 3, MapIds.Village, 5, 5, business: "library");
            var graph = InteractionMenu.WithBuilding(InteractionMenu.Minimal("marcus", null), carpenter);
            var menu = graph.Nodes.Find(n => n.Id == "menu");
            Assert.IsTrue(menu.Choices.Exists(c => c.Text == InteractionMenu.BuildKey && c.Effects.Contains("talk.build")));
            Assert.AreEqual(1, menu.Choices.Count(c => c.Text == InteractionMenu.BuildKey), "added once");
            InteractionMenu.WithBuilding(graph, carpenter);
            Assert.AreEqual(1, menu.Choices.Count(c => c.Text == InteractionMenu.BuildKey), "and not again");
            Assert.IsTrue(menu.Choices.Last().Default, "Goodbye stays last");

            var plain = InteractionMenu.WithBuilding(InteractionMenu.Minimal("tilda", null), other);
            Assert.IsFalse(plain.Nodes.Find(n => n.Id == "menu").Choices.Exists(c => c.Text == InteractionMenu.BuildKey));
        }
    }
}
