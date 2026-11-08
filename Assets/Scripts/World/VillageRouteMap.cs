using UnityEngine;

namespace FarmGame2D.World
{
    public static class VillageRouteMap
    {
        public static VillageWaypointNetwork CreateMainStreet()
        {
            var points = new[]
            {
                new Vector2(-3.6f, 3.5f),
                new Vector2(-6f, 3.5f),
                new Vector2(-10f, 5.7f),
                new Vector2(3.6f, 3.5f),
                new Vector2(6f, 3.5f),
                new Vector2(10f, 5.7f),
                new Vector2(0f, 6f),
                new Vector2(-3.6f, -0.8f),
                new Vector2(0f, -0.8f),
                new Vector2(3.6f, -0.8f),
                new Vector2(-6f, 0f),
                new Vector2(6f, 0f),
                new Vector2(-11.5f, 7.2f),
                new Vector2(11.5f, 7.2f),
                new Vector2(-4.4f, 5.6f),
                new Vector2(4.4f, 5.6f)
            };
            var links = new[]
            {
                new[] { 1, 7, 14 },
                new[] { 0, 2, 10 },
                new[] { 1, 12 },
                new[] { 4, 9, 15 },
                new[] { 3, 5, 11 },
                new[] { 4, 13 },
                new[] { 14, 15 },
                new[] { 0, 8, 10 },
                new[] { 7, 9 },
                new[] { 3, 8, 11 },
                new[] { 1, 7 },
                new[] { 4, 9 },
                new[] { 2 },
                new[] { 5 },
                new[] { 0, 6 },
                new[] { 3, 6 }
            };
            return new VillageWaypointNetwork(points, links);
        }
    }
}
