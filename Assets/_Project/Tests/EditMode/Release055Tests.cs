using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-13: a building put down on the player walled them in, the farm's props would not go into the backpack, and a level-up choice said nothing about what it did.
    public class Release055Tests
    {
        [Test]
        public void ABuilding_MayNotBePutDownOnThePlayer_ButTheDoorCellIsOpen()
        {
            var barn = FarmBuildings.Barn;                                    // 7 wide, 5 high, the door 3 cells in
            var door = new Vector3Int(51, 18, 0);                             // so the barn covers x 48 to 54, y 18 to 22
            Assert.IsTrue(HammerMode.StandsInside(barn, door, new Vector3Int(51, 19, 0)), "directly above the door: the case in the bug report");
            Assert.IsTrue(HammerMode.StandsInside(barn, door, new Vector3Int(48, 18, 0)), "the left corner");
            Assert.IsTrue(HammerMode.StandsInside(barn, door, new Vector3Int(54, 22, 0)), "the right roof corner");
            Assert.IsFalse(HammerMode.StandsInside(barn, door, door), "on the door cell: it is open");
            Assert.IsFalse(HammerMode.StandsInside(barn, door, new Vector3Int(51, 17, 0)), "in front of the door");
            Assert.IsFalse(HammerMode.StandsInside(barn, door, new Vector3Int(47, 18, 0)), "beside it");
            Assert.IsFalse(HammerMode.StandsInside(barn, door, new Vector3Int(51, 23, 0)), "behind it");
        }

        [Test]
        public void TheFarmsProps_AreItemsAndDecorations_ThatAreNotSold()
        {
            var items = CraftingDefaults.CreateItems();
            var placeables = CraftingDefaults.CreatePlaceables();
            foreach (var id in CraftingDefaults.FarmProps)
            {
                var item = items.Single(i => i.Id == ItemIds.Machine(id));
                Assert.AreEqual(ItemCategory.Furniture, item.Category, id);
                Assert.AreEqual(id, item.PlaceableId, id);
                Assert.IsFalse(item.SoldIn != null && item.SoldIn.Contains("carpenter"), id + " is not sold: the scene's own prop becomes it");
                var placeable = placeables.Single(p => p.Id == id);
                Assert.AreEqual(PlaceableKind.Decor, placeable.Kind, id);
                Assert.IsTrue(HammerMode.TakesToBackpack(placeable.Kind));
            }
        }

        [Test]
        public void EveryProfessionHasAWordedEffect_SoThatTheChoiceCanBeSeen()
        {
            var path = Path.Combine(Application.dataPath, "_Project", "Resources", "Localization", "en.json");
            var text = File.ReadAllText(path);
            foreach (var effect in System.Enum.GetValues(typeof(ProfessionEffect)))
                StringAssert.Contains("\"profession.effect." + effect + "\"", text, "the text for " + effect);
            foreach (var row in Professions.Rows) Assert.IsFalse(string.IsNullOrEmpty(Professions.Describe(row)), row.Id);
        }
    }
}
