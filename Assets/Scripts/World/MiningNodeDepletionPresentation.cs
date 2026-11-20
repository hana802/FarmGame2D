using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class MiningNodeDepletionPresentation : MonoBehaviour
    {
        private const float DepletedAlpha = 0.58f;
        private static readonly Vector2[] ChipOffsets =
        {
            new(-0.39f, 0.12f), new(-0.23f, 0.48f), new(-0.07f, 0.34f),
            new(0.15f, 0.54f), new(0.32f, 0.23f), new(0.43f, 0.04f)
        };
        private static Sprite crackSprite;
        private static Sprite chipSprite;
        private SpriteRenderer rockRenderer;
        private SpriteRenderer crackRenderer;
        private SpriteRenderer[] chips;

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

        public void PresentImpact(Vector2 point, Color oreColor, float progress)
        {
            if (chips == null) CreateChips();

            for (var i = 0; i < chips.Length; i++)
            {
                var chip = chips[i];
                var offset = ChipOffsets[i] * (0.12f + progress)
                    + Vector2.up * (Mathf.Sin(progress * Mathf.PI) * 0.19f);
                chip.transform.position = point + offset;
                chip.transform.rotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 1f : -1f) * (18f + 180f * progress));
                var color = Color.Lerp(i % 2 == 0 ? new Color(0.91f, 0.84f, 0.57f) : oreColor,
                    new Color(0.36f, 0.29f, 0.24f), progress);
                color.a = 1f - progress;
                chip.color = color;
                chip.enabled = progress < 1f;
            }
        }

        public void HideImpact()
        {
            if (chips == null) return;
            foreach (var chip in chips) chip.enabled = false;
        }

        private void CreateChips()
        {
            var root = new GameObject("Mining Impact Chips").transform;
            root.SetParent(transform, false);
            chipSprite ??= PrototypeSpriteFactory.CreateSprite(Color.white);
            chips = new SpriteRenderer[ChipOffsets.Length];
            for (var i = 0; i < chips.Length; i++)
            {
                var chip = new GameObject("Stone Chip").AddComponent<SpriteRenderer>();
                chip.transform.SetParent(root, false);
                chip.transform.localScale = Vector3.one * (i % 2 == 0 ? 0.082f : 0.052f);
                chip.sprite = chipSprite;
                chip.sortingOrder = rockRenderer.sortingOrder + 2;
                chips[i] = chip;
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
