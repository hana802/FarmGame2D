using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class DialogueInteractable : InteractableBehaviour
    {
        private string speaker = "주민";
        private NpcDialogueSequence dialogue = new(new[] { "좋은 하루예요." });
        private int portraitRow = -1;

        public override string Prompt => $"{speaker}와 상호작용";
        public int DialogueCount => dialogue.Count;

        public void Configure(string speakerName, string line, int npcPortraitRow = -1)
        {
            Configure(speakerName, new[] { line }, npcPortraitRow);
        }

        public void Configure(string speakerName, string[] lines, int npcPortraitRow = -1)
        {
            speaker = speakerName;
            dialogue = new NpcDialogueSequence(lines);
            portraitRow = npcPortraitRow;
        }

        public override void Interact(PrototypePlayerController player)
        {
            var socialAgent = GetComponent<FarmGame2D.World.NpcSocialAgent>();
            socialAgent?.CancelConversation();
            GetComponent<FarmGame2D.World.WanderingMover>()?.Pause(30f);
            socialAgent?.FacePlayer();
            NpcInteractionOverlay.Show(gameObject.name, speaker, portraitRow, dialogue.TakeNext);
        }
    }

    public sealed class DoorInteractable : InteractableBehaviour
    {
        private UnityEngine.Vector2 destination;
        private string prompt = "문 열기";
        private string arrivalMessage = "집 안으로 들어왔습니다.";

        public override string Prompt => prompt;
        public UnityEngine.Vector2 Destination => destination;

        public void Configure(UnityEngine.Vector2 target, string interactionPrompt, string message)
        {
            destination = target;
            prompt = interactionPrompt;
            arrivalMessage = message;
        }

        public override void Interact(PrototypePlayerController player)
        {
            player.Teleport(destination);
            GameSession.Instance?.ShowMessage(arrivalMessage);
        }
    }

    public sealed class ShopInteractable : InteractableBehaviour
    {
        private ShopKind shopKind = ShopKind.SeedsAndAnimals;

        public override string Prompt => shopKind == ShopKind.Mining
            ? "광물 상점 열기"
            : "씨앗·동물 상점 열기";

        public void Configure(ShopKind kind)
        {
            shopKind = kind;
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.OpenShop(shopKind);
        }
    }

    public sealed class NoticeInteractable : InteractableBehaviour
    {
        private string prompt = "문 확인하기";
        private string message = "아직 들어갈 수 없습니다.";

        public override string Prompt => prompt;

        public void Configure(string interactionPrompt, string noticeMessage)
        {
            prompt = interactionPrompt;
            message = noticeMessage;
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.ShowMessage(message);
        }
    }

    public sealed class QuestBoardInteractable : InteractableBehaviour
    {
        public override string Prompt => "광장 의뢰 게시판 보기";

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.OpenQuestBoard();
        }
    }

    public sealed class BedInteractable : InteractableBehaviour
    {
        public override string Prompt => "취침하고 다음 날로 이동";

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.Sleep();
        }
    }

    public sealed class SellBoxInteractable : InteractableBehaviour
    {
        public override string Prompt => "농산물과 광물 모두 판매";

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.SellAllProduce();
        }
    }

    public sealed class ChickenInteractable : InteractableBehaviour
    {
        public override string Prompt => "닭 돌보기";

        public override void Interact(PrototypePlayerController player)
        {
            GetComponent<FarmGame2D.World.WanderingMover>()?.Pause(1.5f);
            GameSession.Instance?.InteractChicken();
        }
    }

    public sealed class CowInteractable : InteractableBehaviour
    {
        public override string Prompt => "소 돌보기";

        public override void Interact(PrototypePlayerController player)
        {
            GetComponent<FarmGame2D.World.WanderingMover>()?.Pause(1.5f);
            GameSession.Instance?.InteractCow();
        }
    }

    public sealed class MiningNodeInteractable : InteractableBehaviour
    {
        private const float WindupTime = 0.10f;
        private const float StrikeTime = 0.22f;
        private const float SwingDuration = 0.50f;
        private int nodeId;
        private UnityEngine.SpriteRenderer nodeRenderer;
        private FarmGame2D.World.MiningNodeDepletionPresentation depletionPresentation;
        private PrototypePlayerController miner;

        public override string Prompt
        {
            get
            {
                var session = GameSession.Instance;
                if (session == null || nodeId >= session.Data.miningNodes.Count)
                {
                    return "광맥 확인";
                }

                var node = session.Data.miningNodes[nodeId];
                return node.IsAvailable
                    ? $"곡괭이로 {ItemCatalog.GetLabel(node.ore)} 캐기"
                    : $"광맥 재생까지 {node.regrowDaysRemaining}일";
            }
        }

        public void Configure(int id)
        {
            nodeId = id;
            nodeRenderer = GetComponent<UnityEngine.SpriteRenderer>();
            depletionPresentation = GetComponent<FarmGame2D.World.MiningNodeDepletionPresentation>()
                ?? gameObject.AddComponent<FarmGame2D.World.MiningNodeDepletionPresentation>();
            depletionPresentation.Initialize(nodeRenderer);
            if (GameSession.Instance != null)
            {
                GameSession.Instance.StateChanged += Refresh;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.StateChanged -= Refresh;
            }
        }

        public override void Interact(PrototypePlayerController player)
        {
            var session = GameSession.Instance;
            if (!isActiveAndEnabled || session == null || player == null || !player.isActiveAndEnabled
                || player.IsMining || player.IsFishing || miner != null)
                return;

            if (!session.CanMineNode(nodeId))
            {
                session.MineNode(nodeId);
                return;
            }

            miner = player;
            StartCoroutine(PlaySwing(session));
        }

        private IEnumerator PlaySwing(GameSession session)
        {
            var data = session.Data;
            var direction = ((Vector2)transform.position - (Vector2)miner.transform.position).normalized;
            var contact = (Vector2)transform.position - direction * 0.20f + Vector2.up * 0.33f;
            var oreColor = GetOreColor(data.miningNodes[nodeId].ore);
            miner.BeginMining(transform.position);
            var elapsed = 0f;
            var struck = false;
            var mined = false;
            while (miner != null && miner.isActiveAndEnabled && GameSession.Instance == session && session.Data == data)
            {
                if (elapsed >= StrikeTime && !struck)
                {
                    struck = true;
                    mined = session.MineNode(nodeId);
                }

                miner.PresentMiningSwing(contact, CalculatePullback(elapsed));
                if (mined)
                    depletionPresentation.PresentImpact(contact, oreColor, Mathf.InverseLerp(StrikeTime, SwingDuration, elapsed));

                if (elapsed >= SwingDuration) break;
                yield return null;
                elapsed = Mathf.Min(SwingDuration, elapsed + Time.deltaTime);
            }
            ClearSwing();
        }

        private static float CalculatePullback(float elapsed)
        {
            if (elapsed < WindupTime)
                return Mathf.SmoothStep(22f, 108f, elapsed / WindupTime);
            if (elapsed < StrikeTime)
                return Mathf.SmoothStep(108f, 0f, Mathf.InverseLerp(WindupTime, StrikeTime, elapsed));
            return Mathf.SmoothStep(0f, 42f, Mathf.InverseLerp(StrikeTime, SwingDuration, elapsed));
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            ClearSwing();
        }

        private void ClearSwing()
        {
            if (miner != null) miner.EndMining();
            miner = null;
            if (depletionPresentation != null) depletionPresentation.HideImpact();
        }

        private void Refresh()
        {
            var session = GameSession.Instance;
            if (nodeRenderer == null || session == null || nodeId >= session.Data.miningNodes.Count)
            {
                return;
            }

            var node = session.Data.miningNodes[nodeId];
            depletionPresentation.SetState(!node.IsAvailable, GetOreColor(node.ore));
        }

        private static UnityEngine.Color GetOreColor(GameItem ore)
        {
            return ore switch
            {
                GameItem.CopperOre => new UnityEngine.Color(0.72f, 0.38f, 0.2f),
                GameItem.IronOre => new UnityEngine.Color(0.55f, 0.62f, 0.68f),
                GameItem.GoldOre => new UnityEngine.Color(0.95f, 0.72f, 0.18f),
                _ => new UnityEngine.Color(0.46f, 0.44f, 0.42f)
            };
        }
    }

    public sealed class WeedInteractable : InteractableBehaviour
    {
        private GameSession session;
        private int weedId;

        public override string Prompt => "잡초 뽑기";

        public void Configure(GameSession gameSession, int id)
        {
            session = gameSession;
            weedId = id;
            session.StateChanged += Refresh;
            Refresh();
        }

        public override void Interact(PrototypePlayerController player)
        {
            session?.PullWeed(weedId);
        }

        private void Refresh()
        {
            gameObject.SetActive(session.Data.pulledWeedIds?.Contains(weedId) != true);
        }

        private void OnDestroy()
        {
            if (session != null) session.StateChanged -= Refresh;
        }
    }
}
