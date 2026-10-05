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
            Assert.AreEqual(new Vector3Int(11, 10, 0), AimMath.Target(Me, new Vector3Int(11, 10, 0), Vector2Int.down));
            Assert.AreEqual(new Vector3Int(9, 11, 0), AimMath.Target(Me, new Vector3Int(9, 11, 0), Vector2Int.down));
            Assert.AreEqual(new Vector3Int(11, 9, 0), AimMath.Target(Me, new Vector3Int(11, 9, 0), Vector2Int.up));
        }

        [Test]
        public void ADistantPointer_AimsInItsDirection_NeverFartherThanOneCell()
        {
            Assert.AreEqual(new Vector3Int(11, 11, 0), AimMath.Target(Me, new Vector3Int(30, 25, 0), Vector2Int.down));
            Assert.AreEqual(new Vector3Int(9, 10, 0), AimMath.Target(Me, new Vector3Int(-5, 10, 0), Vector2Int.down));
            Assert.AreEqual(new Vector3Int(10, 9, 0), AimMath.Target(Me, new Vector3Int(10, -40, 0), Vector2Int.up));
        }

        [Test]
        public void ThePointerOverThePlayer_FallsBackToTheFacing()
        {
            Assert.AreEqual(new Vector3Int(10, 11, 0), AimMath.Target(Me, Me, Vector2Int.up));
            Assert.AreEqual(new Vector3Int(9, 10, 0), AimMath.Target(Me, Me, Vector2Int.left));
        }

        [Test]
        public void TheFacing_FollowsTheLongerAxis_AndKeepsItOnATie()
        {
            Assert.AreEqual(Vector2Int.right, AimMath.FacingToward(Me, new Vector3Int(11, 10, 0), Vector2Int.up));
            Assert.AreEqual(Vector2Int.down, AimMath.FacingToward(Me, new Vector3Int(10, 9, 0), Vector2Int.up));
            Assert.AreEqual(Vector2Int.left, AimMath.FacingToward(Me, new Vector3Int(8, 11, 0), Vector2Int.up));
            Assert.AreEqual(Vector2Int.up, AimMath.FacingToward(Me, new Vector3Int(11, 11, 0), Vector2Int.up), "a diagonal keeps the current facing");
        }
    }
}
