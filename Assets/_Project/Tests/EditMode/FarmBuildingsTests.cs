using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The farm's greenhouse, coop and barn are saved data with a footprint and a door, so that the player can put them where they like.
    public class FarmBuildingsTests
    {
        static bool AllOpen(int x, int y) => true;

        [Test]
        public void TheDefaults_PutTheDoorsWhereTheyAlwaysWere()
        {
            var d = FarmBuildings.Defaults();
            (int x, int y) Door(string id) => FarmBuildings.DoorCell(FarmBuildings.Type(id), d.First(b => b.TypeId == id));
            Assert.AreEqual((19, 20), Door("greenhouse"));
            Assert.AreEqual((28, 20), Door("coop"));
            Assert.AreEqual((35, 20), Door("barn"));
        }

        [Test]
        public void EnsureDefaults_SeedsAnOldSave_AndKeepsWhereTheBuildingsWereMoved()
        {
            var state = new GameState();
            FarmBuildings.EnsureDefaults(state);
            Assert.AreEqual(3, state.FarmBuildings.Count);
            FarmBuildings.Find(state, "coop").X = 40;
            FarmBuildings.EnsureDefaults(state);
            Assert.AreEqual(3, state.FarmBuildings.Count, "not added twice");
            Assert.AreEqual(40, FarmBuildings.Find(state, "coop").X, "a moved building stays moved");
        }

        [Test]
        public void TheFootprint_HasARoofOnTop_AndTheDoorInTheBottomRow()
        {
            foreach (var type in FarmBuildings.Types)
            {
                var at = new FarmBuildingState { TypeId = type.Id, X = 10, Y = 30 };
                Assert.IsTrue(FarmBuildings.Covers(type, at, 10, 30));
                Assert.IsTrue(FarmBuildings.Covers(type, at, 10 + type.W - 1, 30 + type.H - 1));
                Assert.IsFalse(FarmBuildings.Covers(type, at, 10 + type.W, 30));
                Assert.IsTrue(FarmBuildings.IsRoof(type, at, 30 + type.H - 1));
                Assert.IsFalse(FarmBuildings.IsRoof(type, at, 30));
                Assert.AreEqual(30, FarmBuildings.DoorCell(type, at).y);
                Assert.IsTrue(type.DoorX > 0 && type.DoorX < type.W - 1, type.Id + ": the door is not in a corner");
            }
        }

        [Test]
        public void CanPlace_AcceptsOpenGround_AndExplainsEveryRefusal()
        {
            var coop = FarmBuildings.Coop;
            var others = FarmBuildings.Defaults();
            Assert.AreEqual(FarmBuildings.Placement.Ok, FarmBuildings.CanPlace(coop, 50, 30, 88, 64, others, AllOpen));
            Assert.AreEqual(FarmBuildings.Placement.OffMap, FarmBuildings.CanPlace(coop, 86, 30, 88, 64, others, AllOpen), "runs off the east edge");
            Assert.AreEqual(FarmBuildings.Placement.OffMap, FarmBuildings.CanPlace(coop, 50, 1, 88, 64, others, AllOpen), "no room in front of the door");
            Assert.AreEqual(FarmBuildings.Placement.OnHouse, FarmBuildings.CanPlace(coop, 8, 24, 88, 64, others, AllOpen));
            Assert.AreEqual(FarmBuildings.Placement.OnBuilding, FarmBuildings.CanPlace(coop, 33, 20, 88, 64, others, AllOpen), "on the barn");
            Assert.AreEqual(FarmBuildings.Placement.OnBuilding, FarmBuildings.CanPlace(coop, 22, 20, 88, 64, others, AllOpen), "touching the greenhouse leaves no cell between");
            Assert.AreEqual(FarmBuildings.Placement.Blocked, FarmBuildings.CanPlace(coop, 50, 30, 88, 64, others, (x, y) => !(x == 52 && y == 32)), "a tree in the middle");
            Assert.AreEqual(FarmBuildings.Placement.NoWayIn, FarmBuildings.CanPlace(coop, 50, 30, 88, 64, others, (x, y) => !(x == 52 && y == 28)), "a boulder at the door");
        }

        [Test]
        public void ABuildingBeingMoved_DoesNotBlockItself()
        {
            var coop = FarmBuildings.Coop;
            var others = FarmBuildings.Defaults();
            Assert.AreEqual(FarmBuildings.Placement.Ok, FarmBuildings.CanPlace(coop, 26, 27, 88, 64, others, AllOpen), "just north of where it stands: its own old cells do not count");
        }

        [Test]
        public void TheMovableBuildings_AreNotOnTheFixedRoutes()
        {
            CollectionAssert.AreEquivalent(new[] { MapIds.Greenhouse, MapIds.Coop, MapIds.Barn }, new List<string>(FarmBuildings.Types.Select(t => t.InteriorMap)));
            Assert.IsTrue(FarmBuildings.IsInteriorMap(MapIds.Coop));
            Assert.IsFalse(FarmBuildings.IsInteriorMap(MapIds.Village));
        }
    }
}
