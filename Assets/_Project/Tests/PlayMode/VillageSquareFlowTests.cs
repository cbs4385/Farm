using System;
using System.Collections;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-16, in the real scenes: the village square has its fountain in the middle, ring of paving and benches; the roads are brown and meet it softly; the green has no brown patches
    // and none of the little decorations; the beach has driftwood and rocks you can pick up, with names, freshly laid every day. Set FARM_CAPTURE_DIR for pictures.
    public class VillageSquareFlowTests : PlayModeFixture
    {
        GameSession _session;

        IEnumerator Open(string map, int day = 10, int minute = 8 * 60)
        {
            GroundDecor.ResetForTests();
            if (_session == null)
            {
                Bootstrapper.InitializeServices();
                yield return null;
                _session = GameContext.Current.Session;
                _session.BeginNewGame("Tester", "Test Farm", 0);
                _session.SetFlag(FatigueModel.WarnedFlag);
            }
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, day, minute));
            _session.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 15; i++) yield return null;
        }

        static GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Select(t => t.gameObject).FirstOrDefault(g => g.name == name);

        static System.Collections.Generic.IEnumerable<Vector3Int> All(BoundsInt bounds) { foreach (var c in bounds.allPositionsWithin) yield return c; }

        [UnityTest]
        public IEnumerator TheSquare_HasItsFountainInTheMiddle_WithItsRingAndBenches_AndNoGazebo()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            Assert.IsNull(Named("Gazebo"), "no gazebo");
            var fountain = Named("Fountain");
            Assert.IsNotNull(fountain, "the fountain");
            Assert.AreEqual(map.CellCenter(new Vector3Int(25, 17, 0)).x, fountain.transform.position.x, 0.01f, "in the middle of the square, on the lane's axis");
            Assert.AreEqual(map.CellCenter(new Vector3Int(25, 17, 0)).y, fountain.transform.position.y, 0.6f);
            StringAssert.Contains("Fountain", fountain.GetComponent<HoverNote>().HoverLabel, "it has a name");
            var spout = fountain.GetComponent<FrameAnimator>();
            Assert.IsNotNull(spout, "the fountain is animated");
            var seen = new System.Collections.Generic.HashSet<string>();
            for (var end = Time.realtimeSinceStartup + 1.2f; Time.realtimeSinceStartup < end;) { seen.Add(fountain.GetComponent<SpriteRenderer>().sprite.name); yield return null; }
            Assert.GreaterOrEqual(seen.Count, 5, "it spits water: the picture changes through its frames (" + string.Join(", ", seen) + ")");
            Assert.IsNotNull(Named("Plaza_Ring"), "the ring of paving stones");
            Assert.IsNotNull(Named("Bench_Square_NW")); Assert.IsNotNull(Named("Bench_Square_NE")); Assert.IsNotNull(Named("ClockTower"));
            // what blocks in the middle of the square is the fountain's rounded block of cells, and nothing else
            for (var y = 15; y <= 19; y++)
                for (var x = 22; x <= 28; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    var blocked = map.Walls.GetTile(cell) != null || (Physics2D.OverlapPoint(map.CellCenter(cell)) is Collider2D c && !c.isTrigger);
                    Assert.AreEqual(VillageSquare.FountainSolid(x, y), blocked, $"({x},{y})");
                }
            Assert.AreEqual(new Vector3Int(19, 13, 0), map.WorldToCell(Named("HelpWantedBoard").transform.position), "the notice board by the pond");
            Assert.AreEqual(new Vector3Int(34, 12, 0), map.WorldToCell(Named("ClockTower").transform.position), "the clock tower south-east of the square (its picture is two cells wide)");
            var bench = Named("Bench_Square_NW").transform.position;
            Assert.AreEqual(21f, bench.x, 0.01f, "the north-west bench, two cells wide from x 20"); Assert.AreEqual(22.5f, bench.y, 0.01f);
            var east = Named("Bench_Square_NE").transform.position;
            Assert.AreEqual(30f, east.x, 0.01f, "the north-east bench, from x 29"); Assert.AreEqual(22.5f, east.y, 0.01f);
            foreach (var cell in new[] { new Vector2Int(20, 15), new Vector2Int(20, 19), new Vector2Int(30, 15), new Vector2Int(30, 19) })
                Assert.IsNotNull(Named(cell.y == 19 ? $"Lamp_N{cell.x}" : $"Lamp_S{cell.x}"), $"a lamp post at {cell}");
            CaptureHelper.Save("village_square");
        }

        [UnityTest]
        public IEnumerator TheGround_IsGrassOrBrownRoadOrCobbles_WithNoBrownPatchesAndNoSandyPath()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var counts = All(map.Ground.cellBounds).Select(c => map.Ground.GetTile(c)).Where(t => t != null).GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.Count());
            Assert.AreEqual(0, counts.TryGetValue("tile_dirt", out var dirt) ? dirt : 0, "no semi random brown patches on the green");
            Assert.AreEqual(0, counts.TryGetValue("tile_path", out var path) ? path : 0, "no sand coloured path");
            Assert.Greater(counts["tile_road"], 300, "the roads are brown");
            Assert.Greater(counts["tile_cobble"], 70, "and the square is cobbled");
            Assert.Greater(counts["tile_grass"], 1000);
        }

        [UnityTest]
        public IEnumerator WhereTheRoadsMeetTheSquare_ThePavingAndTheDirtAreBlended()
        {
            yield return Open(MapIds.Village);
            var edges = UnityEngine.Object.FindAnyObjectByType<TrailEdges>();
            Assert.IsNotNull(edges);
            Assert.IsNotNull(edges.Junctions, "a layer of pictures where cobbles meet the roads");
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var cells = All(edges.Junctions.cellBounds).Where(c => edges.Junctions.GetTile(c) != null).ToList();
            Assert.GreaterOrEqual(cells.Count, 12, "along all four ways in");
            foreach (var c in cells)
            {
                var own = map.Ground.GetTile(c).name;
                var neighbour = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0) }
                    .Select(d => map.Ground.GetTile(c + d)).Where(t => t != null).Select(t => t.name).ToList();
                Assert.IsTrue(own == "tile_cobble" && neighbour.Contains("tile_road") || own == "tile_road" && neighbour.Contains("tile_cobble"), $"{c}: {own} beside {string.Join(",", neighbour)}");
            }
            foreach (var side in new[] { new Vector3Int(VillageSquare.X0 - 1, 17, 0), new Vector3Int(VillageSquare.X1 + 1, 17, 0), new Vector3Int(25, VillageSquare.Y0 - 1, 0), new Vector3Int(25, VillageSquare.Y1 + 1, 0) })
                Assert.IsTrue(All(new BoundsInt(side - new Vector3Int(2, 2, 0), new Vector3Int(5, 5, 1))).Any(c => edges.Junctions.GetTile(c) != null), $"a join near {side}");
        }

        [UnityTest]
        public IEnumerator TheGreen_HasNoFlowerBoxesPlantersBarrelsBoxesOrWheelbarrows()
        {
            yield return Open(MapIds.Village);
            var names = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Select(t => t.name).ToList();
            foreach (var banned in new[] { "_Flowers", "_Planter", "Barrels_", "BarrelStack_", "Boxes_", "Wheelbarrow_", "HallFlowers", "HallRainBarrel" })
                Assert.IsFalse(names.Any(n => n.Contains(banned)), banned);
        }

        [UnityTest]
        public IEnumerator TheVillagesTrees_AreInCopses()
        {
            yield return Open(MapIds.Village);
            // The trees are pictures in the scene that become tree nodes when the village is first loaded (VillageGreen).
            var trees = _session.GetNodes(MapIds.Village).Nodes.Where(n => n.TypeId == NodeDefaults.Tree).Select(n => (n.X, n.Y)).ToList();
            Assert.That(trees.Count, Is.InRange(80, 400), "copses of trees round the village (the layout itself is checked by VillageSquareTests)");
            var withNeighbour = trees.Count(t => trees.Any(o => o != t && Math.Abs(o.X - t.X) <= 2 && Math.Abs(o.Y - t.Y) <= 2));
            Assert.GreaterOrEqual(withNeighbour / (float)trees.Count, 0.95f, "in the scene too, the trees stand together");
        }

        // ---- the beach ----

        [UnityTest]
        public IEnumerator TheBeach_HasFreshDriftwoodEveryDay_NearTheWater_AnAxeCollects_WithAName()
        {
            yield return Open(MapIds.Beach, day: 10);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var grid = _session.GetNodes(MapIds.Beach);
            var today = grid.Nodes.Where(n => n.TypeId == BeachDriftwood.NodeId).Select(n => (n.X, n.Y)).ToList();
            Assert.That(today.Count, Is.InRange(1, BeachDriftwood.MaxPerDay), "some driftwood");
            foreach (var (x, y) in today)
            {
                Assert.AreEqual("tile_sand", map.Ground.GetTile(new Vector3Int(x, y, 0)).name, "on the sand");
                Assert.IsFalse(map.IsWater(new Vector3Int(x, y, 0)));
                Assert.LessOrEqual(y, BeachDriftwood.StallRow, "between the water and the stall");
            }
            var def = _session.Nodes.Get(BeachDriftwood.NodeId);
            Assert.IsNotNull(def, "the game's content has the driftwood node (regenerate content if not)");
            Assert.AreEqual("Driftwood", NodeLabels.For(def));

            // Break one with an axe: wood, and it is gone; the same day again, it does not come back.
            var first = today[0];
            var hit = grid.Hit(first.X, first.Y, ToolType.Axe, 0, id => _session.Nodes.Get(id), 0.5f);
            Assert.AreEqual(NodeHit.Cleared, hit.Outcome);
            Assert.AreEqual(ItemIds.Wood, hit.DropItemId);
            yield return Open(MapIds.Beach, day: 10, minute: 15 * 60);
            Assert.IsFalse(_session.GetNodes(MapIds.Beach).Nodes.Any(n => n.X == first.X && n.Y == first.Y && n.TypeId == BeachDriftwood.NodeId), "a piece that was taken stays taken on the same day");

            // The next day the tide has brought other pieces.
            yield return Open(MapIds.Beach, day: 11);
            var tomorrow = _session.GetNodes(MapIds.Beach).Nodes.Where(n => n.TypeId == BeachDriftwood.NodeId).Select(n => (n.X, n.Y)).ToList();
            Assert.That(tomorrow.Count, Is.InRange(1, BeachDriftwood.MaxPerDay));
            CollectionAssert.AreNotEquivalent(today, tomorrow, "other pieces, in other places");
            CaptureHelper.Save("beach_driftwood");
        }

        [UnityTest]
        public IEnumerator TheBeach_HasNoPebblesOrDriftwoodDrawnOnTheSand_OnlyThingsYouCanPickUp()
        {
            yield return Open(MapIds.Beach);
            var layer = UnityEngine.Object.FindAnyObjectByType<GroundDecor>().Layer;
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var names = All(layer.cellBounds).Where(c => layer.GetTile(c) != null && map.Ground.GetTile(c).name == "tile_sand").Select(c => layer.GetTile(c).name).Distinct().ToList();
            Assert.IsNotEmpty(names, "dune grass on the sand");
            Assert.IsTrue(names.All(n => n.StartsWith("decor_sand_t")));
            Assert.AreEqual(3, Resources.LoadAll<Sprite>("Decor").Count(s => s.name.StartsWith("decor_sand_")), "only dune grass is drawn on the sand");
        }

        // ---- the festivals round the gazebo ----

        [UnityTest]
        public IEnumerator EachFestival_HasItsPropsInTheSquare_ClearOfTheFountainAndTheLane()
        {
            foreach (var (season, day, holder) in new[] { (Season.Spring, 13, "Festival_spring"), (Season.Summer, 11, "Festival_summer"), (Season.Fall, 16, "Festival_fall"), (Season.Winter, 25, "Festival_winter") })
            {
                GroundDecor.ResetForTests();
                if (_session == null)
                {
                    Bootstrapper.InitializeServices();
                    yield return null;
                    _session = GameContext.Current.Session;
                    _session.BeginNewGame("Tester", "Test Farm", 0);
                    _session.SetFlag(FatigueModel.WarnedFlag);
                }
                _session.Clock.SetTime(new GameDateTime(1, season, day, 12 * 60));
                _session.State.CurrentMap = MapIds.Village;
                var op = SceneManager.LoadSceneAsync(MapIds.Village);
                while (!op.isDone) yield return null;
                for (var i = 0; i < 20; i++) yield return null;
                var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
                var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                player.transform.position = map.CellCenter(new Vector3Int(25, 13, 0));
                for (var i = 0; i < 30; i++) yield return null;
                var go = Named(holder);
                Assert.IsNotNull(go, holder);
                Assert.IsTrue(go.activeInHierarchy, holder + " is out on its day");
                foreach (var piece in go.GetComponentsInChildren<Collider2D>().Where(c => !c.isTrigger))
                {
                    var cell = map.WorldToCell(piece.bounds.center);
                    Assert.IsFalse(VillageSquare.SolidPropCells().Contains(new Vector2Int(cell.x, cell.y)), $"{holder}: {piece.name} stands on something permanent at ({cell.x},{cell.y})");
                    if (cell.y >= VillageSquare.Y0 - 1 && cell.y <= VillageSquare.Y1 + 1)
                        Assert.IsFalse(cell.x >= 24 && cell.x <= 26 && (cell.y == VillageSquare.Y0 || cell.y == VillageSquare.Y0 - 1 || cell.y == VillageSquare.Y1 || cell.y == VillageSquare.Y1 + 1), $"{holder}: {piece.name} blocks the lane at ({cell.x},{cell.y})");
                }
                CaptureHelper.Save("square_" + season.ToString().ToLowerInvariant());
            }
        }

        [UnityTest]
        public IEnumerator TheStreetLamps_LightTwiceAsFarAsTheyDid_AfterDusk()
        {
            yield return Open(MapIds.Village, minute: 22 * 60);
            for (var i = 0; i < 40; i++) yield return null;
            var lights = UnityEngine.Object.FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None).Where(l => l.name == "LampLight").ToList();
            Assert.GreaterOrEqual(lights.Count, 10, "the village's lamp posts");
            foreach (var l in lights)
            {
                Assert.AreEqual(9f, l.pointLightOuterRadius, "nine tiles: twice the 4.5 it was");
                Assert.IsTrue(l.enabled, "lit at ten at night");
            }
        }

        // ---- the people who stand in the square ----

        [UnityTest]
        public IEnumerator EveryPlaceAVillagerOrAFestivalGoerStandsInTheSquare_IsOpenGround()
        {
            yield return Open(MapIds.Village);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var kv in Festivals.Gathering)
                foreach (var (x, y, _) in kv.Value)
                    Assert.IsNull(map.Walls.GetTile(new Vector3Int(x, y, 0)), $"{kv.Key}: ({x},{y}) is blocked");
        }
    }

    // Saves a picture of the world when FARM_CAPTURE_DIR is set.
    static class CaptureHelper
    {
        public static void Save(string name)
        {
            var dir = Environment.GetEnvironmentVariable("FARM_CAPTURE_DIR");
            var cam = Camera.main;
            if (string.IsNullOrEmpty(dir) || cam == null) return;
            System.IO.Directory.CreateDirectory(dir);
            var rt = new RenderTexture(1280, 720, 24);
            var before = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = before;
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            rt.Release();
        }
    }
}
