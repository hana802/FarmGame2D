using System.Collections.Generic;
using UnityEngine;

namespace FarmGame2D.Art
{
    public enum WorldArtKind
    {
        FarmHouse,
        TownHall,
        GeneralStore,
        SeedFeedShop,
        WestCottage,
        EastCottage,
        ChickenCoop,
        Chicken,
        Tree,
        Pond,
        VillageSquare
    }

    public static class WorldArtCatalog
    {
        private static readonly Dictionary<WorldArtKind, Sprite> sprites = new();

        public static Sprite GetSprite(WorldArtKind kind)
        {
            if (sprites.TryGetValue(kind, out var cached))
            {
                return cached;
            }

            var definition = GetDefinition(kind);
            var texture = Resources.Load<Texture2D>(definition.Path);
            if (texture == null)
            {
                Debug.LogError($"World art texture not found: {definition.Path}");
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(
                texture,
                definition.Rect,
                definition.Pivot,
                definition.PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = $"world_{kind}";
            sprites[kind] = sprite;
            return sprite;
        }

        private static WorldArtDefinition GetDefinition(WorldArtKind kind)
        {
            return kind switch
            {
                WorldArtKind.FarmHouse => new("Art/World/farm_house_v1", new Rect(366f, 88f, 804f, 828f), 160f),
                WorldArtKind.TownHall => new("Art/World/town_hall_v1", new Rect(168f, 10f, 1200f, 1004f), 200f),
                WorldArtKind.GeneralStore => new("Art/World/general_store_v1", new Rect(161f, 22f, 1216f, 979f), 220f),
                WorldArtKind.SeedFeedShop => new("Art/World/seed_feed_shop_v1", new Rect(230f, 42f, 1073f, 982f), 205f),
                WorldArtKind.WestCottage => new("Art/World/west_cottage_v1", new Rect(285f, 24f, 965f, 978f), 230f),
                WorldArtKind.EastCottage => new("Art/World/east_cottage_v1", new Rect(287f, 35f, 987f, 974f), 230f),
                WorldArtKind.ChickenCoop => new("Art/World/chicken_coop_v1", new Rect(218f, 27f, 1100f, 981f), 300f),
                WorldArtKind.Chicken => new("Art/World/chicken_v1", new Rect(454f, 118f, 624f, 797f), 750f, new Vector2(0.5f, 0.08f)),
                WorldArtKind.Tree => new("Art/World/tree_v1", new Rect(113f, 69f, 1086f, 1072f), 400f),
                WorldArtKind.Pond => new("Art/World/pond_v1", new Rect(207f, 148f, 1122f, 804f), 240f, new Vector2(0.5f, 0.5f)),
                WorldArtKind.VillageSquare => new("Art/World/village_square_v1", new Rect(272f, 52f, 992f, 915f), 170f, new Vector2(0.5f, 0.08f)),
                _ => throw new System.ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        private readonly struct WorldArtDefinition
        {
            public WorldArtDefinition(string path, Rect rect, float pixelsPerUnit, Vector2? pivot = null)
            {
                Path = path;
                Rect = rect;
                PixelsPerUnit = pixelsPerUnit;
                Pivot = pivot ?? new Vector2(0.5f, 0.05f);
            }

            public string Path { get; }
            public Rect Rect { get; }
            public float PixelsPerUnit { get; }
            public Vector2 Pivot { get; }
        }
    }
}
