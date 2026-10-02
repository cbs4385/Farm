using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-057 (professions, collections, shipping stats, the traveling merchant) and the T-058 gate checks: a two-year
    // simulation using every Milestone 3 system, and a cheap performance guard.
    public class Milestone3Tests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        TestSessionFixture Fixture(bool story = false)
        {
            var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops);
            if (story) f.Session.Story = StoryContent.LoadFromResources();
            return f;
        }

        // ---- professions ---------------------------------------------------------------------------------------------------

        [Test]
        public void ThereAreTwentyProfessions_TwoPerTierAtLevelsFiveAndTen()
        {
            Assert.AreEqual(20, Professions.Rows.Length);
            foreach (var skill in SkillIds.All)
                foreach (var level in new[] { 5, 10 })
                    Assert.AreEqual(2, Professions.Rows.Count(r => r.Skill == skill && r.Level == level), $"{skill} {level}");
        }

        [Test]
        public void Choices_OpenAtTheirLevel_OneOfTwo_AndAreNotRepeated()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                Assert.IsEmpty(Professions.Offers(s.State, SkillIds.Farming, 4));
                Assert.AreEqual(2, Professions.Offers(s.State, SkillIds.Farming, 5).Count);
                Assert.AreEqual(4, Professions.Offers(s.State, SkillIds.Farming, 10).Count, "both tiers open if the first was never chosen");
                s.AddSkillXp(SkillIds.Farming, 1000);                                   // level 6
                Assert.IsTrue(Professions.Choose(s, "tiller"));
                Assert.IsFalse(Professions.Choose(s, "rancher"), "only one of the pair");
                Assert.IsFalse(Professions.Choose(s, "artisan"), "level 10 is not reached");
                Assert.IsEmpty(Professions.Offers(s.State, SkillIds.Farming, 6));
                Assert.IsFalse(Professions.Choose(s, "nope"));
                Assert.That(f.Toasts, Has.Some.Contains("profession"));
            }
        }

        [Test]
        public void Professions_ChangeSellingPrices_ByCategory()
        {
            var db = RealDb();
            var state = new GameState();
            var crop = db.GetItem("crop.pumpkin"); var fish = db.GetItem("fish.tuna"); var egg = db.GetItem("product.egg"); var jam = db.GetItem("artisan.jam");
            Assert.AreEqual(1f, Professions.SellMultiplier(state, crop));
            state.Professions.Add("tiller"); state.Professions.Add("fisher"); state.Professions.Add("rancher"); state.Professions.Add("artisan");
            Assert.AreEqual(1.10f, Professions.SellMultiplier(state, crop), 0.001f);
            Assert.AreEqual(1.25f, Professions.SellMultiplier(state, fish), 0.001f);
            Assert.AreEqual(1.15f, Professions.SellMultiplier(state, egg), 0.001f, "animal products");
            Assert.AreEqual(1.25f, Professions.SellMultiplier(state, jam), 0.001f, "artisan goods");
            Assert.AreEqual(1f, Professions.SellMultiplier(state, db.GetItem("resource.wood")));
        }

        [Test]
        public void TheShippingBin_PaysTheProfessionBonus_AndRecordsTotals()
        {
            var db = RealDb();
            var state = GameState.NewGame("a", "b", db.MaxStack, 1);
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 10, 1000));
            state.SetDate(clock.Now);
            state.Professions.Add("tiller");
            state.ShippingBin.Add(new ItemStack("crop.pumpkin", 2));
            var plainGold = state.Gold;
            var summary = DayCycle.EndDay(state, clock, new Dictionary<string, FarmGrid>(), id => db.TryGetItem(id, out var i) ? i : null, id => null, false);
            Assert.AreEqual((int)(320 * 2 * 1.10f), summary.Earnings);
            Assert.AreEqual(plainGold + summary.Earnings, state.Gold);
            Assert.AreEqual(2, state.ShippedTotals["crop.pumpkin"]);
            Assert.AreEqual(summary.Earnings, state.TotalEarned);
        }

        [Test]
        public void OtherProfessionEffects_ToolEnergyDamageHealthAndBites()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.AddSkillXp(SkillIds.Combat, 4000);
                s.AddSkillXp(SkillIds.Mining, 3000);
                s.AddSkillXp(SkillIds.Fishing, 3000);
                Assert.AreEqual(1f, Professions.EnergyMultiplier(s.State));
                Professions.Choose(s, "geologist");
                Assert.AreEqual(0.8f, Professions.EnergyMultiplier(s.State), 0.001f);
                Professions.Choose(s, "fighter");
                Assert.AreEqual(1.15f, Professions.DamageMultiplier(s.State), 0.001f);
                var maxBefore = s.State.MaxHealth;
                Professions.Choose(s, "scout");
                Assert.AreEqual(maxBefore, s.State.MaxHealth, "fighter and scout are a pair: only one");
                Professions.Choose(s, "defender");
                Assert.AreEqual(maxBefore + 50, s.State.MaxHealth);
                Professions.Choose(s, "trapper");
                Assert.AreEqual(1.3f, Professions.BiteSpeed(s.State), 0.001f);
                var quick = new FishingSession(new[] { FishDefaults.Rows[0] }, 1, 0f, false, 0f, 0.5f, 0.5f, 0.5f, 1.3f);
                var slow = new FishingSession(new[] { FishDefaults.Rows[0] }, 1, 0f, false, 0f, 0.5f, 0.5f, 0.5f, 1f);
                float Wait(FishingSession fs) { var t = 0f; while (fs.State == FishingState.Waiting && t < 30f) { fs.Tick(0.05f); t += 0.05f; } return t; }
                Assert.Less(Wait(quick), Wait(slow));
            }
        }

        // ---- collections ---------------------------------------------------------------------------------------------------

        [Test]
        public void ItemsAreRemembered_OnceHeld_EvenAfterTheyAreGone()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                Assert.IsFalse(s.State.Collected.Contains("fish.carp"));
                s.Backpack.Add("fish.carp", 1);
                Assert.IsTrue(s.State.Collected.Contains("fish.carp"));
                s.Backpack.Remove("fish.carp", 1);
                Assert.IsTrue(s.State.Collected.Contains("fish.carp"), "found once, found forever");
                var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(Newtonsoft.Json.JsonConvert.SerializeObject(s.State));
                Assert.IsTrue(back.Collected.Contains("fish.carp"));
            }
        }

        // ---- the merchant --------------------------------------------------------------------------------------------------

        [Test]
        public void TheMerchant_ComesOnSomeDays_DeterministicallyAndSellsRotatingOre()
        {
            var days = Enumerable.Range(0, 400).Where(d => Merchant.IsHere(77, d)).ToList();
            Assert.That(days.Count, Is.InRange(80, 160), "about three days in ten");
            CollectionAssert.AreEqual(days, Enumerable.Range(0, 400).Where(d => Merchant.IsHere(77, d)).ToList());

            using (var f = Fixture())
            {
                var s = f.Session;
                s.State.WorldSeed = 77;
                var here = days[0]; var away = Enumerable.Range(0, 20).First(d => !Merchant.IsHere(77, d));
                List<string> Stock(int day)
                {
                    s.Clock.SetTime(new GameDateTime((day / 112) + 1, (Season)((day / 28) % 4), day % 28 + 1, 10 * 60));
                    return ShopCatalog.For(s.Db, "merchant", s.World).Select(i => i.Id).ToList();
                }
                CollectionAssert.IsEmpty(Stock(away), "no stall, no stock");
                var stock = Stock(here);
                CollectionAssert.Contains(stock, ItemIds.Coal);
                Assert.AreEqual(2, stock.Count, "coal and one rotating ore");
                var kinds = days.Select(d => string.Join(",", Stock(d))).Distinct().Count();
                Assert.GreaterOrEqual(kinds, 3, "the ore on offer rotates");
            }
        }

        // ---- the gate: two years with everything ----------------------------------------------------------------------------

        sealed class Spy : IDayCycleHook
        {
            public int Order => 0; public int Nights, Dawns;
            public void OnNightFalls(DayCycleContext c) => Nights++;
            public void OnDawn(DayCycleContext c) => Dawns++;
        }

        static string Fingerprint(GameState s) =>
            $"{s.Gold}|{s.Year}|{s.Weather}|{string.Join(",", s.Vars.OrderBy(v => v.Key).Select(v => v.Key + "=" + v.Value))}|{string.Join(",", s.Npcs.OrderBy(n => n.Key).Select(n => n.Key + n.Value.Points))}|{string.Join(",", s.Animals.Select(a => a.Id + a.Happiness))}|{s.Collected.Count}|{s.TotalEarned}";

        static GameState TwoYears(bool spy, int seed, out Spy hook)
        {
            hook = new Spy();
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops))
            {
                var s = f.Session;
                s.State.WorldSeed = seed;
                s.Story = StoryContent.LoadFromResources();
                s.Backpack.Resize(60);
                if (spy) s.Hooks.AddDayCycleHook(hook);
                StoryDay.NewGame(s);
                AnimalRules.Add(s.State, "chicken", "hen", "Hen");
                AnimalRules.Add(s.State, "cow", "cow", "Cow");
                var crop = RealDb().AllCrops.First(c => c.Id == "wheat");
                s.GetGrid(MapIds.Farm).Till(3, 3);

                for (var day = 0; day < 224; day++)
                {
                    var now = s.Clock.Now;
                    // A day of chores: feed the animals, collect, forage, fish, mine, craft, ship, befriend, talk.
                    s.Backpack.Add(AnimalRules.Feed, 2);
                    AnimalRules.FeedAll(s.State, MapIds.Coop, s.Backpack);
                    AnimalRules.FeedAll(s.State, MapIds.Barn, s.Backpack);
                    foreach (var a in s.State.Animals) AnimalRules.Collect(a, s.Backpack, out _);
                    s.AddVar(QuestLog.Stats.Tilled, 1); s.AddVar(QuestLog.Stats.Planted, 1); s.AddVar(QuestLog.Stats.Watered, 1);
                    s.GiveItem("fish.carp", 1);
                    s.GiveItem(ItemIds.CopperOre, 2);
                    s.AddSkillXp(SkillIds.Mining, 10); s.AddSkillXp(SkillIds.Fishing, 8); s.AddSkillXp(SkillIds.Farming, 12); s.AddSkillXp(SkillIds.Combat, 6);
                    NpcInteractions.AddPoints(s, NpcIds.All[day % NpcIds.All.Length], 12);
                    s.State.Npcs[NpcIds.All[day % NpcIds.All.Length]].Met = true;
                    for (var i = 0; i < s.Backpack.Capacity; i++)
                    {
                        var st = s.Backpack.Get(i);
                        if (st != null && (st.ItemId == "fish.carp" || st.ItemId == "product.egg" || st.ItemId == "product.milk")) { s.ShipSlot(i); break; }
                    }
                    s.Clock.SetTime(new GameDateTime(now.Year, now.Season, now.Day, 22 * 60));
                    s.EndDay(false);
                    foreach (var id in s.State.Mailbox.ToList()) { var l = s.Story.Letter(id); if (l != null) Mail.Finish(s, l); }
                    // Mine floor changes and nights must keep state valid.
                    s.State.Mine.Floor = 0;
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(Newtonsoft.Json.JsonConvert.SerializeObject(s.State));
            }
        }

        [Test]
        public void TwoYears_WithEveryMilestoneThreeSystem_RunWithoutErrors_AndAreRepeatable()
        {
            var a = TwoYears(false, 42, out _);
            var b = TwoYears(false, 42, out _);
            Assert.AreEqual(Fingerprint(a), Fingerprint(b));
            Assert.AreEqual(3, a.Year, "224 nights is two years");
            Assert.Greater(a.TotalEarned, 0);
            Assert.Greater(a.Collected.Count, 3);
            Assert.GreaterOrEqual(a.Npcs.Count, 12, "every villager was met");
            Assert.IsTrue(a.Animals.All(x => x.Happiness > 50), "well-fed animals are happy");
            Assert.IsTrue(a.MailKept.Contains("summer_seeds"));
        }

        [Test]
        public void AnIdleHook_StillChangesNothing_OverTwoYears()
        {
            var plain = TwoYears(false, 5, out _);
            var spied = TwoYears(true, 5, out var hook);
            Assert.AreEqual(Fingerprint(plain), Fingerprint(spied));
            Assert.AreEqual(224, hook.Nights);
            Assert.AreEqual(224, hook.Dawns);
        }

        [Test]
        public void ThePerformanceBudget_ForTheMineGenerator_IsComfortable()
        {
            var watch = Stopwatch.StartNew();
            for (var floor = 1; floor <= 40; floor++) MineGenerator.Generate(floor * 7, floor);
            Assert.Less(watch.ElapsedMilliseconds, 500, "forty floors generate in well under half a second");
        }

        [Test]
        public void EveryMilestoneThreeContentIsInTheValidator()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            BusinessHoursRegistry.RegisterConditionAtom();
            var table = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var db = RealDb();
            var problems = StoryValidator.Run(new ValidationInput
            {
                Story = StoryContent.LoadFromResources(), Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey,
                RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            });
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
            foreach (var key in new[] { "item.fish.carp.name", "item.animal.cow.name", "profession.angler", "mine.floor", "hall.title" }) Assert.IsTrue(table.ContainsKey(key), key);
        }
    }
}
