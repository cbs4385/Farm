using System.Collections.Generic;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Tests
{
    // The focus chain of an Options page (playtest 2026-10-16, controller only: "when the active point would hit the bottom of the visible list, things would get weird").
    public class OptionsNavTests
    {
        readonly List<GameObject> _made = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _made) if (go != null) Object.DestroyImmediate(go);
            _made.Clear();
        }

        T Make<T>(string name) where T : Selectable
        {
            var go = new GameObject(name, typeof(RectTransform));
            _made.Add(go);
            return go.AddComponent<T>();
        }

        List<Selectable> Row(params Selectable[] cells) => new List<Selectable>(cells);

        [Test]
        public void UpAndDown_GoToTheRowAboveAndBelow_KeepToTheColumn_AndDoNotWrapRound()
        {
            var a = Make<Button>("A");
            var b1 = Make<Button>("B1"); var b2 = Make<Button>("B2"); var b3 = Make<Button>("B3");
            var c = Make<Button>("C");
            OptionsNav.Wire(new[] { Row(a), Row(b1, b2, b3), Row(c) }, null);

            Assert.IsNull(a.navigation.selectOnUp, "the page does not wrap");
            Assert.AreSame(b1, a.navigation.selectOnDown);
            Assert.AreSame(a, b2.navigation.selectOnUp, "a short row above takes its last control");
            Assert.AreSame(c, b3.navigation.selectOnDown, "a short row below takes its last control");
            Assert.AreSame(b1, c.navigation.selectOnUp, "and the column is kept where it can be");
            Assert.IsNull(c.navigation.selectOnDown, "the last row is the end: down stays");
            foreach (var cell in new Selectable[] { a, b1, b2, b3, c }) Assert.AreEqual(Navigation.Mode.Explicit, cell.navigation.mode, cell.name + " is not left to automatic navigation");
        }

        [Test]
        public void LeftAndRight_WalkAlongARow_AndLeftFromTheFirstControlGoesToTheList()
        {
            var list = Make<Button>("List");
            var b1 = Make<Button>("B1"); var b2 = Make<Button>("B2"); var b3 = Make<Button>("B3");
            OptionsNav.Wire(new[] { Row(b1, b2, b3) }, list);
            Assert.AreSame(list, b1.navigation.selectOnLeft);
            Assert.AreSame(b2, b1.navigation.selectOnRight);
            Assert.AreSame(b1, b2.navigation.selectOnLeft);
            Assert.AreSame(b3, b2.navigation.selectOnRight);
            Assert.IsNull(b3.navigation.selectOnRight, "no wrap");
        }

        [Test]
        public void ASliderAndAControlThatStepsItsValue_KeepLeftAndRightForThemselves()
        {
            var list = Make<Button>("List");
            var slider = Make<Slider>("Slider");
            var choice = Make<Button>("Choice");
            choice.gameObject.AddComponent<ValueStepper>();
            var plain = Make<Button>("Plain");
            OptionsNav.Wire(new[] { Row(slider), Row(choice), Row(plain) }, list);
            Assert.IsNull(slider.navigation.selectOnLeft, "a slider moves its own value with left");
            Assert.IsNull(slider.navigation.selectOnRight);
            Assert.IsNull(choice.navigation.selectOnLeft, "a choice cycles with left");
            Assert.IsNull(choice.navigation.selectOnRight);
            Assert.AreSame(list, plain.navigation.selectOnLeft, "a plain button can go back to the list");
            Assert.AreSame(choice, slider.navigation.selectOnDown, "but all of them still go up and down");
            Assert.AreSame(slider, choice.navigation.selectOnUp);
        }

        [Test]
        public void TheCategoryList_WalksUpAndDown_AndRightGoesIntoThePage()
        {
            var one = Make<Button>("One"); var two = Make<Button>("Two"); var three = Make<Button>("Three");
            var page = Make<Button>("Page");
            OptionsNav.WireList(new Selectable[] { one, two, three }, page);
            Assert.IsNull(one.navigation.selectOnUp);
            Assert.AreSame(two, one.navigation.selectOnDown);
            Assert.AreSame(two, three.navigation.selectOnUp);
            Assert.IsNull(three.navigation.selectOnDown);
            foreach (var b in new[] { one, two, three }) Assert.AreSame(page, b.navigation.selectOnRight);
            Assert.IsNull(one.navigation.selectOnLeft);
        }

        [Test]
        public void TheValueStepper_StepsBackAndForwardOnLeftAndRight_AndIgnoresUpAndDown()
        {
            var go = new GameObject("S", typeof(RectTransform));
            _made.Add(go);
            var steps = new List<int>();
            go.AddComponent<ValueStepper>().Step = steps.Add;
            var stepper = go.GetComponent<ValueStepper>();
            var system = new GameObject("ES").AddComponent<UnityEngine.EventSystems.EventSystem>();
            _made.Add(system.gameObject);
            foreach (var dir in new[] { UnityEngine.EventSystems.MoveDirection.Left, UnityEngine.EventSystems.MoveDirection.Right, UnityEngine.EventSystems.MoveDirection.Up, UnityEngine.EventSystems.MoveDirection.Down })
                stepper.OnMove(new UnityEngine.EventSystems.AxisEventData(system) { moveDir = dir });
            CollectionAssert.AreEqual(new[] { -1, 1 }, steps);
        }
    }
}
