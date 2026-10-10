using System.Linq;
using Farm.Editor;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Owner, 2026-10-09: every building's door faces south, like every interior's door on its south wall.
    public class BuildingFacingTests
    {
        [Test]
        public void Every_village_building_faces_south()
        {
            var facings = MapBuilder.BuildingFacings();
            Assert.GreaterOrEqual(facings.Count, 19);
            Assert.IsEmpty(facings.Where(f => !f.facesSouth).Select(f => f.map).ToList(), "buildings whose door does not face south");
        }

        [Test]
        public void Every_cottage_faces_south()
        {
            Assert.IsEmpty(NpcHomes.All.Where(h => !h.FacesSouth).Select(h => h.Npc).ToList());
        }
    }
}
