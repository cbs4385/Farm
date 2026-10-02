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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // T-036: the game menu (skills, social, calendar, map, journal, crafting) opened and navigated with a simulated
    // keyboard and gamepad.
    public class GameMenuFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        Gamepad _pad;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-menutests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _pad = InputSystem.AddDevice<Gamepad>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Start(string map = MapIds.Farm)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.State.CurrentMap = map;
            _session.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static UiService Ui => ServiceLocator.Get<UiService>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
        }

        static string AllText(MenuPage page) => string.Join("\n", page.Root.GetComponentsInChildren<TextMeshProUGUI>(true).Select(t => t.text));

        // ---- opening and tabs -------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheMenuKey_OpensTheMenu_AndQAndESwitchTabs()
        {
            yield return Start();
            yield return Tap(Key.M);
            Assert.IsTrue(Ui.AnyModalOpen, "the menu opens");
            var menu = Ui.GameMenu;
            Assert.AreEqual(MenuTabs.Skills, menu.Current.Id);

            yield return Tap(Key.E);
            Assert.AreEqual(MenuTabs.Social, menu.Current.Id);
            yield return Tap(Key.E);
            Assert.AreEqual(MenuTabs.Calendar, menu.Current.Id);
            yield return Tap(Key.Q);
            Assert.AreEqual(MenuTabs.Social, menu.Current.Id);

            yield return Tap(Key.Escape);
            Assert.IsFalse(Ui.AnyModalOpen, "Escape closes the menu");
        }

        [UnityTest]
        public IEnumerator TheGamepad_SelectOpens_ShouldersSwitchTabs_BCloses()
        {
            yield return Start();
            yield return Tap(_pad.selectButton);
            Assert.IsTrue(Ui.AnyModalOpen);
            var menu = Ui.GameMenu;
            yield return Tap(_pad.rightShoulder);
            Assert.AreEqual(MenuTabs.Social, menu.Current.Id);
            yield return Tap(_pad.leftShoulder);
            yield return Tap(_pad.leftShoulder);
            Assert.AreEqual(menu.Pages.Last().Id, menu.Current.Id, "tabs wrap around");
            yield return Tap(_pad.buttonEast);
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        [UnityTest]
        public IEnumerator EveryTab_IsReachableAndRefreshesWithoutErrors()
        {
            yield return Start();
            yield return Tap(Key.M);
            var menu = Ui.GameMenu;
            for (var i = 0; i < menu.Pages.Count; i++)
            {
                Assert.IsTrue(menu.Current.Root.gameObject.activeSelf);
                Assert.IsNotEmpty(AllText(menu.Current), $"tab {menu.Current.Id} shows something");
                yield return Tap(Key.E);
            }
        }

        // ---- pages -------------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheSkillsPage_ShowsLevelsAndXp()
        {
            yield return Start();
            _session.AddSkillXp(SkillIds.Farming, 120);                 // level 2 at 100
            yield return Tap(Key.M);
            var text = AllText(Ui.GameMenu.Current);
            StringAssert.Contains("Farming", text);
            StringAssert.Contains("Level 2", text);
            StringAssert.Contains("20 / 150", text, "xp into the level / xp the level needs");
        }

        [UnityTest]
        public IEnumerator TheSocialPage_HidesNamesUntilMet_AndShowsHearts()
        {
            yield return Start();
            yield return Tap(Key.M);
            yield return Tap(Key.E);
            var page = Ui.GameMenu.Current;
            Assert.AreEqual(MenuTabs.Social, page.Id);
            StringAssert.DoesNotContain("Tilda", AllText(page));
            StringAssert.Contains("???", AllText(page));
            yield return Tap(Key.Escape);

            _session.State.Npcs["tilda"] = new NpcState { Met = true, Points = FriendshipModel.PointsPerHeart * 3 + 10 };
            yield return Tap(Key.M);
            yield return Tap(Key.E);
            StringAssert.Contains("Tilda Ashby", AllText(Ui.GameMenu.Current));
            var hearts = Ui.GameMenu.Current.Root.GetComponentsInChildren<Image>(true)
                .Where(i => i.name.StartsWith("Heart") && i.color.r > 0.8f && i.color.g < 0.4f).Count();
            Assert.AreEqual(3, hearts, "three full hearts");
        }

        [UnityTest]
        public IEnumerator TheCalendarPage_StartsOnThisMonth_AndBrowsesSeasons()
        {
            yield return Start();
            yield return Tap(Key.M);
            yield return Tap(Key.E);
            yield return Tap(Key.E);
            var calendar = Ui.GameMenu.Current;
            Assert.AreEqual(MenuTabs.Calendar, calendar.Id);
            StringAssert.Contains("Spring, Year 1", AllText(calendar));
            var next = calendar.Root.GetComponentsInChildren<Button>(true).First(b => b.name == "Next");
            next.onClick.Invoke();
            StringAssert.Contains("Summer, Year 1", AllText(calendar));
            Assert.AreEqual(28, calendar.Root.GetComponentsInChildren<Image>(true).Count(i => i.name.StartsWith("Day")), "a season is 28 days");
        }

        [UnityTest]
        public IEnumerator TheCalendarPage_ListsABirthday_OnceMet()
        {
            yield return Start();
            _session.State.Npcs["tilda"] = new NpcState { Met = true };
            yield return Tap(Key.M);
            yield return Tap(Key.E);
            yield return Tap(Key.E);
            StringAssert.Contains("Tilda Ashby's birthday", AllText(Ui.GameMenu.Current));
        }

        [UnityTest]
        public IEnumerator TheMapPage_MarksWhereYouAre_AndWhoIsWhere()
        {
            yield return Start(MapIds.Village);
            _session.State.Npcs["tilda"] = new NpcState { Met = true };
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 10 * 60));
            yield return Tap(Key.M);
            for (var i = 0; i < 3; i++) yield return Tap(Key.E);
            var map = Ui.GameMenu.Current;
            Assert.AreEqual(MenuTabs.Map, map.Id);
            var village = map.Root.GetComponentsInChildren<Image>(true).First(i => i.name == MapIds.Village);
            StringAssert.Contains("(you are here)", village.GetComponentInChildren<TextMeshProUGUI>().text);
            var store = map.Root.GetComponentsInChildren<Image>(true).First(i => i.name == MapIds.GeneralStore);
            StringAssert.Contains("Tilda Ashby", store.GetComponentInChildren<TextMeshProUGUI>().text, "she is at work on a Wednesday morning");
        }
    }
}
