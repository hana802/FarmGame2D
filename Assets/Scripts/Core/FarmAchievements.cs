using System.Collections.Generic;

namespace FarmGame2D.Core
{
    public static class FarmAchievements
    {
        public readonly struct Entry
        {
            public Entry(string title, string detail, bool earned)
            {
                Title = title;
                Detail = detail;
                Earned = earned;
            }

            public string Title { get; }
            public string Detail { get; }
            public bool Earned { get; }
        }

        public static IEnumerable<Entry> GetAll(GameSaveData data)
        {
            var weddingNameId = !string.IsNullOrEmpty(data.spouseNpcId) ? data.spouseNpcId : data.weddingSpouseNpcId;
            yield return new Entry("우리의 결혼식", data.weddingSeen
                ? $"{BondEventService.GetResidentName(weddingNameId)} 님과 결혼식을 올렸어요."
                : "결혼식이 열리면 기록돼요.", data.weddingSeen);
            yield return new Entry("친구가 된 날", "강아지와 처음 마음을 나눈 날", data.dogCompanionEventSeen);
            yield return new Entry("언제나 함께", "강아지와 오래도록 우정 쌓기", data.dogEndingSeen);
            yield return new Entry("첫 수확", "처음으로 작물 수확하기", data.firstHarvestAchievement);
            yield return new Entry("첫 달걀", "닭장에서 첫 달걀 거두기", data.firstEggAchievement);

            foreach (var friendship in data.npcFriendships)
            {
                if (friendship?.endingSeen != true) continue;
                var name = BondEventService.GetResidentName(friendship.npcId);
                yield return new Entry($"{name} 님과의 우정", "주민과의 특별한 우정 이야기", true);
            }
        }
    }
}
