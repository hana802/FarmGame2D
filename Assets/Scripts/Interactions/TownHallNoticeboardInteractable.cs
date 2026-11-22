using FarmGame2D.Core;
using FarmGame2D.Player;

namespace FarmGame2D.Interactions
{
    public sealed class TownHallNoticeboardInteractable : InteractableBehaviour
    {
        public override string Prompt => "마을 공지 읽기";

        public override void Interact(PrototypePlayerController player)
        {
            var session = GameSession.Instance;
            if (session == null) return;
            session.ShowMessage(SeasonCalendar.IsWeekend(session.Data.day)
                ? "오늘은 주말 장터가 열려요. 광장 동쪽에서 만나요."
                : "주말 장터는 토요일과 일요일에 열려요.");
        }
    }
}
