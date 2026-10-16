using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Spec WP11 (docs/NPC_INTERCONNECTED_GAMEPLAY_SPEC.md): the polish that must already be true of the conversations, checked so that it stays true. Text speed and its stream-mode
    // floor are covered by DialoguePacingTests and StreamModeTests. Pictures that do not exist yet are listed in docs/art_prompts, not drawn as placeholders.
    public class PolishAuditTests
    {
        static readonly string[] Npcs = NarrativeCoverageTests.Npcs;

        static GameDatabase Db() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void EveryVillager_HasAPortraitForEveryExpression_NotTheBasePictureStandingIn()
        {
            var missing = new List<string>();
            foreach (var npc in Db().AllNpcs.Where(n => Npcs.Contains(n.Id)))
                foreach (var expression in NpcDefinition.ExpressionNames.Where(e => e != "neutral"))
                    if (npc.PortraitFor(expression) == npc.PortraitFor(null)) missing.Add($"{npc.Id}: {expression}");
            Assert.IsEmpty(missing, "portraits that fall back to the base picture: " + string.Join(", ", missing));
        }

        [Test]
        public void EveryExpressionTheStoryNames_IsOneTheVillagersHave()
        {
            var story = StoryContent.LoadFromResources();
            var known = new HashSet<string>(NpcDefinition.ExpressionNames);
            var unknown = new List<string>();
            foreach (var d in story.Dialogues)
                foreach (var n in d.Nodes)
                    if (!string.IsNullOrEmpty(n.Expression) && !known.Contains(n.Expression)) unknown.Add($"{d.Id}.{n.Id}: {n.Expression}");
            foreach (var e in story.Events)
                foreach (var step in e.Steps)
                {
                    if (!string.IsNullOrEmpty(step.Expression) && !known.Contains(step.Expression)) unknown.Add($"{e.Id}: {step.Expression}");
                    if (step.Type == "expression" && !string.IsNullOrEmpty(step.Name) && !known.Contains(step.Name)) unknown.Add($"{e.Id}: {step.Name}");
                }
            Assert.IsEmpty(unknown, "expressions with no portrait: " + string.Join("; ", unknown));
        }

        [Test]
        public void EveryVillager_SoundsLikeThemselves_AndHasASignatureOfTheirOwn()
        {
            var profiles = Npcs.Select(n => VoiceProfiles.For(n)).ToList();
            Assert.AreEqual(Npcs.Length, profiles.Select(p => (p.Pitch, p.Wave, p.CharsPerBlip)).Distinct().Count(), "no two villagers share a voice");
            var signatures = Npcs.Select(n => VoiceSynth.Signature(n)).ToList();
            foreach (var (npc, sound) in Npcs.Zip(signatures, (n, s) => (n, s))) Assert.IsTrue(sound != null && sound.Length > 0, npc + " has a signature sound");
            var hashes = signatures.Select(s => s.Take(2000).Aggregate(17, (h, v) => h * 31 + Mathf.RoundToInt(v * 1000f))).Distinct().Count();
            Assert.AreEqual(Npcs.Length, hashes, "no two signatures are the same sound");
        }

        // A heart gained during a conversation is announced when the conversation is over, never over the dialogue box (playtest request, WP11).
        [Test]
        public void ANoticeIsHeldWhileAConversationIsOpen_AndTheTimerDoesNotRunDown()
        {
            Assert.AreEqual(1.5f, HudView.AdvanceToast(1.5f, dialogueOpen: true, dt: 0.5f), "held: the time left is kept");
            Assert.AreEqual(1.0f, HudView.AdvanceToast(1.5f, dialogueOpen: false, dt: 0.5f));
            Assert.AreEqual(0f, HudView.AdvanceToast(0.2f, dialogueOpen: false, dt: 0.5f), "never below nothing");
        }
    }
}
