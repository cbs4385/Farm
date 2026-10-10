using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // One table for the shops and the hall (VillageShops): the scene builder, the routes and the map all read it, so it has to make sense.
    public class VillageShopsTests
    {
        [Test]
        public void The_shops_do_not_overlap_each_other_or_the_cottages()
        {
            var rects = VillageShops.All.Select(s => (s.Map, s.Footprint)).ToList();
            rects.AddRange(NpcHomes.All.Select(h => (h.Map, new RectInt(h.X0, h.Y0, h.X1 - h.X0 + 1, h.Y1 - h.Y0 + 1))));
            for (var i = 0; i < rects.Count; i++)
                for (var j = i + 1; j < rects.Count; j++)
                    Assert.IsFalse(rects[i].Footprint.Overlaps(rects[j].Footprint), $"{rects[i].Map} overlaps {rects[j].Map}");
        }

        [Test]
        public void Every_door_is_in_its_footprint_and_inside_the_village()
        {
            foreach (var s in VillageShops.All)
            {
                Assert.GreaterOrEqual(s.DoorX, s.X0, s.Map);
                Assert.LessOrEqual(s.DoorX, s.X1, s.Map);
                Assert.Less(s.Y1, MapLayout.VillageH - WoodLayout.EdgeThickness, s.Map + " keeps clear of the edge band");
                Assert.Less(s.InteriorDoorX, s.InteriorW, s.Map + " has its inside door inside");
                Assert.IsNotNull(VillageShops.For(s.Map));
            }
        }

        [Test]
        public void The_routes_and_the_map_follow_the_table()
        {
            foreach (var s in VillageShops.All)
            {
                Assert.IsTrue(MapRoutes.All.Any(e => e.From == MapIds.Village && e.To == s.Map && e.ExitX == s.DoorX && e.ExitY == s.DoorY), s.Map + " has a route from its door");
                Assert.IsTrue(Farm.UI.WorldMapLayout.VillageBuildingDoors.Any(d => d.map == s.Map && d.x == s.DoorX && d.y == s.DoorY), s.Map + " is on the map at its door");
            }
        }
    }
}
