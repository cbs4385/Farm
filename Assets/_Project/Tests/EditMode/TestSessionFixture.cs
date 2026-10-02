using System;
using System.Collections.Generic;
using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Tests
{
    // A GameSession with a tiny database and a throwaway save folder, for EditMode tests that need the real session
    // rules (flags, backpack, gold) without a scene. Dispose it when the test ends.
    public sealed class TestSessionFixture : IDisposable
    {
        public readonly GameObject Go;
        public readonly GameSession Session;
        public readonly EventBus Bus = new EventBus();
        public readonly List<string> Toasts = new List<string>();
        readonly string _root;

        public TestSessionFixture(IEnumerable<ItemDefinition> items = null, IEnumerable<CropDefinition> crops = null,
            IEnumerable<NpcDefinition> npcs = null)
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-story-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            Go = new GameObject("test-session");
            Session = Go.AddComponent<GameSession>();
            // The new-game backpack holds the starting tools and seeds; without their definitions every seed would take a slot.
            var all = new List<ItemDefinition>(items ?? new ItemDefinition[0]);
            foreach (var tool in new[] { (ItemIds.Hoe, ToolType.Hoe), (ItemIds.WateringCan, ToolType.WateringCan), (ItemIds.Axe, ToolType.Axe),
                                        (ItemIds.Pickaxe, ToolType.Pickaxe), (ItemIds.Scythe, ToolType.Scythe) })
                if (!all.Exists(i => i.Id == tool.Item1)) all.Add(ItemDefinition.Create(tool.Item1, ItemCategory.Tool, 1, toolType: tool.Item2));
            if (!all.Exists(i => i.Id == ItemIds.Seed("parsnip"))) all.Add(ItemDefinition.Create(ItemIds.Seed("parsnip"), ItemCategory.Seed, cropId: "parsnip"));
            var db = GameDatabase.Create(all, crops ?? new CropDefinition[0]);
            if (npcs != null) db.SetNpcs(npcs);
            Session.Init(Bus, db, new SaveService(_root));
            Session.BeginDevGame();
            Bus.Subscribe<ToastRequested>(t => Toasts.Add(t.Message));
            StoryConditions.Register();
            Effects.ResetForTests();
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(Go);
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        public static ItemDefinition Item(string id, ItemCategory category = ItemCategory.Crop, int sell = 10) =>
            ItemDefinition.Create(id, category, sellPrice: sell);
    }
}
