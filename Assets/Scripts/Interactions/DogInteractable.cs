using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.World;

namespace FarmGame2D.Interactions
{
    public sealed class DogInteractable : InteractableBehaviour
    {
        public override string Prompt => "강아지 쓰다듬기";

        public override void Interact(PrototypePlayerController player)
        {
            ReactToPetting();
        }

        private void OnMouseDown()
        {
            ReactToPetting();
        }

        private void ReactToPetting()
        {
            GetComponent<DogCompanionController>()?.Celebrate();
            GameSession.Instance?.PetDog();
        }
    }
}
