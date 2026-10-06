using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // The bug report window in the real game: reachable from the main menu and the pause menu, refuses an empty report, and packs a real one.
    public class BugReportFlowTests
    {
        string _root;
        string _opened;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-bugflow-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            BugReport.OpenUrl = url => _opened = url;
        }

        [TearDown]
        public void TearDown()
        {
            BugReport.OpenUrl = url => Application.OpenURL(url);
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static Button ButtonNamed(string name) => UnityEngine.Object.FindObjectsByType<Button>().FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name == name);

        [UnityTest]
        public IEnumerator ThePauseMenu_HasAReportButton_ThatOpensTheWindow_AndARealReportIsPacked()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            var ui = ServiceLocator.Get<UiService>();
            ui.ShowPause();
            yield return null;
            var report = ButtonNamed("ReportBug");
            Assert.IsNotNull(report, "the pause menu has a Report a bug button");
            report.onClick.Invoke();
            yield return null;

            var screen = ui.BugReporter;
            Assert.IsTrue(screen.IsOpen);
            screen.Fill("", "", true);
            ButtonNamed("Send").onClick.Invoke();
            Assert.IsNull(_opened, "an empty report is refused");
            Assert.IsNotNull(UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().FirstOrDefault(t => t.gameObject.activeInHierarchy && t.text.Contains("subject")), "and says why");

            screen.Fill("Test report from the tests", "A description of what went wrong, long enough to be accepted.", true);
            ButtonNamed("Send").onClick.Invoke();
            Assert.IsNotNull(_opened, "the mail program is opened");
            StringAssert.StartsWith("mailto:" + BugReport.Address, _opened);
            StringAssert.StartsWith("bug-", screen.LastFileName);
            var zip = Path.Combine(BugReport.DefaultFolder(Application.persistentDataPath), screen.LastFileName);
            try
            {
                Assert.IsTrue(File.Exists(zip));
                var entries = BugReport.Contents(zip);
                CollectionAssert.Contains(entries, "report.txt");
                CollectionAssert.Contains(entries, "save.json", "the save was included");
            }
            finally { if (File.Exists(zip)) File.Delete(zip); }
        }

        [UnityTest]
        public IEnumerator TheMainMenu_HasAReportButton()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var op = SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
            var report = ButtonNamed("ReportBug");
            Assert.IsNotNull(report, "the main menu has a Report a bug button");
            report.onClick.Invoke();
            yield return null;
            Assert.IsTrue(ServiceLocator.Get<UiService>().BugReporter.IsOpen);
        }
    }
}
