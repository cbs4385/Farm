using System.Collections;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest 2026-10-13: the tester could not buy the animals. The list had more rows than the window shows, and the animals, the dearest and so the last, were below the bottom
    // edge with no way to reach them. They are sold at the general store now (owner, 2026-10-13), whose list is long too.
    public class ShopScrollFlowTests : PlayModeFixture
    {
        [UnityTest]
        public IEnumerator TheGeneralStoresAnimals_CanBeScrolledTo_AndBought()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var s = GameContext.Current.Session;
            s.BeginNewGame("Tester", "Test Farm", 0);
            s.SetFlag(FatigueModel.WarnedFlag);
            s.State.CurrentMap = MapIds.Farm;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            s.State.Gold = 5000;

            ((UiService)GameContext.Current.Ui).ShowShop("general");
            for (var i = 0; i < 4; i++) yield return null;
            var list = GameObject.Find("List");
            var scroll = list.GetComponentInParent<ScrollRect>();
            Assert.IsNotNull(scroll, "the shop list scrolls");
            var chicken = list.transform.Find("animal.chicken");
            var sheep = list.transform.Find("animal.sheep");
            Assert.IsNotNull(chicken, "the animals are in the list");
            Assert.IsNotNull(sheep);
            Assert.Greater(((RectTransform)scroll.content).rect.height, ((RectTransform)scroll.transform).rect.height, "there is more than the window shows");

            scroll.verticalNormalizedPosition = 0f;                          // down to the dearest
            for (var i = 0; i < 3; i++) yield return null;
            var view = new Vector3[4];
            ((RectTransform)scroll.transform).GetWorldCorners(view);
            var row = new Vector3[4];
            ((RectTransform)sheep).GetWorldCorners(row);
            Assert.GreaterOrEqual(row[0].y, view[0].y - 0.5f, "the last row (the sheep) is inside the window once scrolled down");

            var buy = sheep.GetComponentsInChildren<Button>().First();
            buy.onClick.Invoke();
            var ui = (UiService)GameContext.Current.Ui;
            if (ui.Quantity != null && ui.Quantity.IsOpen) ui.Quantity.Submit(1);          // Buy... asks how many
            Assert.AreEqual(1, s.Backpack.Count("animal.sheep"), "and can be bought");
        }
    }
}
