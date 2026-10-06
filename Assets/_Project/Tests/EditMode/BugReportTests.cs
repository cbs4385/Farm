using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Farm.Core;
using NUnit.Framework;

namespace Farm.Tests
{
    // The bug report: what it asks for, what it packs, what it leaves out, and the mail it opens.
    public class BugReportTests
    {
        string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "farm-bug-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_dir, "logs"));
            BugReport.OpenUrl = _ => { };
        }

        [TearDown]
        public void TearDown()
        {
            BugReport.OpenUrl = url => UnityEngine.Application.OpenURL(url);
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        BugReport.Request Request(string subject = "The cat walks through a wall", string body = "I stood by the library and the cat went through the wall.") => new BugReport.Request
        {
            Subject = subject,
            Body = body,
            UtcNow = new DateTime(2026, 10, 6, 12, 30, 0, DateTimeKind.Utc),
            OutputDirectory = Path.Combine(_dir, "BugReports"),
            LogDirectory = Path.Combine(_dir, "logs"),
            ConsoleLogPath = Path.Combine(_dir, "Player.log"),
            SystemInfo = "Operating system: Test OS",
            GameInfo = "Farm 0.0.1",
        };

        [TestCase(null, "something happened")]
        [TestCase("", "something happened")]
        [TestCase("ab", "something happened")]
        public void ASubjectIsRequired(string subject, string body) => Assert.AreEqual("bug.need_subject", BugReport.Validate(subject, body));

        [TestCase("A subject", null)]
        [TestCase("A subject", "   ")]
        [TestCase("A subject", "no")]
        public void ADescriptionIsRequired(string subject, string body) => Assert.AreEqual("bug.need_body", BugReport.Validate(subject, body));

        [Test]
        public void AGoodReport_IsValid() => Assert.IsNull(BugReport.Validate("The cat", "It walked through a wall."));

        [Test]
        public void TheMailIsAddressedToTheDevelopers_WithTheSubjectAndTheDescription()
        {
            var url = BugReport.BuildMailto("The cat walks through a wall", "I stood by the library.", @"C:\x\bug-1.zip", "Farm 0.0.1");
            StringAssert.StartsWith("mailto:gamestrubios@gmail.com?subject=", url);
            StringAssert.Contains(Uri.EscapeDataString("[Farm bug] The cat walks through a wall"), url);
            StringAssert.Contains(Uri.EscapeDataString("I stood by the library."), url);
            StringAssert.Contains(Uri.EscapeDataString("bug-1.zip"), url, "it names the file to attach");
        }

        [Test]
        public void TheMailLink_NeverGetsTooLong_WhateverIsTyped()
        {
            var huge = string.Concat(Enumerable.Repeat("The cat went through the wall again. Ünïcödé text too. ", 200));
            var url = BugReport.BuildMailto(new string('s', 120), huge, @"C:\Users\someone\AppData\LocalLow\Studio\Farm\BugReports\bug-20261006-123000.zip", "Farm 0.0.1");
            Assert.LessOrEqual(url.Length, BugReport.MaxUrlLength);
            StringAssert.Contains(Uri.EscapeDataString("bug-20261006-123000.zip"), url, "the file to attach is always named");
        }

        [Test]
        public void Redact_RemovesTheHomeFolderTheAccountNameAndTheMachine()
        {
            var text = @"Loaded C:\Users\Alice Smith\AppData\LocalLow\Farm\farm.log by alice smith on ALICE-PC; also C:/Users/Alice Smith/x";
            var clean = BugReport.Redact(text, @"C:\Users\Alice Smith", "alice smith", "ALICE-PC");
            StringAssert.DoesNotContain("Alice", clean);
            StringAssert.DoesNotContain("alice", clean);
            StringAssert.Contains("<home>", clean);
            StringAssert.Contains("<machine>", clean);
        }

        [Test]
        public void Redact_LeavesAShortNameAlone_SoTheLogIsNotMangled() =>
            Assert.AreEqual("a b c", BugReport.Redact("a b c", "", "b", ""));

        [Test]
        public void Create_PacksTheReportTheLogsAndTheSave_WithoutTheAccountName()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            File.WriteAllText(Path.Combine(_dir, "logs", CrashLog.FileName), "[Error] boom at " + home + "/Farm\nsecond line");
            File.WriteAllText(Path.Combine(_dir, "logs", CrashLog.PreviousName), "an earlier run");
            File.WriteAllText(Path.Combine(_dir, "Player.log"), "unity log");
            var r = Request();
            r.SaveJson = "{\"Gold\":500}";

            var result = BugReport.Create(r);
            Assert.IsTrue(result.Ok, result.Error);
            Assert.AreEqual("bug-20261006-123000.zip", Path.GetFileName(result.ZipPath));
            CollectionAssert.AreEquivalent(new[] { "report.txt", "logs/farm.log", "logs/farm.prev.log", "logs/Player.log", "save.json" }, BugReport.Contents(result.ZipPath));

            using (var zip = ZipFile.OpenRead(result.ZipPath))
            {
                string Read(string name) { using (var s = new StreamReader(zip.GetEntry(name).Open())) return s.ReadToEnd(); }
                StringAssert.Contains("The cat walks through a wall", Read("report.txt"));
                StringAssert.Contains("Test OS", Read("report.txt"));
                StringAssert.Contains("[Error] boom", Read("logs/farm.log"));
                if (home.Length >= 3) StringAssert.DoesNotContain(home, Read("logs/farm.log"));
                Assert.AreEqual("{\"Gold\":500}", Read("save.json"));
            }
            StringAssert.StartsWith("mailto:" + BugReport.Address, result.MailtoUrl);
        }

        [Test]
        public void Create_WorksWithNoLogsAndNoSave_AndLeavesTheSaveOutUnlessGiven()
        {
            var result = BugReport.Create(Request());
            Assert.IsTrue(result.Ok, result.Error);
            CollectionAssert.AreEqual(new[] { "report.txt" }, BugReport.Contents(result.ZipPath));
        }

        [Test]
        public void Create_RefusesAnEmptyReport_AndWritesNothing()
        {
            var result = BugReport.Create(Request(subject: "", body: ""));
            Assert.IsFalse(result.Ok);
            Assert.AreEqual("bug.need_subject", result.Error);
            Assert.IsFalse(Directory.Exists(Path.Combine(_dir, "BugReports")));
        }

        [Test]
        public void Create_KeepsOnlyTheNewestReports()
        {
            for (var i = 0; i < BugReport.KeepReports + 4; i++)
            {
                var r = Request();
                r.UtcNow = new DateTime(2026, 10, 6, 12, 0, i, DateTimeKind.Utc);
                Assert.IsTrue(BugReport.Create(r).Ok);
            }
            var files = Directory.GetFiles(Path.Combine(_dir, "BugReports"), "bug-*.zip");
            Assert.AreEqual(BugReport.KeepReports, files.Length);
            Assert.IsTrue(files.Any(f => f.EndsWith("-120013.zip")), "the newest is kept");
            Assert.IsFalse(files.Any(f => f.EndsWith("-120000.zip")), "the oldest is gone");
        }

        [Test]
        public void ALog_ThatIsOpenInAnotherProgram_CanStillBeRead_AndAHugeOneIsCut()
        {
            var path = Path.Combine(_dir, "big.log");
            using (var open = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var line = new string('x', 99) + "\n";
                var w = new StreamWriter(open);
                for (var i = 0; i < 20000; i++) w.Write(line);
                w.Flush();
                var tail = BugReport.ReadTail(path);
                Assert.IsNotNull(tail);
                Assert.LessOrEqual(tail.Length, BugReport.MaxLogBytes + 100);
                StringAssert.StartsWith("[... earlier lines left out ...]", tail);
            }
        }

        [Test]
        public void Send_OpensTheMailProgram_WithTheMailLink()
        {
            string opened = null;
            BugReport.OpenUrl = url => opened = url;
            var result = BugReport.Send(Request());
            Assert.IsTrue(result.Ok);
            Assert.AreEqual(result.MailtoUrl, opened);
        }

        [Test]
        public void Send_StillSucceeds_WhenNoMailProgramCanBeOpened()
        {
            BugReport.OpenUrl = _ => throw new InvalidOperationException("no mail program");
            var result = BugReport.Send(Request());
            Assert.IsTrue(result.Ok, "the file is made, and the screen tells the player where it is");
            Assert.IsTrue(File.Exists(result.ZipPath));
        }

        [Test]
        public void TheFolderLink_IsAFileUrl() =>
            Assert.AreEqual("file:///C:/Users/x/BugReports", BugReport.FolderUrl(@"C:\Users\x\BugReports"));
    }
}
