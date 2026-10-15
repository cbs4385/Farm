using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // The registry of icons (playtest 2026-10-15: every icon needs a name on hover). Each screen is opened in the real game and every picture on it is looked up: it must have a UiTip above
    // it (a name, or a deliberate "decoration" or "has its own tooltip" mark). A new icon without one fails here, with the path of the offender.
    public class UiTipCoverageFlowTests : PlayModeFixture
    {
        static UiService Ui => (UiService)GameContext.Current.Ui;
        GameSession _session;

        IEnumerator Start()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            for (var slot = 0; slot < _session.Backpack.Capacity; slot++)
                if (_session.Backpack.Get(slot) != null) _session.Backpack.RemoveFromSlot(slot, _session.Backpack.Get(slot).Count);
            _session.Backpack.Add(ItemIds.Stone, 8, preferredSlot: 5);
            _session.Backpack.Add(ItemIds.Wood, 3, quality: 2, preferredSlot: 6);
        }

        static string PathOf(Transform t)
        {
            var path = t.name;
            for (t = t.parent; t != null; t = t.parent) path = t.name + "/" + path;
            return path;
        }

        static bool IsPicture(Graphic g)
        {
            if (!g.isActiveAndEnabled) return false;
            if (g is Image image && image.sprite != null) return true;
            if (g.name == "Quality") return true;                                   // the quality diamond on a slot
            return g.name == "Fill" && g.transform.parent != null && g.transform.parent.GetComponent<Button>() != null;      // a colour swatch
        }

        // Everything on screen that is a picture and has no name.
        static List<string> Unnamed()
        {
            var bad = new List<string>();
            foreach (var g in Object.FindObjectsByType<Graphic>())
            {
                if (!IsPicture(g)) continue;
                var tip = UiTip.For(g.transform);
                if (tip == null) bad.Add(PathOf(g.transform) + " has no UiTip");
                else if (tip.Mode == UiTip.Kind.Label && string.IsNullOrEmpty(tip.Current)) bad.Add(PathOf(g.transform) + " has a UiTip with no text");
                else if (tip.Mode == UiTip.Kind.Label && tip.Current.Contains("uitip.")) bad.Add(PathOf(g.transform) + " shows a missing key: " + tip.Current);
            }
            return bad;
        }

        static IEnumerator Check(string what, List<string> into)
        {
            for (var i = 0; i < 3; i++) yield return null;
            foreach (var line in Unnamed()) into.Add(what + ": " + line);
        }

        [UnityTest]
        public IEnumerator EveryPictureOnTheMainScreens_HasAName()
        {
            yield return Start();
            var bad = new List<string>();
            yield return Check("HUD", bad);

            Ui.ToggleInventory();
            yield return Check("backpack", bad);
            Ui.CloseAllModals();

            Ui.ShowGameMenu(MenuTabs.Skills);
            foreach (var page in Ui.GameMenu.Pages.Select(p => p.Id).ToList())
            {
                Ui.GameMenu.SelectTab(page);
                yield return Check("menu/" + page, bad);
            }
            Ui.CloseAllModals();

            Ui.ShowShop("general");
            yield return Check("shop", bad);
            Ui.CloseAllModals();

            Ui.ShowShipping();
            yield return Check("shipping", bad);
            Ui.CloseAllModals();

            var objects = _session.GetObjects(MapIds.Farm);
            var placed = objects.Place(_session.Db.AllPlaceables.First(p => p.Id == "chest"), 28, 8, "tipchest");
            objects.ChestOf(placed).Add(ItemIds.Wood, 4, quality: 1);
            Ui.ShowChest(placed.Id);
            yield return Check("chest", bad);
            Ui.CloseAllModals();

            Ui.ShowAvatarCreator(new AvatarData(), _ => { });
            yield return Check("farmer creator", bad);
            Ui.CloseAllModals();

            Ui.ShowOptions();
            yield return Check("options", bad);
            Ui.CloseAllModals();

            Assert.IsEmpty(bad, string.Join("\n", bad));
        }

        [UnityTest]
        public IEnumerator AHudNamePolledUnderThePointer_ShowsItsLabel()
        {
            yield return Start();
            yield return null;
            var time = Object.FindObjectsByType<UiTip>().First(t => t.Mode == UiTip.Kind.Label && t.name != null && t.Current == L.Get("uitip.hud.time"));
            var corners = new Vector3[4];
            ((RectTransform)time.transform).GetWorldCorners(corners);
            Assert.AreEqual(L.Get("uitip.hud.time"), UiTip.PolledAt((corners[0] + corners[2]) * 0.5f));
            Assert.IsNull(UiTip.PolledAt(new Vector2(-50f, -50f)));
        }
    }
}
