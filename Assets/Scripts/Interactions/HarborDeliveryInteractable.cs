using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class HarborDeliveryInteractable : InteractableBehaviour
    {
        public override string Prompt
        {
            get
            {
                var data = GameSession.Instance?.Data;
                if (data == null || HarborDelivery.IsComplete(data)) return "민서의 납품 안내 보기";
                return data.inventory.Count(HarborDelivery.Catch) >= HarborDelivery.Quantity
                    ? $"고등어 {HarborDelivery.Quantity}마리 납품 · {HarborDelivery.Reward} G"
                    : "민서의 납품 의뢰 확인";
            }
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.DeliverHarborFish();
        }
    }
}
