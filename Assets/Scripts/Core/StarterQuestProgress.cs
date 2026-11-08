using System;

namespace FarmGame2D.Core
{
    public enum StarterQuestId
    {
        Harvest,
        Egg,
        Weeds,
        Complete
    }

    public readonly struct StarterQuestStatus
    {
        public StarterQuestStatus(StarterQuestId id, string title, int current, int target, bool completed)
        {
            Id = id;
            Title = title;
            Current = current;
            Target = target;
            Completed = completed;
        }

        public StarterQuestId Id { get; }
        public string Title { get; }
        public int Current { get; }
        public int Target { get; }
        public bool Completed { get; }
    }

    public readonly struct StarterQuestCompletion
    {
        public StarterQuestCompletion(string title, int reward)
        {
            Title = title;
            Reward = reward;
        }

        public string Title { get; }
        public int Reward { get; }
        public bool HasCompletion => Reward > 0;
    }

    public static class StarterQuestProgress
    {
        private const int HarvestTarget = 3;
        private const int EggTarget = 1;
        private const int WeedTarget = 3;

        public static StarterQuestStatus GetCurrent(GameSaveData data)
        {
            if (!data.harvestQuestCompleted)
            {
                return GetHarvest(data);
            }

            if (!data.eggQuestCompleted)
            {
                return GetEgg(data);
            }

            if (!data.weedQuestCompleted)
            {
                return GetWeeds(data);
            }

            return new StarterQuestStatus(StarterQuestId.Complete, "초보 퀘스트 완료", 1, 1, true);
        }

        public static StarterQuestStatus[] GetAll(GameSaveData data)
        {
            return new[] { GetHarvest(data), GetEgg(data), GetWeeds(data) };
        }

        public static StarterQuestCompletion TryCompleteCurrent(GameSaveData data)
        {
            if (!data.harvestQuestCompleted && data.wheatHarvested >= HarvestTarget)
            {
                return Complete(data, StarterQuestId.Harvest, "작물 3개 수확", 100);
            }

            if (data.harvestQuestCompleted && !data.eggQuestCompleted && data.firstEggAchievement)
            {
                return Complete(data, StarterQuestId.Egg, "달걀 1개 얻기", 75);
            }

            var pulledWeeds = data.pulledWeedIds?.Count ?? 0;
            if (data.eggQuestCompleted && !data.weedQuestCompleted && pulledWeeds >= WeedTarget)
            {
                return Complete(data, StarterQuestId.Weeds, "잡초 3개 뽑기", 100);
            }

            return default;
        }

        private static StarterQuestCompletion Complete(GameSaveData data, StarterQuestId id, string title, int reward)
        {
            switch (id)
            {
                case StarterQuestId.Harvest:
                    data.harvestQuestCompleted = true;
                    break;
                case StarterQuestId.Egg:
                    data.eggQuestCompleted = true;
                    break;
                case StarterQuestId.Weeds:
                    data.weedQuestCompleted = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }

            data.money += reward;
            return new StarterQuestCompletion(title, reward);
        }

        private static StarterQuestStatus GetHarvest(GameSaveData data)
        {
            return new StarterQuestStatus(
                StarterQuestId.Harvest,
                "작물 3개 수확",
                Math.Min(data.wheatHarvested, HarvestTarget),
                HarvestTarget,
                data.harvestQuestCompleted);
        }

        private static StarterQuestStatus GetEgg(GameSaveData data)
        {
            return new StarterQuestStatus(
                StarterQuestId.Egg,
                "달걀 1개 얻기",
                data.firstEggAchievement ? 1 : 0,
                EggTarget,
                data.eggQuestCompleted);
        }

        private static StarterQuestStatus GetWeeds(GameSaveData data)
        {
            return new StarterQuestStatus(
                StarterQuestId.Weeds,
                "잡초 3개 뽑기",
                Math.Min(data.pulledWeedIds?.Count ?? 0, WeedTarget),
                WeedTarget,
                data.weedQuestCompleted);
        }
    }
}
