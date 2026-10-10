using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The reports of 2026-10-10.
    public class PlaytestOct10Tests
    {
        [TestCase(MapIds.Village)]
        [TestCase(MapIds.Farm)]
        [TestCase(MapIds.Forest)]
        public void Every_pond_can_be_fished(string map) => Assert.AreEqual(FishSpots.Pond, FishSpots.ForMap(map));

        [Test]
        public void The_beach_is_still_the_ocean_and_a_shop_has_no_fish() => Assert.AreEqual(FishSpots.Ocean, FishSpots.ForMap(MapIds.Beach));

        [Test]
        public void Shells_on_the_beach_are_to_be_left_for_others_and_plants_so_that_more_can_grow()
        {
            Assert.AreEqual("toast.forage_last.shore", ForageRules.ReminderKey("toast.forage_last", "forage.seashell"));
            Assert.AreEqual("hover.forage.shore", ForageRules.ReminderKey("hover.forage", "forage.clam"));
            Assert.AreEqual("toast.forage_last", ForageRules.ReminderKey("toast.forage_last", "forage.dandelion"));
            foreach (var key in new[] { "toast.forage_last.shore", "hover.forage.shore", "hover.forage_last.shore" })
                StringAssert.Contains("for others", L.Get(key, "x"));
        }

        [Test]
        public void The_input_asset_has_a_right_click_for_windows()
        {
            var asset = UnityEngine.Resources.Load<UnityEngine.InputSystem.InputActionAsset>(InputNames.ResourcePath);
            var action = asset.FindAction($"{InputNames.UiMap}/{InputNames.RightClick}");
            Assert.IsNotNull(action, "without it a right-click never reaches a window");
            Assert.IsTrue(action.bindings[0].path.Contains("rightButton"));
        }
    }
}
