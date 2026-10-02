using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // A scrolling list of the recipes of one station with their ingredients and a Craft button (T-037). Used by the
    // kitchen screen and the Crafting tab of the game menu.
    public sealed class RecipeListPanel
    {
        readonly UiService _ui;
        readonly string _station;
        readonly RectTransform _content;

        public RecipeListPanel(UiService ui, string station, Transform parent)
        {
            _ui = ui;
            _station = station;

            var scroll = UiKit.Rect("Scroll", parent);
            UiKit.Size(scroll.gameObject, -1f, -1f, 1f, 1f);
            var catcher = scroll.gameObject.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            var rect = scroll.gameObject.AddComponent<ScrollRect>();
            scroll.gameObject.AddComponent<RectMask2D>();
            scroll.gameObject.AddComponent<ScrollSelectionFollower>();
            var content = UiKit.VStack(scroll, "Content", 4f, 4);
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            rect.content = crt;
            rect.viewport = scroll;
            rect.horizontal = false;
            rect.scrollSensitivity = 30f;
            rect.movementType = ScrollRect.MovementType.Clamped;
            rect.verticalScrollbar = UiKit.MakeScrollbar(scroll);
            rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            _content = crt;
        }

        public void Refresh()
        {
            var session = _ui.Session;
            UiKit.ClearChildren(_content);
            var recipes = session.Recipes.ForStation(_station).ToList();
            // Known recipes first, then the ones a skill has yet to unlock.
            foreach (var recipe in recipes.Where(session.KnowsRecipe).Concat(recipes.Where(r => !session.KnowsRecipe(r))))
                BuildRow(session, recipe);
            if (recipes.Count == 0) UiKit.Label(_content, L.Get("crafting.nothing"), 18f, TextAlignmentOptions.Center, UiKit.DimText);
        }

        void BuildRow(GameSession session, RecipeDefinition recipe)
        {
            var known = session.KnowsRecipe(recipe);
            var output = session.Db.TryGetItem(recipe.OutputItemId, out var item) ? item : null;
            var row = UiKit.HStack(_content, recipe.Id, 8f);
            UiKit.Size(row.gameObject, -1f, 46f);

            var icon = UiKit.Panel(row.transform, "Icon", known ? Color.white : new Color(0.3f, 0.3f, 0.3f));
            icon.sprite = output != null ? output.Icon : null;
            icon.preserveAspect = true;
            UiKit.Size(icon.gameObject, 36f, 36f);

            var name = output != null ? L.Get(output.NameKey) : recipe.OutputItemId;
            if (recipe.OutputCount > 1) name = $"{name} x{recipe.OutputCount}";
            var title = UiKit.Label(row.transform, known ? name : L.Get("crafting.locked", L.Get("skill." + recipe.SkillId), recipe.SkillLevel), 18f,
                TextAlignmentOptions.Left, known ? UiKit.TextColor : UiKit.DimText);
            UiKit.Size(title.gameObject, 250f, 40f);

            var ingredients = known ? string.Join(", ", recipe.Ingredients.Select(i => IngredientText(session, i))) : string.Empty;
            var needs = UiKit.Label(row.transform, ingredients, 15f, TextAlignmentOptions.Left, UiKit.DimText);
            UiKit.Size(needs.gameObject, -1f, 40f, 1f);

            if (!known) return;
            var craftable = CraftingRules.HasIngredients(recipe, session.Backpack);
            var button = UiKit.MakeButton(row.transform, L.Get("crafting.craft"), () => Craft(recipe), 90f, 34f);
            button.name = "Craft";
            button.interactable = craftable;
        }

        static string IngredientText(GameSession session, RecipeIngredient i)
        {
            var have = CraftingRules.Available(i, session.Backpack);
            var name = i.AnyOf != null && i.AnyOf.Length > 0 ? L.Get(i.ItemId)
                : session.Db.TryGetItem(i.ItemId, out var item) ? L.Get(item.NameKey) : i.ItemId;
            return $"{i.Count} {name} ({have})";
        }

        void Craft(RecipeDefinition recipe)
        {
            var session = _ui.Session;
            switch (session.Craft(recipe, _station))
            {
                case CraftResult.Ok:
                    session.Toast(L.Get("crafting.made", session.Db.TryGetItem(recipe.OutputItemId, out var item) ? L.Get(item.NameKey) : recipe.OutputItemId));
                    AudioService.PlayIfAvailable(Sfx.Harvest);
                    break;
                case CraftResult.NoRoom: session.Toast(L.Get("toast.inventory_full")); break;
                case CraftResult.MissingIngredients: session.Toast(L.Get("crafting.missing")); break;
            }
            var selected = UnityEngine.EventSystems.EventSystem.current != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
            var rowName = selected != null && selected.transform.parent != null ? selected.transform.parent.name : null;
            Refresh();
            // Keep keyboard focus on the same recipe after the list is rebuilt.
            if (rowName != null)
            {
                var again = _content.Find(rowName);
                var button = again != null ? again.GetComponentInChildren<Button>() : null;
                if (button != null && button.interactable) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }
    }

    // The kitchen: cook with what is in the backpack.
    public sealed class CraftingScreen : UiScreen
    {
        readonly Dictionary<string, RecipeListPanel> _panels = new Dictionary<string, RecipeListPanel>();
        readonly RectTransform _body;
        readonly TextMeshProUGUI _title;
        string _station;

        public CraftingScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Crafting", new Vector2(780f, 470f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            _title = UiKit.Label(stack.transform, "", 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _body = (RectTransform)UiKit.VStack(stack.transform, "Body", 0f).transform;
            UiKit.Size(_body.gameObject, -1f, -1f, 1f, 1f);
            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenStation(string station)
        {
            _station = station;
            _title.text = L.Get("crafting.title." + station);
            if (!_panels.TryGetValue(station, out var list))
            {
                list = new RecipeListPanel(Ui, station, _body);
                _panels[station] = list;
            }
            list.Refresh();
            Open();
        }
    }

    // The Crafting tab of the game menu: recipes made by hand.
    public sealed class CraftingPage : MenuPage
    {
        RecipeListPanel _list;
        public override string Id => MenuTabs.Crafting;

        protected override void Build(UiService ui, RectTransform content)
        {
            var stack = UiKit.VStack(content, "Crafting", 6f, 10);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("crafting.title.hand"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            _list = new RecipeListPanel(ui, Stations.Hand, stack.transform);
        }

        public override void Refresh(UiService ui) => _list.Refresh();
    }
}
