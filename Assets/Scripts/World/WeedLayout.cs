using System.Collections.Generic;
using UnityEngine;

namespace FarmGame2D.World
{
    internal static class WeedLayout
    {
        public const int FarmWeedCount = 4;

        public static IReadOnlyList<Vector2> Positions { get; } = new Vector2[]
        {
            new(2.5f, -9.5f), new(-10f, -5.5f), new(13.5f, -10.5f), new(14f, -3.5f),
            new(-14.5f, 12f), new(14.5f, 14f), new(-15f, 3f), new(12.5f, 2f)
        };
    }
}
