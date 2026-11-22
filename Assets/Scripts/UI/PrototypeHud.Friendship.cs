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
        private GUIStyle friendshipNameStyle;
        private GUIStyle friendshipHeartStyle;
        private GUIStyle friendshipScoreStyle;

        private void RefreshFriendshipResidents()
        {
            friendshipResidents = FindObjectsByType<DialogueInteractable>();
            Array.Sort(friendshipResidents, (left, right) => left.PortraitRow.CompareTo(right.PortraitRow));
        }

        private void DrawFriendshipPage(GameSaveData data, Rect area)
        {
            friendshipNameStyle ??= CreateSurfaceStyle(18, Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            friendshipHeartStyle ??= new GUIStyle(friendshipNameStyle)
            {
                fontSize = 23,
                richText = true
            };
            friendshipScoreStyle ??= CreateSurfaceStyle(15, Ink, FontStyle.Normal, TextAnchor.MiddleCenter);

            const int columns = 4;
            const float gap = 12f;
            const float cardHeight = 156f;
            var cardWidth = (area.width - (columns + 1) * gap) / columns;
            var rowCount = Mathf.CeilToInt((float)friendshipResidents.Length / columns);
            var top = area.center.y - (rowCount * (cardHeight + gap) - gap) * 0.5f;
            for (var index = 0; index < friendshipResidents.Length; index++)
            {
                var row = index / columns;
                var count = Mathf.Min(columns, friendshipResidents.Length - row * columns);
                var left = area.center.x - (count * (cardWidth + gap) - gap) * 0.5f;
                var card = new Rect(left + index % columns * (cardWidth + gap), top + row * (cardHeight + gap),
                    cardWidth, cardHeight);
                DrawFriendshipResident(card, friendshipResidents[index], data);
            }
        }

        private void DrawFriendshipResident(Rect card, DialogueInteractable resident, GameSaveData data)
        {
            var id = resident.gameObject.name;
            var spouse = string.Equals(data.spouseNpcId, id, StringComparison.Ordinal);
            phoneSkin.DrawCard(card, warm: spouse);
            var portrait = new Rect(card.center.x - 30f, card.y + 11f, 60f, 60f);
            farmSkin.DrawPixelPanel(portrait, Paper, PaperEdge, false);
            farmSkin.DrawSprite(new Rect(portrait.x + 3f, portrait.y + 3f, 54f, 54f),
                GameArtCatalog.GetNpcPortrait(resident.PortraitRow));
            if (spouse)
                GUI.Label(new Rect(card.xMax - 63f, card.y + 10f, 54f, 24f), "배우자", friendshipScoreStyle);
            GUI.Label(HudLayout.InsetRow(card, 5f, 74f, 26f), resident.DisplayName, friendshipNameStyle);

            var maxPoints = NpcFriendshipService.MaxPoints;
            var points = Mathf.Clamp(NpcFriendshipService.GetPoints(data, id), 0, maxPoints);
            var filled = points / NpcFriendshipService.PointsPerHeart;
            var empty = maxPoints / NpcFriendshipService.PointsPerHeart - filled;
            var hearts = $"<color=#A94F55>{new string('♥', filled)}</color><color=#997D66>{new string('♡', empty)}</color>";
            GUI.Label(HudLayout.InsetRow(card, 5f, 100f, 26f), hearts, friendshipHeartStyle);
            GUI.Label(HudLayout.InsetRow(card, 5f, 128f, 20f), $"{points} / {maxPoints}", friendshipScoreStyle);
        }
    }
}
