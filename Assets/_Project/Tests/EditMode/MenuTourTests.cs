using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;

namespace Farm.Tests
{
    // The guided tour of the game menu (owner, 2026-10-09): every tab has a short description, the Journal comes first, and no tab is left out.
    public class MenuTourTests
    {
        [SetUp]
        public void SetUp() => L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));

        [Test]
        public void TheTour_StartsWithTheJournal_AndCoversEveryTabOnce()
        {
            Assert.AreEqual(MenuTabs.Journal, MenuTourScreen.Order[0]);
            var all = typeof(MenuTabs).GetFields().Select(f => (string)f.GetRawConstantValue()).OrderBy(x => x).ToList();
            CollectionAssert.AreEqual(all, MenuTourScreen.Order.OrderBy(x => x).ToList(), "every tab of the menu is described, once");
        }

        [Test]
        public void EveryTab_HasAShortDescription_AndTheTourHasItsWords()
        {
            foreach (var id in MenuTourScreen.Order)
            {
                Assert.IsTrue(L.Has("tour." + id), id);
                Assert.IsTrue(L.Has("menu.tab." + id), id);
                Assert.Less(L.Get("tour." + id).Length, 260, id + ": brief");
            }
            foreach (var key in new[] { "tour.step", "tour.next", "tour.back", "tour.done", "tour.skip", "tour.end" }) Assert.IsTrue(L.Has(key), key);
            Assert.AreNotEqual("tour.end", L.Get("tour.end"));
            StringAssert.DoesNotContain("[[", L.Get("tour.end"), "the control names are filled in");
        }

        [Test]
        public void EveryPartOfTheScreen_HasACaptionAndATitle_WithTheControlNamesFilledIn()
        {
            foreach (var id in MenuTourScreen.HudSteps)
            {
                Assert.IsTrue(L.Has("tour.hud." + id), id);
                Assert.IsTrue(L.Has("tour.hud." + id + ".title"), id + " title");
                StringAssert.DoesNotContain("[[", L.Get("tour.hud." + id), id);
            }
            var controls = L.Get("tour.hud.controls");
            foreach (var word in new[] { "Move", "Use", "Talk", "Backpack", "Menu", "Journal", "Pause" }) StringAssert.Contains(word, controls, "the controls call out " + word);
        }

        [Test]
        public void TheWalkthrough_CoversTheStatusBarTheGoldTheEnergyTheItemBarTheQuestsAndTheControls_InThatOrder()
        {
            CollectionAssert.AreEqual(new[] { "statusbar", "gold", "energy", "hotbar", "tracker", "controls" }, MenuTourScreen.HudSteps);
        }
    }
}
