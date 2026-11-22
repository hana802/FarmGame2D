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
        DeliverWood,
        DeliverStone,
        DeliverCopper,
        DeliverIron,
        DeliverGold,
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
        public const int BoardRefreshDays = 7;
        public const int BoardOfferCount = 10;
        private const int HarvestTarget = 3;
        private const int EggTarget = 1;
        private const int WeedTarget = 3;
        private const int FishingTarget = 2;
        private const int MiningTarget = 3;
        private const int WoodDeliveryTarget = 8;
        private const int StoneDeliveryTarget = 6;
        private const int CopperDeliveryTarget = 4;
        private const int IronDeliveryTarget = 3;
        private const int GoldDeliveryTarget = 2;

        public static StarterQuestStatus GetCurrent(GameSaveData data)
        {
            EnsureCurrentWeek(data);
            return data.activeQuestId < 0
                ? new StarterQuestStatus(StarterQuestId.Complete, "광장 게시판에서 의뢰 선택", 0, 1, false)
                : GetStatus(data, (StarterQuestId)data.activeQuestId);
        }

        public static StarterQuestStatus[] GetAll(GameSaveData data)
        {
            EnsureCurrentWeek(data);
            return CurrentOffers(data);
        }

        public static StarterQuestStatus[] GetBoardOffers(GameSaveData data)
        {
            return GetAll(data);
        }

        public static void EnsureCurrentWeek(GameSaveData data)
        {
            var week = Math.Max(0, data.day - 1) / BoardRefreshDays;
            if (data.questWeekReady && data.questBoardWeek == week)
            {
                return;
            }

            data.questWeekReady = true;
            data.questBoardWeek = week;
            data.activeQuestId = -1;
            data.harvestQuestCompleted = false;
            data.eggQuestCompleted = false;
            data.weedQuestCompleted = false;
            data.fishingQuestCompleted = false;
            data.miningQuestCompleted = false;
            data.woodDeliveryQuestCompleted = false;
            data.stoneDeliveryQuestCompleted = false;
            data.copperDeliveryQuestCompleted = false;
            data.ironDeliveryQuestCompleted = false;
            data.goldDeliveryQuestCompleted = false;
            data.harvestQuestBaseline = data.wheatHarvested;
            data.eggQuestBaseline = data.eggsCollected;
            data.fishQuestBaseline = data.fishCaught;
            data.mineQuestBaseline = data.oreMined;
            data.pulledWeedIds ??= new System.Collections.Generic.List<int>();
            data.pulledWeedIds.Clear();
        }

        public static int GetDaysUntilBoardRefresh(int day)
        {
            return BoardRefreshDays - PositiveModulo(Math.Max(1, day) - 1, BoardRefreshDays);
        }

        public static bool TryAccept(GameSaveData data, StarterQuestId id)
        {
            EnsureCurrentWeek(data);
            if (data.activeQuestId >= 0 || IsCompleted(data, id) || id == StarterQuestId.Complete)
            {
                return false;
            }

            data.activeQuestId = (int)id;
            return true;
        }

        public static StarterQuestCompletion TryCompleteCurrent(GameSaveData data)
        {
            EnsureCurrentWeek(data);
            if (data.activeQuestId < 0)
            {
                return default;
            }

            var status = GetStatus(data, (StarterQuestId)data.activeQuestId);
            if (status.Current < status.Target || !TrySpendDelivery(data, status.Id))
            {
                return default;
            }

            return Complete(data, status.Id, status.Title, GetReward(status.Id));
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
                case StarterQuestId.DeliverWood:
                    data.woodDeliveryQuestCompleted = true;
                    break;
                case StarterQuestId.DeliverStone:
                    data.stoneDeliveryQuestCompleted = true;
                    break;
                case StarterQuestId.DeliverCopper:
                    data.copperDeliveryQuestCompleted = true;
                    break;
                case StarterQuestId.DeliverIron:
                    data.ironDeliveryQuestCompleted = true;
                    break;
                case StarterQuestId.DeliverGold:
                    data.goldDeliveryQuestCompleted = true;
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
                StarterQuestId.DeliverWood or StarterQuestId.DeliverStone
                    or StarterQuestId.DeliverCopper or StarterQuestId.DeliverIron
                    or StarterQuestId.DeliverGold => GetDelivery(data, id),
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
                StarterQuestId.DeliverWood => data.woodDeliveryQuestCompleted,
                StarterQuestId.DeliverStone => data.stoneDeliveryQuestCompleted,
                StarterQuestId.DeliverCopper => data.copperDeliveryQuestCompleted,
                StarterQuestId.DeliverIron => data.ironDeliveryQuestCompleted,
                StarterQuestId.DeliverGold => data.goldDeliveryQuestCompleted,
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
                StarterQuestId.DeliverWood => 160,
                StarterQuestId.DeliverStone => 140,
                StarterQuestId.DeliverCopper => 200,
                StarterQuestId.DeliverIron => 220,
                StarterQuestId.DeliverGold => 260,
                _ => 100
            };
        }

        private static StarterQuestStatus[] CurrentOffers(GameSaveData data)
        {
            return new[]
            {
                GetHarvest(data), GetEgg(data), GetWeeds(data), GetFishing(data), GetMining(data),
                GetDelivery(data, StarterQuestId.DeliverWood),
                GetDelivery(data, StarterQuestId.DeliverStone),
                GetDelivery(data, StarterQuestId.DeliverCopper),
                GetDelivery(data, StarterQuestId.DeliverIron),
                GetDelivery(data, StarterQuestId.DeliverGold)
            };
        }

        private static int WeekCount(int total, int baseline, int target)
        {
            return Math.Min(Math.Max(0, total - baseline), target);
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
                WeekCount(data.wheatHarvested, data.harvestQuestBaseline, HarvestTarget),
                HarvestTarget,
                data.harvestQuestCompleted);
        }

        private static StarterQuestStatus GetEgg(GameSaveData data)
        {
            return new StarterQuestStatus(
                StarterQuestId.Egg,
                "달걀 1개 얻기",
                WeekCount(data.eggsCollected, data.eggQuestBaseline, EggTarget),
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
                WeekCount(data.fishCaught, data.fishQuestBaseline, FishingTarget), FishingTarget, data.fishingQuestCompleted);
        }

        private static StarterQuestStatus GetMining(GameSaveData data)
        {
            return new StarterQuestStatus(StarterQuestId.Mining, "광석 3개 캐기",
                WeekCount(data.oreMined, data.mineQuestBaseline, MiningTarget), MiningTarget, data.miningQuestCompleted);
        }

        private static StarterQuestStatus GetDelivery(GameSaveData data, StarterQuestId id)
        {
            var (item, target, title, completed) = id switch
            {
                StarterQuestId.DeliverWood => (GameItem.Wood, WoodDeliveryTarget, "순자에게 목재 8개", data.woodDeliveryQuestCompleted),
                StarterQuestId.DeliverStone => (GameItem.Stone, StoneDeliveryTarget, "도윤에게 돌 6개", data.stoneDeliveryQuestCompleted),
                StarterQuestId.DeliverCopper => (GameItem.CopperOre, CopperDeliveryTarget, "태오에게 구리 4개", data.copperDeliveryQuestCompleted),
                StarterQuestId.DeliverIron => (GameItem.IronOre, IronDeliveryTarget, "민서에게 철 3개", data.ironDeliveryQuestCompleted),
                _ => (GameItem.GoldOre, GoldDeliveryTarget, "하람에게 금 광석 2개", data.goldDeliveryQuestCompleted)
            };
            var owned = data.inventory?.Count(item) ?? 0;
            return new StarterQuestStatus(id, title, completed ? target : Math.Min(owned, target), target, completed);
        }

        private static bool TrySpendDelivery(GameSaveData data, StarterQuestId id)
        {
            return id switch
            {
                StarterQuestId.DeliverWood => data.inventory.TrySpend(GameItem.Wood, WoodDeliveryTarget),
                StarterQuestId.DeliverStone => data.inventory.TrySpend(GameItem.Stone, StoneDeliveryTarget),
                StarterQuestId.DeliverCopper => data.inventory.TrySpend(GameItem.CopperOre, CopperDeliveryTarget),
                StarterQuestId.DeliverIron => data.inventory.TrySpend(GameItem.IronOre, IronDeliveryTarget),
                StarterQuestId.DeliverGold => data.inventory.TrySpend(GameItem.GoldOre, GoldDeliveryTarget),
                _ => true
            };
        }
    }
}
