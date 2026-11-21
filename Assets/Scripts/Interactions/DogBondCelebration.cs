using FarmGame2D.Core;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class DogBondCelebration : MonoBehaviour
    {
        private const float ReferenceWidth = 1600f;
        private const float ReferenceHeight = 900f;
        private const float LineDuration = 3.2f;
        private const float CardWidth = 480f;
        private const float CardHeight = 124f;

        private static DogBondCelebration instance;

        private BondEventDefinition bondEvent;
        private DogCompanionController dog;
        private float elapsed;
        private int currentLine;
        private GUIStyle titleStyle;
        private GUIStyle lineStyle;

        public static bool IsPlaying => instance != null;

        public static void Show(BondEventDefinition eventToShow, GameSession gameSession)
        {
            if (instance != null || eventToShow.Lines == null || eventToShow.Lines.Length == 0
                || BondEventService.GetPendingDogEvent(gameSession?.Data)?.Kind != eventToShow.Kind)
                return;

            var host = new GameObject("Dog Bond Celebration");
            instance = host.AddComponent<DogBondCelebration>();
            instance.bondEvent = eventToShow;
            instance.dog = FindAnyObjectByType<DogCompanionController>();
            instance.dog?.Celebrate();
            BondEventService.Complete(gameSession.Data, eventToShow);
            gameSession.SaveCurrentStateSilently();
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            var nextLine = Mathf.FloorToInt(elapsed / LineDuration);
            if (nextLine >= bondEvent.Lines.Length)
            {
                Destroy(gameObject);
                return;
            }

            if (nextLine == currentLine) return;
            currentLine = nextLine;
            dog?.Celebrate();
        }

        private void OnGUI()
        {
            if (dog == null || Camera.main == null) return;
            EnsureStyles();

            var cameraPosition = Camera.main.WorldToScreenPoint(dog.transform.position + Vector3.up * 1.35f);
            if (cameraPosition.z < 0f) return;

            var scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            if (scale <= 0f) return;
            var offsetX = (Screen.width - ReferenceWidth * scale) * 0.5f;
            var offsetY = (Screen.height - ReferenceHeight * scale) * 0.5f;
            var x = (cameraPosition.x - offsetX) / scale;
            var y = (Screen.height - cameraPosition.y - offsetY) / scale;
            var progress = (elapsed % LineDuration) / LineDuration;
            var entrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.18f));
            var exit = Mathf.Clamp01((1f - progress) / 0.18f);
            var alpha = Mathf.Min(entrance, exit);
            var card = new Rect(
                Mathf.Clamp(x - CardWidth * 0.5f, 16f, ReferenceWidth - CardWidth - 16f),
                Mathf.Clamp(y - CardHeight - 22f - (1f - entrance) * 12f, 55f, ReferenceHeight - CardHeight - 32f),
                CardWidth,
                CardHeight);

            var oldMatrix = GUI.matrix;
            var oldColor = GUI.color;
            var oldDepth = GUI.depth;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, Vector3.one * scale);
            GUI.depth = -100;

            GUI.color = new Color(0.18f, 0.12f, 0.07f, alpha * 0.8f);
            GUI.DrawTexture(new Rect(card.x + 4f, card.y + 5f, card.width, card.height), Texture2D.whiteTexture);
            GUI.color = new Color(0.34f, 0.21f, 0.11f, alpha);
            GUI.DrawTexture(card, Texture2D.whiteTexture);
            GUI.color = new Color(0.95f, 0.86f, 0.65f, alpha);
            GUI.DrawTexture(new Rect(card.x + 4f, card.y + 4f, card.width - 8f, card.height - 8f), Texture2D.whiteTexture);
            GUI.color = new Color(0.73f, 0.43f, 0.27f, alpha);
            GUI.DrawTexture(new Rect(card.x + 18f, card.y + 43f, card.width - 36f, 2f), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(card.x + 20f, card.y + 6f, card.width - 40f, 34f), $"♥  {bondEvent.Title}  ♥", titleStyle);
            GUI.Label(new Rect(card.x + 26f, card.y + 49f, card.width - 52f, 67f), bondEvent.Lines[currentLine], lineStyle);

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
            GUI.depth = oldDepth;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            var font = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 18);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.31f, 0.17f, 0.1f) }
            };
            lineStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = 19,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.31f, 0.21f, 0.13f) }
            };
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
