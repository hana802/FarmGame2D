using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Art
{
    public static class GameArtCatalog
    {
        private const int CellWidth = 192;
        private const int CellHeight = 256;
        private const float PixelsPerUnit = 160f;

        private static readonly Dictionary<int, Sprite> playerSprites = new();
        private static readonly Dictionary<int, Sprite> npcSprites = new();
        private static Texture2D playerSheet;
        private static Texture2D npcSheet;
        private static Texture2D uiAtlas;

        public static Texture2D UiAtlas => uiAtlas ??= LoadTexture("Art/UI/dialogue_info_ui_concept_v1");

        public static Sprite GetPlayerSprite(CharacterChoice choice, int rowFromBottom, int frame)
        {
            if (choice == CharacterChoice.None)
            {
                return null;
            }

            playerSheet ??= LoadTexture("Art/Characters/player_pair_spritesheet_concept_v1");
            if (playerSheet == null)
            {
                return null;
            }

            var characterOffset = choice == CharacterChoice.Female ? 4 : 0;
            var column = characterOffset + Mathf.Clamp(frame, 0, 3);
            var row = Mathf.Clamp(rowFromBottom, 0, 3);
            var key = ((int)choice * 100) + (row * 10) + frame;
            return GetOrCreate(playerSprites, key, playerSheet, column, row, new Vector2(0.5f, 0.12f));
        }

        public static Sprite GetNpcSprite(int characterRowFromTop, int column = 0)
        {
            npcSheet ??= LoadTexture("Art/NPCs/village_npcs_spritesheet_concept_v1");
            if (npcSheet == null)
            {
                return null;
            }

            var rowFromBottom = 3 - Mathf.Clamp(characterRowFromTop, 0, 3);
            var safeColumn = Mathf.Clamp(column, 0, 7);
            var key = (rowFromBottom * 10) + safeColumn;
            return GetOrCreate(npcSprites, key, npcSheet, safeColumn, rowFromBottom, new Vector2(0.5f, 0.12f));
        }

        private static Sprite GetOrCreate(
            IDictionary<int, Sprite> cache,
            int key,
            Texture2D texture,
            int column,
            int row,
            Vector2 pivot)
        {
            if (cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var rect = new Rect(column * CellWidth, row * CellHeight, CellWidth, CellHeight);
            var sprite = Sprite.Create(texture, rect, pivot, PixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = $"{texture.name}_{column}_{row}";
            cache[key] = sprite;
            return sprite;
        }

        private static Texture2D LoadTexture(string resourcePath)
        {
            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogError($"Art texture not found in Resources: {resourcePath}");
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
    }
}
