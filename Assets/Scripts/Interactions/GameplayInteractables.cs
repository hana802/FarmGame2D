using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class DialogueInteractable : InteractableBehaviour
    {
        private string speaker = "주민";
        private NpcDialogueSequence dialogue = new(new[] { "좋은 하루예요." });
        private int portraitRow = -1;

        public override string Prompt => $"{speaker}와 대화";
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
            GetComponent<FarmGame2D.World.NpcSocialAgent>()?.CancelConversation();
            GetComponent<FarmGame2D.World.WanderingMover>()?.Pause(4f);
            GameSession.Instance?.ShowDialogue(speaker, dialogue.TakeNext(), portraitRow);
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
        private int nodeId;
        private UnityEngine.SpriteRenderer nodeRenderer;

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
            GameSession.Instance?.MineNode(nodeId);
        }

        private void Refresh()
        {
            var session = GameSession.Instance;
            if (nodeRenderer == null || session == null || nodeId >= session.Data.miningNodes.Count)
            {
                return;
            }

            nodeRenderer.color = session.Data.miningNodes[nodeId].IsAvailable
                ? GetOreColor(session.Data.miningNodes[nodeId].ore)
                : new UnityEngine.Color(0.32f, 0.3f, 0.28f);
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
        private int weedId;

        public override string Prompt => "잡초 뽑기";

        public void Configure(int id)
        {
            weedId = id;
        }

        public override void Interact(PrototypePlayerController player)
        {
            if (GameSession.Instance?.PullWeed(weedId) == true)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }
}
