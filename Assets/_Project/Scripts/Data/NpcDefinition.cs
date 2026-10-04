using System;
using System.Collections.Generic;
using Farm.Core;
using UnityEngine;

namespace Farm.Data
{
    // Where an NPC is from `Minute` on (minute of the day, 360..1799) until the next stop. The NPC leaves the previous
    // stop at `Minute` and walks here, so a stop is also a departure time.
    [Serializable]
    public sealed class NpcStop
    {
        public int Minute;
        public string Map;
        public int X;
        public int Y;
        public string Facing = "down";   // up, down, left, right: how they stand once there
    }

    // One possible day for an NPC. The first entry (highest Priority first, then list order) whose Condition holds
    // when the day starts is used for the whole day. Conditions can use season, weather, weekday, flags, vars...
    // (a hidden night schedule is an entry with a higher priority and a condition on a flag). A stop's
    // own Condition is not needed: put a different day in a different entry.
    [Serializable]
    public sealed class NpcScheduleEntry
    {
        public string Id;
        public string Condition;
        public int Priority;
        public List<NpcStop> Stops = new List<NpcStop>();
    }

    // Static data for one villager. The Id is stable and used in saves. Text lives in the string table
    // (`npc.<id>.name`) and dialogue in Resources/Story (`npc.<id>.talk` is the dialogue set used for talking).
    [CreateAssetMenu(menuName = "Farm/NPC")]
    public sealed class NpcDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] string _nameKey;
        [SerializeField] int _birthdaySeason;            // Season index
        [SerializeField] int _birthdayDay = 1;
        [SerializeField] bool _romanceable;
        [SerializeField] string _allegiance;             // optional layers read it (unaware / cult / resister); unused by the base game
        [SerializeField] string _homeMap;
        [SerializeField] int _homeX;
        [SerializeField] int _homeY;
        [SerializeField] string _business;               // shop id they work at, if any
        [SerializeField] Sprite _down, _up, _left, _right, _portrait;
        [SerializeField] Sprite[] _expressions;          // aligned with ExpressionNames; a missing entry falls back to the portrait
        [SerializeField] string[] _loved = new string[0];       // item ids
        [SerializeField] string[] _liked = new string[0];
        [SerializeField] string[] _disliked = new string[0];
        [SerializeField] string[] _lovedCategories = new string[0];   // ItemCategory names, for everything of a kind
        [SerializeField] string[] _dislikedCategories = new string[0];
        [SerializeField] List<NpcScheduleEntry> _schedule = new List<NpcScheduleEntry>();

        public string Id => _id;
        public string NameKey => _nameKey;
        public Season BirthdaySeason => (Season)Mathf.Clamp(_birthdaySeason, 0, 3);
        public int BirthdayDay => _birthdayDay;
        public bool Romanceable => _romanceable;
        public string Allegiance => _allegiance;
        public string HomeMap => _homeMap;
        public int HomeX => _homeX;
        public int HomeY => _homeY;
        public string Business => _business;
        public Sprite Portrait => _portrait;

        // The portrait variants dialogue lines can ask for (T-095). Art arrives with T-130.
        public static readonly string[] ExpressionNames = { "neutral", "happy", "sad", "surprised", "embarrassed", "thinking" };

        public Sprite PortraitFor(string expression)
        {
            if (_expressions != null && !string.IsNullOrEmpty(expression))
            {
                var i = System.Array.IndexOf(ExpressionNames, expression);
                if (i >= 0 && i < _expressions.Length && _expressions[i] != null) return _expressions[i];
            }
            return _portrait;
        }
        public IReadOnlyList<string> Loved => _loved;
        public IReadOnlyList<string> Liked => _liked;
        public IReadOnlyList<string> Disliked => _disliked;
        public IReadOnlyList<string> LovedCategories => _lovedCategories;
        public IReadOnlyList<string> DislikedCategories => _dislikedCategories;
        public IReadOnlyList<NpcScheduleEntry> Schedule => _schedule;

        public string TalkSetId => $"npc.{_id}.talk";

        public Sprite SpriteFor(Vector2Int facing)
        {
            if (facing == Vector2Int.up) return _up != null ? _up : _down;
            if (facing == Vector2Int.left) return _left != null ? _left : _down;
            if (facing == Vector2Int.right) return _right != null ? _right : _down;
            return _down;
        }

        public bool IsBirthday(GameDateTime now) => now.Season == BirthdaySeason && now.Day == _birthdayDay;

        public static NpcDefinition Create(string id, Season birthdaySeason, int birthdayDay, string homeMap, int homeX, int homeY,
            bool romanceable = false, string business = null)
        {
            var npc = CreateInstance<NpcDefinition>();
            npc._id = id;
            npc.name = id;
            npc._nameKey = $"npc.{id}.name";
            npc._birthdaySeason = (int)birthdaySeason;
            npc._birthdayDay = birthdayDay;
            npc._homeMap = homeMap;
            npc._homeX = homeX;
            npc._homeY = homeY;
            npc._romanceable = romanceable;
            npc._business = business;
            return npc;
        }

        public NpcDefinition WithTastes(string[] loved, string[] liked, string[] disliked, string[] lovedCategories = null, string[] dislikedCategories = null)
        {
            _loved = loved ?? new string[0];
            _liked = liked ?? new string[0];
            _disliked = disliked ?? new string[0];
            _lovedCategories = lovedCategories ?? new string[0];
            _dislikedCategories = dislikedCategories ?? new string[0];
            return this;
        }

        public NpcDefinition WithSchedule(IEnumerable<NpcScheduleEntry> entries)
        {
            _schedule = new List<NpcScheduleEntry>(entries);
            return this;
        }

        // The expression portraits, aligned with ExpressionNames; null entries fall back to the base portrait (T-130).
        public void SetExpressions(Sprite[] expressions) => _expressions = expressions;

        public void SetSprites(Sprite down, Sprite up, Sprite left, Sprite right, Sprite portrait)
        {
            _down = down; _up = up; _left = left; _right = right; _portrait = portrait;
        }
    }
}
