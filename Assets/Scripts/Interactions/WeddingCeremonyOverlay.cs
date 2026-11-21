using FarmGame2D.Audio;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class WeddingCeremonyOverlay : MonoBehaviour
    {
        private const float ReferenceWidth = 1600f;
        private const float ReferenceHeight = 900f;
        private const float CeremonyDuration = 13f;

        private static WeddingCeremonyOverlay instance;

        private BondEventDefinition ceremony;
        private GameSession session;
        private string spouseName;
        private float elapsed;
        private bool chimePlayed;
        private GUIStyle titleStyle;
        private GUIStyle captionStyle;
        private GUIStyle buttonStyle;
        private GUIStyle smallStyle;

        public static bool IsPlaying => instance != null;

        public static void Show(BondEventDefinition wedding, GameSession gameSession, string displayName)
        {
            if (instance != null || gameSession == null || wedding.Kind != BondEventKind.Wedding) return;

            var host = new GameObject("Wedding Ceremony");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<WeddingCeremonyOverlay>();
            instance.ceremony = wedding;
            instance.session = gameSession;
            instance.spouseName = displayName;
        }

        private void Update()
        {
            elapsed = Mathf.Min(CeremonyDuration, elapsed + Time.unscaledDeltaTime);
            if (!chimePlayed && elapsed >= 8f)
            {
                chimePlayed = true;
                GameAudioController.Play(GameAudioCue.WeddingChime);
            }

            if (Input.GetKeyDown(KeyCode.Escape)) Destroy(gameObject);
        }

        private void OnGUI()
        {
            if (session == null || session.Data == null) return;
            EnsureStyles();

            var oldMatrix = GUI.matrix;
            var oldDepth = GUI.depth;
            var scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            var offsetX = (Screen.width - ReferenceWidth * scale) * 0.5f;
            var offsetY = (Screen.height - ReferenceHeight * scale) * 0.5f;
            GUI.depth = -200;
            GUI.matrix = Matrix4x4.identity;
            WeddingCeremonyArt.Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.16f, 0.1f, 0.08f));
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, Vector3.one * scale);

            WeddingCeremonyArt.Draw(session.Data.characterChoice, ceremony.NpcId, elapsed);
            GUI.Label(new Rect(190f, 34f, 1220f, 78f), "♥  우리의 결혼식  ♥", titleStyle);
            GUI.Label(new Rect(190f, 109f, 1220f, 44f), $"당신과 {spouseName}의 특별한 하루", smallStyle);

            var captionIndex = elapsed < 4.5f ? 0 : elapsed < 8f ? 1 : 2;
            var caption = ceremony.Lines[Mathf.Min(captionIndex, ceremony.Lines.Length - 1)];
            WeddingCeremonyArt.Fill(new Rect(220f, 699f, 1160f, 132f), new Color(0.19f, 0.11f, 0.08f, 0.9f));
            WeddingCeremonyArt.Fill(new Rect(229f, 708f, 1142f, 114f), new Color(0.94f, 0.83f, 0.65f, 0.97f));
            GUI.Label(new Rect(260f, 721f, 1080f, 88f), caption, captionStyle);

            if (elapsed >= CeremonyDuration)
            {
                if (GUI.Button(new Rect(650f, 842f, 300f, 48f), "결혼식 마치기", buttonStyle)) Finish();
            }
            else if (GUI.Button(new Rect(1390f, 20f, 170f, 45f), "나중에 보기", buttonStyle))
            {
                Destroy(gameObject);
            }

            GUI.matrix = oldMatrix;
            GUI.depth = oldDepth;
        }

        private void Finish()
        {
            BondEventService.Complete(session.Data, ceremony);
            session.SaveCurrentStateSilently();
            Destroy(gameObject);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            var font = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 18);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = 50, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.38f, 0.17f, 0.14f) }
            };
            smallStyle = new GUIStyle(titleStyle) { fontSize = 25 };
            captionStyle = new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = 27, wordWrap = true,
                alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.26f, 0.14f, 0.1f) }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                font = font, fontSize = 22, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.22f, 0.12f, 0.09f) }
            };
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
