using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame2D.World
{
    public static class PrototypeSpriteFactory
    {
        public static Sprite CreateSprite(Color color)
        {
            var texture = new Texture2D(1, 1)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Prototype {color}"
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        public static Tile CreateTile(Color color)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = CreateSprite(color);
            tile.colliderType = Tile.ColliderType.None;
            return tile;
        }

        public static GameObject CreateBlock(
            string name,
            Vector2 position,
            Vector2 size,
            Color color,
            Transform parent = null,
            bool collider = false)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSprite(color);

            if (collider)
            {
                gameObject.AddComponent<BoxCollider2D>();
            }

            return gameObject;
        }
    }
}
