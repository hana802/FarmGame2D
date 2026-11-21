using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class FarmFacilityInteractable : InteractableBehaviour
    {
        private FarmFacility facility;
        public override string Prompt => facility == FarmFacility.Kitchen ? "주방에서 요리하기" : "농가·온실 공사 보기";

        public void Configure(FarmFacility type) => facility = type;

        public override void Interact(PrototypePlayerController player) => GameSession.Instance?.OpenFarmFacility(facility);
    }
}
