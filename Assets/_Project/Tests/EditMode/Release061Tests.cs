using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace Farm.Tests
{
    // Playtest notes of 2026-10-13 (the second batch): the mine's ways in and out, no slimes in cozy, the item bar's rows, the crafting button of a pad, the health bar's call-out.
    public class Release061Tests
    {
        static Dictionary<string, string> Table() =>
            JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));

        // ---- the mine ---------------------------------------------------------------------------------------------

        // A plain walk from the arrival cell that cannot pass through a wall, the stairs up, the lift or any ore or rock.
        static bool[] Walkable(MineFloor f, bool nodesBlock)
        {
            var seen = new bool[f.Width * f.Height];
            var blocked = new HashSet<(int, int)> { f.Up, f.Elevator };
            if (nodesBlock) foreach (var n in f.Nodes) blocked.Add((n.X, n.Y));
            var q = new Queue<(int, int)>();
            q.Enqueue(f.Spawn);
            seen[f.Spawn.y * f.Width + f.Spawn.x] = true;
            while (q.Count > 0)
            {
                var (x, y) = q.Dequeue();
                foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    int nx = x + dx, ny = y + dy;
                    if (f.IsWall(nx, ny) || seen[ny * f.Width + nx] || blocked.Contains((nx, ny))) continue;
                    seen[ny * f.Width + nx] = true;
                    q.Enqueue((nx, ny));
                }
            }
            return seen;
        }

        [Test]
        public void TheLadderDown_CanAlwaysBeWalkedTo_WithTheStairsTheLiftAndEveryRockInTheWay()
        {
            foreach (var seed in new[] { 1, 99, 12345, -777, 4242, 31337 })
                for (var floor = 1; floor < MineGenerator.Floors; floor++)
                {
                    var f = MineGenerator.Generate(MineGenerator.FloorSeed(seed, 5, floor), floor);
                    var seen = Walkable(f, nodesBlock: true);
                    Assert.IsTrue(seen[f.Ladder.Value.y * f.Width + f.Ladder.Value.x], $"floor {floor}, seed {seed}: the ladder cannot be walked to");
                    Assert.IsFalse(f.Nodes.Any(n => System.Math.Abs(n.X - f.Ladder.Value.x) + System.Math.Abs(n.Y - f.Ladder.Value.y) <= 1), "no ore beside the ladder");
                }
        }

        [Test]
        public void TheStairsUp_AndTheLift_StandBesideTheArrivalCell_InOpenGround()
        {
            for (var floor = 1; floor <= MineGenerator.Floors; floor++)
            {
                var f = MineGenerator.Generate(floor * 29, floor);
                Assert.IsFalse(f.IsWall(f.Up.x, f.Up.y), "the way up");
                Assert.IsFalse(f.IsWall(f.Elevator.x, f.Elevator.y), "the lift");
                var seen = Walkable(f, nodesBlock: false);
                Assert.IsTrue(seen[f.Spawn.y * f.Width + f.Spawn.x] && (seen[f.Spawn.y * f.Width + f.Spawn.x + 1] || seen[(f.Spawn.y - 1) * f.Width + f.Spawn.x]),
                    $"floor {floor}: the arrival cell leads somewhere with the stairs and the lift in place");
                Assert.IsFalse(f.Nodes.Any(n => (n.X, n.Y) == f.Up || (n.X, n.Y) == f.Elevator));
            }
        }

        [Test]
        public void Cozy_HasNoSlimes_AndTheBatsAreLeft()
        {
            var all = Enumerable.Range(1, 40).SelectMany(fl => Enumerable.Range(0, 4).SelectMany(s => MineGenerator.Generate(s * 977 + fl, fl, slimes: false).Enemies)).Select(e => e.EnemyId).Distinct().ToList();
            CollectionAssert.DoesNotContain(all, "slime");
            CollectionAssert.Contains(all, "cave_bat");
            var withSlimes = Enumerable.Range(1, 15).SelectMany(fl => Enumerable.Range(0, 4).SelectMany(s => MineGenerator.Generate(s * 977 + fl, fl).Enemies)).Select(e => e.EnemyId).Distinct().ToList();
            CollectionAssert.Contains(withSlimes, "slime", "the other settings keep them");
        }

        [Test]
        public void TheMarkersOfTheStairs_AreThePropsOfTheCavesPack_AndTheirPicturesAreThere()
        {
            foreach (var kind in new[] { MineStairs.StairKind.Up, MineStairs.StairKind.Down, MineStairs.StairKind.Elevator })
                Assert.IsTrue(File.Exists("Assets/_Project/Resources/" + MineController.PropOf(kind) + ".png"), kind.ToString());
        }

        // ---- the item bar ------------------------------------------------------------------------------------------

        [Test]
        public void TheBar_ShowsTheRowOfTheSelectedSlot_AndRowsStepRoundTheBackpack()
        {
            Assert.AreEqual(1, HotbarRows.Rows(12));
            Assert.AreEqual(3, HotbarRows.Rows(36));
            Assert.AreEqual(5, HotbarRows.Pick(0, 5, 36), "a number key picks that place of the row shown");
            Assert.AreEqual(17, HotbarRows.Pick(12, 5, 36));
            Assert.AreEqual(13, HotbarRows.Step(12, 1, 36), "the next item stays in the row");
            Assert.AreEqual(12, HotbarRows.Step(23, 1, 36), "and goes round at its end");
            Assert.AreEqual(23, HotbarRows.Step(12, -1, 36));
            Assert.AreEqual(17, HotbarRows.ChangeRow(5, 1, 36), "the row buttons keep the place in the row");
            Assert.AreEqual(5, HotbarRows.ChangeRow(29, 1, 36), "round to the first row");
            Assert.AreEqual(29, HotbarRows.ChangeRow(5, -1, 36));
            Assert.AreEqual(5, HotbarRows.ChangeRow(5, 1, 12), "one row has nowhere to go");
            Assert.AreEqual(17, HotbarRows.ChangeRow(5, 1, 18), "a short last row takes what it has");
            Assert.AreEqual(17, HotbarRows.ChangeRow(11, 1, 18), "a short last row takes its last slot");
        }

        // ---- the keys ----------------------------------------------------------------------------------------------

        [Test]
        public void Crafting_HasAKeyAndAPadButton_AndTheRowsHaveBoth()
        {
            var asset = UnityEngine.Resources.Load<InputActionAsset>(InputNames.ResourcePath);
            var gameplay = asset.FindActionMap(InputNames.GameplayMap, true);
            var crafting = gameplay[InputNames.Crafting].bindings.Select(b => b.path).ToList();
            CollectionAssert.Contains(crafting, "<Keyboard>/c");
            CollectionAssert.Contains(crafting, "<Gamepad>/leftTrigger");
            foreach (var name in new[] { InputNames.RowNext, InputNames.RowPrev })
            {
                var paths = gameplay[name].bindings.Select(b => b.path).ToList();
                Assert.IsTrue(paths.Any(p => p.StartsWith("<Keyboard>")), name + " key");
                if (name == InputNames.RowNext) Assert.IsTrue(paths.Any(p => p.StartsWith("<Gamepad>")), name + " pad");
            }
            var pad = gameplay.actions.Where(a => a.name != InputNames.Move).SelectMany(a => a.bindings.Select(b => b.path)).Where(p => p.StartsWith("<Gamepad>")).ToList();
            CollectionAssert.AreEquivalent(pad.Distinct().ToList(), pad, "a pad button does one thing");
            var table = Table();
            foreach (var key in new[] { "action.RowNext", "action.RowPrev", "prompt.kb.RowNext", "prompt.pad.RowNext", "prompt.kb.RowPrev", "prompt.pad.RowPrev", "prompt.pad.Crafting" })
                Assert.IsTrue(table.ContainsKey(key), key);
        }

        [Test]
        public void TheFarm_GrowsDandelionsInSpring_AndOnlyThen()
        {
            var farm = ForageDefaults.CreateTables().First(t => t.Id == "farm.clutter");
            var dandelion = farm.Entries.First(e => e.NodeId == "dandelion");
            Assert.IsTrue(dandelion.Seasons.HasFlag(SeasonMask.Spring));
            Assert.IsFalse(dandelion.Seasons.HasFlag(SeasonMask.Summer) || dandelion.Seasons.HasFlag(SeasonMask.Fall) || dandelion.Seasons.HasFlag(SeasonMask.Winter));
        }

        // ---- the options and the labels -----------------------------------------------------------------------------

        [Test]
        public void TheWorldLabel_FollowsTheAimedCell_UnlessTheMouseIsTheOneAiming()
        {
            Assert.IsTrue(HoverInspector.FollowsTarget(pad: true, mouseAiming: true));
            Assert.IsTrue(HoverInspector.FollowsTarget(pad: false, mouseAiming: false), "walking with the keys: the label is on what is faced");
            Assert.IsFalse(HoverInspector.FollowsTarget(pad: false, mouseAiming: true));
        }

        [Test]
        public void EveryKeyAndMouseBinding_OfBothMaps_HasARow_InTheOptions()
        {
            var src = File.ReadAllText("Assets/_Project/Scripts/UI/OptionsScreen.cs");
            StringAssert.Contains("AlternateRows();", src);
            StringAssert.Contains("DpadRows();", src);
            StringAssert.Contains("WithControlsHavingToMatchPath(\"<Mouse>\")", src);
            var table = Table();
            foreach (var key in new[] { "options.alternates", "options.alt", "options.alt_part", "options.dpad_part", "action.Navigate" }) Assert.IsTrue(table.ContainsKey(key), key);
        }

        // ---- the Help tree and the health bar --------------------------------------------------------------------------

        [Test]
        public void EveryTool_SaysWhereItIsFound_AndEveryHelpTopicHasATitle()
        {
            var table = Table();
            foreach (var tool in HelpPage.ToolOrder) Assert.IsTrue(table.ContainsKey(HelpPage.WhereKey(tool)), tool.ToString());
            StringAssert.Contains("Fish Stall", table[HelpPage.WhereKey(ToolType.Rod)]);
            foreach (var key in HelpPage.Topics.Concat(HelpPage.Controls))
            {
                Assert.IsTrue(table.ContainsKey(key), key);
                Assert.IsTrue(table.ContainsKey(HelpPage.TitleKeyOf(key)), key + " title");
            }
        }

        [Test]
        public void TheHealthBar_IsExplainedOnce_ByAMessage_AndInTheHelp()
        {
            var table = Table();
            Assert.IsTrue(table.ContainsKey("tutorial.health_bar"));
            StringAssert.Contains("heart", table["tutorial.health_bar"]);
            Assert.AreEqual("tutorial.health_bar", HudView.HealthBarFlag);
        }
    }
}
