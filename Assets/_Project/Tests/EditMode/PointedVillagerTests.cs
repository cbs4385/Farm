using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-11: "the crafted items need to be bigger" and "I can only click on Tilda from the side and not the front".
    public class PointedVillagerTests
    {
        static Vector3Int Cell(Vector3 p) => new Vector3Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), 0);

        static NpcActor Villager(Vector3 at)
        {
            var go = new GameObject("npc", typeof(SpriteRenderer));
            go.transform.position = at;
            return go.AddComponent<NpcActor>();
        }

        [Test]
        public void TheVillagersPicture_IsTwoCellsTall_AndReachesAcrossACounter()
        {
            var tilda = Villager(new Vector3(10.5f, 10.5f, 0f));            // stands on cell (10, 10); her head is in (10, 11)
            var list = new[] { tilda };
            var player = new Vector3Int(10, 8, 0);                          // two cells in front of her, a counter between
            Assert.AreSame(tilda, PlayerActions.PointedVillager(player, new Vector3Int(10, 10, 0), list, Cell), "her feet");
            Assert.AreSame(tilda, PlayerActions.PointedVillager(player, new Vector3Int(10, 11, 0), list, Cell), "her head");
            Assert.IsNull(PlayerActions.PointedVillager(player, new Vector3Int(10, 12, 0), list, Cell), "above her head is not her");
            Assert.IsNull(PlayerActions.PointedVillager(player, new Vector3Int(11, 10, 0), list, Cell), "nor is the next cell");
            Assert.IsNull(PlayerActions.PointedVillager(new Vector3Int(10, 5, 0), new Vector3Int(10, 10, 0), list, Cell), "too far from the player");
            Assert.AreSame(tilda, PlayerActions.PointedVillager(new Vector3Int(9, 10, 0), new Vector3Int(10, 11, 0), list, Cell), "and from the side");
            Object.DestroyImmediate(tilda.gameObject);
        }

        [Test]
        public void Machines_AreDrawnBigger_ButChestsSprinklersAndFurnitureAreNot()
        {
            Assert.AreEqual(1.5f, PlacedObjectActor.ScaleFor(PlaceableKind.Machine));
            Assert.AreEqual(1.5f, PlacedObjectActor.ScaleFor(PlaceableKind.Scarecrow));
            Assert.AreEqual(1f, PlacedObjectActor.ScaleFor(PlaceableKind.Chest));
            Assert.AreEqual(1f, PlacedObjectActor.ScaleFor(PlaceableKind.Sprinkler));
            Assert.AreEqual(1f, PlacedObjectActor.ScaleFor(PlaceableKind.Decor));
        }
    }
}
