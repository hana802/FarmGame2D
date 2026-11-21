using FarmGame2D.Farming;
using FarmGame2D.World;

namespace FarmGame2D.Core
{
    public enum SpouseFarmChore
    {
        WaterCrops,
        FeedAnimals,
        PullWeeds
    }

    public sealed partial class GameSession
    {
        public const int SpouseChoreTileLimit = 3;

        public bool HasSpouseFarmHelp(string npcId) => Data.weddingSeen && BondEventService.IsSpouse(Data, npcId);

        public string GetSpouseFarmHelpRestriction(string npcId)
        {
            if (!HasSpouseFarmHelp(npcId)) return "결혼식을 마친 배우자에게 부탁할 수 있어요.";
            if (Data.spouseHelpLastDay >= Data.day) return "오늘은 이미 도움을 받았어요.";
            if (Data.hour < 6 || Data.hour >= 12) return "아침 6시부터 정오 전까지 부탁할 수 있어요.";
            return string.Empty;
        }

        public bool TryRequestSpouseFarmHelp(string npcId, SpouseFarmChore chore, out string message)
        {
            message = GetSpouseFarmHelpRestriction(npcId);
            if (message.Length > 0) return false;

            var helped = chore switch
            {
                SpouseFarmChore.WaterCrops => TryWaterCropsForSpouse(out message),
                SpouseFarmChore.FeedAnimals => TryFeedAnimalsForSpouse(out message),
                SpouseFarmChore.PullWeeds => TryClearWeedsForSpouse(out message),
                _ => false
            };
            if (!helped) return false;

            Data.spouseHelpLastDay = Data.day;
            Message = message;
            if (chore == SpouseFarmChore.PullWeeds) CompleteCurrentStarterQuest();
            NotifyChanged();
            SaveSilently();
            return true;
        }

        private bool TryWaterCropsForSpouse(out string message)
        {
            var watered = 0;
            foreach (var plot in Data.farmPlots)
            {
                if (plot.state != SoilState.Planted) continue;
                plot.state = SoilState.Watered;
                if (++watered == SpouseChoreTileLimit) break;
            }

            message = watered > 0 ? $"밭 {watered}칸에 물을 줬어요." : "지금 물을 줄 작물이 없어요.";
            return watered > 0;
        }

        private bool TryFeedAnimalsForSpouse(out string message)
        {
            var chickens = Data.chickenFedToday ? 0 : Data.ownedChickens;
            var cows = Data.cowFedToday ? 0 : Data.ownedCows;
            if (chickens + cows == 0)
            {
                message = "가축들이 오늘 먹이를 모두 먹었어요.";
                return false;
            }

            var plan = DogCompanionRules.PlanMorningCare(Data.inventory.Count(GameItem.Feed), chickens, cows);
            if (!plan.Helped || !Data.inventory.TrySpend(GameItem.Feed, plan.FeedSpent))
            {
                message = "가축에게 줄 사료가 부족해요.";
                return false;
            }

            Data.chickenFedToday |= plan.ChickensFed;
            Data.cowFedToday |= plan.CowsFed;
            var animals = plan.ChickensFed && plan.CowsFed ? "닭과 소" : plan.ChickensFed ? "닭" : "소";
            message = $"{animals}에게 먹이를 줬어요. 사료 {plan.FeedSpent}개를 썼어요.";
            return true;
        }

        private bool TryClearWeedsForSpouse(out string message)
        {
            var cleared = 0;
            for (var weedId = 0; weedId < WeedLayout.FarmWeedCount && cleared < SpouseChoreTileLimit; weedId++)
            {
                if (CollectWeed(weedId)) cleared++;
            }

            message = cleared > 0 ? $"농장의 잡초 {cleared}개를 뽑아 가방에 넣었어요." : "농장에 뽑을 잡초가 없어요.";
            return cleared > 0;
        }
    }
}
