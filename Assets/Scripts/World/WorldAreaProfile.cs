using UnityEngine;

namespace FarmGame2D.World
{
    public static class WorldAreaProfile
    {
        public const float InteriorStartX = 30f;
        public const float OutdoorCameraSize = 6.75f;
        public const float InteriorCameraSize = 3.35f;
        public const int OutdoorReferenceWidth = 480;
        public const int OutdoorReferenceHeight = 270;
        public const int InteriorReferenceWidth = 400;
        public const int InteriorReferenceHeight = 225;
        public const float HouseInteriorBoundaryX = 47f;
        public const float FarmhouseBedroomBoundaryX = 63f;
        public const float TownHallBoundaryX = 83f;
        public static readonly Vector2 FarmConstructionSite = new(3.05f, -13.15f);
        public static readonly Vector2 FarmhouseInteriorCenter = new(40f, -9.25f);
        public static readonly Vector2 FarmhouseEntranceSpawn = new(40f, -11.2f);
        public static readonly Vector2 FarmhouseBedroomCenter = new(70f, -9.25f);
        public static readonly Vector2 FarmhouseBedroomEntry = new(70f, -11.2f);
        internal static readonly Rect FarmhouseKitchenCounter = new(1.16f, 1.54f, 2.73f, 0.6f);

        public static readonly Vector2 TownHallCenter = new(90f, -9.25f);
        public static readonly Vector2 TownHallEntry = TownHallCenter + new Vector2(0f, -2.82f);
        public static readonly Vector2 TownHallExit = new(0f, 9.68f);

        public static bool IsTownHall(Vector2 position) => position.x >= TownHallBoundaryX;

        public static bool IsFarmhouseBedroom(Vector2 position) =>
            position.x >= FarmhouseBedroomBoundaryX && !IsTownHall(position);

        public static bool NeedsFarmhouseRelocation(float x, float y, int upgradeLevel)
        {
            if (IsFarmhouseBedroom(new Vector2(x, y))) return upgradeLevel == 0;
            if (upgradeLevel >= 2 && OverlapsFarmhouseKitchenCounter(new Vector2(x, y))) return true;
            return x >= InteriorStartX && x < HouseInteriorBoundaryX
                && (x < 36f || x > 44f || y > -6.65f);
        }

        internal static bool OverlapsFarmhouseKitchenCounter(Vector2 playerPosition)
        {
            var local = playerPosition - FarmhouseInteriorCenter;
            // The player's feet collider is 0.5 by 0.32, centered 0.14 units above its pivot.
            return local.x > FarmhouseKitchenCounter.xMin - 0.25f && local.x < FarmhouseKitchenCounter.xMax + 0.25f
                && local.y > FarmhouseKitchenCounter.yMin - 0.30f && local.y < FarmhouseKitchenCounter.yMax + 0.02f;
        }

        public static bool IsInterior(Vector2 position)
        {
            return position.x >= InteriorStartX;
        }
    }
}
