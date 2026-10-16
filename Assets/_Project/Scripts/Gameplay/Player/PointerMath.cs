using UnityEngine;

namespace Farm.Gameplay
{
    public static class PointerMath
    {
        // Where the mouse aims, with the sensitivity setting applied: how far the pointer is from the player on the screen is multiplied, so that at 2 the tool square follows the mouse
        // twice as eagerly (a pointer half a tile away aims a whole tile away) and at 0.5 half as eagerly. At 1 it is the pointer itself. It is read afresh every frame, so the slider
        // takes effect at once. The cursor the operating system draws is never moved. (pure)
        public static Vector2 AimPoint(Vector2 pointer, Vector2 player, float sensitivity) => player + (pointer - player) * sensitivity;
    }
}
