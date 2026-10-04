using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The friendship a slice scene promises must survive template expansion and be an effect the scene actually runs.
    public class SliceRewardTests
    {
        [Test]
        public void EverySliceScene_ExpandsToAFriendEffectAndItsOwnFlag()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            foreach (var id in new[] { "wren_heart4", "wren_heart10", "wren_friend", "hazel_heart6", "bram_heart10", "bram_friend", "hazel_friend" })
            {
                var effects = story.Event(id).Steps.Where(s => s.Type == "effects").SelectMany(s => s.Effects ?? new System.Collections.Generic.List<string>()).ToList();
                Assert.IsTrue(effects.Any(e => e.StartsWith("friend:")), $"{id}: a friend effect (has: {string.Join(" | ", effects)})");
                Assert.IsTrue(effects.Any(e => e == "flag:event." + id), $"{id}: marks itself done");
            }
        }
    }
}
