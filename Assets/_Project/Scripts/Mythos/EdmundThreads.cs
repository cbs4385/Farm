using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Edmund's threads (spec WP9): the cellar door the villagers say he would not let Marcus oil, and the note behind the farmhouse range. Both exist only with the layer on (at
    // intensity 0 the cellar door is just a stuck door that is never drawn as an object). Juno's key and the dialogue that points the way are data (mythos_threads.json).
    public static class EdmundThreads
    {
        public const string StoveNoteFlag = "edmund.stove_note";
        public const string BoxOpenedFlag = "edmund.box_opened";
        public const string NoteLetter = "mythos_uncle_note", TruthLetter = "mythos_uncle_truth";
        public const int TruthLore = 4;           // the box answers once the player has the lore and has seen the wood

        public static readonly Vector3Int CellarCell = new Vector3Int(84, 20, 0);          // beside the farmhouse, east of its door (clear of the mailbox, bin, hay and barrels)
        public static readonly Vector3Int StoveCell = new Vector3Int(18, 2, 0);            // the farmhouse kitchen, beside the range

        public enum CellarResult { Locked, Early, Opens }

        // What the cellar door does for the player. (pure)
        public static CellarResult Resolve(bool hasKey, int lore, bool woodsOpen) =>
            !hasKey ? CellarResult.Locked : lore < TruthLore || !woodsOpen ? CellarResult.Early : CellarResult.Opens;

        const string CellarDoorLabel = "hover.cellar_door", StoveLabel = "hover.stove_plate";

        public static void OnMapLoaded(MapLoadedContext ctx)
        {
            var s = ctx.Session;
            if (!MythosLevel.On(s)) return;
            if (ctx.MapId == MapIds.Farm && !s.HasFlag(BoxOpenedFlag))
                Spawn(ctx, "CellarDoor", CellarDoorLabel, CellarCell, ItemIds.Machine("chest"), solid: true, door: true);
            if (ctx.MapId == MapIds.FarmHouse && !s.HasFlag(StoveNoteFlag) && s.GetVar(MythosIds.Vars.Lore) >= 1)
                Spawn(ctx, "StoveNote", StoveLabel, StoveCell, ItemIds.Seed("parsnip"), solid: false, door: false);          // placeholder pictures until the art pass (docs/STATUS.md)
        }

        static void Spawn(MapLoadedContext ctx, string name, string label, Vector3Int cell, string pictureItem, bool solid, bool door)
        {
            var go = new GameObject(name);
            go.transform.position = ctx.Map.CellCenter(cell);
            var sr = go.AddComponent<SpriteRenderer>();
            LitSprites.Apply(sr);
            sr.sortingOrder = 5;
            if (ctx.Session.Db.TryGetItem(pictureItem, out var item)) sr.sprite = item.Icon;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
            box.isTrigger = !solid;
            go.AddComponent<EdmundObject>().Init(ctx.Session, label, door);
        }

        // Shows a letter at once (it was found, not posted) and keeps it in the journal when it is read.
        public static void ReadNow(GameSession s, string letterId)
        {
            var letter = s.Story.Letter(letterId);
            if (letter == null) return;
            if (!Mail.Known(s.State, letterId)) s.State.Mailbox.Add(letterId);
            if (s.Ui != null) s.Ui.ShowLetter(letter, () => Mail.Finish(s, letter));
            else Mail.Finish(s, letter);
        }
    }

    // The cellar door (door) or the loose plate behind the range.
    public sealed class EdmundObject : MonoBehaviour, IInteractable
    {
        GameSession _session;
        string _label;
        bool _door;

        public string HoverLabel => L.Get(_label);

        public void Init(GameSession session, string label, bool door) { _session = session; _label = label; _door = door; }

        void Update()
        {
            if (_session != null && (!_session.InGame || !MythosLevel.On(_session))) Destroy(gameObject);
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!MythosLevel.On(s)) return;
            if (!_door)
            {
                s.SetFlag(EdmundThreads.StoveNoteFlag);
                s.Toast(L.Get("mythos.stove.found"));
                EdmundThreads.ReadNow(s, EdmundThreads.NoteLetter);
                Destroy(gameObject);
                return;
            }
            switch (EdmundThreads.Resolve(s.Backpack.Has(MythosData.BrassKey, 1), s.GetVar(MythosIds.Vars.Lore), s.HasFlag(MythosIds.Flags.WoodsOpen)))
            {
                case EdmundThreads.CellarResult.Locked: s.Toast(L.Get("mythos.cellar.locked")); break;
                case EdmundThreads.CellarResult.Early: s.Toast(L.Get("mythos.cellar.early")); break;
                default:
                    s.SetFlag(EdmundThreads.BoxOpenedFlag);
                    EdmundThreads.ReadNow(s, EdmundThreads.TruthLetter);
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
