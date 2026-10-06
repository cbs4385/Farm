using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest request: hovering a hotbar slot says what the item is, and for a tool how and on what to use it.
    public class HotbarTooltipTests
    {
        static System.Collections.Generic.Dictionary<string, string> En() =>
            L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));

        [Test]
        public void EveryToolInTheGame_HasAHowToUseText()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var en = En();
            var tools = db.AllItems.Where(i => i.IsTool).ToList();
            Assert.GreaterOrEqual(tools.Count, 8);
            foreach (var tool in tools)
            {
                var key = HotbarTooltip.UseKey(tool.ToolType);
                Assert.IsNotNull(key, tool.Id);
                Assert.IsTrue(en.TryGetValue(key, out var text) && text.Length > 20, tool.Id + " has no how-to-use text (" + key + ")");
                Assert.IsTrue(en.ContainsKey(tool.NameKey) && en.ContainsKey(tool.DescriptionKey), tool.Id);
            }
            Assert.IsTrue(en["hotbar.how_to_use"].Contains("{0}"));
        }

        [Test]
        public void ANonTool_HasNoHowToUseKey()
        {
            Assert.IsNull(HotbarTooltip.UseKey(ToolType.None));
        }
    }
}
