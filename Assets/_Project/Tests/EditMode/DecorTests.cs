using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Furniture and decorations (playtest request, 2026-10-04): twelve pieces sold at the carpenter's, set down in the house or on the farm,
    // picked up again, and never in a doorway.
    public class DecorTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void EveryPiece_IsAFurnitureItem_WithAPlaceable_ASprite_AnIcon_AndATextForSale()
        {
            var db = RealDb();
            var en = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            Assert.AreEqual(12, CraftingDefaults.Decor.Length);
            foreach (var (id, price) in CraftingDefaults.Decor)
            {
                Assert.IsTrue(db.TryGetItem(ItemIds.Machine(id), out var item), id);
                Assert.AreEqual(ItemCategory.Furniture, item.Category, id);
                Assert.AreEqual(id, item.PlaceableId, id);
                Assert.AreEqual(price, item.BuyPrice, id);
                Assert.Contains("carpenter", item.SoldIn.ToList(), id + " is sold by the carpenter");
                Assert.IsNotNull(item.Icon, id + " icon");
                var def = db.AllPlaceables.FirstOrDefault(p => p.Id == id);
                Assert.IsNotNull(def, id + " placeable");
                Assert.AreEqual(PlaceableKind.Decor, def.Kind, id);
                Assert.IsNotNull(def.Sprite, id + " world sprite");
                Assert.IsTrue(en.ContainsKey($"item.machine.{id}.name") && en.ContainsKey($"item.machine.{id}.desc"), id);
            }
        }

        [Test]
        public void OnlyTheRug_CanBeWalkedOver()
        {
            var db = RealDb();
            foreach (var (id, _) in CraftingDefaults.Decor)
                Assert.AreEqual(id == "rug", db.AllPlaceables.First(p => p.Id == id).Walkable, id);
        }

        [Test]
        public void FurnitureGoesOnlyInTheFarmhouseAndOnTheFarm()
        {
            Assert.IsTrue(DecorRules.AllowedOn(MapIds.FarmHouse));
            Assert.IsTrue(DecorRules.AllowedOn(MapIds.Farm));
            foreach (var map in new[] { MapIds.Village, MapIds.Saloon, MapIds.Forest, MapIds.Beach, MapIds.CommunityHall, MapIds.Barn })
                Assert.IsFalse(DecorRules.AllowedOn(map), map);
        }

        [Test]
        public void NothingBlocksADoorway_ButARugMayLieNextToOne()
        {
            var door = new Vector3Int(5, 0, 0);
            var doors = new[] { door };
            Assert.IsTrue(DecorRules.BlocksDoor(door, true, doors), "not even a rug on the door cell");
            Assert.IsTrue(DecorRules.BlocksDoor(door, false, doors));
            Assert.IsTrue(DecorRules.BlocksDoor(new Vector3Int(5, 1, 0), false, doors), "solid furniture right in front of the door");
            Assert.IsTrue(DecorRules.BlocksDoor(new Vector3Int(4, 1, 0), false, doors), "or diagonally beside it");
            Assert.IsFalse(DecorRules.BlocksDoor(new Vector3Int(5, 1, 0), true, doors), "a rug can lie in front of a door");
            Assert.IsFalse(DecorRules.BlocksDoor(new Vector3Int(5, 3, 0), false, doors), "two cells away is fine");
            Assert.IsFalse(DecorRules.BlocksDoor(new Vector3Int(5, 1, 0), false, new Vector3Int[0]));
        }

        [Test]
        public void PickingFurnitureUp_PutsItBackInThePack_AndFreesTheCell_ButNotIntoAFullPack()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                var grid = s.GetObjects(MapIds.FarmHouse);
                var def = s.Placeables.Get("bookshelf");
                Assert.IsNotNull(def, "the session knows the furniture");
                grid.Place(def, 4, 4, "shelf");
                Assert.IsTrue(grid.Has(4, 4));
                var before = s.Backpack.Count(def.ItemId);
                Assert.AreEqual(PlacedInteractions.PickUpResult.Ok, PlacedInteractions.PickUp(s, MapIds.FarmHouse, 4, 4));
                Assert.AreEqual(before + 1, s.Backpack.Count(def.ItemId));
                Assert.IsFalse(grid.Has(4, 4), "the cell is free again");

                // A full pack keeps the piece where it stands.
                grid.Place(def, 5, 5, "shelf2");
                s.Backpack.Remove(def.ItemId, s.Backpack.Count(def.ItemId));      // (a shelf already in the pack would take the piece into its stack)
                for (var i = 0; i < s.Backpack.Capacity; i++) s.Backpack.Add("filler" + i, 1);
                Assert.AreEqual(PlacedInteractions.PickUpResult.NoRoom, PlacedInteractions.PickUp(s, MapIds.FarmHouse, 5, 5));
                Assert.IsTrue(grid.Has(5, 5));
            }
        }
    }
}
