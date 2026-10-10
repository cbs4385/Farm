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
    // Furniture in the real game (playtest request, 2026-10-04): set a rug and a bookshelf down in the farmhouse, pick one up with the interact
    // key, and never block a doorway.
    public class DecorFlowTests : PlayModeFixture
    {
        GameSession _session;
        PlayerActions _actions;
        PlayerController _player;
        FarmMap _map;

        IEnumerator Enter(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.Backpack.Add("machine.rug", 1);
            _session.Backpack.Add("machine.bookshelf", 2);
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            _actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            _map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            Assert.IsNotNull(_actions); Assert.IsNotNull(_player); Assert.IsNotNull(_map);
        }

        void Select(string itemId)
        {
            for (var i = 0; i < _session.Backpack.Capacity; i++)
                if (_session.Backpack.Get(i)?.ItemId == itemId) { _session.State.SelectedHotbar = i; return; }
            Assert.Fail(itemId + " is not in the pack");
        }

        // Stands the player on a free floor cell, well away from every door, facing a free cell to the right.
        void StandInTheOpen()
        {
            var doors = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None)
                .Select(w => _map.WorldToCell(w.GetComponent<Collider2D>().bounds.center)).ToList();
            var b = _map.Ground.cellBounds;
            for (var y = b.yMax - 1; y >= b.yMin; y--)
                for (var x = b.xMin; x < b.xMax - 1; x++)
                {
                    var me = new Vector3Int(x, y, 0);
                    var ahead = me + Vector3Int.right;
                    if (!_map.CanPlaceAt(me) || !_map.CanPlaceAt(ahead)) continue;
                    if (DecorRules.BlocksDoor(me, false, doors) || DecorRules.BlocksDoor(ahead, false, doors)) continue;
                    _player.Teleport(_map.CellCenter(me) + new Vector3(0f, -0.25f, 0f));
                    _player.Face(Vector2Int.right);
                    return;
                }
            Assert.Fail("no open floor in the room");
        }

        [UnityTest]
        public IEnumerator InTheFarmhouse_AShelfAndARugCanBeSetDown_AndTheShelfPickedUpAgain()
        {
            yield return Enter(MapIds.FarmHouse);
            StandInTheOpen();
            yield return null;
            var objects = _session.GetObjects(MapIds.FarmHouse);

            Select("machine.bookshelf");
            var cell = _actions.Target;
            _actions.UseSelected();
            yield return null;
            Assert.IsTrue(objects.Has(cell.x, cell.y), "the bookshelf stands on the target cell");
            Assert.AreEqual(1, _session.Backpack.Count("machine.bookshelf"), "one left in the pack");
            var actor = UnityEngine.Object.FindObjectsByType<PlacedObjectActor>(FindObjectsSortMode.None).FirstOrDefault(a => a.Definition.Id == "bookshelf");
            Assert.IsNotNull(actor, "it is drawn");
            Assert.IsNotNull(actor.GetComponent<SpriteRenderer>().sprite, "with a sprite");
            Assert.IsFalse(actor.GetComponent<Collider2D>().isTrigger, "a bookshelf is solid");
            Assert.AreEqual(L.Get("item.machine.bookshelf.name"), actor.HoverLabel, "and the mouse names it");

            actor.Interact(_actions);                        // the interact key picks it up
            yield return null;
            Assert.IsFalse(objects.Has(cell.x, cell.y), "picked up");
            Assert.AreEqual(2, _session.Backpack.Count("machine.bookshelf"));

            // A rug can be walked over, and lies under the player.
            Select("machine.rug");
            var rugCell = _actions.Target;
            _actions.UseSelected();
            yield return null;
            var rug = UnityEngine.Object.FindObjectsByType<PlacedObjectActor>(FindObjectsSortMode.None).FirstOrDefault(a => a.Definition.Id == "rug");
            Assert.IsNotNull(rug);
            Assert.IsTrue(rug.GetComponent<Collider2D>().isTrigger, "a rug can be walked over");
            Assert.IsTrue(objects.Has(rugCell.x, rugCell.y));
        }

        [UnityTest]
        public IEnumerator Furniture_CannotBeSetInADoorway()
        {
            yield return Enter(MapIds.FarmHouse);
            var warp = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First();
            var door = _map.WorldToCell(warp.GetComponent<Collider2D>().bounds.center);
            // Stand one cell inside, facing the door.
            var inside = door + Vector3Int.up;
            _player.Teleport(_map.CellCenter(inside) + new Vector3(0f, -0.25f, 0f));
            _player.Face(Vector2Int.down);
            yield return null;
            Select("machine.bookshelf");
            Assert.AreEqual(door, _actions.Target, "the square is on the door");
            _actions.UseSelected();
            yield return null;
            Assert.IsFalse(_session.GetObjects(MapIds.FarmHouse).Has(door.x, door.y), "nothing stands in the doorway");
            Assert.AreEqual(2, _session.Backpack.Count("machine.bookshelf"), "the shelf is still in the pack");
        }

        [UnityTest]
        public IEnumerator InTheVillage_FurnitureIsRefused()
        {
            yield return Enter(MapIds.Village);
            Select("machine.bookshelf");
            var cell = _actions.Target;
            _actions.UseSelected();
            yield return null;
            Assert.IsFalse(_session.GetObjects(MapIds.Village).Has(cell.x, cell.y));
            Assert.AreEqual(2, _session.Backpack.Count("machine.bookshelf"));
        }
    }
}
