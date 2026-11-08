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
        public static readonly Vector2 FarmhouseInteriorCenter = new(40f, -9.25f);

        public static bool IsInterior(Vector2 position)
        {
            return position.x >= InteriorStartX;
        }
    }
}
