using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The mine's dark and the torch (owner, 2026-10-11).
    public class TorchTests
    {
        [Test]
        public void TheMineIsATenthAsBright_AndBlue()
        {
            var c = DayNightLighting.Cave;
            Assert.AreEqual(0.1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)), 0.001f, "a tenth of the full light (owner, 2026-10-12): the torch is needed");
            Assert.Greater(c.b, c.g);
            Assert.Greater(c.g, c.r, "tinted blue");
        }

        [Test]
        public void TheTorchGlow_StaysOrangeRedAndYellow_AndMoves()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < 400; i++)
            {
                var g = TorchLight.GlowAt(i * 0.1f);
                Assert.GreaterOrEqual(g.r, 0.9f, "always warm");
                Assert.LessOrEqual(g.b, 0.35f);
                Assert.GreaterOrEqual(g.g, 0.2f);
                seen.Add(Mathf.RoundToInt(g.g * 20f));
            }
            Assert.GreaterOrEqual(seen.Count, 8, "the colour moves between red, orange and yellow");
            Assert.AreEqual(10f, TorchLight.Radius, "ten tiles: twice what it was (owner, 2026-10-16)");
        }

        [Test]
        public void ATorch_IsMadeFromWood_ByHand()
        {
            var recipe = CraftingDefaults.CreateRecipes().Single(r => r.OutputItemId == ItemIds.Torch);
            Assert.AreEqual(Stations.Hand, recipe.Station);
            CollectionAssert.AreEqual(new[] { ItemIds.Wood }, recipe.Ingredients.Select(i => i.ItemId));
            Assert.IsTrue(CraftingDefaults.CreateItems().Any(i => i.Id == ItemIds.Torch));
        }
    }
}
