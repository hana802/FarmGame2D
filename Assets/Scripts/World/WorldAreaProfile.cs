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
        public const float HouseExpansionDepth = 3.4f;
        public const float HouseMainWidth = 8f;
        public const float HouseWorkshopWidth = 12f;
        public const float HouseKitchenWidth = 14f;
        public const float HouseInteriorBoundaryX = 47f;
        public static readonly Vector2 FarmConstructionSite = new(3.05f, -13.15f);
        public static readonly Vector2 FarmhouseInteriorCenter = new(40f, -9.25f);

        public static float HouseRoomWidth(int level) => level switch
        {
            2 => HouseKitchenWidth,
            1 => HouseWorkshopWidth,
            _ => HouseMainWidth
        };

        public static bool IsInterior(Vector2 position)
        {
            return position.x >= InteriorStartX;
        }
    }
}
