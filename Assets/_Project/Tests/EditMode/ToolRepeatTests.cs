using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class ToolRepeatTests
    {
        static readonly Vector3Int Cell = new Vector3Int(3, 4, 0);

        [Test]
        public void EveryToolRepeatsExceptTheRodAndTheHammer()
        {
            foreach (var x in new[] { ToolType.Axe, ToolType.Pickaxe, ToolType.Hoe, ToolType.WateringCan, ToolType.Scythe, ToolType.Sword })
                Assert.IsTrue(ToolRepeat.Repeats(x), x.ToString());
            foreach (var x in new[] { ToolType.Rod, ToolType.Hammer, ToolType.None })
                Assert.IsFalse(ToolRepeat.Repeats(x), x.ToString());          // the rod and the hammer are not swung at a cell: they open their own modes
            var r = new ToolRepeat();
            r.Arm(ToolType.Rod, Cell);
            Assert.IsFalse(r.Armed);
            r.Arm(ToolType.Hoe, Cell);
            Assert.IsTrue(r.Armed);
        }

        [Test]
        public void IsDueOnlyOnceTheSwingTimeHasPassed_AndStaysDueUntilTheNextActionIsMade()
        {
            var r = new ToolRepeat();
            r.Arm(ToolType.Axe, Cell);
            Assert.IsFalse(r.Tick(ToolRepeat.SwingSeconds * 0.5f, true, false, false));
            Assert.IsTrue(r.Tick(ToolRepeat.SwingSeconds * 0.5f, true, false, false));
            Assert.IsTrue(r.Tick(0.01f, true, false, false), "still due: nothing was done at the aimed cell, so the next cell it moves to is acted on at once");
            r.Arm(ToolType.Axe, Cell);
            Assert.IsFalse(r.Tick(0.01f, true, false, false), "the clock starts over after each action");
        }

        [Test]
        public void ReleaseBlockedOrWalking_LetGo()
        {
            foreach (var c in new[] { (false, false, false), (true, true, false), (true, false, true) })
            {
                var r = new ToolRepeat();
                r.Arm(ToolType.Pickaxe, Cell);
                Assert.IsFalse(r.Tick(1f, c.Item1, c.Item2, c.Item3));
                Assert.IsFalse(r.Armed);
                Assert.IsFalse(r.Tick(1f, true, false, false), "once let go it does not come back by itself");
            }
        }

        [Test]
        public void NeverArmedNeverFires()
        {
            Assert.IsFalse(new ToolRepeat().Tick(5f, true, false, false));
        }
    }
}
