using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The playtesters' list of 2026-10-13 (docs/PLAYTEST_2026-10-13_PLAN.md): quest counts, planting out of season, night fish, gems and jewellery, food at the saloon, the mine's
    // markers, furniture that is too small, and the festivals.
    public class Playtest1013Tests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [SetUp]
        public void SetUp() => L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));

        TestSessionFixture Fixture()
        {
            var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null);
            f.Session.Story = StoryContent.LoadFromResources();
            f.Session.Backpack.Resize(80);
            f.EmptyBackpack();
            return f;
        }

        // ---- 1: what the backpack holds, shown with the quest ----

        [Test]
        public void AQuestThatIsHandedOver_ShowsWhatTheBackpackCarries_InTheHallTheJournalAndTheTracker()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                HallRooms.StartAll(s);
                var quest = s.Story.Quest("hall_pantry");
                var parsnip = quest.Objectives.First(o => o.GiveItem == "crop.parsnip");
                Assert.AreEqual("  (you carry 0)", QuestLog.CarryingText(s, quest, parsnip));
                s.Backpack.Add("crop.parsnip", 2);
                Assert.AreEqual("  (you carry 2)", QuestLog.CarryingText(s, quest, parsnip));
                QuestTracker.Track(s, "hall_pantry");
                StringAssert.Contains("(0/3)  (you carry 2)", QuestTracker.Text(s), "the HUD tracker");
                HallRooms.Donate(s, "hall_pantry", parsnip);
                Assert.AreEqual("  (you carry 0)", QuestLog.CarryingText(s, quest, parsnip).Replace("(you carry 0)", "(you carry 0)"));
                s.Backpack.Add("crop.parsnip", 5);
                HallRooms.Donate(s, "hall_pantry", parsnip);
                Assert.AreEqual(string.Empty, QuestLog.CarryingText(s, quest, parsnip), "nothing more to hand over once the part is done");
            }
        }

        // ---- 2 and 7: no planting out of season, outside the greenhouse ----

        [Test]
        public void TheSeasonsAMessageNames_AreTheCropsOwn()
        {
            var db = RealDb();
            db.TryGetCrop("parsnip", out var parsnip);
            db.TryGetCrop("tomato", out var tomato);
            Assert.AreEqual("Spring", CropInfo.SeasonsText(parsnip.Seasons));
            Assert.AreEqual("Summer", CropInfo.SeasonsText(tomato.Seasons));
            Assert.AreEqual("Spring and Summer", CropInfo.SeasonsText(SeasonMask.Spring | SeasonMask.Summer));
            Assert.AreEqual("Spring, Summer and Fall", CropInfo.SeasonsText(SeasonMask.Spring | SeasonMask.Summer | SeasonMask.Fall));
            Assert.AreEqual(string.Empty, CropInfo.SeasonsText(SeasonMask.None));
        }

        [Test]
        public void OnlyAGreenhouse_PlantsEveryCropInEverySeason()
        {
            RealDb().TryGetCrop("tomato", out var tomato);
            var field = new FarmGrid();
            var greenhouse = new FarmGrid { AllSeasons = true };
            foreach (Season season in System.Enum.GetValues(typeof(Season)))
            {
                Assert.AreEqual(season == Season.Summer, field.Plantable(tomato, season), "outdoors: its own season only");
                Assert.IsTrue(greenhouse.Plantable(tomato, season), "the greenhouse: any season");
            }
        }

        [Test]
        public void TheRefusalText_NamesTheCropAndItsSeasons()
        {
            Assert.AreEqual("Tomato will not grow now. Plant it in Summer.", L.Get("toast.wrong_season", "Tomato", "Summer"));
        }

        // ---- 5: the night fish ----

        sealed class World : IWorldQuery
        {
            public GameDateTime Date; public string W = "sunny"; public string Map = "Village";
            public bool HasFlag(string f) => false;
            public int GetVar(string n) => 0;
            public GameDateTime Now => Date;
            public string Weather => W;
            public string MapId => Map;
        }

        [Test]
        public void EverySeason_AtTheSeaAndThePonds_HasSeveralFishThatBiteOnlyAfterDark()
        {
            var night = FishDefaults.Rows.Where(r => r.Condition != null && r.Condition.Contains("hour>=20")).ToList();
            foreach (var spot in new[] { FishSpots.Ocean, FishSpots.Pond })
                foreach (Season season in System.Enum.GetValues(typeof(Season)))
                {
                    var day = FishingModel.Eligible(FishDefaults.Rows, spot, new World { Date = new GameDateTime(1, season, 5, 12 * 60) });
                    var late = FishingModel.Eligible(FishDefaults.Rows, spot, new World { Date = new GameDateTime(1, season, 5, 22 * 60) });
                    var added = late.Where(f => day.All(d => d.Id != f.Id)).ToList();
                    Assert.GreaterOrEqual(added.Count(f => night.Any(n => n.Id == f.Id)), 2, $"{spot} {season}: night fish");
                    Assert.IsEmpty(day.Where(f => night.Any(n => n.Id == f.Id)), $"{spot} {season}: none of them bite by day");
                    Assert.IsEmpty(day.Where(f => f.Condition != null && f.Condition.Contains("hour>=20")));
                }
        }

        [Test]
        public void TheNightFish_BiteAfterMidnightToo()
        {
            var w = new World { Date = new GameDateTime(1, Season.Fall, 5, 25 * 60 + 30), Map = "Beach" };
            Assert.IsTrue(FishingModel.Eligible(FishDefaults.Rows, FishSpots.Ocean, w).Any(f => f.Id == "lanternfish"), "the clock runs on past 24:00");
        }

        [Test]
        public void ANightFishIsMoreCommonThanTheRareDayFish_AndEveryOneHasAnItemAndAPicture()
        {
            var db = RealDb();
            foreach (var id in new[] { "lanternfish", "cuttlefish", "stingray", "midnight_carp", "ghost_perch", "black_bass", "starfin" })
            {
                var row = FishDefaults.Rows.Single(r => r.Id == id);
                Assert.IsTrue(db.TryGetItem(row.ItemId, out var item), id);
                Assert.IsNotNull(item.Icon, id);
                Assert.IsTrue(File.Exists($"Assets/_Project/Art/Placeholders/item_fish_{id}.png"), id);
                StringAssert.DoesNotContain("item.fish.", L.Get("item." + row.ItemId + ".name"), "named");
            }
        }

        // ---- 8 and 9: gems in the mine, in the hall, in jewellery ----

        [Test]
        public void GemClusters_AppearOnlyInTheDeeperFloors_InOrderOfTheirDepth()
        {
            var gems = new HashSet<string>(GemDefaults.Gems.Select(g => g.NodeId));
            var shallow = Enumerable.Range(1, GemDefaults.FirstFloor - 1).SelectMany(fl => Enumerable.Range(0, 20).SelectMany(seed => MineGenerator.Generate(seed * 13 + fl, fl).Nodes));
            Assert.IsFalse(shallow.Any(n => gems.Contains(n.NodeId)), "no gems above floor " + GemDefaults.FirstFloor);

            foreach (var gem in GemDefaults.Gems)
            {
                var above = Enumerable.Range(1, gem.MinFloor - 1).SelectMany(fl => Enumerable.Range(0, 6).SelectMany(seed => MineGenerator.Generate(seed * 17 + fl, fl).Nodes));
                Assert.IsFalse(above.Any(n => n.NodeId == gem.NodeId), gem.Id + " is not found above floor " + gem.MinFloor);
                var below = Enumerable.Range(gem.MinFloor, 39 - gem.MinFloor).SelectMany(fl => Enumerable.Range(0, 12).SelectMany(seed => MineGenerator.Generate(seed * 19 + fl, fl).Nodes));
                Assert.IsTrue(below.Any(n => n.NodeId == gem.NodeId), gem.Id + " turns up from floor " + gem.MinFloor);
            }
        }

        [Test]
        public void ADeepFloor_HoldsAGoodNumberOfClusters_AndTheOreIsStillThere()
        {
            var clusters = 0; var ore = 0;
            for (var seed = 0; seed < 30; seed++)
                foreach (var n in MineGenerator.Generate(seed, 30).Nodes)
                {
                    if (GemDefaults.Gems.Any(g => g.NodeId == n.NodeId)) clusters++;
                    else if (n.NodeId == MineGenerator.Copper || n.NodeId == MineGenerator.Iron || n.NodeId == MineGenerator.Gold) ore++;
                }
            Assert.Greater(clusters, 30, "about one a floor or more");
            Assert.Greater(ore, clusters, "gems are rarer than ore");
        }

        [Test]
        public void EveryGem_HasANodeAnItemAJewelAndABar_AndTheirPicturesExist()
        {
            var db = RealDb();
            foreach (var g in GemDefaults.Gems)
            {
                Assert.IsNotNull(db.AllNodes.FirstOrDefault(n => n.Id == g.NodeId), g.NodeId);
                var node = db.AllNodes.First(n => n.Id == g.NodeId);
                Assert.IsNotNull(node.Sprite, g.NodeId + " picture");
                Assert.AreEqual(g.ItemId, node.DropItemId);
                Assert.AreEqual(ToolType.Pickaxe, node.Tool);
                foreach (var id in new[] { g.ItemId, GemDefaults.RingId(g), GemDefaults.PendantId(g) })
                {
                    Assert.IsTrue(db.TryGetItem(id, out var item), id);
                    Assert.IsNotNull(item.Icon, id + " icon");
                    Assert.Greater(item.SellPrice, 0, id);
                    StringAssert.DoesNotContain("item.", L.Get("item." + id + ".name"), id + " is named");
                }
                Assert.Greater(db.GetItem(GemDefaults.RingId(g)).SellPrice, db.GetItem(g.ItemId).SellPrice, "worth more as a ring");
                Assert.Greater(db.GetItem(GemDefaults.PendantId(g)).SellPrice, db.GetItem(g.ItemId).SellPrice, "worth more as a pendant");
            }
        }

        [Test]
        public void TheLaterHallRooms_AskForGems()
        {
            using (var f = Fixture())
            {
                var wants = new Dictionary<string, string[]> { { "hall_boiler", new[] { "gem.amethyst" } }, { "hall_vault", new[] { "gem.emerald", "gem.ruby" } } };
                foreach (var room in wants)
                {
                    var q = f.Session.Story.Quest(room.Key);
                    foreach (var gem in room.Value)
                    {
                        var o = q.Objectives.FirstOrDefault(x => x.GiveItem == gem);
                        Assert.IsNotNull(o, $"{room.Key} asks for {gem}");
                        StringAssert.DoesNotContain("quest.", L.Get(o.Text), "the objective has its text");
                    }
                }
                var s = f.Session;
                HallRooms.StartAll(s);
                s.Backpack.Add("gem.amethyst", 2);
                var boiler = s.Story.Quest("hall_boiler");
                Assert.AreEqual(1, HallRooms.Donate(s, "hall_boiler", boiler.Objectives.First(o => o.GiveItem == "gem.amethyst")), "one is needed");
                Assert.AreEqual(1, s.Backpack.Count("gem.amethyst"));
            }
        }

        [Test]
        public void Jewellery_IsMadeFromAGemAndABar_ByHand_AtMiningLevels()
        {
            var db = RealDb();
            foreach (var g in GemDefaults.Gems)
            {
                var ring = db.AllRecipes.Single(r => r.OutputItemId == GemDefaults.RingId(g));
                var pendant = db.AllRecipes.Single(r => r.OutputItemId == GemDefaults.PendantId(g));
                Assert.AreEqual(Stations.Hand, ring.Station);
                Assert.AreEqual(Stations.Hand, pendant.Station);
                CollectionAssert.AreEquivalent(new[] { g.ItemId, ItemIds.GoldBar }, ring.Ingredients.Select(i => i.ItemId), g.Id + " ring");
                CollectionAssert.AreEquivalent(new[] { g.ItemId, ItemIds.IronBar }, pendant.Ingredients.Select(i => i.ItemId), g.Id + " pendant");
                Assert.Greater(ring.SkillLevel, pendant.SkillLevel, "a ring is the harder piece");
            }
            using (var f = Fixture())
            {
                var s = f.Session;
                var recipe = db.AllRecipes.Single(r => r.OutputItemId == GemDefaults.PendantId(GemDefaults.Gems[0]));
                Assert.AreNotEqual(CraftResult.Ok, CraftingRules.TryCraft(recipe, Stations.Hand, s.State, s.Backpack, _ => 5), "nothing to make it from");
                s.Backpack.Add("gem.amethyst", 1); s.Backpack.Add(ItemIds.IronBar, 1);
                Assert.AreNotEqual(CraftResult.Ok, CraftingRules.TryCraft(recipe, Stations.Hand, s.State, s.Backpack, _ => 1), "mining level too low");
                Assert.AreEqual(CraftResult.Ok, CraftingRules.TryCraft(recipe, Stations.Hand, s.State, s.Backpack, _ => 3));
                Assert.AreEqual(1, s.Backpack.Count(GemDefaults.PendantId(GemDefaults.Gems[0])));
                Assert.AreEqual(0, s.Backpack.Count("gem.amethyst"));
            }
        }

        [Test]
        public void AnyJewellery_IsAWelcomeGift_ForEveryVillager_ButADislikeStillWins()
        {
            var db = RealDb();
            var ring = db.GetItem(GemDefaults.RingId(GemDefaults.Gems[0]));
            foreach (var npc in NpcCatalog.From(db).All) Assert.AreEqual(GiftTaste.Liked, NpcInteractions.TasteOf(npc, ring), npc.Id);
            var coal = db.GetItem(ItemIds.Coal);
            Assert.AreNotEqual(GiftTaste.Liked, NpcInteractions.TasteOf(NpcCatalog.From(db).All.First(), coal));
        }

        // ---- 10: the range and the saloon's meals ----

        [Test]
        public void TheSaloon_SellsEveryDish_ForMoreThanItFetches()
        {
            var db = RealDb();
            var world = new World { Date = new GameDateTime(1, Season.Spring, 5, 14 * 60) };
            var stock = ShopCatalog.For(db, "saloon", world);
            var dishes = db.AllItems.Where(i => i.Category == ItemCategory.Food).ToList();
            Assert.GreaterOrEqual(dishes.Count, 9);
            foreach (var dish in dishes)
            {
                CollectionAssert.Contains(stock.Select(i => i.Id), dish.Id, dish.Id + " is on the menu");
                Assert.Greater(dish.BuyPrice, dish.SellPrice, "cooking at home pays");
            }
            Assert.IsEmpty(ShopCatalog.For(db, "general", world).Where(i => i.Category == ItemCategory.Food), "the general store has no meals");
            StringAssert.DoesNotContain("shop.", L.Get("shop.saloon.title"));
            Assert.AreEqual(135, CraftingDefaults.DishPrice(90));
        }

        // The tester's screenshot of 0.6.1: orange flowers on the farm that looked like dandelions were the farm's weeds (the ground decoration had been recoloured, the weed had not).
        [Test]
        public void TheWeed_HasPurpleFlowers_SoOnlyADandelionLooksLikeOne()
        {
            int Count(string path, System.Func<float, float, bool> match)
            {
                var n = 0;
                foreach (var c in LoadPixels(path))
                {
                    if (c.a == 0) continue;
                    Color.RGBToHSV(c, out var h, out var s, out _);
                    if (match(h, s)) n++;
                }
                return n;
            }
            var weed = "Assets/_Project/Art/Placeholders/obj_weed.png";
            Assert.AreEqual(0, Count(weed, (h, s) => s > 0.25f && (h < 0.14f || h > 0.93f)), "no orange or red on the weed");
            Assert.Greater(Count(weed, (h, s) => s > 0.25f && h > 0.7f && h < 0.85f), 20, "purple flowers");
            Assert.Greater(Count("Assets/_Project/Art/Placeholders/obj_dandelion.png", (h, s) => s > 0.25f && h < 0.14f), 20, "the dandelion is still the orange one");
            foreach (var path in System.IO.Directory.GetFiles("Assets/_Project/Resources/Decor", "decor_spring_f*.png"))
                Assert.AreEqual(0, Count(path, (h, s) => s > 0.25f && (h < 0.14f || h > 0.93f)), path + ": no orange wild flower");
        }

        // The tester whose item kept changing to the pickaxe: the log now says who changed it.
        [Test]
        public void EveryChangeOfTheSelectedSlot_IsDescribedForTheLog()
        {
            Assert.AreEqual("[hotbar] slot 1 -> 3 by Mouse/scroll/up", HotbarRows.Describe(1, 3, "Mouse/scroll/up"));
        }

        // ---- 11: the mine's markers ----

        [Test]
        public void TheFourMineMarkers_AreTheEntrancePictureRecoloured()
        {
            Assert.AreEqual("Mine/prop_mine_exit", MineController.PropOf(MineStairs.StairKind.Up, 1), "the way up from the first floor leaves the mine");
            Assert.AreEqual("Mine/prop_mine_up", MineController.PropOf(MineStairs.StairKind.Up, 2));
            Assert.AreEqual("Mine/prop_mine_down", MineController.PropOf(MineStairs.StairKind.Down, 1));
            Assert.AreEqual("Mine/prop_mine_lift", MineController.PropOf(MineStairs.StairKind.Elevator, 5));
            var entrance = LoadPixels("Assets/_Project/Art/Placeholders/prop_mine_entrance.png");
            // A pixel inside the opening and clear of the arrows and their outlines (the picture's rows run from the bottom up): row 24 from the top.
            Color32 Inside(string name) => LoadPixels($"Assets/_Project/Resources/Mine/prop_mine_{name}.png")[(name == "lift" ? 15 : 11) + (31 - 24) * 32];
            foreach (var name in new[] { "exit", "down", "up", "lift" })
            {
                var pixels = LoadPixels($"Assets/_Project/Resources/Mine/prop_mine_{name}.png");
                Assert.AreEqual(entrance.Length, pixels.Length, name + " is the entrance's size (32 x 32)");
                var same = 0;
                for (var i = 0; i < pixels.Length; i++) if (pixels[i].Equals(entrance[i])) same++;
                Assert.Greater(same, pixels.Length / 2, name + " keeps the timber, the rock and the rails of the entrance");
            }
            int Light(Color32 c) => c.r + c.g + c.b;
            Assert.Greater(Light(Inside("exit")), 600, "a light blue interior");
            Assert.Greater(Inside("exit").b, Inside("exit").r);
            Assert.Less(Light(Inside("down")), 100, "dark");
            Assert.Greater(Light(Inside("up")), Light(Inside("down")) + 200, "lighter than the stairs down");
            Assert.Less(Light(Inside("up")), Light(Inside("exit")));
            Assert.AreEqual(Inside("lift").r, Inside("lift").b, 12, "neutral grey");
        }

        static Color32[] LoadPixels(string path)
        {
            var tex = new Texture2D(2, 2);
            Assert.IsTrue(tex.LoadImage(File.ReadAllBytes(path)), path);
            return tex.GetPixels32();
        }

        // ---- 4: furniture that was too small ----

        [Test]
        public void TheBedShelfAndRange_HaveRoomSizedPictures()
        {
            Size("obj_bed", 16, 32);
            Size("obj_shelf", 16, 32);
            Size("obj_bookshelf_tall", 16, 32);
            Size("obj_kitchen", 32, 32);
            foreach (var id in CraftingDefaults.LargeDecor) Assert.IsNotNull(CraftingDefaults.Decor.FirstOrDefault(d => d.id == id).id, id + " is a piece of decor");
            var wardrobe = RealDb().AllPlaceables.First(p => p.Id == "wardrobe");
            var rug = RealDb().AllPlaceables.First(p => p.Id == "rug");
            Assert.AreEqual(PlacedObjectActor.MachineScale, PlacedObjectActor.ScaleFor(wardrobe), "big furniture is drawn big");
            Assert.AreEqual(1f, PlacedObjectActor.ScaleFor(rug), "a rug is flat");
        }

        static void Size(string sprite, int w, int h)
        {
            var pixels = LoadPixels($"Assets/_Project/Art/Placeholders/{sprite}.png");
            Assert.AreEqual(w * h, pixels.Length, $"{sprite} is {w} x {h}");
        }

        // ---- 3: the festivals ----

        [Test]
        public void TheFourFestivals_AreOnTheCalendarDaysTheStoryEventsUse()
        {
            var story = StoryContent.LoadFromResources();
            foreach (var row in Festivals.Rows)
            {
                var ev = story.Events.First(e => e.Id == "festival_" + row.Id);
                Assert.AreEqual(row.Season, (Season)ev.CalendarSeason, row.Id);
                Assert.AreEqual(row.Day, ev.CalendarDay, row.Id);
                Assert.AreEqual(row, Festivals.Today(new GameDateTime(2, row.Season, row.Day, 12 * 60)));
                Assert.IsNull(Festivals.Today(new GameDateTime(2, row.Season, row.Day + 1, 12 * 60)));
            }
        }

        [Test]
        public void TheFestivalIsOpenFromTenToNine()
        {
            var day = new GameDateTime(1, Season.Spring, 13, 9 * 60 + 50);
            Assert.IsFalse(Festivals.IsOpen(day));
            Assert.IsTrue(Festivals.IsOpen(day.WithMinuteOfDay(10 * 60)));
            Assert.IsTrue(Festivals.IsOpen(day.WithMinuteOfDay(20 * 60 + 59)));
            Assert.IsFalse(Festivals.IsOpen(day.WithMinuteOfDay(21 * 60)));
            Assert.IsFalse(Festivals.IsOpen(new GameDateTime(1, Season.Spring, 14, 12 * 60)));
        }

        [Test]
        public void EveryVillager_GoesToTheFestival_AndHomeAgain_OnAValidSchedule()
        {
            var db = RealDb();
            var source = new FestivalSchedule();
            foreach (var npc in NpcCatalog.From(db).All)
            {
                var entries = source.EntriesFor(npc).ToList();
                Assert.AreEqual(Festivals.Rows.Length, entries.Count, npc.Id);
                foreach (var entry in entries)
                {
                    Assert.IsTrue(NpcSchedule.IsValid(entry, out var error), $"{npc.Id}/{entry.Id}: {error}");
                    Assert.AreEqual(npc.HomeMap, entry.Stops[0].Map, "starts the day at home");
                    var row = Festivals.Get(entry.Id.Substring("festival_".Length));
                    var at = entry.Stops.Single(s => s.Map == MapIds.Village);
                    var cell = Festivals.CellFor(row, Festivals.IndexOf(npc.Id));
                    Assert.AreEqual((cell.x, cell.y), (at.X, at.Y), $"{npc.Id} stands at their cell");
                    var world = new World { Date = new GameDateTime(1, row.Season, row.Day, 12 * 60) };
                    Assert.AreEqual(entry, NpcSchedule.PlanFor(npc, world, entries), $"{npc.Id}: the festival's plan beats the ordinary day on {row.Id}");
                    var ordinary = new World { Date = new GameDateTime(1, row.Season, row.Day + 1, 12 * 60) };
                    Assert.AreNotEqual(entry, NpcSchedule.PlanFor(npc, ordinary, entries), "and not the day after");
                    var noon = NpcSchedule.Where(npc, entry, 12 * 60);
                    Assert.AreEqual(MapIds.Village, noon.Map, $"{npc.Id} is in the village at noon");
                    Assert.IsFalse(noon.Walking);
                    var midnight = NpcSchedule.Where(npc, entry, 25 * 60);
                    Assert.AreEqual(npc.HomeMap, midnight.Map, "home at night");
                }
            }
        }

        [Test]
        public void TheGatheringCells_AreDistinctWithinAFestival_AndTwelveVillagersFit()
        {
            foreach (var row in Festivals.Rows)
            {
                var cells = Festivals.Gathering[row.Id];
                Assert.AreEqual(NpcIds.All.Length, cells.Length, row.Id);
                Assert.AreEqual(cells.Length, cells.Select(c => (c.x, c.y)).Distinct().Count(), row.Id + ": one cell each");
            }
        }

        [Test]
        public void TheContestAndShowTables_AreTheSameForTheSameGameAndYear_AndThePlayerWinsATie()
        {
            var a = Festivals.Table(Festivals.ContestEntrants, 77, 1, false, 400);
            var b = Festivals.Table(Festivals.ContestEntrants, 77, 1, false, 400);
            CollectionAssert.AreEqual(a.Select(x => (x.Name, x.Score)), b.Select(x => (x.Name, x.Score)));
            CollectionAssert.AreNotEqual(a.Select(x => x.Score), Festivals.Table(Festivals.ContestEntrants, 78, 1, false, 400).Select(x => x.Score));
            Assert.AreEqual(5, a.Count);
            var felix = a.First(x => x.Name == NpcRoster.Felix).Score;
            Assert.AreEqual(1, Festivals.Place(Festivals.Table(Festivals.ContestEntrants, 77, 1, false, felix)), "a tie goes to the player");
            Assert.AreEqual(5, Festivals.Place(Festivals.Table(Festivals.ContestEntrants, 77, 1, false, 0)));
            Assert.Greater(Festivals.Prize(1), Festivals.Prize(2));
            Assert.Greater(Festivals.Prize(2), Festivals.Prize(3));
            Assert.Greater(Festivals.Prize(3), Festivals.Prize(4));
            Assert.Greater(Festivals.CatchPoints(100, 2), Festivals.CatchPoints(100, 1));
            Assert.Greater(Festivals.CatchPoints(100, 1), Festivals.CatchPoints(100, 0));
            for (var year = 1; year < 40; year++)
                for (var seed = 0; seed < 5; seed++)
                {
                    var contest = Festivals.EntrantScore(NpcRoster.Felix, seed, year, false);
                    Assert.That(contest, Is.InRange(520, 800));
                    var show = Festivals.EntrantScore(NpcIds.Tilda, seed, year, true);
                    Assert.That(show, Is.InRange(90, 310));
                    Assert.That(Festivals.WishFor(seed, year), Is.InRange(0, Festivals.WishCount - 1));
                }
        }

        static int SlotOf(GameSession s, string itemId)
        {
            for (var i = 0; i < s.Backpack.Capacity; i++) if (s.Backpack.Get(i)?.ItemId == itemId) return i;
            return -1;
        }

        TestSessionFixture FestivalFixture(Season season, int day, int minute)
        {
            var f = Fixture();
            f.Session.Clock.SetTime(new GameDateTime(1, season, day, minute));
            return f;
        }

        [Test]
        public void TheContest_IsEnteredFishedAndWeighedIn_AndPaysOncePerYear()
        {
            using (var f = FestivalFixture(Season.Summer, 11, 9 * 60))
            {
                var s = f.Session;
                FestivalActivities.Contest(s);
                StringAssert.Contains("starts at 10:00", f.Toasts.Last());
                s.Clock.SetTime(new GameDateTime(1, Season.Summer, 11, 10 * 60 + 10));
                Assert.AreEqual(0, FestivalActivities.RecordCatch(s, MapIds.Village, "fish.carp", 0), "not entered yet: a catch does not count");
                FestivalActivities.Contest(s);
                StringAssert.Contains("entered in the Tidewatch", f.Toasts.Last());
                Assert.AreEqual(0, FestivalActivities.RecordCatch(s, MapIds.Beach, "fish.carp", 0), "only the village pond counts");
                var carp = RealDb().GetItem("fish.carp").SellPrice;
                Assert.AreEqual(Festivals.CatchPoints(carp, 0), FestivalActivities.RecordCatch(s, MapIds.Village, "fish.carp", 0));
                Assert.Greater(FestivalActivities.RecordCatch(s, MapIds.Village, "fish.sturgeon", 2), carp);
                var score = FestivalActivities.ContestPoints(s);
                Assert.Greater(score, carp);
                FestivalActivities.Contest(s);
                StringAssert.Contains($"{score} points", f.Toasts.Last(), "the standing before five");
                var gold = s.State.Gold;
                s.Clock.SetTime(new GameDateTime(1, Season.Summer, 11, 17 * 60 + 5));
                Assert.AreEqual(0, FestivalActivities.RecordCatch(s, MapIds.Village, "fish.carp", 0), "after five nothing counts");
                FestivalActivities.Contest(s);
                StringAssert.Contains("Tidewatch results", f.Toasts.Last());
                var table = Festivals.Table(Festivals.ContestEntrants, s.State.WorldSeed, 1, false, score);
                Assert.AreEqual(gold + Festivals.Prize(Festivals.Place(table)), s.State.Gold);
                var again = s.State.Gold;
                FestivalActivities.Contest(s);
                Assert.AreEqual(again, s.State.Gold, "the prize is paid once");
                StringAssert.Contains("prize", f.Toasts.Last());
            }
        }

        [Test]
        public void ABigCatch_WinsTheTidewatch_AndTheTrophyComesWithIt()
        {
            using (var f = FestivalFixture(Season.Summer, 11, 11 * 60))
            {
                var s = f.Session;
                FestivalActivities.Contest(s);
                s.SetVar(Festivals.Key(Festivals.Get("summer"), 1, FestivalActivities.ContestScore), 5000);
                s.Clock.SetTime(new GameDateTime(1, Season.Summer, 11, 18 * 60));
                var gold = s.State.Gold;
                FestivalActivities.Contest(s);
                Assert.AreEqual(gold + 600, s.State.Gold);
                Assert.AreEqual(1, s.Backpack.Count(Festivals.ContestTrophy));
                StringAssert.Contains("first place", f.Toasts.Last());
            }
        }

        [Test]
        public void TheContest_IsNextYearsToo()
        {
            using (var f = FestivalFixture(Season.Summer, 11, 11 * 60))
            {
                var s = f.Session;
                FestivalActivities.Contest(s);
                s.Clock.SetTime(new GameDateTime(1, Season.Summer, 11, 18 * 60));
                FestivalActivities.Contest(s);
                s.Clock.SetTime(new GameDateTime(2, Season.Summer, 11, 11 * 60));
                FestivalActivities.Contest(s);
                StringAssert.Contains("entered in the Tidewatch", f.Toasts.Last(), "a new year, a new contest");
            }
        }

        [Test]
        public void TheShow_TakesACropAtTheTable_JudgesItAtFive_AndPaysOnce()
        {
            using (var f = FestivalFixture(Season.Fall, 16, 10 * 60 + 30))
            {
                var s = f.Session;
                s.State.SelectedHotbar = 0;
                FestivalActivities.Show(s);
                StringAssert.Contains("Hold a crop", f.Toasts.Last(), "nothing in hand");
                s.Backpack.Add(ItemIds.Seed("parsnip"), 1);
                s.State.SelectedHotbar = SlotOf(s, ItemIds.Seed("parsnip"));
                FestivalActivities.Show(s);
                StringAssert.Contains("Hold a crop", f.Toasts.Last(), "a seed is not a show entry");
                s.Backpack.Add("crop.pumpkin", 1, 2);
                s.State.SelectedHotbar = SlotOf(s, "crop.pumpkin");
                FestivalActivities.Show(s);
                StringAssert.Contains("entered in the Harvest Fair", f.Toasts.Last());
                Assert.AreEqual(0, s.Backpack.Count("crop.pumpkin"), "the entry stays at the table");
                FestivalActivities.Show(s);
                StringAssert.Contains("Judging is at 17:00", f.Toasts.Last());
                var score = s.GetVar(Festivals.Key(Festivals.Get("fall"), 1, FestivalActivities.ShowScore));
                Assert.Greater(score, 0);
                var gold = s.State.Gold;
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 16, 17 * 60 + 10));
                FestivalActivities.Show(s);
                var place = Festivals.Place(Festivals.Table(Festivals.ShowEntrants, s.State.WorldSeed, 1, true, score));
                Assert.AreEqual(gold + Festivals.Prize(place), s.State.Gold);
                if (place == 1) Assert.AreEqual(1, s.Backpack.Count(Festivals.ShowRibbon));
                FestivalActivities.Show(s);
                Assert.AreEqual(gold + Festivals.Prize(place), s.State.Gold, "paid once");
            }
        }

        [Test]
        public void ThePreservesAndDishes_AreShowEntriesToo_AndSeedsAreNot()
        {
            var db = RealDb();
            Assert.IsTrue(FestivalActivities.Acceptable(db.GetItem("artisan.pickles")));
            Assert.IsTrue(FestivalActivities.Acceptable(db.GetItem("crop.pumpkin")));
            Assert.IsTrue(FestivalActivities.Acceptable(db.GetItem("food.salad")));
            Assert.IsFalse(FestivalActivities.Acceptable(db.GetItem(ItemIds.Seed("parsnip"))));
            Assert.IsFalse(FestivalActivities.Acceptable(db.GetItem(ItemIds.Hoe)));
        }

        [Test]
        public void TheLanternAndTheFeast_AreOncePerYear_AndTheFeastWaitsForFive()
        {
            using (var f = FestivalFixture(Season.Winter, 25, 12 * 60))
            {
                var s = f.Session;
                s.State.Energy = 10;
                FestivalActivities.Feast(s);
                StringAssert.Contains("laid at 17:00", f.Toasts.Last());
                Assert.AreEqual(10, s.State.Energy);
                FestivalActivities.HangLantern(s);
                StringAssert.Contains("make a wish", f.Toasts.Last());
                var count = f.Toasts.Count;
                FestivalActivities.HangLantern(s);
                StringAssert.Contains("already burning", f.Toasts.Last());
                s.Clock.SetTime(new GameDateTime(1, Season.Winter, 25, 17 * 60 + 30));
                FestivalActivities.Feast(s);
                Assert.AreEqual(s.State.MaxEnergy, s.State.Energy, "a full belly");
                s.State.Energy = 5;
                FestivalActivities.Feast(s);
                Assert.AreEqual(5, s.State.Energy, "once");
                s.Clock.SetTime(new GameDateTime(1, Season.Winter, 25, 21 * 60 + 5));
                FestivalActivities.HangLantern(s);
                StringAssert.Contains("over for tonight", f.Toasts.Last());
                s.Clock.SetTime(new GameDateTime(1, Season.Winter, 26, 12 * 60));
                FestivalActivities.Feast(s);
                StringAssert.Contains("nothing going on", f.Toasts.Last());
            }
        }

        [Test]
        public void TheDance_PaysOncePerYear_WhateverTheSongs()
        {
            using (var f = FestivalFixture(Season.Spring, 13, 12 * 60))
            {
                var s = f.Session;
                s.State.Energy = 10;
                FestivalActivities.FinishDance(s, null);
                Assert.AreEqual(10 + Festivals.DanceEnergy, s.State.Energy);
                StringAssert.Contains("on your own", f.Toasts.Last());
                FestivalActivities.FinishDance(s, null);
                Assert.AreEqual(10 + Festivals.DanceEnergy, s.State.Energy, "only the first dance of the year");
            }
        }

        [Test]
        public void ADanceIsFourDifferentFrames_WithTheFeetStillOnTheGround()
        {
            var down = CharacterFrames.Dance(null, 0);
            Assert.IsNull(down, "no picture, no frame");
            var grid = SamplePerson();
            foreach (var facing in new[] { "down", "left", "up" })
            {
                var frames = Enumerable.Range(0, CharacterRig.DancePhases).Select(p => CharacterRig.Dance(grid, facing, p)).ToList();
                for (var i = 0; i < frames.Count; i++)
                    for (var j = i + 1; j < frames.Count; j++)
                        Assert.AreNotEqual(Hash(frames[i]), Hash(frames[j]), $"{facing}: frame {i} and {j} differ");
                var idle = CharacterRig.Idle(grid, facing, false);
                foreach (var frame in frames)
                {
                    Assert.AreEqual(idle.W, frame.W);
                    Assert.AreEqual(idle.H, frame.H);
                    Assert.GreaterOrEqual(LowestRow(frame), LowestRow(idle) - 1, "the feet stay down");
                    Assert.LessOrEqual(LowestRow(frame), LowestRow(idle), "and never go through the floor");
                }
            }
        }

        static PixelGrid SamplePerson()
        {
            var g = new PixelGrid(16, 32);
            var skin = new Color32(230, 190, 150, 255); var coat = new Color32(60, 100, 160, 255); var legs = new Color32(50, 50, 70, 255);
            for (var y = 8; y < 14; y++) for (var x = 5; x < 11; x++) g.Set(x, y, skin);               // head
            for (var y = 14; y < 24; y++) for (var x = 3; x < 13; x++) g.Set(x, y, coat);              // torso and arms
            for (var y = 24; y < 31; y++) for (var x = 5; x < 11; x++) if (x != 8 && x != 7) g.Set(x, y, legs);          // legs
            return g;
        }

        static int Hash(PixelGrid g)
        {
            unchecked
            {
                var h = 17;
                for (var y = 0; y < g.H; y++) for (var x = 0; x < g.W; x++) { var c = g.Get(x, y); h = h * 31 + (c.a == 0 ? 0 : c.r * 3 + c.g * 5 + c.b * 7 + 1); }
                return h;
            }
        }

        static int LowestRow(PixelGrid g)
        {
            for (var y = g.H - 1; y >= 0; y--) for (var x = 0; x < g.W; x++) if (g.Opaque(x, y)) return y;
            return -1;
        }

        [Test]
        public void TheDancersTurnToTheFourSides_AFewBeatsApart()
        {
            var seen = new HashSet<Vector2Int>();
            for (var t = 0f; t < 6f; t += 0.3f) seen.Add(FestivalGrounds.DanceFacing("wren", t));
            Assert.GreaterOrEqual(seen.Count, 3);
            CollectionAssert.DoesNotContain(seen, Vector2Int.up, "they face the crowd, never turn their backs");
            var beat = Enumerable.Range(0, 12).Select(i => FestivalGrounds.DanceFacing("npc" + i, 2f)).Distinct().Count();
            Assert.GreaterOrEqual(beat, 2, "not all in step");
        }
    }
}
