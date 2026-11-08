using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class DogHeartEffect : MonoBehaviour
    {
        private const float Lifetime = 1.25f;
        private static Sprite heartSprite;

        private SpriteRenderer heartRenderer;
        private float delay;
        private float elapsed;
        private float drift;

        public static void Spawn(Vector2 position)
        {
            for (var index = 0; index < 3; index++)
            {
                var heart = new GameObject($"Dog Heart {index + 1}").AddComponent<DogHeartEffect>();
                heart.transform.position = position + new Vector2((index - 1) * 0.34f, 0.82f + (index * 0.08f));
                heart.delay = index * 0.12f;
                heart.drift = (index - 1) * 0.14f;
            }
        }

        private void Awake()
        {
            heartRenderer = gameObject.AddComponent<SpriteRenderer>();
            heartRenderer.sprite = GetHeartSprite();
            heartRenderer.sortingOrder = 2200;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed < delay)
            {
                heartRenderer.color = new Color(1f, 1f, 1f, 0f);
                return;
            }

            var progress = Mathf.Clamp01((elapsed - delay) / Lifetime);
            transform.position += new Vector3(drift, 0.7f, 0f) * Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.15f, Mathf.Sin(progress * Mathf.PI));
            heartRenderer.color = new Color(1f, 0.35f, 0.48f, 1f - progress);
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private static Sprite GetHeartSprite()
        {
            if (heartSprite != null)
            {
                return heartSprite;
            }

            const int size = 8;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "dog_heart_pixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var transparent = new Color32(0, 0, 0, 0);
            var pink = new Color32(255, 92, 122, 255);
            var rows = new[]
            {
                "00011000", "00111100", "01111110", "11111111",
                "11111111", "11011011", "10000001", "00000000"
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    pixels[y * size + x] = rows[y][x] == '1' ? pink : transparent;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            heartSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 18f);
            heartSprite.name = "dog_heart_pixel";
            return heartSprite;
        }
    }
}
