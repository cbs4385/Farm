using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-051/T-052: the mine generator (determinism, reachability, depth rules) and combat (damage, the enemy brain, loot).
    public class MineAndCombatTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        // ---- the generator ---------------------------------------------------------------------------------------------

        static string Describe(MineFloor f) =>
            string.Join("", f.Wall.Select(w => w ? '#' : '.')) + $"|{f.Spawn}|{f.Ladder}|" + string.Join(",", f.Nodes.Select(n => $"{n.X},{n.Y},{n.NodeId}"))
            + "|" + string.Join(",", f.Enemies.Select(e => $"{e.X},{e.Y},{e.EnemyId}"));

        [Test]
        public void SameSeedAndFloor_GiveTheSameFloor_DifferentOnesDiffer()
        {
            Assert.AreEqual(Describe(MineGenerator.Generate(42, 7)), Describe(MineGenerator.Generate(42, 7)));
            Assert.AreNotEqual(Describe(MineGenerator.Generate(42, 7)), Describe(MineGenerator.Generate(43, 7)));
            Assert.AreNotEqual(Describe(MineGenerator.Generate(42, 7)), Describe(MineGenerator.Generate(42, 8)));
            Assert.AreNotEqual(MineGenerator.FloorSeed(1, 5, 3), MineGenerator.FloorSeed(1, 6, 3), "a new day, a new floor");
        }

        [Test]
        public void AllFortyFloors_AreReachable_ForManySeeds()
        {
            foreach (var seed in new[] { 1, 99, 12345, -777 })
                for (var floor = 1; floor <= MineGenerator.Floors; floor++)
                {
                    var f = MineGenerator.Generate(MineGenerator.FloorSeed(seed, 3, floor), floor);
                    var dist = MineGenerator.Distances(f);
                    Assert.IsFalse(f.IsWall(f.Spawn.x, f.Spawn.y), $"floor {floor} spawn");
                    if (floor < MineGenerator.Floors)
                    {
                        Assert.IsTrue(f.Ladder.HasValue, $"floor {floor} has a ladder");
                        Assert.GreaterOrEqual(dist[f.Ladder.Value.y * f.Width + f.Ladder.Value.x], 10, $"floor {floor}: the ladder is a real walk away");
                    }
                    else Assert.IsFalse(f.Ladder.HasValue, "the bottom has no ladder");
                    foreach (var n in f.Nodes) Assert.GreaterOrEqual(dist[n.Y * f.Width + n.X], 0, $"floor {floor}: node reachable");
                    foreach (var e in f.Enemies) Assert.GreaterOrEqual(dist[e.Y * f.Width + e.X], 0, $"floor {floor}: enemy reachable");
                    Assert.IsTrue(f.IsWall(0, 0) && f.IsWall(f.Width - 1, f.Height - 1), "a wall border");
                }
        }

        [Test]
        public void TheSpawnHasRoomForTheStairsAndTheElevator()
        {
            for (var floor = 1; floor <= 40; floor++)
            {
                var f = MineGenerator.Generate(floor * 13, floor);
                Assert.IsFalse(f.IsWall(f.Spawn.x - 1, f.Spawn.y), "stairs up");
                Assert.IsFalse(f.IsWall(f.Spawn.x, f.Spawn.y + 1), "the elevator");
            }
        }

        [Test]
        public void Ore_FollowsDepth()
        {
            string[] Kinds(int from, int to) => Enumerable.Range(from, to - from + 1)
                .SelectMany(fl => Enumerable.Range(0, 6).SelectMany(s => MineGenerator.Generate(s * 977 + fl, fl).Nodes)).Select(n => n.NodeId).Distinct().ToArray();
            CollectionAssert.DoesNotContain(Kinds(1, 9), MineGenerator.Iron);
            CollectionAssert.DoesNotContain(Kinds(1, 24), MineGenerator.Gold);
            CollectionAssert.Contains(Kinds(1, 9), MineGenerator.Copper);
            CollectionAssert.Contains(Kinds(10, 24), MineGenerator.Iron);
            CollectionAssert.Contains(Kinds(25, 39), MineGenerator.Gold);
            CollectionAssert.Contains(Kinds(1, 39), MineGenerator.Coal);
        }

        [Test]
        public void Monsters_FollowDepth_AndTheBossWaitsAtTheBottom()
        {
            var shallow = Enumerable.Range(1, 2).SelectMany(fl => Enumerable.Range(0, 8).SelectMany(s => MineGenerator.Generate(s + fl * 100, fl).Enemies)).Select(e => e.EnemyId).Distinct().ToList();
            CollectionAssert.IsSubsetOf(shallow, new[] { "slime" }, "only slimes on the first floors");
            var deep = MineGenerator.Generate(5, 38).Enemies.Select(e => EnemyDefaults.Row(e.EnemyId)).ToList();
            Assert.IsTrue(deep.All(e => e.MinFloor <= 38 && e.MaxFloor >= 38 && !e.Boss));
            Assert.Greater(MineGenerator.Generate(5, 36).Enemies.Count, MineGenerator.Generate(5, 1).Enemies.Count, "deeper floors are busier");

            var boss = MineGenerator.Generate(5, 40);
            Assert.IsTrue(boss.IsBoss);
            Assert.AreEqual(new[] { EnemyDefaults.Boss }, boss.Enemies.Select(e => e.EnemyId).ToArray());
            Assert.IsEmpty(boss.Nodes);
        }

        [Test]
        public void Elevator_StopsEveryFifthFloorReached()
        {
            CollectionAssert.AreEqual(new[] { 1 }, MineGenerator.ElevatorFloors(1));
            CollectionAssert.AreEqual(new[] { 1 }, MineGenerator.ElevatorFloors(4));
            CollectionAssert.AreEqual(new[] { 1, 5, 10 }, MineGenerator.ElevatorFloors(12));
            Assert.AreEqual(9, MineGenerator.ElevatorFloors(40).Count);
        }

        [Test]
        public void DetRandom_IsStable()
        {
            var a = new DetRandom(5); var b = new DetRandom(5);
            for (var i = 0; i < 20; i++) Assert.AreEqual(a.Next(), b.Next());
            var r = new DetRandom(1);
            for (var i = 0; i < 1000; i++) { var u = r.Unit(); Assert.That(u, Is.InRange(0f, 0.9999999f)); }
        }

        // ---- content --------------------------------------------------------------------------------------------------

        [Test]
        public void MineNodes_AndDrops_AreGenerated()
        {
            var db = RealDb();
            foreach (var n in MineGenerator.CreateNodes())
            {
                var node = db.AllNodes.FirstOrDefault(x => x.Id == n.Id);
                Assert.IsNotNull(node, n.Id);
                Assert.IsNotNull(node.Sprite, n.Id);
                Assert.IsTrue(db.TryGetItem(node.DropItemId, out _));
                Assert.AreEqual(0f, node.SpawnWeight, "never farm clutter");
            }
            foreach (var e in EnemyDefaults.Rows)
                foreach (var d in e.Drops) Assert.IsTrue(db.TryGetItem(d.ItemId, out _), $"{e.Id} drops {d.ItemId}");
            foreach (var w in CombatModel.Weapons) { Assert.IsTrue(db.TryGetItem(w.ItemId, out var item)); Assert.AreEqual(ToolType.Sword, item.ToolType); }
        }

        [Test]
        public void TheFarmerStartsWithASword_AndTheBlacksmithSellsBetterOnes()
        {
            var db = RealDb();
            var state = GameState.NewGame("a", "b", db.MaxStack);
            Assert.IsTrue(Inventory.FromData(state.Backpack, db.MaxStack).Has(ItemIds.Sword));
            var stock = ShopCatalog.For(db, "blacksmith", new FakeWorld()).Select(i => i.Id).ToList();
            CollectionAssert.IsSupersetOf(stock, new[] { "tool.sword_steel", "tool.sword_gold" });
        }

        sealed class FakeWorld : IWorldQuery
        {
            public bool HasFlag(string f) => false;
            public int GetVar(string n) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Mine";
        }

        // ---- combat arithmetic -----------------------------------------------------------------------------------------

        [Test]
        public void Swing_UsesTheWeaponPlusCombatLevel()
        {
            Assert.AreEqual(10, CombatModel.SwingDamage(ItemIds.Sword, 1));
            Assert.AreEqual(15, CombatModel.SwingDamage(ItemIds.Sword, 6));
            Assert.Greater(CombatModel.SwingDamage("tool.sword_gold", 1), CombatModel.SwingDamage("tool.sword_steel", 1));
        }

        [Test]
        public void ABasicSword_KillsTheFirstFloorMonstersInAFewSwings()
        {
            var slime = EnemyDefaults.Row("slime");
            Assert.LessOrEqual(Mathf.CeilToInt(slime.Hp / (float)CombatModel.SwingDamage(ItemIds.Sword, 1)), 3);
            Assert.GreaterOrEqual(slime.Hp, 10);
            var warden = EnemyDefaults.Row(EnemyDefaults.Boss);
            Assert.Greater(Mathf.CeilToInt(warden.Hp / (float)CombatModel.SwingDamage("tool.sword_gold", 10)), 15, "the boss is a real fight even with the best blade");
        }

        [Test]
        public void KnockOut_CostsATenthOfTheGold_Capped_AndHalfHealthBack()
        {
            Assert.AreEqual(50, CombatModel.KnockOutGoldLoss(500));
            Assert.AreEqual(1000, CombatModel.KnockOutGoldLoss(500000));
            Assert.AreEqual(0, CombatModel.KnockOutGoldLoss(5));
            Assert.AreEqual(50, CombatModel.KnockOutHealth(100));
        }

        [Test]
        public void Loot_RollsEachDropOnItsChance_AndLuckHelps()
        {
            var slime = EnemyDefaults.Row("slime");
            int Drops(float luck) { var n = 0; for (var i = 0; i < 2000; i++) n += CombatModel.Loot(slime, luck, k => ((i * 2 + k) % 2000 + 0.5f) / 2000f).Count; return n; }
            Assert.Greater(Drops(0f), 0);
            Assert.Greater(Drops(1f), Drops(0f));
            Assert.AreEqual(Drops(0f), Drops(-1f), "bad luck does not reduce drops");
            var boss = CombatModel.Loot(EnemyDefaults.Row(EnemyDefaults.Boss), 0f, k => 0.99f);
            CollectionAssert.AreEquivalent(new[] { ItemIds.GoldBar, "resource.warden_core" }, boss.Select(b => b.item).ToArray());
            Assert.AreEqual(3, boss.First(b => b.item == ItemIds.GoldBar).count);
        }

        // ---- the enemy brain -------------------------------------------------------------------------------------------

        [Test]
        public void TheBrain_IdlesFarAway_ChasesWhenNear_AndStrikesWhenAdjacent()
        {
            var brain = new EnemyBrain(EnemyDefaults.Row("slime"), 1);
            var far = brain.Tick(0.1f, Vector2.zero, new Vector2(50, 0));
            Assert.AreEqual(EnemyMode.Idle, brain.Mode);
            Assert.IsFalse(far.Strike);

            var chase = brain.Tick(0.1f, Vector2.zero, new Vector2(3, 0));
            Assert.AreEqual(EnemyMode.Chase, brain.Mode);
            Assert.Greater(chase.Move.x, 0f, "towards the player");

            var strike = brain.Tick(0.1f, Vector2.zero, new Vector2(0.5f, 0));
            Assert.AreEqual(EnemyMode.Attack, brain.Mode);
            Assert.IsTrue(strike.Strike);
            Assert.IsFalse(brain.Tick(0.1f, Vector2.zero, new Vector2(0.5f, 0)).Strike, "it has to wait between blows");
            for (var i = 0; i < 12; i++) brain.Tick(0.1f, Vector2.zero, new Vector2(0.5f, 0));
            Assert.IsTrue(brain.Tick(0.2f, Vector2.zero, new Vector2(0.5f, 0)).Strike || brain.Mode == EnemyMode.Attack);
        }

        [Test]
        public void ADamagedBrain_IsStunnedBriefly_AndDiesAtZero()
        {
            var brain = new EnemyBrain(EnemyDefaults.Row("slime"), 1);
            Assert.IsFalse(brain.Hurt(5));
            Assert.AreEqual(EnemyMode.Hurt, brain.Mode);
            var step = brain.Tick(0.1f, Vector2.zero, new Vector2(2, 0));
            Assert.AreEqual(Vector2.zero, step.Move, "stunned");
            brain.Tick(0.3f, Vector2.zero, new Vector2(2, 0));
            Assert.AreNotEqual(EnemyMode.Hurt, brain.Mode);
            Assert.IsTrue(brain.Hurt(100));
            Assert.AreEqual(EnemyMode.Dead, brain.Mode);
            Assert.IsFalse(brain.Hurt(5), "already dead");
            Assert.AreEqual(Vector2.zero, brain.Tick(1f, Vector2.zero, Vector2.one).Move);
        }

        [Test]
        public void EveryEnemy_HasSaneBalanceData()
        {
            foreach (var e in EnemyDefaults.Rows)
            {
                Assert.Greater(e.Hp, 0, e.Id);
                Assert.Greater(e.Damage, 0, e.Id);
                Assert.Greater(e.Speed, 0f, e.Id);
                Assert.LessOrEqual(e.MinFloor, e.MaxFloor, e.Id);
                Assert.IsNotEmpty(e.Drops, e.Id);
                Assert.Less(e.Damage, 40, "a single touch never one-shots a full-health player");
            }
            Assert.AreEqual(5, EnemyDefaults.Rows.Count(e => !e.Boss), "five kinds of enemy");
        }

        // ---- health, knockout ------------------------------------------------------------------------------------------

        [Test]
        public void Hurt_ReducesHealth_AndReportsZero()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                Assert.IsFalse(Combat.Hurt(s, 30));
                Assert.AreEqual(70, s.State.Health);
                Assert.IsTrue(Combat.Hurt(s, 500));
                Assert.AreEqual(0, s.State.Health);
                Combat.Heal(s, 30);
                Assert.AreEqual(30, s.State.Health);
                Combat.Heal(s, 500);
                Assert.AreEqual(s.State.MaxHealth, s.State.Health);
            }
        }

        [Test]
        public void MineStateAndFloorSurviveASaveRoundTrip()
        {
            var state = new GameState();
            state.Mine.Floor = 12; state.Mine.Deepest = 15; state.Mine.Kills = 44; state.Mine.GenDay = 9; state.Mine.GenFloor = 12;
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(Newtonsoft.Json.JsonConvert.SerializeObject(state));
            Assert.AreEqual((12, 15, 44), (back.Mine.Floor, back.Mine.Deepest, back.Mine.Kills));
            Assert.AreEqual(0, Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>("{\"Gold\":1}").Mine.Floor);
        }
    }
}
