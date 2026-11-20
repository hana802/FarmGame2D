using System;
using FarmGame2D.Core;
using FarmGame2D.UI;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed partial class NpcInteractionOverlay : MonoBehaviour
    {
        private const float ReferenceWidth = 1600f;
        private const float ReferenceHeight = 900f;
        private static NpcInteractionOverlay instance;

        private string npcId = string.Empty;
        private string speaker = string.Empty;
        private int portraitRow = -1;
        private string dialogueLine;
        private int openedFrame;
        private bool choosingGift;
        private BondEventDefinition? activeBondEvent;
        private int bondPage;
        private Vector2 giftScroll;
        private GUIStyle titleStyle;
        private GUIStyle actionCaptionStyle;
        private FarmGuiSkin farmSkin;
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
            instance.choosingGift = false;
            instance.choosingFarmHelp = false;
            var data = GameSession.Instance?.Data;
            instance.activeBondEvent = data == null ? null :
                BondEventService.GetPendingNpcEnding(data, instance.npcId) ??
                BondEventService.GetPendingWedding(data, instance.npcId) ??
                BondEventService.GetMarriageProposal(data, instance.npcId);
            instance.bondPage = 0;
            instance.giftScroll = Vector2.zero;
            instance.openedFrame = Time.frameCount;
            instance.dialogueLine = instance.activeBondEvent.HasValue
                ? string.Empty
                : dialogueProvider?.Invoke() ?? "좋은 하루예요.";
            instance.enabled = true;
        }

        public static void ShowDogEvent(BondEventDefinition bondEvent)
        {
            Show(string.Empty, "강아지", -1, null);
            instance.activeBondEvent = bondEvent;
            instance.bondPage = 0;
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
                if (choosingFarmHelp) choosingFarmHelp = false;
                else if (choosingGift) choosingGift = false;
                else Close();
            }
            else if (!choosingFarmHelp && !choosingGift && !activeBondEvent.HasValue && Time.frameCount != openedFrame
                && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
            {
                Close();
            }
        }

        private void OnGUI()
        {
            if (!enabled || GameSession.Instance == null) return;

            EnsureStyles();
            var oldDepth = GUI.depth;
            GUI.depth = -100;
            var oldMatrix = GUI.matrix;
            try
            {
                var data = GameSession.Instance.Data;
                if (activeBondEvent.HasValue) DrawBondOverlay(data);
                else DrawDialogueAndActions(data);
            }
            finally
            {
                GUI.matrix = oldMatrix;
                GUI.depth = oldDepth;
            }
        }

        private void DrawBondOverlay(GameSaveData data)
        {
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
            var isSpouse = data.spouseNpcId == npcId && !string.IsNullOrEmpty(npcId);
            GUI.Label(new Rect(panel.x + 45f, panel.y + 38f, panel.width - 90f, 55f), isSpouse ? $"{speaker}  ♥ 배우자" : speaker, titleStyle);
            if (activeBondEvent?.Kind == BondEventKind.DogCompanion || activeBondEvent?.Kind == BondEventKind.DogEnding)
                GUI.Label(new Rect(panel.x + 45f, panel.y + 100f, panel.width - 90f, 44f), $"친밀도  {data.dogAffection}", centeredStyle);
            else
            {
                var points = NpcFriendshipService.GetPoints(data, npcId);
                var hearts = NpcFriendshipService.GetHeartCount(data, npcId);
                var heartText = new string('♥', hearts) + new string('♡', 5 - hearts);
                GUI.Label(new Rect(panel.x + 45f, panel.y + 100f, panel.width - 90f, 44f), $"호감도  {heartText}  {points}/{NpcFriendshipService.MaxPoints}", centeredStyle);
            }

            DrawBondEvent(panel, data);
        }

        private void DrawDialogueAndActions(GameSaveData data)
        {
            var viewport = HudLayout.CalculateViewport(Screen.width, Screen.height);
            GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.OffsetX, viewport.OffsetY, 0f), Quaternion.identity,
                new Vector3(viewport.Scale, viewport.Scale, 1f));
            if (choosingFarmHelp)
            {
                DrawFarmHelpMenu(new Rect(380f, 145f, 520f, 430f), GameSession.Instance);
                return;
            }

            var dialogue = HudLayout.CalculateDialogueLayout();
            farmSkin.DrawDialogue(dialogue, speaker, dialogueLine, portraitRow);
            DrawFriendship(dialogue.Panel, data);
            if (!choosingGift && GUI.Button(dialogue.Panel, GUIContent.none, GUIStyle.none))
            {
                Close();
                return;
            }

            const float actionWidth = 400f;
            var right = dialogue.Panel.xMax - 34f;
            var top = dialogue.Panel.yMin;
            var gift = new Rect(right - actionWidth, top - 223f, actionWidth, 52f);
            var close = new Rect(gift.x, top - 106f, actionWidth, 52f);
            if (choosingGift)
            {
                DrawGiftSelection(new Rect(gift.x, top - 266f, actionWidth, 246f), data);
                return;
            }

            var giftedToday = NpcFriendshipService.HasGiftedToday(data, npcId);
            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !giftedToday;
            if (farmSkin.DrawButton(gift, giftedToday ? "오늘 선물 완료" : "선물하기", true))
            {
                choosingGift = true;
                giftScroll = Vector2.zero;
            }
            GUI.enabled = previousEnabled;
            if (farmSkin.DrawButton(close, "닫기"))
            {
                Close();
                return;
            }

            DrawOptionalActions(gift, data);
        }

        private void DrawFriendship(Rect panel, GameSaveData data)
        {
            var points = NpcFriendshipService.GetPoints(data, npcId);
            var hearts = NpcFriendshipService.GetHeartCount(data, npcId);
            var heartText = new string('♥', hearts) + new string('♡', 5 - hearts);
            var relation = data.spouseNpcId == npcId ? "배우자 호감도" : "호감도";
            var badge = new Rect(panel.xMax - 418f, panel.y + 23f, 364f, 37f);
            farmSkin.DrawPixelPanel(badge, new Color32(250, 235, 208, 255), new Color32(198, 155, 116, 255), false);
            GUI.Label(badge, $"{relation}   <color=#984C44>{heartText}</color>   {points}/{NpcFriendshipService.MaxPoints}",
                actionCaptionStyle);
        }

        private void DrawOptionalActions(Rect gift, GameSaveData data)
        {
            const float gap = 10f;
            var width = (gift.width - gap) * 0.5f;
            var replay = BondEventService.GetNpcEndingForReplay(data, npcId);
            var wedding = BondEventService.GetWeddingForReplay(data, npcId);
            var row = new Rect(gift.x, gift.yMax + 5f, width, 26f);
            if (replay.HasValue && farmSkin.DrawButton(row, "우정 엔딩 다시 보기"))
            {
                OpenBondReplay(replay.Value);
                return;
            }
            if (replay.HasValue) row.x += width + gap;
            if (wedding.HasValue && farmSkin.DrawButton(row, "결혼식 다시 보기"))
            {
                OpenBondReplay(wedding.Value);
                return;
            }

            var chore = new Rect(gift.x, gift.yMax + 34f, gift.width, 26f);
            if (GameSession.Instance.HasSpouseFarmHelp(npcId) && farmSkin.DrawButton(chore, "농장일 부탁하기", true))
                OpenFarmHelp();
        }

        private void OpenBondReplay(BondEventDefinition replay)
        {
            activeBondEvent = replay;
            bondPage = 0;
        }

        private void DrawGiftSelection(Rect panel, GameSaveData data)
        {
            farmSkin.DrawPixelPanel(panel, new Color32(250, 231, 201, 255), new Color32(107, 70, 44, 255));
            GUI.Label(new Rect(panel.x + 28f, panel.y + 18f, panel.width - 56f, 28f),
                "선물할 물건을 골라주세요", actionCaptionStyle);
            var ownedItems = NpcFriendshipService.GetOwnedGiftableItems(data.inventory);
            var viewport = new Rect(panel.x + 30f, panel.y + 49f, panel.width - 60f, 140f);
            var contentHeight = Mathf.Max(viewport.height, ownedItems.Count * 40f);
            giftScroll = GUI.BeginScrollView(viewport, giftScroll, new Rect(0f, 0f, viewport.width - 20f, contentHeight));

            for (var index = 0; index < ownedItems.Count; index++)
            {
                var item = ownedItems[index];
                var itemRect = new Rect(0f, index * 40f, viewport.width - 20f, 36f);
                if (farmSkin.DrawButton(itemRect, $"{ItemCatalog.GetLabel(item)}  ×{data.inventory.Count(item)}"))
                {
                    GUI.EndScrollView();
                    GiveGift(data, item);
                    return;
                }
            }

            if (ownedItems.Count == 0)
            {
                GUI.Label(new Rect(0f, 18f, viewport.width - 20f, 85f),
                    "선물할 수 있는 수확물·물고기·광물이 없어요.", actionCaptionStyle);
            }

            GUI.EndScrollView();
            if (farmSkin.DrawButton(new Rect(panel.center.x - 70f, panel.yMax - 48f, 140f, 34f), "뒤로"))
                choosingGift = false;
        }

        private void GiveGift(GameSaveData data, GameItem item)
        {
            var result = NpcFriendshipService.TryGiveGift(data, npcId, item);
            if (result.Success) GameSession.Instance.SaveCurrentStateSilently();

            var pending = result.Success ? BondEventService.GetPendingNpcEnding(data, npcId) : null;
            if (pending.HasValue)
            {
                choosingGift = false;
                activeBondEvent = pending;
                bondPage = 0;
                return;
            }

            choosingGift = false;
            dialogueLine = result.Message;
        }

        private void DrawBondEvent(Rect panel, GameSaveData data)
        {
            var bondEvent = activeBondEvent.Value;
            GUI.Label(new Rect(panel.x + 70f, panel.y + 160f, panel.width - 140f, 52f), bondEvent.Title, titleStyle);
            GUI.Label(new Rect(panel.x + 90f, panel.y + 240f, panel.width - 180f, 190f), bondEvent.Lines[bondPage], centeredStyle);
            GUI.Label(new Rect(panel.x + 90f, panel.y + 445f, panel.width - 180f, 35f), $"{bondPage + 1} / {bondEvent.Lines.Length}", centeredStyle);
            var isLastPage = bondPage == bondEvent.Lines.Length - 1;
            if (isLastPage && bondEvent.Kind == BondEventKind.MarriageProposal)
            {
                if (GUI.Button(new Rect(panel.x + 100f, panel.y + 500f, 280f, 56f), "결혼하기", buttonStyle) &&
                    BondEventService.TryAcceptMarriage(data, bondEvent.NpcId))
                {
                    GameSession.Instance.SaveCurrentStateSilently();
                    activeBondEvent = BondEventService.GetPendingWedding(data, bondEvent.NpcId);
                    bondPage = 0;
                }
                if (GUI.Button(new Rect(panel.x + 420f, panel.y + 500f, 280f, 56f), "나중에", buttonStyle)) Close();
                return;
            }

            if (GUI.Button(new Rect(panel.x + 270f, panel.y + 500f, 260f, 56f), isLastPage ? "마치기" : "계속", buttonStyle))
            {
                if (isLastPage)
                {
                    BondEventService.Complete(data, bondEvent);
                    GameSession.Instance.SaveCurrentStateSilently();
                    if (bondEvent.Kind == BondEventKind.NpcEnding)
                    {
                        activeBondEvent = BondEventService.GetPendingWedding(data, bondEvent.NpcId) ??
                            BondEventService.GetMarriageProposal(data, bondEvent.NpcId);
                        bondPage = 0;
                        if (!activeBondEvent.HasValue) Close();
                    }
                    else Close();
                }
                else bondPage++;
            }
        }

        private void Close()
        {
            choosingFarmHelp = false;
            choosingGift = false;
            activeBondEvent = null;
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
            farmSkin = new FarmGuiSkin(font);
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
            centeredStyle = new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = 23, wordWrap = true, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.25f, 0.16f, 0.09f) }
            };
            actionCaptionStyle = new GUIStyle(centeredStyle) { fontSize = 17, fontStyle = FontStyle.Bold, richText = true };
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
