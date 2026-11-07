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
    }
}
