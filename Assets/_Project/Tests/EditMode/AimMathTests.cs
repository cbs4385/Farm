using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Aiming the tool square with the mouse: it stays on the eight cells around the player.
    public class AimMathTests
    {
        static readonly Vector3Int Me = new Vector3Int(10, 10, 0);

        [Test]
        public void ThePointerOverANeighbour_PutsTheSquareThere_IncludingDiagonals()
        {
            Assert.AreEqual(new Vector3Int(11, 10, 0), AimMath.Target(Me, new Vector3Int(11, 10, 0)));
            Assert.AreEqual(new Vector3Int(9, 11, 0), AimMath.Target(Me, new Vector3Int(9, 11, 0)));
            Assert.AreEqual(new Vector3Int(11, 9, 0), AimMath.Target(Me, new Vector3Int(11, 9, 0)));
        }

        [Test]
        public void ADistantPointer_AimsInItsDirection_NeverFartherThanOneCell()
        {
            Assert.AreEqual(new Vector3Int(11, 11, 0), AimMath.Target(Me, new Vector3Int(30, 25, 0)));
            Assert.AreEqual(new Vector3Int(9, 10, 0), AimMath.Target(Me, new Vector3Int(-5, 10, 0)));
            Assert.AreEqual(new Vector3Int(10, 9, 0), AimMath.Target(Me, new Vector3Int(10, -40, 0)));
        }

        [Test]
        public void ThePointerOverThePlayer_AimsAtTheCellUnderThePlayer()
        {
            // Playtest report: the cell under the avatar could not be tilled without walking off it.
            Assert.AreEqual(Me, AimMath.Target(Me, Me));
        }

        [Test]
        public void TheFacing_FollowsTheLongerAxis_AndKeepsItOnATie()
        {
            Assert.AreEqual(Vector2Int.right, AimMath.FacingToward(Me, new Vector3Int(11, 10, 0), Vector2Int.up));
            Assert.AreEqual(Vector2Int.down, AimMath.FacingToward(Me, new Vector3Int(10, 9, 0), Vector2Int.up));
            Assert.AreEqual(Vector2Int.left, AimMath.FacingToward(Me, new Vector3Int(8, 11, 0), Vector2Int.up));
            Assert.AreEqual(Vector2Int.up, AimMath.FacingToward(Me, new Vector3Int(11, 11, 0), Vector2Int.up), "a diagonal keeps the current facing");
            Assert.AreEqual(Vector2Int.left, AimMath.FacingToward(Me, Me, Vector2Int.left), "the cell under the player keeps the facing");
        }
    }
}
