using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-07: the door swung when changing maps without going through a door (the road to the village, the mine entrance).
    public class DoorSwingTests
    {
        [Test]
        public void TheDoorSwings_OnlyWhenOneEndIsTheInsideOfABuilding()
        {
            Assert.IsTrue(Warp.ShowsDoor(MapIds.Farm, MapIds.FarmHouse), "into the house");
            Assert.IsTrue(Warp.ShowsDoor(MapIds.FarmHouse, MapIds.Farm), "and out again");
            Assert.IsTrue(Warp.ShowsDoor(MapIds.Village, MapIds.GeneralStore));
            Assert.IsTrue(Warp.ShowsDoor(MapIds.Saloon, MapIds.Village));
            Assert.IsTrue(Warp.ShowsDoor(MapIds.Farm, MapIds.Coop));
            Assert.IsTrue(Warp.ShowsDoor(MapIds.Barn, MapIds.Farm));
            Assert.IsTrue(Warp.ShowsDoor(MapIds.Farm, MapIds.Greenhouse));
        }

        [Test]
        public void NoDoorSwings_OnTheRoads_TheBeach_TheWoodsOrTheMine()
        {
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Farm, MapIds.Village), "the road from the farm to the village");
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Village, MapIds.Farm));
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Village, MapIds.Forest));
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Village, MapIds.Beach));
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Forest, MapIds.Mine), "into the mine");
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Mine, MapIds.Forest), "and out of it");
            Assert.IsFalse(Warp.ShowsDoor(MapIds.Forest, MapIds.Woods));
            Assert.IsFalse(Warp.ShowsDoor(null, MapIds.Village), "no game");
        }

        [Test]
        public void EveryBuildingInterior_IsAShippedMap()
        {
            foreach (var map in MapIds.Interiors) CollectionAssert.Contains(MapIds.All, map, map);
            Assert.IsFalse(MapIds.IsInterior(MapIds.Mine));
            Assert.IsFalse(MapIds.IsInterior(MapIds.Farm));
        }
    }
}
