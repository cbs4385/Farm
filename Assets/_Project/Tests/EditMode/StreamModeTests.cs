using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-145: the name filter, stream mode's effective settings, the choice countdown, and the guarantee that the UI never
    // shows file paths or account names.
    public class StreamModeTests
    {
        [SetUp]
        public void SetUp() => NameFilter.ResetForTests();

        // ---- the name filter ----

        [TestCase("Meadow")]
        [TestCase("Farmer")]
        [TestCase("Sunny Acres")]
        [TestCase("Turnip Hollow")]
        [TestCase("Scunthorpe")]
        [TestCase("Assassin")]
        [TestCase("Dickens")]
        [TestCase("Cockburn")]
        [TestCase("Sussex")]
        [TestCase("Class Act")]
        [TestCase("Mississippi")]
        [TestCase("Shiitake Farm")]
        [TestCase("Bob!")]
        [TestCase("Ann-Marie")]
        [TestCase("Åsa")]
        [TestCase("")]
        [TestCase("   ")]
        public void InnocentNames_AreAllowed(string name) => Assert.IsTrue(NameFilter.IsAllowed(name), name);

        [TestCase("fuck")]
        [TestCase("FUCK")]
        [TestCase("Big Dick Farm")]
        [TestCase("sh1t")]
        [TestCase("a55hole")]
        [TestCase("f u c k")]
        [TestCase("f.u.c.k")]
        [TestCase("d i c k")]
        [TestCase("Hitler Farm")]
        [TestCase("xxhitlerxx")]
        [TestCase("Nazi")]
        [TestCase("p0rn")]
        [TestCase("kys")]
        [TestCase("Mr. Bitch")]
        public void BlockedNames_AreRefused_EvenWhenDisguised(string name) => Assert.IsFalse(NameFilter.IsAllowed(name), name);

        [Test]
        public void Sanitize_FallsBackToTheDefault()
        {
            Assert.AreEqual("Meadow", NameFilter.Sanitize("Meadow", "x"));
            Assert.AreEqual("Sunny Acres", NameFilter.Sanitize("  Sunny Acres ", "x"), "trimmed");
            Assert.AreEqual("Farmer", NameFilter.Sanitize("f u c k", "Farmer"));
            Assert.AreEqual("Farmer", NameFilter.Sanitize("", "Farmer"));
            Assert.AreEqual("Farmer", NameFilter.Sanitize(null, "Farmer"));
        }

        [Test]
        public void TheBlocklist_CanBeReplaced_AndCommentsAreIgnored()
        {
            NameFilter.SetBlocklist(new[] { "# a comment", "", "turnip", "~zzz" });
            Assert.IsFalse(NameFilter.IsAllowed("Turnip"));
            Assert.IsFalse(NameFilter.IsAllowed("a z z z b"));
            Assert.IsTrue(NameFilter.IsAllowed("fuck"), "the old list is gone");
            Assert.IsTrue(NameFilter.IsAllowed("a comment"));
            NameFilter.ResetForTests();
            Assert.IsFalse(NameFilter.IsAllowed("fuck"), "back to the shipped list");
        }

        [Test]
        public void TheShippedList_Loads_AndIsNotEmpty()
        {
            var text = File.ReadAllText("Assets/_Project/Resources/Filters/blocked_names.txt");
            var entries = text.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0 && l[0] != '#').ToList();
            Assert.Greater(entries.Count, 30);
            Assert.Greater(entries.Count(e => e[0] == '~'), 3);
            foreach (var e in entries.Where(e => e[0] == '~')) Assert.GreaterOrEqual(e.Length - 1, 4, e + ": a stem must be long enough not to hit innocent words");
        }

        [Test]
        public void ANewGame_NeverStartsWithABlockedName()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            using (var f = new TestSessionFixture(db.AllItems, db.AllCrops, null))
            {
                f.Session.BeginNewGame("f u c k", "Hitler Farm", -1);
                Assert.AreEqual("Farmer", f.Session.State.PlayerName);
                Assert.AreEqual("Meadow", f.Session.State.FarmName);
                f.Session.BeginNewGame("Sam", "Sunny Acres", -1);
                Assert.AreEqual("Sam", f.Session.State.PlayerName);
                Assert.AreEqual("Sunny Acres", f.Session.State.FarmName);
            }
        }

        // ---- stream mode settings ----

        [Test]
        public void StreamMode_EnlargesDialogueText_AndSpeedsItUp_WithoutChangingTheSavedChoices()
        {
            var s = new SettingsData { TextScale = 1f, DialogueSpeed = 0 };
            Assert.AreEqual(1f, s.DialogueTextFactor);
            Assert.AreEqual(0, s.EffectiveDialogueSpeed);
            s.StreamMode = true;
            Assert.AreEqual(SettingsData.StreamDialogueTextFactor, s.DialogueTextFactor, 0.0001f);
            Assert.Greater(s.DialogueTextFactor, 1f);
            Assert.AreEqual(SettingsData.StreamMinDialogueSpeed, s.EffectiveDialogueSpeed);
            Assert.AreEqual(1f, s.TextScale, "the player's own UI size is untouched: stream mode only enlarges the dialogue text");
            Assert.AreEqual(0, s.DialogueSpeed);
            s.DialogueSpeed = 3;
            Assert.AreEqual(3, s.EffectiveDialogueSpeed, "instant stays instant");
            s.StreamMode = false;
            Assert.AreEqual(1f, s.DialogueTextFactor);
        }

        [Test]
        public void TheNewSettings_DefaultOff_AndOldFilesLoad()
        {
            var s = new SettingsData();
            Assert.IsFalse(s.StreamMode);
            Assert.AreEqual(0, s.ChoiceTimer);
            var old = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsData>("{\"MasterVolume\":0.5}");
            Assert.IsFalse(old.StreamMode);
            Assert.AreEqual(0, old.ChoiceTimer);
            s.ChoiceTimer = 500; s.Clamp();
            Assert.AreEqual(120, s.ChoiceTimer);
            s.ChoiceTimer = -3; s.Clamp();
            Assert.AreEqual(0, s.ChoiceTimer);
            CollectionAssert.AreEqual(new[] { 0, 15, 30, 60 }, SettingsData.ChoiceTimerSteps);
        }

        // ---- the countdown ----

        static DialogueOption Opt(int i, bool isDefault = false) => new DialogueOption(i, "o" + i, null, isDefault);

        [Test]
        public void TheCountdown_FiresOnce_WhenTheTimeIsUp()
        {
            var c = new ChoiceCountdown();
            Assert.IsFalse(c.Running);
            Assert.IsFalse(c.Tick(1f), "not running: never fires");
            c.Start(3f);
            Assert.IsTrue(c.Running);
            Assert.AreEqual(3, c.SecondsLeft);
            Assert.IsFalse(c.Tick(1f));
            Assert.AreEqual(2, c.SecondsLeft);
            Assert.IsFalse(c.Tick(1.5f));
            Assert.AreEqual(1, c.SecondsLeft, "rounds up: shows 1 until it is over");
            Assert.IsTrue(c.Tick(0.6f));
            Assert.IsFalse(c.Running);
            Assert.IsFalse(c.Tick(5f), "fires once");
            Assert.AreEqual(0, c.SecondsLeft);
        }

        [Test]
        public void ACountdownOfZero_NeverRuns()
        {
            var c = new ChoiceCountdown();
            c.Start(0f);
            Assert.IsFalse(c.Running);
            c.Start(10f);
            c.Stop();
            Assert.IsFalse(c.Running);
            Assert.AreEqual(0f, c.Remaining);
        }

        [Test]
        public void OnTimeout_TheDefaultChoiceIsTaken_OtherwiseTheFirst()
        {
            Assert.AreEqual(2, ChoiceCountdown.Choose(new[] { Opt(0), Opt(1), Opt(2, true) }));
            Assert.AreEqual(0, ChoiceCountdown.Choose(new[] { Opt(0), Opt(1) }));
            Assert.AreEqual(-1, ChoiceCountdown.Choose(new DialogueOption[0]));
            Assert.AreEqual(-1, ChoiceCountdown.Choose(null));
        }

        // ---- the guarantee ----

        [Test]
        public void TheUi_NeverShowsFilePathsOrAccountNames()
        {
            var forbidden = new[] { "persistentDataPath", "dataPath", "Environment.UserName", "SystemInfo.deviceName", "PlatformServices.Current.Name", "GetFullPath", "UserProfile" };
            foreach (var file in Directory.GetFiles("Assets/_Project/Scripts/UI", "*.cs", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                foreach (var token in forbidden)
                    Assert.IsFalse(text.Contains(token), $"{Path.GetFileName(file)} uses {token}: the UI must not show paths or account names");
            }
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var kv in table)
            {
                Assert.IsFalse(kv.Value.Contains(":\\") || kv.Value.Contains("/Users/") || kv.Value.Contains("AppData") || kv.Value.Contains("persistent"), kv.Key + " mentions a path");
            }
        }

        [Test]
        public void TheStreamStrings_Exist()
        {
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var key in new[] { "options.stream_mode", "options.choice_timer", "options.choice_timer.seconds", "newgame.name_blocked", "dialogue.timer", "ui.off" })
                Assert.IsTrue(table.ContainsKey(key), key);
        }

        [Test]
        public void TheOfferedFarmerName_IsNeverTheAccountNameInStreamMode()
        {
            Assert.AreEqual("Farmer", DefaultFarmerName.Offer(true, "chris"));
            Assert.AreEqual("Chris", DefaultFarmerName.Offer(false, "chris"));
        }

        [Test]
        public void TheOfferedFarmerName_IsTheTidiedAccountName_OrTheFallback()
        {
            NameFilter.SetBlocklist(new[] { "badword" });
            try
            {
                Assert.AreEqual("Chris", NameFilter.FromAccountName("chris", 16, "Farmer"), "a lower-case name gets a capital");
                Assert.AreEqual("McKay", NameFilter.FromAccountName("McKay", 16, "Farmer"), "mixed case is left alone");
                Assert.AreEqual("Ann lee", NameFilter.FromAccountName("  ann.lee ".Replace('.', ' '), 16, "Farmer"));
                Assert.AreEqual("Abcdefghijklmnop", NameFilter.FromAccountName("abcdefghijklmnopqrstuv", 16, "Farmer"), "cut to fit the field");
                Assert.AreEqual("Farmer", NameFilter.FromAccountName("", 16, "Farmer"));
                Assert.AreEqual("Farmer", NameFilter.FromAccountName(null, 16, "Farmer"));
                Assert.AreEqual("Farmer", NameFilter.FromAccountName("...", 16, "Farmer"));
                Assert.AreEqual("Farmer", NameFilter.FromAccountName("badword", 16, "Farmer"), "a name the filter blocks is not offered");
            }
            finally { NameFilter.ResetForTests(); }
        }
    }
}
