using UnityEngine;

namespace FarmGame2D.Interactions
{
    public static class FishingCastMotion
    {
        public const float ReleaseTime = 0.42f;

        public static float Pullback(float elapsed)
        {
            if (elapsed < 0.24f)
            {
                return Mathf.Lerp(95f, 28f, elapsed / 0.24f);
            }

            if (elapsed < ReleaseTime)
            {
                return Mathf.Lerp(28f, -6f, (elapsed - 0.24f) / (ReleaseTime - 0.24f));
            }

            return -4f;
        }
    }

    public static class FishingCastTarget
    {
        public const float DefaultCastDistance = 0.85f;

        public static Vector2 Resolve(Vector2 fishingSpot, Vector2 player, float distance = DefaultCastDistance)
        {
            var towardWater = fishingSpot - player;
            if (towardWater.sqrMagnitude < 0.0001f)
            {
                towardWater = Vector2.up;
            }

            return fishingSpot + towardWater.normalized * Mathf.Max(0f, distance);
        }
    }
}
