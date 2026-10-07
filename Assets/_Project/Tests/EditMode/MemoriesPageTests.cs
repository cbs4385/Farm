using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-07: the memories tab display was broken. The groups it lists are the festivals, each villager with scenes, then the specials.
    public class MemoriesPageTests
    {
        [Test]
        public void EveryMemory_IsInExactlyOneGroup_FestivalsFirst_SpecialsLast()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            using (var f = new TestSessionFixture(db.AllItems, db.AllCrops, null))
            {
                var s = f.Session;
                s.Story = StoryContent.LoadFromResources();
                var all = Memories.All(s.Story);
                var groups = MemoriesPage.Groups(s, all);
                Assert.AreEqual(Memories.FestivalGroup, groups.First().Id);
                Assert.AreEqual(Memories.OtherGroup, groups.Last().Id);
                Assert.AreEqual(all.Count, groups.Sum(g => g.Events.Count), "nothing is lost or counted twice");
                CollectionAssert.AllItemsAreUnique(groups.Select(g => g.Id));
                foreach (var g in groups)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(g.Name), g.Id);
                }
            }
        }
    }
}
