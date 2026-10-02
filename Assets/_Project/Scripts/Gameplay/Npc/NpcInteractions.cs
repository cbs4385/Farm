using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public readonly struct NpcTalked
    {
        public readonly string NpcId;
        public NpcTalked(string npcId) { NpcId = npcId; }
    }

    public readonly struct NpcGifted
    {
        public readonly string NpcId, ItemId; public readonly GiftTaste Taste;
        public NpcGifted(string npcId, string itemId, GiftTaste taste) { NpcId = npcId; ItemId = itemId; Taste = taste; }
    }

    public enum GiftResult { Given, NothingSelected, NotGiftable, AlreadyToday, WeekLimit }

    // Talking to and giving gifts to villagers, and the morning bookkeeping of friendship. Rules live in FriendshipModel;
    // this connects them to the session, the dialogue system and the events other systems listen to.
    public static class NpcInteractions
    {
        public static NpcState StateOf(GameState state, string npcId)
        {
            if (!state.Npcs.TryGetValue(npcId, out var s)) state.Npcs[npcId] = s = new NpcState();
            return s;
        }

        public static GiftTaste TasteOf(NpcDefinition npc, ItemDefinition item)
        {
            if (npc.Loved.Contains(item.Id)) return GiftTaste.Loved;
            if (npc.Liked.Contains(item.Id)) return GiftTaste.Liked;
            if (npc.Disliked.Contains(item.Id)) return GiftTaste.Disliked;
            var category = item.Category.ToString();
            if (npc.LovedCategories.Contains(category)) return GiftTaste.Loved;
            if (npc.DislikedCategories.Contains(category)) return GiftTaste.Disliked;
            return GiftTaste.Neutral;
        }

        // Adds (or removes) friendship points, announcing a new heart. Returns the new total.
        public static int AddPoints(GameSession session, string npcId, int points)
        {
            var state = StateOf(session.State, npcId);
            var before = FriendshipModel.Hearts(state.Points);
            state.Points = FriendshipModel.Clamp(state.Points + points);
            var after = FriendshipModel.Hearts(state.Points);
            if (after > before && session.Npcs.Get(npcId) is NpcDefinition npc)
                session.Toast(L.Get("npc.heart_up", L.Get(npc.NameKey), after));
            return state.Points;
        }

        // The player talks to a villager. The first talk each day is worth a little friendship.
        public static bool Talk(GameSession session, NpcDefinition npc)
        {
            var state = StateOf(session.State, npc.Id);
            var today = session.Clock.Now.TotalDays;
            state.Met = true;
            if (!state.TalkedToday)
            {
                state.TalkedToday = true;
                AddPoints(session, npc.Id, FriendshipModel.TalkPoints);
            }
            state.LastContactDay = today;
            session.Publish(new NpcTalked(npc.Id));

            var set = session.Story.Set(npc.TalkSetId);
            var dialogue = set?.Pick(session.World, today * 7919 + StableHash(npc.Id));
            if (dialogue == null || !session.BeginDialogue(dialogue))
            {
                session.Toast(L.Get("npc.no_reply", L.Get(npc.NameKey)));
                return false;
            }
            return true;
        }

        // Gives the stack in a backpack slot (one item). Returns why it did not work, or Given.
        public static GiftResult Gift(GameSession session, NpcDefinition npc, int slot)
        {
            var stack = slot >= 0 && slot < session.Backpack.Capacity ? session.Backpack.Get(slot) : null;
            if (stack == null) return GiftResult.NothingSelected;
            if (!session.Db.TryGetItem(stack.ItemId, out var item) || item.IsTool) return GiftResult.NotGiftable;

            var state = StateOf(session.State, npc.Id);
            var now = session.Clock.Now;
            var week = FriendshipModel.WeekOf(now.TotalDays);
            if (state.GiftWeek != week) { state.GiftWeek = week; state.GiftsThisWeek = 0; }
            if (state.GiftsToday >= FriendshipModel.MaxGiftsPerDay) return GiftResult.AlreadyToday;
            if (state.GiftsThisWeek >= FriendshipModel.MaxGiftsPerWeek) return GiftResult.WeekLimit;

            var taste = TasteOf(npc, item);
            var birthday = npc.IsBirthday(now);
            session.Backpack.RemoveFromSlot(slot, 1);
            state.GiftsToday++;
            state.GiftsThisWeek++;
            state.Met = true;
            state.LastContactDay = now.TotalDays;
            AddPoints(session, npc.Id, FriendshipModel.GiftPoints(taste, birthday));
            session.AddVar(QuestLog.Stats.Gifts, 1);
            session.Publish(new NpcGifted(npc.Id, item.Id, taste));

            var candidates = new List<string>();
            if (birthday && taste != GiftTaste.Disliked) { candidates.Add($"npc.{npc.Id}.gift.birthday"); candidates.Add("gift.birthday"); }
            candidates.Add($"npc.{npc.Id}.gift.{taste.ToString().ToLowerInvariant()}");
            candidates.Add($"gift.{taste.ToString().ToLowerInvariant()}");
            foreach (var id in candidates)
                if (session.Story.Dialogue(id) != null && session.BeginDialogue(id)) break;
            return GiftResult.Given;
        }

        public static void ToastFor(GameSession session, GiftResult result)
        {
            switch (result)
            {
                case GiftResult.AlreadyToday: session.Toast(L.Get("gift.already_today")); break;
                case GiftResult.WeekLimit: session.Toast(L.Get("gift.week_limit")); break;
                case GiftResult.NotGiftable: session.Toast(L.Get("gift.not_giftable")); break;
            }
        }

        // Called when a new day has begun: daily counters reset, weekly gift counts roll over, and villagers the player
        // has ignored for a few days grow cooler (the rate goes through the decay hook, so dread can speed it up).
        public static void NewDay(GameState state, GameDateTime newDate, GameHooks hooks)
        {
            var today = newDate.TotalDays;
            var week = FriendshipModel.WeekOf(today);
            foreach (var kv in state.Npcs)
            {
                var s = kv.Value;
                s.TalkedToday = false;
                s.GiftsToday = 0;
                if (s.GiftWeek != week) { s.GiftWeek = week; s.GiftsThisWeek = 0; }
                if (!s.Met || s.LastContactDay < 0) continue;
                var rate = hooks != null ? hooks.ComputeFriendshipDecay(kv.Key, FriendshipModel.BaseDecayPerDay, state) : FriendshipModel.BaseDecayPerDay;
                var loss = FriendshipModel.DecayFor(today - s.LastContactDay, rate);
                if (loss > 0) s.Points = FriendshipModel.Clamp(s.Points - loss);
            }
        }

        public static int StableHash(string text)
        {
            unchecked
            {
                var h = 17;
                foreach (var c in text) h = h * 31 + c;
                return h & 0x7FFFFFFF;
            }
        }
    }
}
