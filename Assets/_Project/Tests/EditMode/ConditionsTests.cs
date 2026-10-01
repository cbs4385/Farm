using System.Collections.Generic;
using Farm.Core;
using NUnit.Framework;

namespace Farm.Tests
{
    public class ConditionsTests
    {
        sealed class FakeWorld : IWorldQuery
        {
            public HashSet<string> Flags = new HashSet<string>();
            public Dictionary<string, int> Vars = new Dictionary<string, int>();
            public GameDateTime Time = new GameDateTime(1, Season.Fall, 16, 22 * 60);   // full moon, 22:00
            public string WeatherId = "sunny";
            public string Map = "Farm";

            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => Vars.TryGetValue(name, out var v) ? v : 0;
            public GameDateTime Now => Time;
            public string Weather => WeatherId;
            public string MapId => Map;
        }

        FakeWorld _w;

        [SetUp]
        public void SetUp()
        {
            _w = new FakeWorld();
            Conditions.ClearCustomForTests();
        }

        bool Eval(string expr) => Conditions.Evaluate(expr, _w);

        [Test]
        public void EmptyOrNull_IsAlwaysTrue()
        {
            Assert.IsTrue(Eval(null));
            Assert.IsTrue(Eval(""));
            Assert.IsTrue(Eval("   "));
        }

        [Test]
        public void TrueFalse_Literals()
        {
            Assert.IsTrue(Eval("true"));
            Assert.IsFalse(Eval("false"));
        }

        [Test]
        public void Flag_Atom()
        {
            Assert.IsFalse(Eval("flag:mythos.cult_known"));
            _w.Flags.Add("mythos.cult_known");
            Assert.IsTrue(Eval("flag:mythos.cult_known"));
        }

        [Test]
        public void Var_Comparisons()
        {
            _w.Vars["dread"] = 5;
            Assert.IsTrue(Eval("var:dread>=5"));
            Assert.IsTrue(Eval("var:dread>4"));
            Assert.IsFalse(Eval("var:dread>5"));
            Assert.IsTrue(Eval("var:dread<=5"));
            Assert.IsTrue(Eval("var:dread<6"));
            Assert.IsTrue(Eval("var:dread==5"));
            Assert.IsTrue(Eval("var:dread!=4"));
            Assert.IsTrue(Eval("var:unset==0"), "unset variables read as 0");
            Assert.IsTrue(Eval("var:cult.standing>=0"), "dotted names");
        }

        [Test]
        public void Calendar_Atoms()
        {
            Assert.IsTrue(Eval("season:fall"));
            Assert.IsTrue(Eval("season:FALL"), "case-insensitive");
            Assert.IsFalse(Eval("season:spring"));
            Assert.IsTrue(Eval("moon:full"));
            Assert.IsFalse(Eval("moon:new"));
            Assert.IsTrue(Eval("hour>=22"));
            Assert.IsFalse(Eval("hour>22"));
            Assert.IsTrue(Eval("day==16"));
            Assert.IsTrue(Eval("year>=1"));
        }

        [Test]
        public void Weather_And_Map_Atoms()
        {
            _w.WeatherId = "fog";
            _w.Map = "Woods";
            Assert.IsTrue(Eval("weather:fog"));
            Assert.IsFalse(Eval("weather:rain"));
            Assert.IsTrue(Eval("map:Woods"));
            Assert.IsFalse(Eval("map:Farm"));
        }

        [Test]
        public void Operators_Precedence_And_Parentheses()
        {
            // && binds tighter than ||
            Assert.IsTrue(Eval("true || false && false"));
            Assert.IsFalse(Eval("(true || false) && false"));
            Assert.IsTrue(Eval("!false && !(false || false)"));
            Assert.IsFalse(Eval("!true"));
            Assert.IsTrue(Eval("!!true"));
        }

        [Test]
        public void RealisticExpression()
        {
            _w.Vars["dread"] = 3;
            _w.Flags.Add("mythos.woods_open");
            Assert.IsTrue(Eval("flag:mythos.woods_open && moon:full && var:dread>=3 && hour>=20 && !weather:rain"));
            _w.WeatherId = "rain";
            Assert.IsFalse(Eval("flag:mythos.woods_open && moon:full && var:dread>=3 && hour>=20 && !weather:rain"));
        }

        [Test]
        public void CustomAtoms_CanBeRegistered_AndSeeTheWorld()
        {
            Conditions.Register("hearts", (arg, world) => arg == "mara>=4" && world.GetVar("hearts.mara") >= 4);
            _w.Vars["hearts.mara"] = 4;
            Assert.IsTrue(Eval("hearts:mara>=4"));
            _w.Vars["hearts.mara"] = 2;
            Assert.IsFalse(Eval("hearts:mara>=4"));
        }

        [Test]
        public void MalformedExpressions_Throw_WithHelpfulMessages()
        {
            foreach (var bad in new[] { "flag:", "bogus:x", "(true", "true &&", "var:dread", "var:dread>=x", "season:monsoon", "moon:purple", "&& true", "hour>=" })
            {
                var ex = Assert.Throws<ConditionException>(() => Eval(bad), bad);
                StringAssert.Contains(bad.Trim(), ex.Message);
            }
        }

        [Test]
        public void Validate_ReportsErrors_WithoutThrowing()
        {
            Assert.IsTrue(Conditions.Validate("flag:a && var:b>=1", out var error));
            Assert.IsNull(error);
            Assert.IsFalse(Conditions.Validate("nonsense:1", out error));
            Assert.IsNotEmpty(error);
            Assert.IsTrue(Conditions.Validate(null, out _));
        }

        [Test]
        public void TryEvaluate_ReturnsFalseOnError_InsteadOfThrowing()
        {
            Assert.IsFalse(Conditions.TryEvaluate("nonsense:1", _w, out _));
            Assert.IsTrue(Conditions.TryEvaluate("true", _w, out var result));
            Assert.IsTrue(result);
        }

        [Test]
        public void RegisteringAnAtom_InvalidatesCachedParses()
        {
            Assert.IsFalse(Conditions.Validate("late:thing", out _));
            Conditions.Register("late", (arg, world) => true);
            Assert.IsTrue(Conditions.Validate("late:thing", out _));
        }

        [Test]
        public void MoonPhase_FollowsTheSeasonCycle()
        {
            Assert.AreEqual(MoonPhase.New, new GameDateTime(1, Season.Spring, 1).MoonPhase);
            Assert.AreEqual(MoonPhase.New, new GameDateTime(1, Season.Winter, 3).MoonPhase);
            Assert.AreEqual(MoonPhase.New, new GameDateTime(1, Season.Spring, 4).MoonPhase);
            Assert.AreEqual(MoonPhase.WaxingCrescent, new GameDateTime(1, Season.Spring, 5).MoonPhase);
            Assert.AreEqual(MoonPhase.Full, new GameDateTime(1, Season.Spring, 15).MoonPhase);
            Assert.AreEqual(MoonPhase.Full, new GameDateTime(1, Season.Spring, 18).MoonPhase);
            Assert.AreEqual(MoonPhase.WaningGibbous, new GameDateTime(1, Season.Spring, 19).MoonPhase);
            Assert.AreEqual(MoonPhase.WaningCrescent, new GameDateTime(1, Season.Spring, 28).MoonPhase);
        }
    }
}
