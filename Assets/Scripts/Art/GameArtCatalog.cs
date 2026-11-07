using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Art
{
    public static class GameArtCatalog
    {
        private const int PlayerFrameWidth = 160;
        private const int PlayerFrameHeight = 208;
        private const float PixelsPerUnit = 160f;

        private static readonly int[] CharacterRowBottoms = { 64, 272, 496, 712 };
        private static readonly int[] MaleFrameLefts = { 89, 243, 399, 555 };
        private static readonly int[] FemaleFrameLefts = { 824, 978, 1134, 1289 };
        private static readonly int[] NpcFrameLefts = { 80, 240, 400, 560, 816, 976, 1136, 1296 };

        private static readonly Dictionary<int, Sprite> playerSprites = new();
        private static readonly Dictionary<int, Sprite> npcSprites = new();
        private static readonly Dictionary<int, Sprite> npcPortraits = new();
        private static readonly Dictionary<FarmTool, Sprite> heldToolSprites = new();
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

            var safeFrame = Mathf.Clamp(frame, 0, 3);
            var row = Mathf.Clamp(rowFromBottom, 0, 3);
            var key = ((int)choice * 100) + (row * 10) + safeFrame;
            var frameLefts = choice == CharacterChoice.Female ? FemaleFrameLefts : MaleFrameLefts;
            var rect = new Rect(frameLefts[safeFrame], CharacterRowBottoms[row], PlayerFrameWidth, PlayerFrameHeight);
            return GetOrCreate(playerSprites, key, playerSheet, rect, new Vector2(0.5f, 0.12f));
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
            var rect = new Rect(NpcFrameLefts[safeColumn], CharacterRowBottoms[rowFromBottom], PlayerFrameWidth, PlayerFrameHeight);
            return GetOrCreate(npcSprites, key, npcSheet, rect, new Vector2(0.5f, 0.12f));
        }

        public static Sprite GetNpcPortrait(int characterRowFromTop)
        {
            npcSheet ??= LoadTexture("Art/NPCs/village_npcs_spritesheet_concept_v1");
            if (npcSheet == null)
            {
                return null;
            }

            var rowFromBottom = 3 - Mathf.Clamp(characterRowFromTop, 0, 3);
            if (npcPortraits.TryGetValue(rowFromBottom, out var cached))
            {
                return cached;
            }

            var rect = new Rect(96f, CharacterRowBottoms[rowFromBottom] + 76f, 128f, 128f);
            var sprite = Sprite.Create(npcSheet, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = $"npc_portrait_{characterRowFromTop}";
            npcPortraits[rowFromBottom] = sprite;
            return sprite;
        }

        public static Sprite GetHeldToolSprite(FarmTool tool)
        {
            if (heldToolSprites.TryGetValue(tool, out var cached))
            {
                return cached;
            }

            var (path, rect) = tool switch
            {
                FarmTool.Hoe => ("Art/Items/held_hoe_v1", new Rect(528f, 269f, 513f, 467f)),
                FarmTool.Seeds => ("Art/Items/held_seeds_v1", new Rect(571f, 292f, 360f, 425f)),
                FarmTool.WateringCan => ("Art/Items/held_watering_can_v1", new Rect(547f, 324f, 441f, 357f)),
                _ => (string.Empty, default)
            };

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var texture = LoadTexture(path);
            if (texture == null)
            {
                return null;
            }

            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 420f, 0, SpriteMeshType.FullRect);
            sprite.name = $"held_{tool}";
            heldToolSprites[tool] = sprite;
            return sprite;
        }

        private static Sprite GetOrCreate(
            IDictionary<int, Sprite> cache,
            int key,
            Texture2D texture,
            Rect rect,
            Vector2 pivot)
        {
            if (cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var sprite = Sprite.Create(texture, rect, pivot, PixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = $"{texture.name}_{key}";
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
