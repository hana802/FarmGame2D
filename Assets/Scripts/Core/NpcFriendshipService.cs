using System;
using System.Collections.Generic;

namespace FarmGame2D.Core
{
    [Serializable]
    public sealed class NpcFriendshipData
    {
        public string npcId = string.Empty;
        public int points;
        public int lastGiftDay;
        public bool endingSeen;
    }

    public enum NpcGiftReaction { Neutral, Liked, Loved }

    public readonly struct NpcGiftResult
    {
        public NpcGiftResult(bool success, string message, NpcGiftReaction reaction, int pointsAdded, int totalPoints)
        {
            Success = success;
            Message = message;
            Reaction = reaction;
            PointsAdded = pointsAdded;
            TotalPoints = totalPoints;
        }

        public bool Success { get; }
        public string Message { get; }
        public NpcGiftReaction Reaction { get; }
        public int PointsAdded { get; }
        public int TotalPoints { get; }
    }

    public static class NpcFriendshipService
    {
        public const int MaxPoints = 100;
        public const int PointsPerHeart = 20;

        private static readonly GameItem[] GiftableItems =
        {
            GameItem.Wheat, GameItem.Egg, GameItem.Potato, GameItem.Strawberry,
            GameItem.Tomato, GameItem.Corn, GameItem.Pumpkin, GameItem.SweetPotato,
            GameItem.Milk, GameItem.Stone, GameItem.CopperOre, GameItem.IronOre,
            GameItem.GoldOre, GameItem.RiverFish, GameItem.SeaBass, GameItem.GoldenCarp
        };

        public static NpcFriendshipData GetOrCreate(GameSaveData data, string npcId)
        {
            if (data == null || string.IsNullOrWhiteSpace(npcId)) return null;

            var existing = Find(data, npcId);
            if (existing != null) return existing;

            data.npcFriendships ??= new List<NpcFriendshipData>();
            var created = new NpcFriendshipData { npcId = npcId };
            data.npcFriendships.Add(created);
            return created;
        }

        private static NpcFriendshipData Find(GameSaveData data, string npcId)
        {
            if (data?.npcFriendships == null || string.IsNullOrWhiteSpace(npcId)) return null;

            foreach (var friendship in data.npcFriendships)
            {
                if (friendship != null && string.Equals(friendship.npcId, npcId, StringComparison.Ordinal)) return friendship;
            }

            return null;
        }

        public static int GetPoints(GameSaveData data, string npcId) => Find(data, npcId)?.points ?? 0;

        public static int GetHeartCount(GameSaveData data, string npcId) => Math.Min(5, GetPoints(data, npcId) / PointsPerHeart);

        public static bool HasGiftedToday(GameSaveData data, string npcId)
        {
            var friendship = GetOrCreate(data, npcId);
            return friendship != null && friendship.lastGiftDay == data.day;
        }

        public static IReadOnlyList<GameItem> GetOwnedGiftableItems(InventoryModel inventory)
        {
            var ownedItems = new List<GameItem>();
            if (inventory == null) return ownedItems;

            foreach (var item in GiftableItems)
            {
                if (inventory.Count(item) > 0) ownedItems.Add(item);
            }

            return ownedItems;
        }

        public static NpcGiftResult TryGiveGift(GameSaveData data, string npcId, GameItem item)
        {
            if (data == null || data.inventory == null || string.IsNullOrWhiteSpace(npcId)) return Failed("지금은 선물을 줄 수 없어요.");

            var friendship = GetOrCreate(data, npcId);
            if (friendship.lastGiftDay == data.day) return Failed("오늘은 이미 선물을 건넸어요. 내일 다시 선물할 수 있어요.", friendship.points);
            if (!IsGiftable(item) || data.inventory.Count(item) <= 0) return Failed("선물할 수 있는 물건이 없어요.", friendship.points);

            var reaction = GetReaction(npcId, item);
            var pointsAdded = reaction switch
            {
                NpcGiftReaction.Loved => 20,
                NpcGiftReaction.Liked => 10,
                _ => 4
            };

            if (!data.inventory.TrySpend(item, 1)) return Failed("선물할 물건이 부족해요.", friendship.points);

            friendship.points = Math.Min(MaxPoints, friendship.points + pointsAdded);
            friendship.lastGiftDay = data.day;
            return new NpcGiftResult(true, ReactionMessage(reaction), reaction, pointsAdded, friendship.points);
        }

        private static bool IsGiftable(GameItem item)
        {
            foreach (var giftableItem in GiftableItems)
            {
                if (giftableItem == item) return true;
            }

            return false;
        }

        private static NpcGiftReaction GetReaction(string npcId, GameItem item)
        {
            if ((npcId == "Guide" && item == GameItem.Potato)
                || (npcId == "Elder" && item == GameItem.SweetPotato)
                || (npcId == "Young Villager" && item == GameItem.Corn)
                || (npcId == "Fisher Minseo" && item == GameItem.GoldenCarp)
                || (npcId == "Florist Taeo" && item == GameItem.Strawberry)
                || (npcId == "Baker Yuna" && item == GameItem.Wheat)
                || (npcId == "Rancher Sora" && item == GameItem.Milk)) return NpcGiftReaction.Loved;

            if ((npcId == "Guide" && (item == GameItem.Strawberry || item == GameItem.GoldenCarp))
                || (npcId == "Elder" && (item == GameItem.Milk || item == GameItem.Wheat))
                || (npcId == "Young Villager" && (item == GameItem.Tomato || item == GameItem.RiverFish))
                || (npcId == "Fisher Minseo" && (item == GameItem.SeaBass || item == GameItem.RiverFish))
                || (npcId == "Florist Taeo" && (item == GameItem.Pumpkin || item == GameItem.Tomato))
                || (npcId == "Baker Yuna" && (item == GameItem.Strawberry || item == GameItem.Pumpkin || item == GameItem.Milk))
                || (npcId == "Rancher Sora" && (item == GameItem.Egg || item == GameItem.Corn))) return NpcGiftReaction.Liked;

            return NpcGiftReaction.Neutral;
        }

        private static string ReactionMessage(NpcGiftReaction reaction)
        {
            return reaction switch
            {
                NpcGiftReaction.Loved => "정말 좋아하는 선물이에요! 고마워요!",
                NpcGiftReaction.Liked => "마음에 들어요. 잘 간직할게요!",
                _ => "선물 고마워요. 마음이 참 따뜻하네요."
            };
        }

        private static NpcGiftResult Failed(string message, int totalPoints = 0)
            => new(false, message, NpcGiftReaction.Neutral, 0, totalPoints);
    }
}
