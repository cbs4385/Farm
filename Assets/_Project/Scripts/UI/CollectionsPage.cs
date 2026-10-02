using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The collections tab (T-057): every crop, forage, fish, artisan good, dish and resource the player has held (shown dim
    // until found), and what has been sold through the shipping bin.
    public sealed class CollectionsPage : MenuPage
    {
        RectTransform _body;
        public override string Id => MenuTabs.Collections;

        static readonly (string key, ItemCategory[] categories)[] Sections =
        {
            ("collections.crops", new[] { ItemCategory.Crop }), ("collections.forage", new[] { ItemCategory.Forage }),
            ("collections.fish", new[] { ItemCategory.Fish }), ("collections.goods", new[] { ItemCategory.Artisan, ItemCategory.Food }),
            ("collections.resources", new[] { ItemCategory.Resource }),
        };

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Collections", 4f, 8);
            UiKit.Stretch((RectTransform)stack.transform);
            _body = (RectTransform)stack.transform;
        }

        public override void Refresh(UiService ui)
        {
            var s = ui.Session;
            UiKit.ClearChildren(_body);
            foreach (var (key, categories) in Sections)
            {
                var items = s.Db.AllItems.Where(i => categories.Contains(i.Category)).OrderBy(i => i.Id).ToList();
                var found = items.Count(i => s.State.Collected.Contains(i.Id));
                UiKit.Label(_body, L.Get(key) + $"  {found}/{items.Count}", 17f, TextAlignmentOptions.Left, UiKit.Accent);
                var grid = UiKit.Rect("Grid", _body);
                var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
                layout.cellSize = new Vector2(22f, 22f);
                layout.spacing = new Vector2(2f, 2f);
                UiKit.Size(grid.gameObject, -1f, items.Count > 36 ? 50f : 24f);
                foreach (var item in items)
                {
                    var icon = UiKit.Panel(grid, item.Id, s.State.Collected.Contains(item.Id) ? Color.white : new Color(0.15f, 0.12f, 0.1f, 1f));
                    icon.sprite = item.Icon;
                    icon.preserveAspect = true;
                }
            }
            var shipped = s.State.ShippedTotals.Values.Sum();
            UiKit.Label(_body, L.Get("collections.shipped", shipped, s.State.TotalEarned), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            var top = s.State.ShippedTotals.OrderByDescending(p => p.Value).FirstOrDefault();
            if (top.Key != null && s.Db.TryGetItem(top.Key, out var topItem))
                UiKit.Label(_body, L.Get("collections.top", L.Get(topItem.NameKey), top.Value), 16f, TextAlignmentOptions.Left, UiKit.DimText);
        }
    }
}
