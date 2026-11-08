using UnityEngine;

namespace FarmGame2D.Player
{
    public static class PlayerMotionPresentation
    {
        public const float MoveSpeed = 2.8f;
        private const float WalkFramesPerSecond = 6f;
        private static readonly int[] WalkFrames = { 1, 2, 3, 2 };

        public static int GetAnimationFrame(float elapsedTime, bool moving)
        {
            if (!moving)
            {
                return 0;
            }

            var index = Mathf.FloorToInt(elapsedTime * WalkFramesPerSecond) % WalkFrames.Length;
            return WalkFrames[index];
        }

        public static int GetFacingRow(Vector2 movement, int currentRow)
        {
            if (movement.sqrMagnitude <= 0.01f)
            {
                return currentRow;
            }

            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y))
            {
                return movement.x < 0f ? 2 : 1;
            }

            return movement.y > 0f ? 0 : 3;
        }
    }
}
