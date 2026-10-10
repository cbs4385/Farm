using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farm.Tests
{
    // Technical-debt review 2026-10-09: adding a BoxCollider2D to an object that already has a picture fits the collider to the picture, so props blocked cells beside
    // and above the ones they stand on (the clock tower's was a cell and a half too high). Every solid box in the built scenes sits on its object's own cell.
    public class ColliderPlacementTests
    {
        static readonly string[] Outdoor = { MapIds.Farm, MapIds.Village, MapIds.Forest, MapIds.Beach, MapIds.Mine };

        [TestCaseSource(nameof(Outdoor))]
        public void Solid_boxes_sit_on_the_cell_of_their_object(string map)
        {
            EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{map}.unity");
            var off = new List<string>();
            foreach (var box in Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
            {
                if (box.isTrigger || box.GetComponent<SpriteRenderer>() == null || box.GetComponent<PlayerController>() != null) continue;      // the player's own box is set by hand
                if (box.offset.sqrMagnitude > 0.0004f) off.Add($"{box.name} offset {box.offset}");
            }
            Assert.IsEmpty(off, string.Join("\n", off.Take(15)));
        }
    }
}
