using System.Linq;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Loads the layer's story data (dialogue, events, quests, letters, random events) from Resources/Mythos into the game's
    // story content. Everything in it is gated by `horror:<n>` conditions, so it is inert at intensity 0.
    public static class MythosContent
    {
        public static void Load(StoryContent story)
        {
            if (story == null) return;
            foreach (var asset in Resources.LoadAll<TextAsset>(MythosModule.ResourceFolder))
            {
                var source = "mythos/" + asset.name;
                if (story.Sources.Contains(source)) continue;
                story.AddJson(asset.text, source);
            }
        }
    }
}
