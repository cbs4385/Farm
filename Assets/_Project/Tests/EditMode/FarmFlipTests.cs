using System.Linq;
using Farm.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Farm.Tests
{
    // The farm was mirrored (2026-10-10, a playtest: the house stood at the far end from the village): the house is by the road to the village, nothing overlaps it,
    // and a save made before the flip is mirrored with it.
    public class FarmFlipTests
    {
        [Test]
        public void TheFarmhouse_StandsByTheVillageEnd_AndTheDoorIsInsideItsFootprint()
        {
            Assert.Greater(FarmBuildings.HouseX0, MapLayout.FarmW / 2, "the house is on the village side of the farm");
            Assert.That(MapLayout.FarmHouseDoorX, Is.InRange(FarmBuildings.HouseX0, FarmBuildings.HouseX1));
            Assert.Less(MapLayout.FarmExitX - MapLayout.FarmHouseDoorX, 12, "a short walk from the door to the village road");
        }

        [Test]
        public void TheDefaultBuildings_StandWestOfTheHouse_InsideTheMap_WithoutTouchingEachOther()
        {
            var placed = FarmBuildings.Defaults();
            foreach (var b in placed)
            {
                var type = FarmBuildings.Type(b.TypeId);
                Assert.Less(b.X + type.W, FarmBuildings.HouseX0 - 1, b.TypeId + " clears the house");
                Assert.GreaterOrEqual(b.X, 1);
                var result = FarmBuildings.CanPlace(type, b.X, b.Y, MapLayout.FarmW, MapLayout.FarmH, placed, (x, y) => true);
                Assert.AreEqual(FarmBuildings.Placement.Ok, result, b.TypeId);
            }
        }

        static JObject OldSave() => JObject.Parse(@"{
            ""SaveVersion"": 1,
            ""Maps"": {
                ""Farm"": { ""Tiles"": [{""X"": 14, ""Y"": 29}], ""Nodes"": [{""X"": 3, ""Y"": 5}], ""Objects"": [{""X"": 20, ""Y"": 18}] },
                ""Village"": { ""Tiles"": [{""X"": 14, ""Y"": 29}], ""Nodes"": [], ""Objects"": [] }
            },
            ""FarmBuildings"": [{""TypeId"": ""greenhouse"", ""X"": 15, ""Y"": 20}, {""TypeId"": ""coop"", ""X"": 26, ""Y"": 20}, {""TypeId"": ""barn"", ""X"": 32, ""Y"": 20}],
            ""Fixtures"": [{""Map"": ""Farm"", ""Id"": ""shipping_bin"", ""X"": 13, ""Y"": 17}, {""Map"": ""Farm"", ""Id"": ""farm_hay"", ""X"": 13, ""Y"": 21}, {""Map"": ""FarmHouse"", ""Id"": ""bed"", ""X"": 1, ""Y"": 6}]
        }");

        [Test]
        public void AnOldSave_IsMirrored_ThePlacesOfTheOldDefaultsBecomeTheNewDefaults()
        {
            var save = OldSave();
            new FarmMirrorMigration().Migrate(save);

            Assert.AreEqual(73, (int)save["Maps"]["Farm"]["Tiles"][0]["X"], "tilled soil");
            Assert.AreEqual(84, (int)save["Maps"]["Farm"]["Nodes"][0]["X"], "a tree or rock");
            Assert.AreEqual(67, (int)save["Maps"]["Farm"]["Objects"][0]["X"], "a chest or machine");
            Assert.AreEqual(29, (int)save["Maps"]["Farm"]["Tiles"][0]["Y"], "the height does not change");
            Assert.AreEqual(14, (int)save["Maps"]["Village"]["Tiles"][0]["X"], "other maps are not touched");

            foreach (var b in save["FarmBuildings"])
            {
                var def = FarmBuildings.Defaults().First(d => d.TypeId == (string)b["TypeId"]);
                Assert.AreEqual(def.X, (int)b["X"], b["TypeId"] + ": the old default place is the new default place");
            }

            Assert.AreEqual(73, (int)save["Fixtures"][0]["X"], "the 2 x 2 bin keeps its left cell");
            Assert.AreEqual(74, (int)save["Fixtures"][1]["X"]);
            Assert.AreEqual(1, (int)save["Fixtures"][2]["X"], "a fixture in the farmhouse is not touched");
        }

        [Test]
        public void MirroringTwice_GivesTheSaveBack()
        {
            var save = OldSave();
            var migration = new FarmMirrorMigration();
            migration.Migrate(save);
            migration.Migrate(save);
            Assert.IsTrue(JToken.DeepEquals(OldSave()["FarmBuildings"], save["FarmBuildings"]));
            Assert.AreEqual(14, (int)save["Maps"]["Farm"]["Tiles"][0]["X"]);
        }

        [Test]
        public void ASavedGame_AtTheOldVersion_LoadsThroughTheRealSaveService_Mirrored()
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "farm-flip-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                var svc = new SaveService(root, new ISaveMigration[] { new FarmMirrorMigration() });
                var state = new GameState();
                svc.Save(0, state);
                var json = JObject.Parse(System.IO.File.ReadAllText(svc.SlotPath(0)));
                json["SaveVersion"] = 1;
                json["FarmBuildings"] = OldSave()["FarmBuildings"];
                System.IO.File.WriteAllText(svc.SlotPath(0), json.ToString());

                Assert.IsTrue(svc.TryLoad(0, out var loaded, out var error), error);
                Assert.AreEqual(GameState.CurrentVersion, loaded.SaveVersion);
                Assert.AreEqual(FarmBuildings.Greenhouse.DefaultX, FarmBuildings.Find(loaded, "greenhouse").X);
            }
            finally { if (System.IO.Directory.Exists(root)) System.IO.Directory.Delete(root, true); }
        }
    }
}
