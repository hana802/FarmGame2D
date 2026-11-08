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
        private static readonly Dictionary<CharacterChoice, Sprite> playerPortraits = new();
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

        public static Sprite GetPlayerPortrait(CharacterChoice choice)
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

            if (playerPortraits.TryGetValue(choice, out var cached))
            {
                return cached;
            }

            var frameLefts = choice == CharacterChoice.Female ? FemaleFrameLefts : MaleFrameLefts;
            var rect = new Rect(frameLefts[0] + 16f, CharacterRowBottoms[3] + 76f, 128f, 128f);
            var portrait = Sprite.Create(playerSheet, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            portrait.name = $"player_portrait_{choice}";
            playerPortraits[choice] = portrait;
            return portrait;
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
            var dedicatedPortrait = GetDedicatedNpcPortrait(characterRowFromTop);
            if (dedicatedPortrait != null)
            {
                return dedicatedPortrait;
            }

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

        private static Sprite GetDedicatedNpcPortrait(int portraitId)
        {
            var resourcePath = portraitId switch
            {
                4 => "Art/NPCs/romance_minseo_portrait_v1",
                5 => "Art/NPCs/romance_taeo_portrait_v1",
                _ => null
            };
            if (resourcePath == null)
            {
                return null;
            }

            var cacheKey = 100 + portraitId;
            if (npcPortraits.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var texture = LoadTexture(resourcePath);
            if (texture == null)
            {
                return null;
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width,
                0,
                SpriteMeshType.FullRect);
            sprite.name = portraitId == 4 ? "npc_portrait_minseo" : "npc_portrait_taeo";
            npcPortraits[cacheKey] = sprite;
            return sprite;
        }

        public static Sprite GetHeldToolSprite(FarmTool tool)
        {
            if (heldToolSprites.TryGetValue(tool, out var cached))
            {
                return cached;
            }

            if (tool == FarmTool.FishingRod)
            {
                var fishingRod = CreateFishingRodSprite();
                heldToolSprites[tool] = fishingRod;
                return fishingRod;
            }

            if (tool == FarmTool.Shovel)
            {
                var shovel = CreateShovelSprite();
                heldToolSprites[tool] = shovel;
                return shovel;
            }

            var (path, rect) = tool switch
            {
                FarmTool.Hoe => ("Art/Items/held_hoe_v1", new Rect(528f, 269f, 513f, 467f)),
                FarmTool.Seeds => ("Art/Items/held_seeds_v1", new Rect(571f, 292f, 360f, 425f)),
                FarmTool.WateringCan => ("Art/Items/held_watering_can_v1", new Rect(547f, 324f, 441f, 357f)),
                FarmTool.Pickaxe => ("Art/Items/held_hoe_v1", new Rect(528f, 269f, 513f, 467f)),
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

        private static Sprite CreateFishingRodSprite()
        {
            var texture = new Texture2D(12, 20)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "held_fishing_rod_v1"
            };
            var clear = new Color(0f, 0f, 0f, 0f);
            var pixels = new Color[12 * 20];
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = clear;
            }
            texture.SetPixels(pixels);
            for (var y = 2; y < 18; y++)
            {
                var x = Mathf.Clamp(2 + (y / 4), 2, 8);
                texture.SetPixel(x, y, new Color(0.42f, 0.22f, 0.08f));
            }
            for (var y = 4; y < 18; y++)
            {
                texture.SetPixel(9, y, new Color(0.82f, 0.86f, 0.8f));
            }
            texture.SetPixel(9, 3, new Color(0.92f, 0.24f, 0.18f));
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 12, 20), new Vector2(0.35f, 0.2f), 16f, 0, SpriteMeshType.FullRect);
        }

        private static Sprite CreateShovelSprite()
        {
            var texture = new Texture2D(12, 20)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "held_shovel_v1"
            };
            var pixels = new Color[12 * 20];
            texture.SetPixels(pixels);

            var outline = new Color(0.16f, 0.1f, 0.08f);
            var wood = new Color(0.55f, 0.29f, 0.11f);
            var metal = new Color(0.65f, 0.7f, 0.67f);
            for (var y = 7; y < 17; y++)
            {
                texture.SetPixel(5, y, outline);
                texture.SetPixel(6, y, wood);
            }

            for (var x = 4; x <= 7; x++)
            {
                texture.SetPixel(x, 17, outline);
                texture.SetPixel(x, 18, wood);
            }

            for (var y = 2; y <= 6; y++)
            {
                var inset = y == 2 ? 1 : 0;
                for (var x = 3 + inset; x <= 8 - inset; x++)
                {
                    texture.SetPixel(x, y, x == 3 + inset || x == 8 - inset ? outline : metal);
                }
            }

            texture.SetPixel(4, 1, outline);
            texture.SetPixel(5, 1, metal);
            texture.SetPixel(6, 1, metal);
            texture.SetPixel(7, 1, outline);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 12, 20), new Vector2(0.5f, 0.2f), 16f, 0, SpriteMeshType.FullRect);
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
