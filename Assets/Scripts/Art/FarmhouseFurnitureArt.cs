using UnityEngine;

namespace FarmGame2D.Art
{
    internal static class FarmhouseFurnitureArt
    {
        private static Texture2D atlas;
        private static Sprite workbench, sink, range, chest, workshopFloor, kitchenFloor;

        public static Sprite Workbench => workbench ??= Fixture("Oak workbench", new Rect(59, 613, 596, 432), 225f);
        public static Sprite Sink => sink ??= Fixture("Sage sink counter", new Rect(772, 615, 599, 423), 228f);
        public static Sprite Range => range ??= Fixture("Cast iron cooking range", new Rect(171, 50, 371, 481), 292f);
        public static Sprite Chest => chest ??= Fixture("Workshop tool chest", new Rect(861, 119, 419, 288), 300f);

        private static Sprite Fixture(string name, Rect rect, float pixelsPerUnit)
        {
            atlas ??= Resources.Load<Texture2D>("Art/World/farmhouse_furniture_v1");
            var sprite = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.06f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        public static Sprite GetFloor(int width, bool kitchen) => kitchen
            ? kitchenFloor ??= CreateFloor(width, true)
            : workshopFloor ??= CreateFloor(width, false);

        private static Sprite CreateFloor(int width, bool kitchen)
        {
            const int resolution = 256;
            var pixels = new Color32[resolution * resolution];
            for (var y = 0; y < resolution; y++)
            for (var x = 0; x < resolution; x++)
            {
                var column = x * width * 2 / resolution;
                if (kitchen)
                {
                    var tileX = x * width % resolution;
                    var tileY = y * 4 % resolution;
                    var light = (column / 2 + y * 4 / resolution) % 2 == 0;
                    pixels[y * resolution + x] = tileX < 19 || tileY < 9
                        ? new Color32(143, 132, 106, 255)
                        : light ? new Color32(200, 187, 154, 255) : new Color32(157, 166, 128, 255);
                    continue;
                }

                var board = y / 23;
                var joint = (x * width + board * 97) % 384;
                var shade = (board * 11 + (x * width + board * 97) / 384 * 7) % 16;
                var grain = (x * 7 + y * 19) % 23 < 2 ? -4 : 0;
                pixels[y * resolution + x] = y % 23 < 2 || joint < width
                    ? new Color32(136, 91, 60, 255)
                    : new Color32((byte)(181 + shade + grain), (byte)(128 + shade + grain), (byte)(85 + shade / 2), 255);
            }

            var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
            {
                name = kitchen ? "Sage kitchen tiles" : "Staggered workshop oak planks",
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f),
                resolution, 0, SpriteMeshType.FullRect);
        }
    }
}
