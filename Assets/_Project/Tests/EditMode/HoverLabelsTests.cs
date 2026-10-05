using System;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest feedback: not every icon was obvious (the shipping bin). Everything the player can use names itself when the mouse is over it.
    public class HoverLabelsTests
    {
        // Optional-layer objects name themselves with their own text, and have their own tests.
        static bool IsBase(Type t) => t.Namespace == "Farm.Gameplay" && t.Assembly.GetName().Name == "Farm.Gameplay";

        [Test]
        public void EveryBaseInteractable_DeclaresAHoverLabel()
        {
            var types = typeof(IInteractable).Assembly.GetTypes()
                .Where(t => typeof(IInteractable).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && IsBase(t)).ToList();
            Assert.GreaterOrEqual(types.Count, 12, "the interactables of the base game");
            foreach (var t in types)
                Assert.IsNotNull(t.GetProperty("HoverLabel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly), t.Name + " has no HoverLabel");
        }

        [Test]
        public void TheLabelTexts_Exist_AndAreShortPlainSentences()
        {
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var key in new[] { "hover.shipping_bin", "hover.bed", "hover.mailbox", "hover.kitchen", "hover.hall_board", "hover.help_board", "hover.shop", "hover.upgrades", "hover.trough", "hover.stairs", "hover.door", "options.hover_labels" })
            {
                Assert.IsTrue(en.ContainsKey(key), key);
                Assert.LessOrEqual(en[key].Length, 80, key + " fits next to the pointer");
                Assert.IsFalse(en[key].Contains("!"), key);
            }
            StringAssert.Contains("{0}", en["hover.door"]);
        }
    }
}
