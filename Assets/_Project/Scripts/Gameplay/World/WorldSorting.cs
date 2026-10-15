using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Gameplay
{
    // How the outdoor maps decide what is drawn over what. Things that stand on the ground (the farmer, villagers, trees, rocks) share one sorting order and are put in order by
    // height on the screen: the one lower down is nearer and drawn over the one above it (the renderer's transparency sort axis, Renderer2D.asset, set by Axis). Playtest 2026-10-15:
    // "the trees in the forest still look strange" - the trees were tiles at order 2 (always under the farmer) and pictures at 11 (always over him), with no height order between them.
    public static class WorldSorting
    {
        public const int ActorOrder = 10;               // the farmer, villagers, trees and the other things that stand on the ground
        public const int TallOrder = 11;                // tall props and buildings, which cover whoever stands behind them (BuildingDepth moves these)

        // The transparency sort axis: up the screen is away (drawn first), with a hair of x so that two things at the same height always come out in the same order.
        public static readonly Vector3 Axis = new Vector3(0.001f, 1f, 0f);

        // The distance along the axis: the larger, the further away, the earlier it is drawn.
        public static float Depth(Vector3 position) => position.x * Axis.x + position.y * Axis.y;

        // Puts a map in this order whatever its scene was built with (scenes made before 2026-10-15 have the nodes at order 2): the nodes (trees, rocks, stumps) are drawn one tile at a
        // time at the actors' order, and the camera sorts by Axis. The scene's own trees (the edge band) are pictures at the same order (ObjectSway, MapBuilder).
        public static void Apply(FarmMap map, Camera camera)
        {
            if (map != null && map.Nodes != null && map.Nodes.TryGetComponent<TilemapRenderer>(out var renderer))
            {
                renderer.mode = TilemapRenderer.Mode.Individual;
                renderer.sortingOrder = ActorOrder;
            }
            if (camera == null) return;
            camera.transparencySortMode = TransparencySortMode.CustomAxis;
            camera.transparencySortAxis = Axis;
        }

        public static bool DrawnOver(Vector3 front, Vector3 back) => Depth(front) < Depth(back);
    }
}
