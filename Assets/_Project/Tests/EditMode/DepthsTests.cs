using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // "Crawling Depths" in the horror layer: the dressing of the mine from the relic chest's floor down (scaled by intensity, depth and dread) and the tentacles of the ritual.
    public class DepthsTests
    {
        static MineFloor Floor(int seed, int floor) => MineGenerator.Generate(seed * 977, floor);

        // ---- the scale ----

        [Test]
        public void HorrorOff_AndTheFloorsAboveTheChest_GetNothing()
        {
            Assert.AreEqual(0f, DepthsDecor.Intensity(0, 35, 100));
            Assert.AreEqual(0f, DepthsDecor.Intensity(2, DepthsDecor.FirstFloor - 1, 100));
            for (var seed = 1; seed <= 10; seed++)
            {
                Assert.IsEmpty(DepthsDecor.Plan(Floor(seed, 33), seed, 0, 100), "off: the plain mine");
                Assert.IsEmpty(DepthsDecor.Plan(Floor(seed, 12), seed, 2, 100), "above the chest's floor");
            }
        }

        [Test]
        public void TheScale_RisesWithDepthAndWithDread_AndMildIsHalfOfFull()
        {
            Assert.Less(DepthsDecor.Intensity(2, 30, 0), DepthsDecor.Intensity(2, 40, 0), "deeper is stronger");
            Assert.Less(DepthsDecor.Intensity(2, 30, 0), DepthsDecor.Intensity(2, 30, 100), "more dread is stronger");
            Assert.AreEqual(0.35f, DepthsDecor.Intensity(2, 30, 0), 0.001f);
            Assert.AreEqual(1f, DepthsDecor.Intensity(2, 40, 100), 0.001f);
            Assert.AreEqual(DepthsDecor.Intensity(2, 36, 40) * 0.5f, DepthsDecor.Intensity(1, 36, 40), 0.001f);
            for (var floor = 30; floor <= 40; floor++)
                for (var dread = 0; dread <= 100; dread += 10)
                    Assert.That(DepthsDecor.Intensity(2, floor, dread), Is.InRange(0.35f, 1f));
        }

        [Test]
        public void MoreDreadAndMoreDepth_GiveMorePieces()
        {
            int Pieces(int floor, int dread) => Enumerable.Range(1, 12).Sum(seed => DepthsDecor.Plan(Floor(seed, floor), seed, 2, dread).Count);
            Assert.Less(Pieces(31, 0), Pieces(31, 100));
            Assert.Less(Pieces(31, 50), Pieces(39, 50));
        }

        // ---- what each level shows ----

        [Test]
        public void Mild_HasVeinsUrnsAndShortColumns_ButNoEyesStatuesArcsOrAltar()
        {
            var kinds = new HashSet<DepthsKind>();
            for (var seed = 1; seed <= 20; seed++)
                foreach (var p in DepthsDecor.Plan(Floor(seed, 30), seed, 1, 100)) kinds.Add(p.Kind);
            CollectionAssert.Contains(kinds, DepthsKind.Vein);
            CollectionAssert.Contains(kinds, DepthsKind.FloorPatch);
            foreach (var banned in new[] { DepthsKind.Eye, DepthsKind.LittleEyes, DepthsKind.Statue, DepthsKind.Arc, DepthsKind.BookAltar, DepthsKind.ColumnTall })
                CollectionAssert.DoesNotContain(kinds, banned, banned + " is for full intensity");
        }

        [Test]
        public void Full_ShowsEverything_AcrossFloors()
        {
            var kinds = new HashSet<DepthsKind>();
            for (var seed = 1; seed <= 20; seed++)
                foreach (var floor in new[] { 30, 34, 38, 40 })
                    foreach (var p in DepthsDecor.Plan(Floor(seed, floor), seed, 2, 100)) kinds.Add(p.Kind);
            foreach (DepthsKind kind in System.Enum.GetValues(typeof(DepthsKind))) CollectionAssert.Contains(kinds, kind, kind.ToString());
        }

        // ---- the pieces are put where they should be ----

        [Test]
        public void ThePlan_IsTheSameForTheSameWorldAndFloor_AndDiffersWithTheWorld()
        {
            var a = DepthsDecor.Plan(Floor(5, 33), 5, 2, 60);
            var b = DepthsDecor.Plan(Floor(5, 33), 5, 2, 60);
            Assert.AreEqual(a.Count, b.Count);
            for (var i = 0; i < a.Count; i++) Assert.AreEqual((a[i].Kind, a[i].X, a[i].Y, a[i].Variant), (b[i].Kind, b[i].X, b[i].Y, b[i].Variant));
            var c = DepthsDecor.Plan(Floor(5, 33), 6, 2, 60);
            Assert.IsFalse(a.Select(p => (p.Kind, p.X, p.Y)).SequenceEqual(c.Select(p => (p.Kind, p.X, p.Y))), "another world, another dressing");
        }

        [Test]
        public void EveryPiece_StandsInsideTheFloor_OnTheRightKindOfCell_AndNothingIsOnTheStairsOrOre()
        {
            for (var seed = 1; seed <= 25; seed++)
                foreach (var floorNumber in new[] { 30, 35, 40 })
                {
                    var f = Floor(seed, floorNumber);
                    var taken = MythosRelics.Taken(f);
                    foreach (var p in DepthsDecor.Plan(f, seed, 2, 80))
                    {
                        Assert.That(p.X, Is.InRange(1, f.Width - 2)); Assert.That(p.Y, Is.InRange(1, f.Height - 2));
                        var onWall = f.IsWall(p.X, p.Y);
                        switch (p.Kind)
                        {
                            case DepthsKind.Vein: case DepthsKind.LittleEyes: case DepthsKind.Eye: case DepthsKind.Statue when !p.Solid: case DepthsKind.ColumnShort: case DepthsKind.ColumnTall: case DepthsKind.Arc:
                                Assert.IsTrue(onWall, $"{p.Kind} at {p.X},{p.Y} is on a wall (seed {seed}, floor {floorNumber})");
                                break;
                            default:
                                Assert.IsFalse(onWall, $"{p.Kind} at {p.X},{p.Y} is on open floor (seed {seed}, floor {floorNumber})");
                                if (p.Kind != DepthsKind.FloorPatch) Assert.IsFalse(taken.Contains((p.X, p.Y)), $"{p.Kind} is not on the stairs, ore or a monster");
                                break;
                        }
                    }
                }
        }

        [Test]
        public void BigPieces_AreInNichesAndNeverOverlapEachOtherOrTheSmallOnes()
        {
            for (var seed = 1; seed <= 25; seed++)
            {
                var f = Floor(seed, 36);
                var covered = new Dictionary<(int, int), DepthsKind>();
                foreach (var p in DepthsDecor.Plan(f, seed, 2, 100))
                {
                    var big = p.Kind == DepthsKind.ColumnShort || p.Kind == DepthsKind.ColumnTall || p.Kind == DepthsKind.Arc || (p.Kind == DepthsKind.Statue && !p.Solid);
                    if (big)
                    {
                        Assert.IsFalse(f.IsWall(p.X, p.Y - 1), "open floor in front of it");
                        for (var dy = 0; dy <= 2; dy++)
                            for (var dx = -1; dx <= 1; dx++)
                            {
                                Assert.IsTrue(f.IsWall(p.X + dx, p.Y + dy), "a niche of wall behind it");
                                Assert.IsFalse(covered.ContainsKey((p.X + dx, p.Y + dy)), $"{p.Kind} does not overlap {(covered.TryGetValue((p.X + dx, p.Y + dy), out var other) ? other.ToString() : "")}");
                                covered[(p.X + dx, p.Y + dy)] = p.Kind;
                            }
                    }
                }
                foreach (var p in DepthsDecor.Plan(f, seed, 2, 100).Where(p => p.Kind == DepthsKind.Vein || p.Kind == DepthsKind.LittleEyes || p.Kind == DepthsKind.Eye))
                    Assert.IsFalse(covered.ContainsKey((p.X, p.Y)), $"a {p.Kind} is not drawn through a statue or column");
            }
        }

        // ---- the shrine round the chest ----

        [Test]
        public void TheChestsFloor_IsDressedAtFull_WithTwoStatuesAndTheBookAltarBesideTheChest_AndTheFloorStaysOnePiece()
        {
            var withShrine = 0;
            for (var seed = 1; seed <= 40; seed++)
            {
                var f = Floor(seed, MythosRelics.Floor);
                var chest = MythosRelics.ChestCell(f, seed).Value;
                var plan = DepthsDecor.Plan(f, seed, 2, 0);
                var solid = plan.Where(p => p.Solid).ToList();
                CollectionAssert.IsSubsetOf(solid.Select(p => p.Kind).Distinct(), new[] { DepthsKind.Statue, DepthsKind.BookAltar });
                foreach (var p in solid)
                {
                    Assert.LessOrEqual(Mathf.Abs(p.X - chest.x), 1); Assert.LessOrEqual(Mathf.Abs(p.Y - chest.y), 1);
                    Assert.AreNotEqual(chest, (p.X, p.Y), "not on the chest");
                }
                Assert.AreEqual(solid.Count, solid.Select(p => (p.X, p.Y)).Distinct().Count(), "no two on one cell");
                var blocked = solid.Select(p => (p.X, p.Y)).Append(chest).ToList();
                var reach = MythosRelics.ReachableAvoiding(f, null);
                Assert.AreEqual(reach.Count - blocked.Count, MythosRelics.ReachableAvoiding(f, blocked).Count, $"seed {seed}: the chest and the shrine do not cut the floor");
                Assert.IsTrue(MythosRelics.ReachableAvoiding(f, blocked).Contains(f.Ladder ?? f.Spawn), "the way down can still be reached");
                if (solid.Count == 3) withShrine++;
                Assert.IsTrue(plan.Any(p => p.Kind == DepthsKind.FloorPatch && p.X == chest.x && p.Y == chest.y), "the floor is cracked under the chest");
            }
            Assert.Greater(withShrine, 30, "nearly every world has room for all three");
        }

        [Test]
        public void AtMild_TheChestGetsNoSolidPieces()
        {
            for (var seed = 1; seed <= 15; seed++)
                Assert.IsFalse(DepthsDecor.Plan(Floor(seed, MythosRelics.Floor), seed, 1, 100).Any(p => p.Solid));
        }

        [Test]
        public void OnlyTheChestsFloor_HasAShrine()
        {
            for (var seed = 1; seed <= 10; seed++)
                Assert.IsFalse(DepthsDecor.Plan(Floor(seed, 34), seed, 2, 100).Any(p => p.Solid || p.Kind == DepthsKind.FloorPatch));
        }

        // ---- the pictures ----

        [Test]
        public void EverySheetTheLayerNames_IsInResources_WithTheFramesItExpects()
        {
            foreach (var (sheet, width, frames) in new[] { ("tentacle_1", 48, 6), ("tentacle_2", 48, 6), ("eye_1", 32, 28), ("eye_2", 32, 28), ("littleeyes_1", 16, 36), ("littleeyes_2", 16, 36), ("vein_a1", 16, 24), ("vein_a2", 16, 24), ("vein_b1", 16, 32), ("vein_b2", 16, 32) })
                Assert.AreEqual(frames, DepthsArt.Frames(sheet, width), sheet);
            for (var i = 0; i < 4; i++) Assert.IsNotNull(DepthsArt.Frame(MythosDepths.VeinSheet(i), 16, 16, 0), MythosDepths.VeinSheet(i));
            for (var i = 0; i < 6; i++) Assert.IsNotNull(DepthsArt.Whole(MythosDepths.StatueSheet(i)), MythosDepths.StatueSheet(i));
            foreach (var name in new[] { "column_short_1", "column_short_2", "column_tall_1", "column_tall_2", "urn_1", "urn_2", "book_altar", "arc_1", "arc_2" })
                Assert.IsNotNull(DepthsArt.Whole(name), name);
            for (var i = 0; i < DepthsArt.TileColumns * DepthsArt.TileRows; i++) Assert.IsNotNull(DepthsArt.Tile(i), "tile " + i);
        }

        [Test]
        public void ARisingTentacle_ShowsTheTopRowsOfItsFrame_FootAtTheGround()
        {
            var full = DepthsArt.Frame("tentacle_1", DepthsArt.TentacleW, DepthsArt.TentacleH, 2);
            var part = DepthsArt.Frame("tentacle_1", DepthsArt.TentacleW, DepthsArt.TentacleH, 2, 20);
            Assert.AreEqual(64, full.rect.height); Assert.AreEqual(20, part.rect.height);
            Assert.AreEqual(full.rect.yMax, part.rect.yMax, "the top of the picture stays the top");
            Assert.AreEqual(0f, part.pivot.y, "the cut edge is the ground");
            Assert.AreSame(part, DepthsArt.Frame("tentacle_1", DepthsArt.TentacleW, DepthsArt.TentacleH, 2, 20), "frames are made once");
            Assert.AreEqual(DepthsArt.Frame("tentacle_1", 48, 64, 1), DepthsArt.Frame("tentacle_1", 48, 64, 7), "the frame number wraps");
        }

        // ---- the tentacles of the ritual ----

        [Test]
        public void TheTentacles_AreUnderGroundBeforeTheRitualAndAfterTheyHaveSunk()
        {
            for (var slot = 0; slot < RitualTentacles.Slots; slot++)
            {
                Assert.AreEqual(0f, RitualTentacles.Target(slot, -1, 3, false));
                Assert.AreEqual(0f, RitualTentacles.Target(slot, RitualModel.DurationMinutes(3) + RitualTentacles.SinkMinutes, 3, false));
                Assert.AreEqual(0f, RitualTentacles.Target(slot, 100, 0, false), "no offerings, no ritual");
            }
        }

        [Test]
        public void TheTentacles_RiseOneAfterAnotherWhileTheLeaderSpeaks_AndAllStand_WhenTheFirstOfferingIsLaid()
        {
            Assert.Greater(RitualTentacles.Target(0, 6, 4, false), 0f);
            Assert.AreEqual(0f, RitualTentacles.Target(5, 6, 4, false), "the last is still under");
            for (var slot = 1; slot < RitualTentacles.Slots; slot++)
                Assert.LessOrEqual(RitualTentacles.Target(slot, 20, 4, false), RitualTentacles.Target(slot - 1, 20, 4, false), "each later one is lower");
            var atTheFirstOffering = RitualModel.SacrificeStart(0);
            for (var slot = 0; slot < RitualTentacles.Slots; slot++) Assert.Greater(RitualTentacles.Target(slot, atTheFirstOffering, 4, false), 0.7f);
            Assert.That(RitualTentacles.RiseStart(RitualTentacles.Slots - 1) + RitualTentacles.RiseMinutes, Is.LessThanOrEqualTo(RitualModel.LeaderMinutes), "up by the time the offerings begin");
        }

        [Test]
        public void TheTentacleOfTheTurn_ReachesTallestForTheOfferingOnTheAltar()
        {
            for (var i = 0; i < 5; i++)
            {
                var offset = RitualModel.SacrificeStart(i) + 5;
                var reaching = RitualTentacles.Reaching(offset, 5);
                Assert.AreEqual(i % RitualTentacles.Slots, reaching);
                Assert.AreEqual(1f, RitualTentacles.Target(reaching, offset, 5, false), 0.0001f);
                for (var slot = 0; slot < RitualTentacles.Slots; slot++)
                    if (slot != reaching) Assert.AreEqual(RitualTentacles.Resting, RitualTentacles.Target(slot, offset, 5, false), 0.0001f);
            }
            Assert.AreEqual(-1, RitualTentacles.Reaching(10, 5), "nobody reaches while the leader speaks");
            Assert.AreEqual(-1, RitualTentacles.Reaching(RitualModel.DurationMinutes(5) + 1, 5));
        }

        [Test]
        public void ADisruptedRitual_SendsTheTentaclesCringingDown_AndTheyStayLow()
        {
            for (var offset = 0; offset < RitualModel.DurationMinutes(3); offset += 7)
                for (var slot = 0; slot < RitualTentacles.Slots; slot++)
                    Assert.LessOrEqual(RitualTentacles.Target(slot, offset, 3, true), RitualTentacles.Cringing + 0.0001f);
        }

        [Test]
        public void AfterTheLastOffering_TheyAllSinkBackSteadily()
        {
            var end = RitualModel.DurationMinutes(3);
            var last = 1f;
            for (var m = 0; m <= RitualTentacles.SinkMinutes; m++)
            {
                var h = RitualTentacles.Target(0, end + m, 3, false);
                Assert.LessOrEqual(h, last + 0.0001f); last = h;
            }
            Assert.AreEqual(0f, last, 0.0001f);
        }

        [Test]
        public void TheRowsOfAPicture_FollowTheHeight_AndSixSlotsStandRoundTheAltar()
        {
            Assert.AreEqual(0, RitualTentacles.Rows(0f, 64)); Assert.AreEqual(32, RitualTentacles.Rows(0.5f, 64)); Assert.AreEqual(64, RitualTentacles.Rows(2f, 64));
            var cells = Enumerable.Range(0, RitualTentacles.Slots).Select(RitualTentacles.Place).ToList();
            Assert.AreEqual(RitualTentacles.Slots, cells.Distinct().Count());
            Assert.IsTrue(cells.Any(c => c.y > 0) && cells.Any(c => c.y < 0), "some behind the altar, some in front");
            foreach (var (x, y) in cells) Assert.AreEqual(1f, x * x / (3.4f * 3.4f) + (y + 0.35f) * (y + 0.35f) / (2.3f * 2.3f), 0.01f, "they stand on an oval round the altar, outside the candles");
        }
    }
}
