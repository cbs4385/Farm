using System.Collections;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest 2026-10-15: a game started outside Steam showed no "Save to" row on the new-game screen, so nothing said why the cloud was not offered.
    public class NewGameWhereFlowTests
    {
        [UnityTest]
        public IEnumerator WithoutSteamCloud_TheNewGameScreenStillShowsTheSaveToRow_AndSaysWhy()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            L.SetLanguage("en");
            var ui = (UiService)GameContext.Current.Ui;
            Assert.IsFalse(ui.Game.Saves.CloudAvailable, "the tests run without Steam");
            var newGame = new NewGameScreen(ui, null);
            newGame.Open();
            yield return null;

            var button = newGame.Root.GetComponentsInChildren<Button>(true).First(b => b.name == "SaveWhereButton");
            Assert.IsTrue(button.gameObject.activeInHierarchy, "the row is shown");
            Assert.IsFalse(button.interactable, "there is nothing to choose without the cloud");
            var text = button.GetComponentInChildren<TextMeshProUGUI>(true).text;
            StringAssert.Contains("this computer", text);
            StringAssert.Contains("Steam Cloud is not available", text);

            button.onClick.Invoke();                                       // a click changes nothing
            yield return null;
            Assert.AreEqual(text, button.GetComponentInChildren<TextMeshProUGUI>(true).text);
        }
    }
}
