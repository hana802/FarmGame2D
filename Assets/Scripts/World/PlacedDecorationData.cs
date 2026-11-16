using System;
using FarmGame2D.Core;

namespace FarmGame2D.World
{
    [Serializable]
    public sealed class PlacedDecorationData
    {
        public GameItem item;
        public float worldX;
        public float worldY;
    }
}
