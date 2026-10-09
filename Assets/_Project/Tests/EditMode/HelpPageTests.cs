using System;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;

namespace Farm.Tests
{
    // Owner, 2026-10-09: the tool hint shows once; everything it says is kept in a Help tab of the menu.
    public class HelpPageTests
    {
        [SetUp]
        public void SetUp() => L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));

        [Test]
        public void TheHelpTab_ListsEveryTool_AndEachHasItsInstructions()
        {
            var tools = Enum.GetValues(typeof(ToolType)).Cast<ToolType>().Where(t => t != ToolType.None).OrderBy(t => t).ToList();
            CollectionAssert.AreEqual(tools, HelpPage.ToolOrder.OrderBy(t => t).ToList(), "every tool is described, once");
            foreach (var tool in HelpPage.ToolOrder)
            {
                var key = HotbarTooltip.UseKey(tool);
                Assert.IsTrue(L.Has(key), key);
                StringAssert.DoesNotContain("[[", L.Get(key), key + ": the control names are filled in");
            }
        }

        [Test]
        public void TheEverydayTopicsAndControls_AreAllWritten_WithTheirControlNamesFilledIn()
        {
            foreach (var key in HelpPage.Topics.Concat(HelpPage.Controls).Concat(new[] { "help.title", "help.hint", "help.tools", "help.everyday", "help.controls", "help.more", "menu.tab.help", "tour.help" }))
            {
                Assert.IsTrue(L.Has(key), key);
                StringAssert.DoesNotContain("[[", L.Get(key), key);
            }
        }

        [Test]
        public void TheToolHintShowsOnce_PerItem()
        {
            Assert.AreEqual(1, HudView.HelpTimes);
        }

        [Test]
        public void HelpIsATabOfTheMenu_AndPartOfTheTour()
        {
            Assert.AreEqual("help", MenuTabs.Help);
            CollectionAssert.Contains(MenuTourScreen.Order, MenuTabs.Help);
        }
    }
}
