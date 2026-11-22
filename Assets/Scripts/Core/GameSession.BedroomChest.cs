namespace FarmGame2D.Core
{
    public sealed partial class GameSession
    {
        public bool BedroomChestOpen { get; private set; }

        public void OpenBedroomChest()
        {
            BedroomChestOpen = true;
            NotifyChanged();
        }

        public void CloseBedroomChest()
        {
            BedroomChestOpen = false;
            NotifyChanged();
        }

        public void MoveBedroomChestItem(GameItem item, int quantity, bool deposit)
        {
            if (!BedroomChestOpen) return;
            var source = deposit ? Data.inventory : Data.bedroomChest;
            var destination = deposit ? Data.bedroomChest : Data.inventory;
            if (!source.TryMoveTo(destination, item, quantity)) return;

            SaveSilently();
            NotifyChanged();
        }
    }
}
