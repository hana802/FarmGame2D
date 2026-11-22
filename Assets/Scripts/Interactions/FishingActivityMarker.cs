using FarmGame2D.Core;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class FishingActivityMarker : MonoBehaviour
    {
        public const string Label = "낚시 중";

        private Transform player;
        private Font font;
        private GUIStyle style;

        public static FishingActivityMarker Attach(Transform playerTransform)
        {
            var marker = new GameObject("Fishing Activity Marker").AddComponent<FishingActivityMarker>();
            marker.player = playerTransform;
            marker.font = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 16);
            return marker;
        }

        private void OnGUI()
        {
            if (player == null || Camera.main == null || !NpcSpeechBubblePresentation.ShouldDraw(Label, IsModalOpen()))
            {
                return;
            }

            var screen = Camera.main.WorldToScreenPoint(player.position + new Vector3(0f, 1.9f));
            if (screen.z <= 0f)
            {
                return;
            }

            style ??= NpcSpeechBubblePresentation.CreateStyle(GUI.skin.label, font);
            style.fontSize = 15;
            const float width = 118f;
            var height = NpcSpeechBubblePresentation.CalculateHeight(style, Label, width);
            var rect = NpcSpeechBubblePresentation.KeepOnScreen(
                new Rect(screen.x - (width * 0.5f), Screen.height - screen.y - height - 4f, width, height),
                Screen.width,
                Screen.height);
            NpcSpeechBubblePresentation.Draw(rect, Label, style);
        }

        private static bool IsModalOpen()
        {
            var session = GameSession.Instance;
            return session != null
                && (session.PhoneOpen || session.ShopOpen || session.InventoryOpen || session.QuestBoardOpen || session.SellBoxOpen || session.TurnipMarketOpen);
        }
    }
}
