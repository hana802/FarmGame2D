using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class DialogueInteractable : InteractableBehaviour
    {
        private string speaker = "주민";
        private string line = "좋은 하루예요.";

        public override string Prompt => $"{speaker}와 대화";

        public void Configure(string speakerName, string dialogue)
        {
            speaker = speakerName;
            line = dialogue;
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.ShowMessage($"{speaker}: {line}");
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
