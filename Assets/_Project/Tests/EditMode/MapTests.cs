using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Tests
{
    // T-031: the generated map scenes are consistent: every warp round-trips, every door has a spawn, and nothing the
    // player needs is walled in.
    public class MapTests
    {
        const string SceneDir = "Assets/_Project/Scenes";

        sealed class WarpInfo
        {
            public Vector3 Pos; public Vector2 Size;
            public string Target, Spawn, Condition, Business, BlockedKey;
        }

        sealed class SpawnInfo { public string Id; public Vector3 Pos; }
        sealed class UseInfo { public string Name; public Vector3 Pos; }

        sealed class GateInfo { public string Condition; public bool Invert; public int Children; public bool AllSolid; }

        // A plain-data snapshot of a scene (scenes are opened one at a time: URP objects to several global lights).
        sealed class Info
        {
            public string Id;
            public bool AllowFarming, Indoor;
            public int Players, Lights, Beds, Bins;
            public List<SpawnInfo> Spawns = new List<SpawnInfo>();
            public List<WarpInfo> Warps = new List<WarpInfo>();
            public List<UseInfo> Uses = new List<UseInfo>();
            public List<string> ShopIds = new List<string>();
            public List<GateInfo> Gates = new List<GateInfo>();
            public HashSet<Vector3Int> Ground = new HashSet<Vector3Int>();
            public HashSet<Vector3Int> Walls = new HashSet<Vector3Int>();
            public HashSet<Vector3Int> Blocked = new HashSet<Vector3Int>();
        }

        static readonly Dictionary<string, Info> Maps = new Dictionary<string, Info>();

        static Vector3Int Cell(Vector3 p) => new Vector3Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), 0);

        static IEnumerable<T> All<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));

        static void Fill(HashSet<Vector3Int> set, Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
                if (map.GetTile(pos) != null) set.Add(new Vector3Int(pos.x, pos.y, 0));
        }

        [OneTimeSetUp]
        public void LoadScenes()
        {
            foreach (var id in MapIds.All)
            {
                var scene = EditorSceneManager.OpenScene($"{SceneDir}/{id}.unity", OpenSceneMode.Single);
                var map = All<FarmMap>(scene).Single();
                var info = new Info
                {
                    Id = map.MapId,
                    AllowFarming = map.AllowFarming,
                    Indoor = All<DayNightLighting>(scene).Single().IsIndoor,
                    Players = All<PlayerController>(scene).Count(),
                    Lights = All<DayNightLighting>(scene).Count(),
                    Beds = All<Bed>(scene).Count(),
                    Bins = All<ShippingBin>(scene).Count(),
                };
                foreach (var sp in All<SpawnPoint>(scene)) info.Spawns.Add(new SpawnInfo { Id = sp.Id, Pos = sp.transform.position });
                foreach (var w in All<Warp>(scene))
                    info.Warps.Add(new WarpInfo
                    {
                        Pos = w.transform.position, Size = w.GetComponent<BoxCollider2D>().size, Target = w.TargetMap,
                        Spawn = w.TargetSpawn, Condition = w.Condition, Business = w.BusinessId, BlockedKey = w.BlockedMessageKey,
                    });
                foreach (var c in All<ShopCounter>(scene)) info.ShopIds.Add(c.ShopId);
                foreach (var u in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<IInteractable>(true)).Cast<Component>()) info.Uses.Add(new UseInfo { Name = u.name, Pos = u.transform.position });
                foreach (var g in All<ConditionalObject>(scene))
                    info.Gates.Add(new GateInfo
                    {
                        Condition = g.Condition, Invert = g.Invert, Children = g.transform.childCount,
                        AllSolid = g.transform.Cast<Transform>().All(t => !t.GetComponent<Collider2D>().isTrigger),
                    });

                var tilemaps = All<Tilemap>(scene).ToArray();
                Fill(info.Ground, tilemaps.Single(t => t.name == "Ground"));
                Fill(info.Walls, tilemaps.Single(t => t.name == "Walls"));
                foreach (var col in All<BoxCollider2D>(scene))
                    if (!col.isTrigger && !col.CompareTag("Player") && col.GetComponentInParent<Tilemap>() == null)
                        info.Blocked.Add(Cell(col.transform.position));
                Maps[id] = info;
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [OneTimeTearDown]
        public void Clear() => Maps.Clear();

        // ---- structure --------------------------------------------------------------------------------------------

        [Test]
        public void EveryMapScene_IsInTheBuild()
        {
            var inBuild = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToList();
            foreach (var id in MapIds.All) CollectionAssert.Contains(inBuild, id);
        }

        [Test]
        public void EveryMap_HasItsIdAPlayerADefaultSpawnAndLighting()
        {
            foreach (var pair in Maps)
            {
                var info = pair.Value;
                Assert.AreEqual(pair.Key, info.Id, "the scene's map id matches its name");
                Assert.IsTrue(info.Spawns.Any(s => s.Id == "default"), $"{info.Id}: default spawn");
                Assert.AreEqual(1, info.Players, info.Id);
                Assert.AreEqual(1, info.Lights, info.Id);
                CollectionAssert.AllItemsAreUnique(info.Spawns.Select(s => s.Id).ToList(), $"{info.Id}: spawn ids are unique");
            }
        }

        [Test]
        public void OnlyTheFarmAllowsFarming()
        {
            foreach (var info in Maps.Values) Assert.AreEqual(info.Id == MapIds.Farm, info.AllowFarming, info.Id);
        }

        [Test]
        public void InteriorsAreIndoor_AndOutdoorMapsAreNot()
        {
            var indoor = new[] { MapIds.FarmHouse, MapIds.GeneralStore, MapIds.Blacksmith, MapIds.Carpenter, MapIds.Saloon, MapIds.Clinic, MapIds.Library };
            foreach (var info in Maps.Values) Assert.AreEqual(indoor.Contains(info.Id), info.Indoor, info.Id);
        }

        // ---- warps ------------------------------------------------------------------------------------------------

        [Test]
        public void EveryWarp_LeadsToARealSpawn_AndRoundTrips()
        {
            foreach (var info in Maps.Values)
                foreach (var warp in info.Warps)
                {
                    if (warp.Target == MapIds.Woods) continue;   // the gated slot, tested below
                    Assert.IsTrue(Maps.TryGetValue(warp.Target, out var target), $"{info.Id} warps to unknown map {warp.Target}");
                    Assert.IsTrue(target.Spawns.Any(s => s.Id == warp.Spawn), $"{info.Id} -> {warp.Target}: no spawn {warp.Spawn}");
                    Assert.IsTrue(target.Warps.Any(w => w.Target == info.Id), $"{warp.Target} has no way back to {info.Id}");
                }
        }

        [Test]
        public void ArrivingThroughAWarp_PutsThePlayerOutsideTheReturnTrigger()
        {
            foreach (var info in Maps.Values)
                foreach (var warp in info.Warps)
                {
                    if (warp.Target == MapIds.Woods) continue;
                    var target = Maps[warp.Target];
                    var spawn = target.Spawns.First(s => s.Id == warp.Spawn);
                    foreach (var back in target.Warps)
                    {
                        var bounds = new Bounds(back.Pos, back.Size);
                        Assert.IsFalse(bounds.Contains(spawn.Pos),
                            $"{warp.Target}/{spawn.Id} lands inside the {back.Target} warp: the player would bounce straight back");
                    }
                }
        }

        [Test]
        public void EveryWarpCondition_IsAValidExpression()
        {
            foreach (var info in Maps.Values)
                foreach (var warp in info.Warps.Where(w => !string.IsNullOrWhiteSpace(w.Condition)))
                {
                    Assert.IsTrue(Conditions.Validate(warp.Condition, out var error), $"{info.Id}: {warp.Condition}: {error}");
                    Assert.IsNotEmpty(warp.BlockedKey, $"{info.Id}: a gated warp explains itself");
                }
        }

        // ---- the village and its businesses -----------------------------------------------------------------------

        [Test]
        public void EveryVillageBuilding_HasADoorWithHours_AndAnInteriorBackToIt()
        {
            BusinessHoursRegistry.RegisterDefaults();
            var doors = Maps[MapIds.Village].Warps.Where(w => !string.IsNullOrEmpty(w.Business)).ToList();
            CollectionAssert.AreEquivalent(
                new[] { MapIds.GeneralStore, MapIds.Blacksmith, MapIds.Carpenter, MapIds.Saloon, MapIds.Clinic, MapIds.Library },
                doors.Select(d => d.Target).ToArray());
            foreach (var door in doors)
            {
                Assert.IsTrue(BusinessHoursRegistry.TryGet(door.Business, out _), $"{door.Business} has no registered hours");
                Assert.IsTrue(Maps[door.Target].Warps.Any(w => w.Target == MapIds.Village && w.Spawn == "from" + door.Target));
            }
        }

        [Test]
        public void TheGeneralStoreLivesInTheVillage_NotOnTheFarm()
        {
            Assert.IsNotEmpty(Maps[MapIds.GeneralStore].ShopIds);
            foreach (var id in Maps[MapIds.GeneralStore].ShopIds) Assert.AreEqual("general", id);
            Assert.IsEmpty(Maps[MapIds.Farm].ShopIds);
            Assert.AreEqual(1, Maps[MapIds.Farm].Bins, "the bin stays on the farm");
            Assert.AreEqual(1, Maps[MapIds.FarmHouse].Beds);
        }

        [Test]
        public void TheFishStallOnTheBeach_KeepsTheFishShopHours()
        {
            BusinessHoursRegistry.RegisterDefaults();
            CollectionAssert.AreEqual(new[] { "fish" }, Maps[MapIds.Beach].ShopIds);
            Assert.IsTrue(BusinessHoursRegistry.TryGet("fish", out _));
        }

        // ---- the gated slot in the forest -------------------------------------------------------------------------

        [Test]
        public void TheForestGate_IsBlockedByBrambles_UntilTheWoodsFlagIsSet()
        {
            var forest = Maps[MapIds.Forest];
            var gate = forest.Warps.Single(w => w.Target == MapIds.Woods);
            Assert.AreEqual("flag:" + MapIds.WoodsOpenFlag, gate.Condition);
            Assert.IsFalse(Maps.ContainsKey(MapIds.Woods), "the Woods scene is not part of the base game");

            var conditional = forest.Gates.Single();
            Assert.AreEqual("flag:" + MapIds.WoodsOpenFlag, conditional.Condition);
            Assert.IsTrue(conditional.Invert, "brambles show while the flag is off");
            Assert.GreaterOrEqual(conditional.Children, 3);
            Assert.IsTrue(conditional.AllSolid);
        }

        // ---- no stuck spots ---------------------------------------------------------------------------------------

        static readonly Vector3Int[] Steps = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        static bool Solid(Info info, Vector3Int cell) =>
            !info.Ground.Contains(cell) || info.Walls.Contains(cell) || info.Blocked.Contains(cell);

        static HashSet<Vector3Int> Reachable(Info info, Vector3 from)
        {
            var start = Cell(from);
            var seen = new HashSet<Vector3Int> { start };
            var queue = new Queue<Vector3Int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var d in Steps)
                {
                    var n = c + d;
                    if (seen.Contains(n) || Solid(info, n)) continue;
                    seen.Add(n);
                    queue.Enqueue(n);
                }
            }
            return seen;
        }

        [Test]
        public void NothingThePlayerNeedsIsWalledIn()
        {
            foreach (var info in Maps.Values)
            {
                var start = info.Spawns.First(s => s.Id == "default").Pos;
                Assert.IsFalse(Solid(info, Cell(start)), $"{info.Id}: the default spawn is not in a wall");
                var reachable = Reachable(info, start);

                foreach (var spawn in info.Spawns)
                    Assert.IsTrue(reachable.Contains(Cell(spawn.Pos)), $"{info.Id}: spawn {spawn.Id} cannot be reached from the default spawn");

                foreach (var warp in info.Warps.Where(w => string.IsNullOrWhiteSpace(w.Condition)))
                {
                    var center = Cell(warp.Pos);
                    var touches = false;
                    for (var dx = -(int)warp.Size.x; dx <= (int)warp.Size.x && !touches; dx++)
                        for (var dy = -(int)warp.Size.y; dy <= (int)warp.Size.y && !touches; dy++)
                            touches = reachable.Contains(center + new Vector3Int(dx, dy, 0));
                    Assert.IsTrue(touches, $"{info.Id}: the warp to {warp.Target} cannot be reached");
                }

                // Counters, the bin and the bed are used from a neighbouring tile.
                foreach (var use in info.Uses)
                    Assert.IsTrue(Steps.Any(d => reachable.Contains(Cell(use.Pos) + d)), $"{info.Id}: {use.Name} cannot be reached");
            }
        }

        [Test]
        public void TheVillageIsOneConnectedTown()
        {
            var info = Maps[MapIds.Village];
            var reachable = Reachable(info, info.Spawns.First(s => s.Id == "default").Pos);
            foreach (var spawn in info.Spawns) Assert.IsTrue(reachable.Contains(Cell(spawn.Pos)), spawn.Id);
            Assert.Greater(reachable.Count, 700, "plenty of open ground to walk on");
        }
    }
}
