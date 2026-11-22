using System;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private DialogueInteractable[] friendshipResidents = Array.Empty<DialogueInteractable>();
        private Vector2 friendshipScroll;
        private GUIStyle friendshipNameStyle;
        private GUIStyle friendshipHeartStyle;
        private GUIStyle friendshipScoreStyle;
        private GUIStyle friendshipGiftStyle;

        private void RefreshFriendshipResidents()
        {
            friendshipResidents = FindObjectsByType<DialogueInteractable>();
            Array.Sort(friendshipResidents, (left, right) => left.PortraitRow.CompareTo(right.PortraitRow));
        }

        private void DrawFriendshipPage(GameSaveData data, Rect area)
        {
            friendshipNameStyle ??= CreateSurfaceStyle(18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            friendshipHeartStyle ??= new GUIStyle(friendshipNameStyle)
            {
                fontSize = 23,
                richText = true
            };
            friendshipScoreStyle ??= CreateSurfaceStyle(15, Ink, FontStyle.Normal, TextAnchor.MiddleLeft);
            friendshipGiftStyle ??= CreateSurfaceStyle(14, new Color32(98, 72, 48, 255));

            const int columns = 2;
            const float gap = 10f;
            const float cardHeight = 168f;
            var rowCount = Mathf.Max(1, Mathf.CeilToInt(friendshipResidents.Length / (float)columns));
            var contentHeight = rowCount * (cardHeight + gap) - gap;
            var scroll = contentHeight > area.height;
            var viewWidth = area.width - (scroll ? 18f : 0f);
            var cardWidth = (viewWidth - gap) / columns;
            friendshipScroll = GUI.BeginScrollView(area, friendshipScroll,
                new Rect(0f, 0f, viewWidth, Mathf.Max(area.height, contentHeight)));
            for (var index = 0; index < friendshipResidents.Length; index++)
            {
                var card = new Rect(index % columns * (cardWidth + gap), index / columns * (cardHeight + gap),
                    cardWidth, cardHeight);
                DrawFriendshipResident(card, friendshipResidents[index], data);
            }

            GUI.EndScrollView();
        }

        private void DrawFriendshipResident(Rect card, DialogueInteractable resident, GameSaveData data)
        {
            var id = resident.gameObject.name;
            var spouse = string.Equals(data.spouseNpcId, id, StringComparison.Ordinal);
            phoneSkin.DrawCard(card, warm: spouse);
            var portrait = new Rect(card.x + 10f, card.y + 12f, 52f, 52f);
            farmSkin.DrawPixelPanel(portrait, Paper, PaperEdge, false);
            farmSkin.DrawSprite(new Rect(portrait.x + 3f, portrait.y + 3f, 46f, 46f),
                GameArtCatalog.GetNpcPortrait(resident.PortraitRow));
            var text = new Rect(portrait.xMax + 10f, card.y + 8f, card.xMax - portrait.xMax - 20f, 24f);
            if (spouse)
                GUI.Label(new Rect(card.xMax - 68f, card.y + 8f, 58f, 22f), "배우자", friendshipScoreStyle);
            GUI.Label(new Rect(text.x, text.y, spouse ? text.width - 62f : text.width, text.height),
                resident.DisplayName, friendshipNameStyle);

            var maxPoints = NpcFriendshipService.MaxPoints;
            var points = Mathf.Clamp(NpcFriendshipService.GetPoints(data, id), 0, maxPoints);
            var filled = points / NpcFriendshipService.PointsPerHeart;
            var empty = maxPoints / NpcFriendshipService.PointsPerHeart - filled;
            var hearts = $"<color=#A94F55>{new string('♥', filled)}</color><color=#997D66>{new string('♡', empty)}</color>";
            GUI.Label(new Rect(text.x, text.y + 24f, 92f, 24f), hearts, friendshipHeartStyle);
            GUI.Label(new Rect(text.x + 96f, text.y + 26f, text.width - 96f, 20f), $"{points} / {maxPoints}", friendshipScoreStyle);
            GUI.Label(new Rect(text.x, text.y + 50f, text.width, 22f),
                FormatGiftLine("정말 좋아함", NpcFriendshipService.GetPreferredItems(id, NpcGiftReaction.Loved)), friendshipGiftStyle);
            GUI.Label(new Rect(text.x, text.y + 72f, text.width, 36f),
                FormatGiftLine("좋아함", NpcFriendshipService.GetPreferredItems(id, NpcGiftReaction.Liked)), friendshipGiftStyle);
            GUI.Label(new Rect(card.x + 12f, card.y + 112f, card.width - 24f, 48f),
                FormatFoodLine(NpcFriendshipService.GetFoodPreferences(id)), friendshipGiftStyle);
        }

        private static string FormatGiftLine(string title, System.Collections.Generic.IReadOnlyList<GameItem> items)
        {
            if (items == null || items.Count == 0) return title;
            var names = new string[items.Count];
            for (var index = 0; index < items.Count; index++) names[index] = ItemCatalog.GetLabel(items[index]);
            return $"{title}  {string.Join(" · ", names)}";
        }

        private static string FormatFoodLine(System.Collections.Generic.IReadOnlyList<FoodPreference> foods)
        {
            if (foods == null || foods.Count == 0) return "음식  아직 없어요";
            var parts = new string[foods.Count];
            for (var index = 0; index < foods.Count; index++)
            {
                var tone = foods[index].Reaction == NpcGiftReaction.Loved ? "정말 좋아함" : "좋아함";
                parts[index] = $"{ItemCatalog.GetLabel(foods[index].Item)} · {tone}";
            }

            return "음식  " + string.Join("\n", parts);
        }
    }
}
