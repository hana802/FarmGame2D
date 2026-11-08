using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class MiningNodeDepletionPresentation : MonoBehaviour
    {
        private const float DepletedAlpha = 0.58f;
        private static Sprite crackSprite;
        private SpriteRenderer rockRenderer;
        private SpriteRenderer crackRenderer;

        public bool CracksVisible => crackRenderer != null && crackRenderer.enabled;
        public float RockAlpha => rockRenderer != null ? rockRenderer.color.a : 0f;

        public void Initialize(SpriteRenderer targetRenderer)
        {
            rockRenderer = targetRenderer;
            if (crackRenderer != null)
            {
                return;
            }

            var cracks = new GameObject("Mined Rock Cracks");
            cracks.transform.SetParent(transform, false);
            cracks.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            crackRenderer = cracks.AddComponent<SpriteRenderer>();
            crackRenderer.sprite = GetCrackSprite();
            crackRenderer.sortingOrder = rockRenderer.sortingOrder + 1;
            crackRenderer.enabled = false;
        }

        public void SetState(bool depleted, Color availableColor)
        {
            if (rockRenderer == null)
            {
                return;
            }

            rockRenderer.color = depleted
                ? new Color(0.3f, 0.28f, 0.26f, DepletedAlpha)
                : new Color(availableColor.r, availableColor.g, availableColor.b, 1f);

            if (crackRenderer != null)
            {
                crackRenderer.enabled = depleted;
            }
        }

        private static Sprite GetCrackSprite()
        {
            if (crackSprite != null)
            {
                return crackSprite;
            }

            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Mined Rock Cracks",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            texture.SetPixels32(pixels);
            var crackColor = new Color32(35, 25, 22, 230);

            DrawLine(texture, 16, 23, 14, 18, crackColor);
            DrawLine(texture, 14, 18, 17, 14, crackColor);
            DrawLine(texture, 17, 14, 15, 8, crackColor);
            DrawLine(texture, 14, 18, 9, 15, crackColor);
            DrawLine(texture, 17, 14, 23, 12, crackColor);
            DrawLine(texture, 15, 8, 11, 5, crackColor);
            DrawLine(texture, 16, 23, 21, 20, crackColor);
            texture.Apply();

            crackSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect);
            crackSprite.name = "world_MinedRockCracks";
            return crackSprite;
        }

        private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color32 color)
        {
            var dx = Mathf.Abs(x1 - x0);
            var stepX = x0 < x1 ? 1 : -1;
            var dy = -Mathf.Abs(y1 - y0);
            var stepY = y0 < y1 ? 1 : -1;
            var error = dx + dy;

            while (true)
            {
                texture.SetPixel(x0, y0, color);
                if (x0 == x1 && y0 == y1)
                {
                    return;
                }

                var doubledError = error * 2;
                if (doubledError >= dy)
                {
                    error += dy;
                    x0 += stepX;
                }

                if (doubledError <= dx)
                {
                    error += dx;
                    y0 += stepY;
                }
            }
        }
    }
}
