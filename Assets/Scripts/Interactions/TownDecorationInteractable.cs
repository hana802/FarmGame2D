using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class TownDecorationInteractable : InteractableBehaviour
    {
        private int decorationIndex;
        private GameItem item;
        public override string Prompt => $"{ItemCatalog.GetLabel(item)} 줍기";

        public void Configure(int index, GameItem decorationItem)
        {
            decorationIndex = index;
            item = decorationItem;
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.PickUpDecoration(decorationIndex);
        }
    }
}
