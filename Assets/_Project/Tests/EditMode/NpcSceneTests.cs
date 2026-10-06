using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Tests
{
    // T-035: the route table and the villagers' stops agree with the real map scenes: doors are where the table says,
    // every stop is on free floor, and every walk between consecutive stops has a path.
    public class NpcSceneTests
    {
        const string SceneDir = "Assets/_Project/Scenes";

        sealed class Snapshot
        {
            public HashSet<Vector2Int> Ground = new HashSet<Vector2Int>();
            public HashSet<Vector2Int> Walls = new HashSet<Vector2Int>();
            public HashSet<Vector2Int> Blocked = new HashSet<Vector2Int>();
            public List<(Vector2Int cell, string target, string spawn)> Warps = new List<(Vector2Int, string, string)>();
            public Dictionary<string, Vector2Int> Spawns = new Dictionary<string, Vector2Int>();
            public WalkGrid Grid;
            public bool Walkable(int x, int y) => Ground.Contains(new Vector2Int(x, y)) && !Walls.Contains(new Vector2Int(x, y)) && !Blocked.Contains(new Vector2Int(x, y));
        }

        static readonly Dictionary<string, Snapshot> Maps = new Dictionary<string, Snapshot>();

        static Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));

        static IEnumerable<T> All<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));

        static void Fill(HashSet<Vector2Int> set, Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
                if (map.GetTile(pos) != null) set.Add(new Vector2Int(pos.x, pos.y));
        }

        [OneTimeSetUp]
        public void LoadScenes()
        {
            foreach (var id in MapIds.All)
            {
                var scene = EditorSceneManager.OpenScene($"{SceneDir}/{id}.unity", OpenSceneMode.Single);
                var snap = new Snapshot();
                foreach (var sp in All<SpawnPoint>(scene)) snap.Spawns[sp.Id] = Cell(sp.transform.position);
                foreach (var w in All<Warp>(scene)) snap.Warps.Add((Cell(w.transform.position), w.TargetMap, w.TargetSpawn));
                var tilemaps = All<Tilemap>(scene).ToArray();
                Fill(snap.Ground, tilemaps.Single(t => t.name == "Ground"));
                Fill(snap.Walls, tilemaps.Single(t => t.name == "Walls"));
                foreach (var col in All<BoxCollider2D>(scene))
                    if (!col.isTrigger && !col.CompareTag("Player") && col.GetComponentInParent<Tilemap>() == null)
                    {
                        // every cell the collider covers (a wide counter or a 2 x 2 bin covers several)
                        var b = col.bounds;
                        for (var x = Mathf.FloorToInt(b.min.x + 0.01f); x <= Mathf.FloorToInt(b.max.x - 0.01f); x++)
                            for (var y = Mathf.FloorToInt(b.min.y + 0.01f); y <= Mathf.FloorToInt(b.max.y - 0.01f); y++)
                                snap.Blocked.Add(new Vector2Int(x, y));
                    }
                var min = new Vector2Int(snap.Ground.Min(c => c.x), snap.Ground.Min(c => c.y));
                var max = new Vector2Int(snap.Ground.Max(c => c.x), snap.Ground.Max(c => c.y));
                snap.Grid = new WalkGrid(min.x, min.y, max.x - min.x + 1, max.y - min.y + 1, snap.Walkable);
                Maps[id] = snap;
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [OneTimeTearDown]
        public void Clear() => Maps.Clear();

        // The shipped villager assets (what the game runs), not the defaults they are generated from.
        static IEnumerable<Farm.Data.NpcDefinition> Npcs() =>
            NpcCatalog.From(Resources.Load<Farm.Data.GameDatabase>(Farm.Data.GameDatabase.ResourcePath)).All;

        static string Describe(Farm.Data.NpcDefinition npc) => string.Join(";", npc.Schedule.Select(e =>
            $"{e.Id}|{e.Condition ?? string.Empty}|{e.Priority}|" + string.Join(",", e.Stops.Select(s => $"{s.Minute}@{s.Map}:{s.X},{s.Y}:{s.Facing}"))));

        [Test]
        public void TheShippedNpcAssets_MatchTheDefaultsTheyAreGeneratedFrom()
        {
            // Fails when NpcDefaults changed but Farm/Setup/Generate Content was not run again.
            foreach (var fresh in NpcDefaults.CreateAll())
            {
                var shipped = Npcs().Single(n => n.Id == fresh.Id);
                Assert.AreEqual(Describe(fresh), Describe(shipped), fresh.Id);
                CollectionAssert.AreEqual(fresh.Loved, shipped.Loved, fresh.Id);
                Assert.AreEqual(fresh.HomeMap, shipped.HomeMap);
            }
        }

        [Test]
        public void RouteEdges_MatchTheRealWarpsAndSpawnPoints()
        {
            foreach (var edge in MapRoutes.All)
            {
                if (!Maps.ContainsKey(edge.From) || !Maps.ContainsKey(edge.To)) continue;     // optional layers ship their own maps
                var from = Maps[edge.From];
                var to = Maps[edge.To];
                var exit = new Vector2Int(edge.ExitX, edge.ExitY);
                Assert.IsTrue(from.Warps.Any(w => w.cell == exit && w.target == edge.To && w.spawn == edge.Spawn),
                    $"{edge.From} has no warp to {edge.To} (spawn {edge.Spawn}) at {exit}");
                Assert.IsTrue(to.Spawns.TryGetValue(edge.Spawn, out var spawn), $"{edge.To} has no spawn '{edge.Spawn}'");
                Assert.AreEqual(new Vector2Int(edge.ArriveX, edge.ArriveY), spawn, $"arrival on {edge.To} via {edge.Spawn}");
                Assert.IsTrue(to.Walkable(edge.ArriveX, edge.ArriveY), $"arrival cell on {edge.To} is blocked");
                Assert.IsTrue(from.Walkable(edge.ExitX, edge.ExitY), $"exit cell on {edge.From} is blocked");
            }
        }

        [Test]
        public void EveryDoorOfEveryMap_IsInTheRouteTable()
        {
            foreach (var pair in Maps)
                foreach (var warp in pair.Value.Warps)
                {
                    if (!MapIds.All.Contains(warp.target)) continue;     // the gated Woods slot
                    Assert.IsTrue(MapRoutes.All.Any(e => e.From == pair.Key && e.To == warp.target && e.ExitX == warp.cell.x && e.ExitY == warp.cell.y),
                        $"{pair.Key}: the warp to {warp.target} at {warp.cell} is not in MapRoutes");
                }
        }

        [Test]
        public void TheFarmAndTheVillage_HaveGrownToTheirLayoutSize_AndEverythingOldKeepsItsCell()
        {
            var farm = Maps[MapIds.Farm];
            Assert.AreEqual(MapLayout.FarmW, farm.Ground.Max(c => c.x) + 1);
            Assert.AreEqual(MapLayout.FarmH, farm.Ground.Max(c => c.y) + 1);
            var village = Maps[MapIds.Village];
            Assert.AreEqual(MapLayout.VillageW, village.Ground.Max(c => c.x) + 1);
            Assert.AreEqual(MapLayout.VillageH, village.Ground.Max(c => c.y) + 1);
            Assert.GreaterOrEqual(farm.Ground.Count, 4 * 44 * 32 / 2, "at least about twice the old land");
            Assert.GreaterOrEqual(village.Ground.Count, 50 * 36 * 3 / 2 - 100, "about one and a half times the old land");
            Assert.AreEqual(new Vector2Int(7, 20), farm.Warps.First(w => w.target == MapIds.FarmHouse).cell, "the farmhouse door did not move");
            Assert.AreEqual(new Vector2Int(MapLayout.FarmExitX, MapLayout.FarmRoadY), farm.Warps.First(w => w.target == MapIds.Village).cell);
            Assert.AreEqual(new Vector2Int(MapLayout.VillageLaneX, MapLayout.VillageForestExitY), village.Warps.First(w => w.target == MapIds.Forest).cell);
        }

        [Test]
        public void EveryScheduleStop_IsOnFreeFloor()
        {
            foreach (var npc in Npcs())
            {
                Assert.IsTrue(Maps[npc.HomeMap].Walkable(npc.HomeX, npc.HomeY), $"{npc.Id}'s home cell is blocked");
                foreach (var entry in npc.Schedule)
                    foreach (var stop in entry.Stops)
                        Assert.IsTrue(Maps[stop.Map].Walkable(stop.X, stop.Y), $"{npc.Id}/{entry.Id}: stop ({stop.X},{stop.Y}) on {stop.Map} is blocked or off the map");
            }
        }

        [Test]
        public void TheTwelveVillagers_AreEightRomanceableAndFourNot_EachWithTwoHeartEventsOrMore()
        {
            var npcs = Npcs().ToList();
            Assert.AreEqual(12, npcs.Count);
            Assert.AreEqual(8, npcs.Count(n => n.Romanceable));
            var story = StoryContent.LoadFromResources();
            foreach (var n in npcs)
            {
                Assert.IsTrue(string.IsNullOrEmpty(n.Allegiance), n.Id);
                Assert.GreaterOrEqual(story.Events.Count(e => e.Id.StartsWith(n.Id + "_heart")), 2, n.Id);
            }
        }

        [Test]
        public void HeartEvents_StageThePlayerOnFreeFloorBesideTheVillager()
        {
            var story = StoryContent.LoadFromResources();
            foreach (var e in story.Events)
            {
                var move = e.Steps.FirstOrDefault(s => s.Type == "move" && s.Actor == "player");
                if (move == null) continue;
                Assert.IsTrue(Maps[e.Map].Walkable(move.X, move.Y), $"{e.Id}: the player's cell ({move.X},{move.Y}) on {e.Map}");
                var npc = Npcs().FirstOrDefault(n => e.Id.StartsWith(n.Id + "_"));
                if (npc == null) continue;          // storyline scenes put their villager on stage themselves (a place step)
                var post = npc.Schedule.SelectMany(s => s.Stops).Where(s => s.Map == e.Map).Select(s => (s.X, s.Y)).ToList();
                Assert.IsTrue(post.Any(p => p.X == move.X && p.Y == move.Y + 1), $"{e.Id}: {npc.Id} stands next to where the player is moved");
            }
        }

        [Test]
        public void EveryShippedScene_IsStagedOnFreeFloor_WithPathsAndNoSharedCells()
        {
            var story = StoryContent.LoadFromResources();
            foreach (var e in story.Events.Where(e => !string.IsNullOrEmpty(e.Map) && Maps.ContainsKey(e.Map)))
            {
                var problems = EventStaging.Check(e, Maps[e.Map].Grid);
                CollectionAssert.IsEmpty(problems, e.Id + ": " + string.Join(" | ", problems));
            }
        }

        // T-103: every template, put on the player's staging cell of every shipped heart event, is physically possible there.
        [Test]
        public void EveryTemplate_IsStagedOnFreeFloor_OnEveryMapWithAHeartEvent()
        {
            var library = StoryContent.LoadFromResources();
            var checkedCount = 0;
            foreach (var e in library.Events)
            {
                var move = e.Steps.FirstOrDefault(s => s.Type == "move" && s.Actor == "player");
                if (move == null || !Maps.ContainsKey(e.Map)) continue;
                var npcId = Npcs().FirstOrDefault(n => e.Id.StartsWith(n.Id + "_"))?.Id;
                if (npcId == null) continue;
                foreach (var template in new[] { "shared_activity", "confession", "heirloom", "shared_meal", "helping_scene", "prank", "performance" })
                {
                    var args = new Newtonsoft.Json.Linq.JObject
                    {
                        ["npc"] = npcId, ["map"] = e.Map, ["px"] = move.X, ["py"] = move.Y, ["nx"] = move.X, ["ny"] = move.Y + 1,
                        ["item"] = "crop.strawberry", ["dish"] = "food.mashed_potato", ["cue"] = "c", ["need"] = "flag:x", ["talk"] = "tilda.chat1",
                    };
                    foreach (var key in new[] { "intro", "doing", "wrap", "give", "thanks", "bite", "ask", "success", "fallback", "setup", "prank", "reaction", "outro", "after" }) args[key] = "k";
                    var story = new StoryContent();
                    var raw = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Story/event_templates.json"));
                    story.AddJson(raw.ToString(), "lib");
                    var tmpl = ((Newtonsoft.Json.Linq.JArray)raw["eventTemplates"]).First(t => (string)t["id"] == template);
                    var declared = ((Newtonsoft.Json.Linq.JArray)tmpl["params"]).Select(t => (string)t).ToList();
                    var trimmed = new Newtonsoft.Json.Linq.JObject();
                    foreach (var p in args.Properties()) if (declared.Contains(p.Name)) trimmed[p.Name] = p.Value;
                    story.AddJson(new Newtonsoft.Json.Linq.JObject { ["eventsFromTemplates"] = new Newtonsoft.Json.Linq.JArray(new Newtonsoft.Json.Linq.JObject { ["id"] = "x", ["template"] = template, ["args"] = trimmed }) }.ToString(), "inst");
                    var ev = story.Event("x");
                    Assert.IsNotNull(ev, $"{template} on {e.Id}: {string.Join(" | ", story.Errors)}");
                    var problems = EventStaging.Check(ev, Maps[e.Map].Grid);
                    CollectionAssert.IsEmpty(problems, $"{template} on {e.Id} ({e.Map}): " + string.Join(" | ", problems));
                    checkedCount++;
                }
            }
            Assert.Greater(checkedCount, 100, "7 templates on 23 heart-event stages");
        }

        [Test]
        public void EveryWalkBetweenStops_HasAPathOnEachMap()
        {
            foreach (var npc in Npcs())
                foreach (var entry in npc.Schedule)
                    for (var i = 1; i < entry.Stops.Count; i++)
                    {
                        var a = entry.Stops[i - 1]; var b = entry.Stops[i];
                        foreach (var leg in MapRoutes.Legs(a.Map, a.X, a.Y, b.Map, b.X, b.Y))
                            Assert.IsNotNull(Maps[leg.Map].Grid.FindPath(leg.FromX, leg.FromY, leg.ToX, leg.ToY),
                                $"{npc.Id}/{entry.Id}: no path on {leg.Map} from ({leg.FromX},{leg.FromY}) to ({leg.ToX},{leg.ToY})");
                    }
        }

        [Test]
        public void ARealPathIsNotMuchLongerThanTheEstimate()
        {
            // The walk time uses straight-line (Manhattan) distance; A* around furniture may be a bit longer, never wildly.
            foreach (var npc in Npcs())
                foreach (var entry in npc.Schedule)
                    for (var i = 1; i < entry.Stops.Count; i++)
                    {
                        var a = entry.Stops[i - 1]; var b = entry.Stops[i];
                        foreach (var leg in MapRoutes.Legs(a.Map, a.X, a.Y, b.Map, b.X, b.Y))
                        {
                            var path = Maps[leg.Map].Grid.FindPath(leg.FromX, leg.FromY, leg.ToX, leg.ToY);
                            var manhattan = Mathf.Abs(leg.ToX - leg.FromX) + Mathf.Abs(leg.ToY - leg.FromY);
                            Assert.LessOrEqual(path.Count - 1, manhattan * 1.5f + 4f, $"{npc.Id}/{entry.Id} on {leg.Map}");
                        }
                    }
        }
    }
}
