using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class DialogueInteractable : InteractableBehaviour
    {
        private string speaker = "주민";
        private string line = "좋은 하루예요.";
        private int portraitRow = -1;

        public override string Prompt => $"{speaker}와 대화";

        public void Configure(string speakerName, string dialogue, int npcPortraitRow = -1)
        {
            speaker = speakerName;
            line = dialogue;
            portraitRow = npcPortraitRow;
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.ShowDialogue(speaker, line, portraitRow);
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
        public override string Prompt => "씨앗·사료 상점 열기";

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.ToggleShop();
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
        public override string Prompt => "밀과 달걀 모두 판매";

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
            GameSession.Instance?.InteractChicken();
        }
    }
}
