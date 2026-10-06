using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // With a simulated mouse in the real game: resting on a hotbar tool says what it is and how to use it; an empty slot or open ground says nothing.
    public class HotbarTooltipFlowTests : InputTestFixture
    {
        string _dataRoot;
        Mouse _mouse;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-hotbartip-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Enter()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        IEnumerator PointAtSlot(int slot)
        {
            var rect = (RectTransform)GameObject.Find("Hotbar").transform.Find("Slot" + slot);
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var centre = (corners[0] + corners[2]) * 0.5f;
            Set(_mouse.position, new Vector2(centre.x, centre.y));
            yield return null;
            Set(_mouse.position, new Vector2(centre.x + 1f, centre.y));
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator OverAToolSlot_TheLabelNamesTheToolAndSaysHowToUseIt_AndOverAnEmptySlotNothingShows()
        {
            yield return Enter();
            var ui = ServiceLocator.Get<UiService>();
            var checkedTools = 0;
            for (var i = 0; i < InputNames.HotbarSlots; i++)
            {
                var stack = _session.Backpack.Get(i);
                if (stack == null || !_session.Db.TryGetItem(stack.ItemId, out var item) || !item.IsTool) continue;
                yield return PointAtSlot(i);
                var text = ui.HoverText;
                Assert.IsNotNull(text, item.Id + ": no label");
                StringAssert.Contains(L.Get(item.NameKey), text, item.Id);
                StringAssert.Contains(L.Get(HotbarTooltip.UseKey(item.ToolType)), text, item.Id + ": how to use");
                checkedTools++;
            }
            Assert.GreaterOrEqual(checkedTools, 5, "the starting tools");

            var empty = Enumerable.Range(0, InputNames.HotbarSlots).First(i => _session.Backpack.Get(i) == null);
            yield return PointAtSlot(empty);
            Assert.IsNull(ui.HoverText, "an empty slot has nothing to say");
        }
    }
}
