using FarmGame2D.Art;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    internal static class WeddingCeremonyArt
    {
        private static Texture2D bridePlayer;
        private static Texture2D brideSpouse;
        private static Texture2D groomPlayer;
        private static Texture2D groomSpouse;

        private static readonly string[] GuestIds =
        {
            "Guide", "Elder", "Young Villager", "Fisher Minseo",
            "Florist Taeo", "Baker Yuna", "Rancher Sora"
        };

        public static void Draw(CharacterChoice playerChoice, string spouseId, float elapsed)
        {
            DrawGarden();
            DrawArch();
            DrawGuests(spouseId, elapsed);
            DrawCouple(playerChoice, spouseId, elapsed);
            if (elapsed >= 8f) DrawCelebration(elapsed);
        }

        private static void DrawGarden()
        {
            for (var band = 0; band < 12; band++)
            {
                var shade = Color.Lerp(new Color(0.55f, 0.78f, 0.91f), new Color(0.89f, 0.94f, 0.82f), band / 11f);
                Fill(new Rect(0f, band * 48f, 1600f, 48f), shade);
            }

            Fill(new Rect(0f, 560f, 1600f, 340f), new Color(0.34f, 0.59f, 0.34f));
            Fill(new Rect(0f, 610f, 1600f, 290f), new Color(0.28f, 0.49f, 0.29f));
            Fill(new Rect(535f, 610f, 530f, 290f), new Color(0.72f, 0.61f, 0.43f));
            Fill(new Rect(555f, 610f, 490f, 290f), new Color(0.87f, 0.76f, 0.55f));
            Fill(new Rect(0f, 542f, 1600f, 18f), new Color(0.31f, 0.42f, 0.21f));

            for (var i = 0; i < 14; i++)
            {
                var x = i * 122f;
                Fill(new Rect(x, 480f, 9f, 95f), new Color(0.42f, 0.29f, 0.16f));
                Fill(new Rect(x, 510f, 122f, 9f), new Color(0.51f, 0.33f, 0.18f));
            }

            for (var i = 0; i < 15; i++)
            {
                var x = i * 113f + 20f;
                DrawFlower(x, 586f + i % 3 * 10f, i % 2 == 0);
            }
        }

        private static void DrawArch()
        {
            var wood = new Color(0.53f, 0.32f, 0.17f);
            var highlight = new Color(0.75f, 0.5f, 0.25f);
            Fill(new Rect(545f, 320f, 24f, 300f), wood);
            Fill(new Rect(1031f, 320f, 24f, 300f), wood);
            Fill(new Rect(559f, 320f, 9f, 300f), highlight);
            Fill(new Rect(1031f, 320f, 9f, 300f), highlight);
            Fill(new Rect(565f, 295f, 470f, 24f), wood);
            Fill(new Rect(600f, 269f, 400f, 23f), wood);
            Fill(new Rect(650f, 247f, 300f, 21f), wood);
            Fill(new Rect(700f, 230f, 200f, 20f), wood);

            for (var i = 0; i < 13; i++)
            {
                var x = 560f + i * 40f;
                DrawFlower(x, 302f - 66f * Mathf.Sin(i / 12f * Mathf.PI), i % 2 == 0);
            }
            DrawFlower(552f, 392f, true);
            DrawFlower(1047f, 392f, false);
        }

        private static void DrawGuests(string spouseId, float elapsed)
        {
            var guestIndex = 0;
            foreach (var guestId in GuestIds)
            {
                if (guestId == spouseId) continue;
                var x = guestIndex < 3 ? 68f + guestIndex * 150f : 1082f + (guestIndex - 3) * 150f;
                var clap = elapsed >= 8f && Mathf.FloorToInt(elapsed * 3f + guestIndex) % 2 == 0;
                var bounce = clap ? -7f : 0f;
                DrawSprite(GetNpcSprite(guestId, clap), new Rect(x, 459f + bounce, 150f, 195f));
                guestIndex++;
            }
        }

        private static void DrawCouple(CharacterChoice playerChoice, string spouseId, float elapsed)
        {
            var entrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 3f));
            var step = elapsed < 3f && Mathf.FloorToInt(elapsed * 5f) % 2 == 0 ? 1 : 0;
            var together = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - 8f) / 2f));
            var playerX = Mathf.Lerp(330f, 630f, entrance) + 26f * together;
            var spouseX = Mathf.Lerp(1100f, 790f, entrance) - 26f * together;
            var bob = Mathf.Sin(elapsed * 3f) * 2f;
            var playerRect = new Rect(playerX, 395f + bob, 180f, 234f);
            var spouseRect = new Rect(spouseX, 395f - bob, 180f, 234f);

            DrawWeddingCharacter(playerRect, playerChoice == CharacterChoice.Female, true);
            DrawWeddingCharacter(spouseRect, IsFemaleNpc(spouseId), false);

            if (elapsed < 8f) return;
            Fill(new Rect(playerX + 113f, 527f, 58f, 8f), new Color(0.9f, 0.75f, 0.56f));
            DrawFlower((playerX + spouseX) * 0.5f + 104f, 509f, true);
        }

        private static void DrawWeddingCharacter(Rect character, bool female, bool player)
        {
            var texture = GetWeddingCharacterTexture(female, player);
            if (texture == null) return;
            GUI.DrawTexture(character, texture, ScaleMode.ScaleToFit, true);
        }

        private static Texture2D GetWeddingCharacterTexture(bool female, bool player)
        {
            if (female && player)
            {
                return LoadWeddingCharacter(ref bridePlayer, "wedding_character_bride_player_v2");
            }

            if (female)
            {
                return LoadWeddingCharacter(ref brideSpouse, "wedding_character_bride_spouse_v2");
            }

            return player
                ? LoadWeddingCharacter(ref groomPlayer, "wedding_character_groom_player_v2")
                : LoadWeddingCharacter(ref groomSpouse, "wedding_character_groom_spouse_v2");
        }

        private static Texture2D LoadWeddingCharacter(ref Texture2D cached, string assetName)
        {
            if (cached != null) return cached;
            cached = Resources.Load<Texture2D>($"Art/Wedding/{assetName}");
            if (cached == null) return null;
            cached.filterMode = FilterMode.Point;
            cached.wrapMode = TextureWrapMode.Clamp;
            return cached;
        }

        private static void DrawCelebration(float elapsed)
        {
            var time = elapsed - 8f;
            for (var i = 0; i < 30; i++)
            {
                var angle = i * 2.399f;
                var travel = (time * (72f + i % 5 * 13f)) % 610f;
                var x = 800f + Mathf.Cos(angle) * travel;
                var y = 444f + Mathf.Sin(angle) * travel * 0.55f + time * 24f;
                var petal = i % 3 == 0 ? new Color(0.97f, 0.69f, 0.72f) : new Color(1f, 0.92f, 0.68f);
                Fill(new Rect(x, y, 9f + i % 3 * 3f, 9f + i % 2 * 3f), petal);
            }
        }

        private static Sprite GetNpcSprite(string id, bool alternate)
        {
            var row = id switch
            {
                "Guide" or "Florist Taeo" => 0,
                "Fisher Minseo" => GameArtCatalog.MinseoWorldArt,
                "Baker Yuna" => 1,
                "Elder" => 2,
                "Young Villager" => 3,
                "Rancher Sora" => GameArtCatalog.SoraWorldArt,
                _ => 0
            };
            return GameArtCatalog.GetNpcSprite(row, alternate ? 4 : 0);
        }

        private static bool IsFemaleNpc(string id)
        {
            return id == "Elder" || id == "Fisher Minseo" || id == "Baker Yuna" || id == "Rancher Sora";
        }

        private static void DrawFlower(float x, float y, bool pink)
        {
            var petal = pink ? new Color(0.94f, 0.58f, 0.66f) : new Color(0.99f, 0.89f, 0.65f);
            Fill(new Rect(x - 6f, y, 12f, 20f), petal);
            Fill(new Rect(x - 15f, y + 6f, 30f, 9f), petal);
            Fill(new Rect(x - 5f, y + 7f, 10f, 10f), new Color(0.89f, 0.62f, 0.25f));
            Fill(new Rect(x - 2f, y + 20f, 4f, 13f), new Color(0.24f, 0.49f, 0.28f));
        }

        private static void DrawSprite(Sprite sprite, Rect destination)
        {
            if (sprite == null) return;
            var texture = sprite.texture;
            var source = sprite.textureRect;
            var uv = new Rect(source.x / texture.width, source.y / texture.height,
                source.width / texture.width, source.height / texture.height);
            GUI.DrawTextureWithTexCoords(destination, texture, uv, true);
        }

        public static void Fill(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
