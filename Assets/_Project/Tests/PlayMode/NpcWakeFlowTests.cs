using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Waking a sleeper, in the real game: the player talks to Tilda asleep in her bed at night; she gets up, loses a little friendship (a stranger) or none
    // (a friend), says her reaction, and goes back to bed after a while. A story effect wakes her without the cost.
    public class NpcWakeFlowTests
    {
        const string Tilda = "tilda";
        string _dataRoot;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-npcwake-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static GameDateTime At(int minute) => new GameDateTime(1, Season.Spring, 3, minute);

        IEnumerator EnterHome(int hearts)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.Npcs[Tilda] = new NpcState { Met = true, Points = hearts * FriendshipModel.PointsPerHeart };
            _session.Clock.SetTime(At(23 * 60 + 30));
            _session.State.CurrentMap = MapIds.HomeTilda;
            _session.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(MapIds.HomeTilda);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
        }

        NpcActor Actor() => UnityEngine.Object.FindObjectsByType<NpcActor>(FindObjectsSortMode.None).FirstOrDefault(a => a.Definition != null && a.Definition.Id == Tilda);
        int Points => NpcInteractions.StateOf(_session.State, Tilda).Points;

        [UnityTest]
        public IEnumerator TalkingToASleepingStranger_WakesHer_CostsALittleFriendship_AndSheSaysHerReaction()
        {
            yield return EnterHome(1);
            var actor = Actor();
            Assert.IsNotNull(actor);
            Assert.IsTrue(actor.Sleeping, "Tilda is asleep in her bed at half past eleven");
            var before = Points;
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();

            actor.Interact(player);
            for (var i = 0; i < 6; i++) yield return null;

            Assert.IsFalse(actor.Sleeping, "she is up");
            Assert.IsFalse(actor.SleepIconShown);
            Assert.AreEqual(before - NpcWake.PenaltyPoints + FriendshipModel.TalkPoints, Points, "a stranger loses a little (and the first talk of the day earns a little)");
            var reactions = ReactionState.Load(_session);
            var spent = reactions.Pending.FirstOrDefault(p => p.Npc == Tilda && p.Id == "wake." + Tilda);
            Assert.IsNotNull(spent, "her grumpy reaction was queued");
            Assert.GreaterOrEqual(spent.ConsumedDay, 0, "and she said it as soon as she was awake");
            Assert.IsFalse(NpcWake.IsAsleep(_session, _session.Npcs.Get(Tilda)));
        }

        [UnityTest]
        public IEnumerator AFriend_IsNotChargedForWaking_AndGetsTheWarmReaction()
        {
            yield return EnterHome(NpcHomes.FriendHearts);
            var actor = Actor();
            var before = Points;
            actor.Interact(UnityEngine.Object.FindAnyObjectByType<PlayerActions>());
            for (var i = 0; i < 6; i++) yield return null;
            Assert.AreEqual(before + FriendshipModel.TalkPoints, Points, "no cost to a friend");
            var spent = ReactionState.Load(_session).Pending.First(p => p.Npc == Tilda && p.Id == "wake." + Tilda + ".friend");
            Assert.GreaterOrEqual(spent.ConsumedDay, 0, "the warm one was the one said (it outranks the grumpy one)");
        }

        [UnityTest]
        public IEnumerator AStoryEffect_WakesHer_WithoutCost_AndSheGoesBackToBedAfterAWhile()
        {
            yield return EnterHome(1);
            var actor = Actor();
            var before = Points;
            Effects.Run(_session, "wake:" + Tilda);
            for (var i = 0; i < 6; i++) yield return null;
            Assert.IsFalse(actor.Sleeping, "woken by someone else");
            Assert.AreEqual(before, Points, "no friendship cost");
            Assert.IsFalse(NpcWake.Wake(_session, Tilda, true), "she is already up: nothing more to wake");

            _session.Clock.SetTime(At(23 * 60 + 30 + NpcWake.WakeMinutes + 5));
            for (var i = 0; i < 12; i++) yield return null;
            Assert.IsTrue(actor.Sleeping, "after a while she is back in bed");
        }

        [UnityTest]
        public IEnumerator SomeoneWhoIsAwake_CannotBeWoken()
        {
            yield return EnterHome(1);
            _session.Clock.SetTime(At(12 * 60));
            for (var i = 0; i < 6; i++) yield return null;
            Assert.IsFalse(NpcWake.Wake(_session, Tilda, true));
            Assert.AreEqual(1 * FriendshipModel.PointsPerHeart, Points, "nothing was charged");
        }
    }
}
