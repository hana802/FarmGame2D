using System;

namespace FarmGame2D.Core
{
    public enum StarterQuestId
    {
        Harvest,
        Egg,
        Weeds,
        Fishing,
        Mining,
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
        public const int BoardRefreshDays = 3;
        private const int HarvestTarget = 3;
        private const int EggTarget = 1;
        private const int WeedTarget = 3;
        private const int FishingTarget = 2;
        private const int MiningTarget = 3;
        private static readonly StarterQuestId[] BoardQuestIds =
        {
            StarterQuestId.Harvest,
            StarterQuestId.Egg,
            StarterQuestId.Weeds,
            StarterQuestId.Fishing,
            StarterQuestId.Mining
        };

        public static StarterQuestStatus GetCurrent(GameSaveData data)
        {
            return data.activeQuestId < 0
                ? new StarterQuestStatus(StarterQuestId.Complete, "광장 게시판에서 의뢰 선택", 0, 1, false)
                : GetStatus(data, (StarterQuestId)data.activeQuestId);
        }

        public static StarterQuestStatus[] GetAll(GameSaveData data)
        {
            return new[] { GetHarvest(data), GetEgg(data), GetWeeds(data), GetFishing(data), GetMining(data) };
        }

        public static StarterQuestStatus[] GetBoardOffers(GameSaveData data)
        {
            const int offerCount = 3;
            var cycle = Math.Max(0, (data.day - 1) / BoardRefreshDays);
            var start = PositiveModulo(data.worldSeed + (cycle * 2), BoardQuestIds.Length);
            var offers = new StarterQuestStatus[offerCount];
            for (var index = 0; index < offerCount; index++)
            {
                offers[index] = GetStatus(data, BoardQuestIds[(start + index) % BoardQuestIds.Length]);
            }

            if (data.activeQuestId < 0)
            {
                return offers;
            }

            var activeId = (StarterQuestId)data.activeQuestId;
            for (var index = 0; index < offers.Length; index++)
            {
                if (offers[index].Id == activeId)
                {
                    return offers;
                }
            }

            offers[offers.Length - 1] = GetStatus(data, activeId);
            return offers;
        }

        public static int GetDaysUntilBoardRefresh(int day)
        {
            return BoardRefreshDays - PositiveModulo(Math.Max(1, day) - 1, BoardRefreshDays);
        }

        public static bool TryAccept(GameSaveData data, StarterQuestId id)
        {
            if (data.activeQuestId >= 0 || IsCompleted(data, id) || id == StarterQuestId.Complete)
            {
                return false;
            }

            data.activeQuestId = (int)id;
            return true;
        }

        public static StarterQuestCompletion TryCompleteCurrent(GameSaveData data)
        {
            if (data.activeQuestId < 0)
            {
                return default;
            }

            var status = GetStatus(data, (StarterQuestId)data.activeQuestId);
            return status.Current >= status.Target
                ? Complete(data, status.Id, status.Title, GetReward(status.Id))
                : default;
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
                case StarterQuestId.Fishing:
                    data.fishingQuestCompleted = true;
                    break;
                case StarterQuestId.Mining:
                    data.miningQuestCompleted = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }

            data.money += reward;
            data.activeQuestId = -1;
            return new StarterQuestCompletion(title, reward);
        }

        private static StarterQuestStatus GetStatus(GameSaveData data, StarterQuestId id)
        {
            return id switch
            {
                StarterQuestId.Harvest => GetHarvest(data),
                StarterQuestId.Egg => GetEgg(data),
                StarterQuestId.Weeds => GetWeeds(data),
                StarterQuestId.Fishing => GetFishing(data),
                StarterQuestId.Mining => GetMining(data),
                _ => new StarterQuestStatus(StarterQuestId.Complete, "의뢰 없음", 0, 1, false)
            };
        }

        private static bool IsCompleted(GameSaveData data, StarterQuestId id)
        {
            return id switch
            {
                StarterQuestId.Harvest => data.harvestQuestCompleted,
                StarterQuestId.Egg => data.eggQuestCompleted,
                StarterQuestId.Weeds => data.weedQuestCompleted,
                StarterQuestId.Fishing => data.fishingQuestCompleted,
                StarterQuestId.Mining => data.miningQuestCompleted,
                _ => true
            };
        }

        private static int GetReward(StarterQuestId id)
        {
            return id switch
            {
                StarterQuestId.Egg => 75,
                StarterQuestId.Fishing => 120,
                StarterQuestId.Mining => 140,
                _ => 100
            };
        }

        private static int PositiveModulo(int value, int divisor)
        {
            var remainder = value % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
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

        private static StarterQuestStatus GetFishing(GameSaveData data)
        {
            return new StarterQuestStatus(StarterQuestId.Fishing, "물고기 2마리 낚기",
                Math.Min(data.fishCaught, FishingTarget), FishingTarget, data.fishingQuestCompleted);
        }

        private static StarterQuestStatus GetMining(GameSaveData data)
        {
            return new StarterQuestStatus(StarterQuestId.Mining, "광석 3개 캐기",
                Math.Min(data.oreMined, MiningTarget), MiningTarget, data.miningQuestCompleted);
        }
    }
}
