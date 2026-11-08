using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class NpcSocialAgent : MonoBehaviour
    {
        private static readonly List<NpcSocialAgent> Agents = new();
        private static readonly string[] OpeningLines =
        {
            "안녕하세요, 오늘은 어때요?",
            "지금 어디 가는 길이에요?",
            "아까부터 뭐 하고 있었어요?",
            "오늘도 많이 바쁜가요?",
            "오늘 밭일은 잘돼 가요?",
            "마을 공기가 참 좋네요.",
            "상점에 새 물건이 들어왔대요.",
            "동물들이 오늘도 활기차네요.",
            "요즘 별일 없으시죠?",
            "점심은 드셨어요?",
            "올해는 어떤 작물을 심을까요?",
            "광장 꽃이 예쁘게 피었네요.",
            "판매 상자가 상점 옆으로 옮겨졌대요.",
            "요즘 광산에 사람이 많더라고요.",
            "오늘 저녁 노을을 보셨나요?",
            "농장 일이 조금은 익숙해졌나요?"
        };
        private static readonly string[] ReplyLines =
        {
            "네, 덕분에 잘 지내요!",
            "광장에 잠깐 들르는 길이에요.",
            "마을을 한 바퀴 걷고 있었어요.",
            "조금 바쁘지만 괜찮아요.",
            "네, 조금 있다가 구경 가려고요.",
            "그러게요. 산책하기 좋은 날이에요.",
            "농장 소식도 나중에 들려주세요.",
            "좋은 하루 보내세요!",
            "별일 없어요. 그쪽은 어때요?",
            "아직요. 같이 먹을래요?",
            "저는 딸기를 심어 보고 싶어요.",
            "그러게요. 지나갈 때마다 기분이 좋아요.",
            "잘됐네요. 장 보면서 팔기도 편하겠어요.",
            "새 광맥을 찾았다는 소문이 있대요.",
            "네, 하늘이 온통 주황빛이었어요.",
            "이제 제법 농부다운 모습이에요!"
        };

        private WanderingMover mover;
        private SpriteRenderer spriteRenderer;
        private NpcSocialAgent partner;
        private System.Random random;
        private GUIStyle bubbleStyle;
        private string displayName;
        private string bubbleText = string.Empty;
        private string pendingReply = string.Empty;
        private float conversationEndsAt;
        private float replyStartsAt;
        private float cooldownUntil;
        private float nextSearchAt;
        private bool replyShown;

        public bool IsConversing => partner != null;
        public string BubbleText => bubbleText;

        public void Configure(string npcName, int seed)
        {
            displayName = npcName;
            random = new System.Random(seed);
            mover = GetComponent<WanderingMover>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            nextSearchAt = Time.time + 1f + (float)random.NextDouble();
        }

        public bool TryStartConversation(NpcSocialAgent other)
        {
            if (!CanTalkTo(other))
            {
                return false;
            }

            var duration = 4.2f;
            partner = other;
            other.partner = this;
            conversationEndsAt = other.conversationEndsAt = Time.time + duration;
            replyStartsAt = other.replyStartsAt = Time.time + (duration * 0.48f);
            replyShown = other.replyShown = false;

            var lineIndex = random.Next(OpeningLines.Length);
            bubbleText = $"{displayName}: {OpeningLines[lineIndex]}";
            pendingReply = string.Empty;
            other.bubbleText = string.Empty;
            other.pendingReply = $"{other.displayName}: {ReplyLines[lineIndex]}";
            mover?.Pause(duration);
            other.mover?.Pause(duration);
            FacePartner();
            other.FacePartner();
            return true;
        }

        public void CancelConversation()
        {
            var other = partner;
            FinishConversation();
            if (other != null && other.partner == this)
            {
                other.FinishConversation();
            }
        }

        private void OnEnable()
        {
            Agents.Add(this);
        }

        private void OnDisable()
        {
            CancelConversation();
            Agents.Remove(this);
        }

        private void Update()
        {
            if (partner != null)
            {
                UpdateConversation();
                return;
            }

            if (Time.time < nextSearchAt || Time.time < cooldownUntil || IsNightTime())
            {
                return;
            }

            nextSearchAt = Time.time + 0.8f + ((float)random.NextDouble() * 0.8f);
            foreach (var candidate in Agents)
            {
                if (candidate == null || candidate == this || GetEntityId() > candidate.GetEntityId())
                {
                    continue;
                }

                if (Vector2.SqrMagnitude(transform.position - candidate.transform.position) > 2.25f)
                {
                    continue;
                }

                if (TryStartConversation(candidate))
                {
                    return;
                }
            }
        }

        private bool CanTalkTo(NpcSocialAgent other)
        {
            return other != null
                && other != this
                && partner == null
                && other.partner == null
                && Time.time >= cooldownUntil
                && Time.time >= other.cooldownUntil
                && !IsNightTime();
        }

        private void UpdateConversation()
        {
            FacePartner();
            if (!replyShown && Time.time >= replyStartsAt)
            {
                bubbleText = pendingReply;
                replyShown = true;
            }

            if (Time.time >= conversationEndsAt)
            {
                CancelConversation();
            }
        }

        private void FinishConversation()
        {
            partner = null;
            bubbleText = string.Empty;
            pendingReply = string.Empty;
            cooldownUntil = Time.time + 8f;
            nextSearchAt = cooldownUntil;
        }

        private void FacePartner()
        {
            if (partner == null || spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.flipX = partner.transform.position.x < transform.position.x;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(bubbleText) || Camera.main == null)
            {
                return;
            }

            var screen = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 1.35f));
            if (screen.z <= 0f)
            {
                return;
            }

            bubbleStyle ??= CreateBubbleStyle();
            var rect = new Rect(screen.x - 95f, Screen.height - screen.y - 38f, 190f, 42f);
            GUI.Box(rect, bubbleText, bubbleStyle);
        }

        private static GUIStyle CreateBubbleStyle()
        {
            return NpcSpeechBubblePresentation.CreateStyle(
                GUI.skin.box,
                Resources.Load<Font>("Fonts/NotoSansKR-VF"));
        }

        private static bool IsNightTime()
        {
            if (GameSession.Instance == null)
            {
                return false;
            }

            var hour = GameSession.Instance.Data.hour;
            return hour >= 20 || hour < 6;
        }
    }
}
