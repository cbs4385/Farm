using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Data;

namespace Farm.Gameplay
{
    public enum RelationshipStage { Acquaintance, Neighbour, Friend, CloseFriend, Confidant, Partners }

    // T-106: named relationship stages. They line up with the talk tiers (friend lines from 3 hearts, close from 6, confidant from 9).
    public static class RelationshipStages
    {
        // T-109: partners is a choice, not a number of hearts: only the scene that sets `partners.<id>` gets it.
        public static RelationshipStage For(int hearts, bool partners) => partners ? RelationshipStage.Partners : For(hearts);

        public static RelationshipStage For(int hearts) =>
            hearts >= 9 ? RelationshipStage.Confidant :
            hearts >= 6 ? RelationshipStage.CloseFriend :
            hearts >= 3 ? RelationshipStage.Friend :
            hearts >= 2 ? RelationshipStage.Neighbour : RelationshipStage.Acquaintance;

        public static string Key(RelationshipStage stage) => "stage." + stage.ToString().ToLowerInvariant();
    }

    // What the player has learned about one villager: the Neighbours page (T-106). Built from the save and the story, never from hidden
    // data: a taste is shown only once the player has given that exact item, a "told you" note only once the topic was asked, and the
    // hint about what comes next names the hearts it needs, not what happens.
    public sealed class NeighbourInfo
    {
        public string Id;
        public bool Met;
        public int Hearts;
        public RelationshipStage Stage;
        public List<string> Loves = new List<string>(), Likes = new List<string>(), Dislikes = new List<string>();     // item ids the player has found out
        public List<string> Told = new List<string>();          // topic label keys
        public string Hint;                                      // "hint.ready", "hint.at" or "hint.done"
        public int HintHearts;
        public string QuestTitleKey;                             // an open quest from this villager, if any
    }

    public static class NeighbourJournal
    {
        static readonly Regex HeartEvent = new Regex(@"^(?<npc>[a-z]+)_heart(?<n>\d+)$", RegexOptions.Compiled);

        public static NeighbourInfo Build(NpcDefinition npc, GameState state, StoryContent story, InteractionState interactions)
        {
            var info = new NeighbourInfo { Id = npc.Id };
            var npcState = state.Npcs.TryGetValue(npc.Id, out var s) ? s : null;
            info.Met = npcState != null && npcState.Met;
            info.Hearts = npcState != null ? FriendshipModel.Hearts(npcState.Points) : 0;
            info.Stage = RelationshipStages.For(info.Hearts, state.Flags.Contains("partners." + npc.Id));
            if (!info.Met) return info;

            bool Given(string item) => interactions != null && interactions.GiftDay.ContainsKey($"{npc.Id}|{item}");
            info.Loves = npc.Loved.Where(Given).ToList();
            info.Likes = npc.Liked.Where(Given).ToList();
            info.Dislikes = npc.Disliked.Where(Given).ToList();

            if (interactions != null)
                info.Told = story.Topics.Where(t => t.Npc == npc.Id && interactions.TopicTimes.TryGetValue(t.Id, out var n) && n > 0)
                    .OrderBy(t => t.Id, StringComparer.Ordinal).Select(t => t.LabelKey).ToList();

            var next = story.Events.Select(e => (e, m: HeartEvent.Match(e.Id))).Where(x => x.m.Success && x.m.Groups["npc"].Value == npc.Id)
                .Select(x => (id: x.e.Id, n: int.Parse(x.m.Groups["n"].Value))).OrderBy(x => x.n).FirstOrDefault(x => !state.EventsSeen.Contains(x.id));
            if (next.id == null) info.Hint = "hint.done";
            else { info.HintHearts = next.n; info.Hint = info.Hearts >= next.n ? "hint.ready" : "hint.at"; }

            var open = state.Quests.FirstOrDefault(q => q.Value.Status == "active" && story.Quest(q.Key)?.Giver == npc.Id);
            if (open.Key != null) info.QuestTitleKey = story.Quest(open.Key).TitleKey;
            return info;
        }
    }

    // T-144: the Gossip Book. Which rare and legendary lines each villager has been heard to say (a count and the lines found, never the
    // ones still hidden), and which of this year's village storylines are solved.
    public sealed class GossipEntry
    {
        public string Npc;
        public int Total;
        public List<string> Found = new List<string>();      // dialogue ids
    }

    public sealed class GossipBookData
    {
        public List<GossipEntry> Villagers = new List<GossipEntry>();
        public int StorylinesThisGame, StorylinesSolved;
        public List<string> SolvedTitleKeys = new List<string>();
        public int FoundTotal, RareTotal;
    }

    public static class GossipBook
    {
        public static bool IsRare(DialogueSetEntry e) => e != null && (e.Rarity == "rare" || e.Rarity == "legendary");

        public static GossipBookData Build(IEnumerable<string> villagerIds, StoryContent story, LineMemory memory, ICollection<string> flags)
        {
            var data = new GossipBookData();
            foreach (var id in villagerIds)
            {
                var set = story.Set($"npc.{id}.talk");
                if (set == null) continue;
                var rares = set.Entries.Where(IsRare).Select(e => e.Dialogue).Distinct().ToList();
                if (rares.Count == 0) continue;
                var entry = new GossipEntry { Npc = id, Total = rares.Count, Found = rares.Where(d => memory != null && memory.WasHeard(set.Id, d)).ToList() };
                data.Villagers.Add(entry);
                data.RareTotal += entry.Total;
                data.FoundTotal += entry.Found.Count;
            }
            foreach (var def in story.Storylines)
            {
                if (!flags.Contains(Storylines.FlagPrefix + def.Id)) continue;
                data.StorylinesThisGame++;
                if (flags.Contains("storydone." + def.Id)) { data.StorylinesSolved++; data.SolvedTitleKeys.Add(def.TitleKey); }
            }
            return data;
        }
    }
}
