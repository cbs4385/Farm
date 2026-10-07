using System;
using System.IO;
using System.Text.RegularExpressions;
using Farm.Core;
using NUnit.Framework;
using UnityEditor;

namespace Farm.Tests
{
    // The game is called Wetherell Farm Saga (2026-10-07). "Farm" stays the code name; the name the player sees follows the product name and the
    // `game.title` text, existing saves are carried over from the old data folder, and the Steam ids are the ones the owner was given.
    public class RenameTests
    {
        const string Name = "Wetherell Farm Saga";

        string _temp;

        [SetUp]
        public void SetUp() => _temp = Path.Combine(Path.GetTempPath(), "farm-rename-" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown() { if (Directory.Exists(_temp)) Directory.Delete(_temp, true); }

        [Test]
        public void TheProductName_AndTheMenuTitle_AreTheGamesName()
        {
            Assert.AreEqual(Name, PlayerSettings.productName);
            var en = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            Assert.AreEqual(Name, en["game.title"]);
        }

        [Test]
        public void TheSteamIds_AreTheOnesGiven_AndNoPlaceholderIsLeft()
        {
            var app = File.ReadAllText("Steam/app_build.vdf");
            StringAssert.Contains("\"AppID\" \"5408390\"", app);
            StringAssert.Contains("\"5408391\" \"depot_windows.vdf\"", app);
            StringAssert.Contains("\"5408392\" \"depot_linux.vdf\"", app);
            StringAssert.Contains("\"DepotID\" \"5408391\"", File.ReadAllText("Steam/depot_windows.vdf"));
            StringAssert.Contains("\"DepotID\" \"5408392\"", File.ReadAllText("Steam/depot_linux.vdf"));
            Assert.IsFalse(Regex.IsMatch(app, "\"(AppID|DepotID)\" \"0\""), "no placeholder id");
            // The upload script still refuses placeholders, so a lost id cannot slip through.
            var script = File.ReadAllText("Steam/upload.sh");
            StringAssert.Contains("\"DepotID\" \"0\"", script);
            StringAssert.Contains("\"AppID\" \"0\"", script);
        }

        // ---- the save migration -------------------------------------------------------------------------------------------

        string Old => Path.Combine(_temp, "Farm");
        string New => Path.Combine(_temp, Name);

        void Write(string path, string text) { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, text); }

        [Test]
        public void TheSavesAndSettings_AreCopiedFromTheOldFolder_OnTheFirstStart_AndTheOldOnesAreKept()
        {
            Write(Path.Combine(Old, "saves", "slot0", "save.json"), "{\"slot\":0}");
            Write(Path.Combine(Old, "saves", "slot1.json"), "{\"slot\":1}");
            Write(Path.Combine(Old, "settings.json"), "{\"HudTransparency\":0.2}");
            Write(Path.Combine(Old, "logs", "farm.log"), "old log");
            Write(Path.Combine(Old, "BugReports", "bug.zip"), "zip");

            var copied = DataMigration.Run(New);

            Assert.AreEqual(3, copied);
            Assert.AreEqual("{\"slot\":0}", File.ReadAllText(Path.Combine(New, "saves", "slot0", "save.json")));
            Assert.AreEqual("{\"slot\":1}", File.ReadAllText(Path.Combine(New, "saves", "slot1.json")));
            Assert.AreEqual("{\"HudTransparency\":0.2}", File.ReadAllText(Path.Combine(New, "settings.json")));
            Assert.IsFalse(File.Exists(Path.Combine(New, "logs", "farm.log")), "logs are not carried over");
            Assert.IsFalse(Directory.Exists(Path.Combine(New, "BugReports")), "nor bug reports");
            Assert.IsTrue(File.Exists(Path.Combine(Old, "saves", "slot0", "save.json")), "the old folder is left as it was");
        }

        [Test]
        public void ANewFolderThatAlreadyHasSaves_IsNeverTouched()
        {
            Write(Path.Combine(Old, "saves", "slot0.json"), "old");
            Write(Path.Combine(New, "saves", "slot0.json"), "new");
            Assert.AreEqual(0, DataMigration.Run(New));
            Assert.AreEqual("new", File.ReadAllText(Path.Combine(New, "saves", "slot0.json")));

            Directory.Delete(Path.Combine(New, "saves"), true);
            Write(Path.Combine(New, "settings.json"), "mine");
            Assert.AreEqual(0, DataMigration.Run(New), "settings of its own count too");
            Assert.AreEqual("mine", File.ReadAllText(Path.Combine(New, "settings.json")));
        }

        [Test]
        public void WithNoOldFolder_OrTheSameFolder_NothingHappens_AndASecondRunCopiesNothing()
        {
            Assert.AreEqual(0, DataMigration.Run(New), "no old folder");
            Assert.AreEqual(0, DataMigration.Run(Old, "Farm"), "the folder is its own old folder");
            Assert.AreEqual(0, DataMigration.Run(null));
            Assert.AreEqual(0, DataMigration.Run(""));

            Write(Path.Combine(Old, "saves", "a.json"), "a");
            Assert.AreEqual(1, DataMigration.Run(New));
            Assert.AreEqual(0, DataMigration.Run(New), "already migrated: the new folder has saves now");
        }

        [Test]
        public void ARootWithATrailingSeparator_IsHandled()
        {
            Write(Path.Combine(Old, "settings.json"), "s");
            Assert.AreEqual(1, DataMigration.Run(New + Path.DirectorySeparatorChar));
            Assert.AreEqual("s", File.ReadAllText(Path.Combine(New, "settings.json")));
        }
    }
}
