using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-122: the paired villager comments on someone's big scene afterwards.
    public class CameoReactionsTests
    {
        [Test]
        public void EveryCameo_ListensForARealScene_AndIsSpokenByADifferentVillager()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var cameos = story.Reactions.Where(r => r.Id.StartsWith("cameo.")).ToList();
            Assert.AreEqual(14, cameos.Count);
            foreach (var r in cameos)
            {
                Assert.IsTrue(Reactions.IsKnownTrigger(r.On), r.Id);
                var sceneId = r.On.Substring("event:".Length);
                Assert.IsNotNull(story.Event(sceneId), r.Id + " listens for " + sceneId);
                Assert.IsFalse(sceneId.StartsWith(r.Npcs + "_"), r.Id + ": a villager does not comment on their own scene");
                Assert.IsTrue(r.Once, r.Id);
                Assert.IsNotNull(story.Dialogue(r.Dialogue), r.Id);
                StringAssert.Contains("hearts:" + r.Npcs, r.Condition);
            }
        }
    }
}
