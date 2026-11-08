using System;
using UnityEngine;

namespace FarmGame2D.World
{
    [Serializable]
    public readonly struct WanderArea
    {
        public WanderArea(Vector2 center, Vector2 halfExtents)
        {
            Center = center;
            HalfExtents = new Vector2(Mathf.Abs(halfExtents.x), Mathf.Abs(halfExtents.y));
        }

        public Vector2 Center { get; }
        public Vector2 HalfExtents { get; }

        public bool Contains(Vector2 point)
        {
            return Mathf.Abs(point.x - Center.x) <= HalfExtents.x
                && Mathf.Abs(point.y - Center.y) <= HalfExtents.y;
        }

        public Vector2 Clamp(Vector2 point)
        {
            return new Vector2(
                Mathf.Clamp(point.x, Center.x - HalfExtents.x, Center.x + HalfExtents.x),
                Mathf.Clamp(point.y, Center.y - HalfExtents.y, Center.y + HalfExtents.y));
        }

        public Vector2 GetPoint(float x01, float y01)
        {
            return new Vector2(
                Mathf.Lerp(Center.x - HalfExtents.x, Center.x + HalfExtents.x, Mathf.Clamp01(x01)),
                Mathf.Lerp(Center.y - HalfExtents.y, Center.y + HalfExtents.y, Mathf.Clamp01(y01)));
        }

        public Vector2 ConstrainStep(Vector2 current, Vector2 next)
        {
            return Contains(current) ? Clamp(next) : next;
        }
    }
}
