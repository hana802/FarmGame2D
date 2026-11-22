using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class BedroomChestInteractable : InteractableBehaviour
    {
        public override string Prompt => "보관함 열기";

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.OpenBedroomChest();
        }
    }
}
