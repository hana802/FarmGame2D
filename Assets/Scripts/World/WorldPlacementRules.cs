using UnityEngine;

namespace FarmGame2D.World
{
    public static class WorldPlacementRules
    {
        private static readonly Bounds[] ReservedAreas =
        {
            new(new Vector3(-12f, -2f), new Vector3(4f, 4f)),
            new(new Vector3(-11f, 2f), new Vector3(7f, 5f)),
            new(new Vector3(0f, 1.2f), new Vector3(10f, 9f)),
            new(new Vector3(0f, 11f), new Vector3(8f, 6f)),
            new(new Vector3(-5.8f, 6.7f), new Vector3(6f, 5f)),
            new(new Vector3(5.8f, 6.7f), new Vector3(6f, 5f)),
            new(new Vector3(-11.5f, 8.2f), new Vector3(5.5f, 5f)),
            new(new Vector3(11.5f, 8.2f), new Vector3(5.5f, 5f)),
            // Includes the full tree canopy, not only its root position, around every mining rock.
            new(new Vector3(-13.25f, -10.6f), new Vector3(8.2f, 8.2f)),
            // Keeps every chicken and cow roaming point clear of trunks and overhanging canopies.
            new(new Vector3(-7.8f, -12f), new Vector3(11.6f, 6.5f)),
            new(new Vector3(-1f, -8.4f), new Vector3(9f, 6f))
        };

        public static bool CanPlaceTree(Vector2 position)
        {
            if (Mathf.Abs(position.x) < 3f || (position.y < -6f && position.x > -7f && position.x < 11f))
            {
                return false;
            }

            foreach (var area in ReservedAreas)
            {
                if (area.Contains(position))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
