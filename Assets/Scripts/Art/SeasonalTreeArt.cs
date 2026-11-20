using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Art
{
    internal sealed class SeasonalTreeArt
    {
        // On tree_v1 the lowest canopy pixels end above the plants at the foot of the trunk.
        private const int CanopyBottom = 365;
        private const float BarkGreenRatio = 0.76f;
        private const float LeafGreenRatio = 0.83f;
        private Color32[] sourcePixels;
        private Color32[] seasonalPixels;
        private Texture2D texture;
        private Season? displayedSeason;

        public Sprite Sprite { get; private set; }

        public bool Refresh(Season season)
        {
            if (displayedSeason == season) return false;
            if (texture == null && !Initialize()) return false;

            var (shadow, midtone, highlight) = season switch
            {
                Season.Spring => (new Color32(58, 39, 53, 255), new Color32(186, 100, 135, 255), new Color32(255, 196, 216, 255)),
                Season.Summer => (new Color32(33, 51, 31, 255), new Color32(91, 143, 48, 255), new Color32(185, 214, 91, 255)),
                Season.Autumn => (new Color32(65, 42, 26, 255), new Color32(197, 97, 30, 255), new Color32(255, 187, 78, 255)),
                _ => (new Color32(51, 68, 78, 255), new Color32(151, 186, 203, 255), new Color32(246, 251, 255, 255))
            };

            for (var index = texture.width * CanopyBottom; index < sourcePixels.Length; index++)
            {
                var pixel = sourcePixels[index];
                if (pixel.a == 0 || pixel.g <= pixel.r * BarkGreenRatio || pixel.g <= pixel.b)
                    continue;

                // Brown branches are warmer than this range. Blend at the edge of the foliage
                // palette and keep the source luminance pattern and transparency.
                var coverage = Mathf.InverseLerp(pixel.r * BarkGreenRatio, pixel.r * LeafGreenRatio, pixel.g);
                var tone = Mathf.InverseLerp(31f, 207f, pixel.g);
                var foliage = tone < 0.5f
                    ? Color32.Lerp(shadow, midtone, tone * 2f)
                    : Color32.Lerp(midtone, highlight, tone * 2f - 1f);
                foliage.a = pixel.a;
                seasonalPixels[index] = Color32.Lerp(pixel, foliage, coverage);
            }

            texture.SetPixels32(seasonalPixels);
            texture.Apply(false);
            displayedSeason = season;
            return true;
        }

        private bool Initialize()
        {
            var original = WorldArtCatalog.GetSprite(WorldArtKind.Tree);
            if (original == null) return false;

            var source = original.texture;
            sourcePixels = source.GetPixels32();
            seasonalPixels = (Color32[])sourcePixels.Clone();
            texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
            {
                name = "Seasonal tree texture",
                filterMode = source.filterMode,
                wrapMode = source.wrapMode
            };
            var rect = original.rect;
            var pivot = new Vector2(original.pivot.x / rect.width, original.pivot.y / rect.height);
            Sprite = UnityEngine.Sprite.Create(texture, rect, pivot, original.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            Sprite.name = "world_Tree_Seasonal";
            return true;
        }

        public void Dispose()
        {
            if (Sprite != null) Object.Destroy(Sprite);
            if (texture != null) Object.Destroy(texture);
        }
    }
}
