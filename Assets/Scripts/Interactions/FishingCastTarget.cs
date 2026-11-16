using UnityEngine;

namespace FarmGame2D.Interactions
{
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
