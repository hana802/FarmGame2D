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
        public static readonly Vector2 FarmConstructionSite = new(3.05f, -13.15f);
        public static readonly Vector2 FarmhouseInteriorCenter = new(40f, -9.25f);
        public static readonly Vector2 FarmhouseEntranceSpawn = new(40f, -11.2f);
        public static readonly Vector2 FarmhouseBedroomCenter = new(70f, -9.25f);
        public static readonly Vector2 FarmhouseBedroomEntry = new(70f, -11.2f);

        public static bool IsFarmhouseBedroom(Vector2 position) => position.x >= FarmhouseBedroomBoundaryX;

        public static bool NeedsFarmhouseRelocation(float x, float y, int upgradeLevel)
        {
            if (x >= FarmhouseBedroomBoundaryX) return upgradeLevel == 0;
            return x >= InteriorStartX && x < HouseInteriorBoundaryX
                && (x < 36f || x > 44f || y > -6.65f);
        }

        public static bool IsInterior(Vector2 position)
        {
            return position.x >= InteriorStartX;
        }
    }
}
