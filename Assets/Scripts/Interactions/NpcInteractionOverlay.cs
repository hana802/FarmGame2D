using System;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class NpcInteractionOverlay : MonoBehaviour
    {
        private const float ReferenceWidth = 1600f;
        private const float ReferenceHeight = 900f;
        private static NpcInteractionOverlay instance;

        private string npcId = string.Empty;
        private string speaker = string.Empty;
        private int portraitRow = -1;
        private Func<string> nextDialogue;
        private bool choosingGift;
        private Vector2 giftScroll;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle centeredStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;
        private Texture2D panelTexture;
        private Texture2D buttonTexture;
        private Font koreanFont;

        public static bool IsOpen => instance != null && instance.enabled;

        public static void Show(string id, string displayName, int dialoguePortraitRow, Func<string> dialogueProvider)
        {
            if (instance == null)
            {
                var host = new GameObject("NPC Interaction Overlay");
                DontDestroyOnLoad(host);
                instance = host.AddComponent<NpcInteractionOverlay>();
            }

            instance.npcId = id ?? string.Empty;
            instance.speaker = displayName ?? "주민";
            instance.portraitRow = dialoguePortraitRow;
            instance.nextDialogue = dialogueProvider;
            instance.choosingGift = false;
            instance.giftScroll = Vector2.zero;
            instance.enabled = true;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (choosingGift) choosingGift = false;
                else Close();
            }
        }

        private void OnGUI()
        {
            if (!enabled || GameSession.Instance == null) return;

            EnsureStyles();
            var oldDepth = GUI.depth;
            GUI.depth = -100;
            var oldMatrix = GUI.matrix;
            var scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            var offsetX = (Screen.width - ReferenceWidth * scale) * 0.5f;
            var offsetY = (Screen.height - ReferenceHeight * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            var previousColor = GUI.color;
            GUI.color = new Color(0.08f, 0.055f, 0.035f, 0.55f);
            GUI.DrawTexture(new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), Texture2D.whiteTexture);
            GUI.color = previousColor;

            var panel = new Rect(400f, 145f, 800f, 610f);
            GUI.Box(panel, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panel.x + 45f, panel.y + 38f, panel.width - 90f, 55f), speaker, titleStyle);

            var data = GameSession.Instance.Data;
            var points = NpcFriendshipService.GetPoints(data, npcId);
            var hearts = NpcFriendshipService.GetHeartCount(data, npcId);
            var heartText = new string('♥', hearts) + new string('♡', 5 - hearts);
            GUI.Label(new Rect(panel.x + 45f, panel.y + 100f, panel.width - 90f, 44f), $"호감도  {heartText}  {points}/{NpcFriendshipService.MaxPoints}", centeredStyle);

            if (choosingGift) DrawGiftSelection(panel, data);
            else DrawMainMenu(panel, data);

            GUI.matrix = oldMatrix;
            GUI.depth = oldDepth;
        }

        private void DrawMainMenu(Rect panel, GameSaveData data)
        {
            GUI.Label(new Rect(panel.x + 70f, panel.y + 165f, panel.width - 140f, 60f), $"{speaker}에게 무엇을 할까요?", centeredStyle);

            if (GUI.Button(new Rect(panel.x + 180f, panel.y + 250f, 440f, 62f), "대화하기", buttonStyle))
            {
                var line = nextDialogue?.Invoke() ?? "좋은 하루예요.";
                Close();
                GameSession.Instance.ShowDialogue(speaker, line, portraitRow);
            }

            var giftedToday = NpcFriendshipService.HasGiftedToday(data, npcId);
            GUI.enabled = !giftedToday;
            if (GUI.Button(new Rect(panel.x + 180f, panel.y + 330f, 440f, 62f), giftedToday ? "오늘 선물 완료" : "선물하기", buttonStyle))
            {
                choosingGift = true;
            }
            GUI.enabled = true;

            if (giftedToday)
            {
                GUI.Label(new Rect(panel.x + 100f, panel.y + 400f, panel.width - 200f, 36f), "선물은 NPC마다 하루에 한 번 줄 수 있어요.", centeredStyle);
            }

            if (GUI.Button(new Rect(panel.x + 270f, panel.y + 500f, 260f, 56f), "닫기", buttonStyle)) Close();
        }

        private void DrawGiftSelection(Rect panel, GameSaveData data)
        {
            GUI.Label(new Rect(panel.x + 70f, panel.y + 155f, panel.width - 140f, 42f), "선물할 물건을 골라주세요", centeredStyle);
            var ownedItems = NpcFriendshipService.GetOwnedGiftableItems(data.inventory);
            var viewport = new Rect(panel.x + 100f, panel.y + 210f, panel.width - 200f, 270f);
            var contentHeight = Mathf.Max(viewport.height - 4f, Mathf.Ceil(ownedItems.Count / 2f) * 58f + 8f);
            giftScroll = GUI.BeginScrollView(viewport, giftScroll, new Rect(0f, 0f, viewport.width - 20f, contentHeight));

            for (var index = 0; index < ownedItems.Count; index++)
            {
                var item = ownedItems[index];
                var column = index % 2;
                var row = index / 2;
                var itemRect = new Rect(column * 295f, row * 58f, 280f, 48f);
                if (GUI.Button(itemRect, $"{ItemCatalog.GetLabel(item)}  ×{data.inventory.Count(item)}", buttonStyle))
                {
                    GiveGift(data, item);
                    GUI.EndScrollView();
                    return;
                }
            }

            if (ownedItems.Count == 0)
            {
                GUI.Label(new Rect(0f, 80f, viewport.width - 20f, 50f), "선물할 수 있는 수확물·물고기·광물이 없어요.", centeredStyle);
            }

            GUI.EndScrollView();
            if (GUI.Button(new Rect(panel.x + 270f, panel.y + 510f, 260f, 56f), "뒤로", buttonStyle)) choosingGift = false;
        }

        private void GiveGift(GameSaveData data, GameItem item)
        {
            var result = NpcFriendshipService.TryGiveGift(data, npcId, item);
            if (result.Success) GameSession.Instance.SaveCurrentStateSilently();

            Close();
            GameSession.Instance.ShowDialogue(speaker, result.Message, portraitRow);
        }

        private void Close()
        {
            choosingGift = false;
            enabled = false;
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;

            panelTexture = SolidTexture(new Color(0.91f, 0.82f, 0.64f, 0.985f));
            buttonTexture = SolidTexture(new Color(0.43f, 0.34f, 0.18f, 1f));
            koreanFont = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 18);
            var font = koreanFont;
            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture },
                border = new RectOffset(8, 8, 8, 8)
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = 36, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.22f, 0.13f, 0.07f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = 23, wordWrap = true,
                normal = { textColor = new Color(0.25f, 0.16f, 0.09f) }
            };
            centeredStyle = new GUIStyle(bodyStyle) { alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                font = font, fontSize = 24, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = buttonTexture, textColor = new Color(1f, 0.94f, 0.78f) },
                hover = { background = buttonTexture, textColor = Color.white },
                active = { background = buttonTexture, textColor = Color.white }
            };
        }

        private static Texture2D SolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            if (panelTexture != null) Destroy(panelTexture);
            if (buttonTexture != null) Destroy(buttonTexture);
            if (instance == this) instance = null;
        }
    }
}
