using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using NUnit.Framework;

namespace Farm.Tests
{
    // The hints name the controls in use: A, B, X, Y, LB and RB on an Xbox pad, E, Esc and click on the keyboard and mouse.
    public class ControlPromptsTests
    {
        System.Collections.Generic.Dictionary<string, string> _table;

        [SetUp]
        public void SetUp()
        {
            _table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            L.SetTable(_table);
            ControlPrompts.ResetForTests();
        }

        [TearDown]
        public void TearDown() => ControlPrompts.ResetForTests();

        [Test]
        public void ATokenBecomesTheNameOfTheControlInUse()
        {
            Assert.AreEqual("Press E to talk", ControlPrompts.Expand("Press [[Interact]] to talk", InputKind.KeyboardMouse));
            Assert.AreEqual("Press A to talk", ControlPrompts.Expand("Press [[Interact]] to talk", InputKind.Gamepad));
            Assert.AreEqual("LB / RB: switch tab   B: close", ControlPrompts.Expand("[[TabPrev]] / [[TabNext]]: switch tab   [[Cancel]]: close", InputKind.Gamepad), "the pad names are Xbox names");
            Assert.AreEqual("Q / E: switch tab   Esc: close", ControlPrompts.Expand("[[TabPrev]] / [[TabNext]]: switch tab   [[Cancel]]: close", InputKind.KeyboardMouse));
        }

        [Test]
        public void ATextWithoutATokenIsLeftAlone_AndAnUnknownOrUnclosedTokenIsKept()
        {
            const string plain = "Nothing to change here";
            Assert.AreSame(plain, ControlPrompts.Expand(plain, InputKind.Gamepad));
            Assert.AreEqual("a Mystery b", ControlPrompts.Expand("a [[Mystery]] b", InputKind.Gamepad), "an unknown name shows as itself");
            Assert.AreEqual("broken [[Interact", ControlPrompts.Expand("broken [[Interact", InputKind.Gamepad));
            Assert.AreEqual("", ControlPrompts.Expand("", InputKind.Gamepad));
            Assert.IsNull(ControlPrompts.Expand(null, InputKind.Gamepad));
        }

        [Test]
        public void LGet_FollowsTheLastControlTouched()
        {
            StringAssert.Contains("Q / E", L.Get("menu.hint"));
            ControlPrompts.SetKind(InputKind.Gamepad);
            Assert.AreEqual("LB / RB: switch tab   B: close", L.Get("menu.hint"));
            StringAssert.DoesNotContain("Esc", L.Get("menu.hint"));
            StringAssert.Contains("press X", L.Get("tool.use.hoe"));
            StringAssert.DoesNotContain("click", L.Get("tool.use.hoe"));
            ControlPrompts.SetKind(InputKind.KeyboardMouse);
            StringAssert.Contains("click (or press C)", L.Get("tool.use.hoe"));
        }

        [Test]
        public void ChangingTheKind_TellsTheScreensOnce()
        {
            var changes = 0;
            ControlPrompts.Changed += () => changes++;
            ControlPrompts.SetKind(InputKind.KeyboardMouse);
            Assert.AreEqual(0, changes, "no change, no news");
            ControlPrompts.SetKind(InputKind.Gamepad);
            ControlPrompts.SetKind(InputKind.Gamepad);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void EveryTokenInTheGame_HasAKeyboardAndAPadName_AndThePadNamesNeverMentionTheKeyboard()
        {
            var tokens = _table.Values.SelectMany(v => Regex.Matches(v, @"\[\[(\w+)\]\]").Cast<Match>().Select(m => m.Groups[1].Value)).Distinct().ToList();
            Assert.Greater(tokens.Count, 10, "the hints use tokens");
            foreach (var token in tokens)
            {
                Assert.IsTrue(_table.ContainsKey("prompt.kb." + token), "keyboard name for " + token);
                Assert.IsTrue(_table.ContainsKey("prompt.pad." + token), "pad name for " + token);
            }
            foreach (var pair in _table.Where(p => p.Key.StartsWith("prompt.pad.")))
                Assert.IsFalse(Regex.IsMatch(pair.Value, @"\b(click|Click|Esc|Enter|Space|Tab|Delete|F8|mouse|scroll)\b") && pair.Key != "prompt.pad.DiscardHelp", pair.Key + " = " + pair.Value);
        }
    }
}
