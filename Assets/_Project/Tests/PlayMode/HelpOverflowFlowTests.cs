using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
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
    // Playtest, 2026-10-09: "the builder's mallet help text overflows the display area". The help for every tool, in the Help tab and in the box over the item bar,
    // fits the space it is shown in (measured on the real screen, tool by tool).
    public class HelpOverflowFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-helpfit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Start()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator EveryToolsInstructions_FitTheirRowInTheHelpTab()
        {
            yield return Start();
            ServiceLocator.Get<UiService>().ShowGameMenu(MenuTabs.Help);
            for (var i = 0; i < 6; i++) yield return null;

            var rows = Resources.FindObjectsOfTypeAll<RectTransform>().Where(r => r.name.StartsWith("Tool_") && r.gameObject.activeInHierarchy).ToList();
            Assert.AreEqual(HelpPage.ToolOrder.Length, rows.Count, "a row for each tool");
            foreach (var row in rows)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(row);
                var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
                var needed = labels.Sum(l => l.preferredHeight);
                Assert.LessOrEqual(needed, row.rect.height + 1f, row.name + ": its words need " + needed + " but the row is " + row.rect.height + " tall");
            }
            Assert.IsTrue(rows.Any(r => r.name == "Tool_Hammer"), "the mallet is there");
        }

        [UnityTest]
        public IEnumerator TheMalletsHelpBox_IsAsTallAsItsWords()
        {
            yield return Start();
            var session = ServiceLocator.Get<GameSession>();
            var hammer = Enumerable.Range(0, session.Backpack.Capacity).First(i => session.Backpack.Get(i)?.ItemId == ItemIds.Hammer);
            session.State.SelectedHotbar = hammer;
            for (var i = 0; i < 6; i++) yield return null;

            var panel = Resources.FindObjectsOfTypeAll<RectTransform>().FirstOrDefault(r => r.name == "SelectionHelp" && r.gameObject.scene.IsValid());
            Assert.IsNotNull(panel);
            Assert.IsTrue(panel.gameObject.activeInHierarchy, "the box shows the first time the mallet is picked");
            var label = panel.GetComponentInChildren<TextMeshProUGUI>();
            StringAssert.Contains("lift it", label.text);
            Assert.LessOrEqual(label.preferredHeight, panel.rect.height, "the words fit the box: they need " + label.preferredHeight + " and the box is " + panel.rect.height);
        }
    }
}
