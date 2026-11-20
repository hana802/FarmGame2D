using System;

namespace FarmGame2D.Core
{
    public enum BondEventKind { NpcEnding, DogCompanion, DogEnding, MarriageProposal, Wedding }

    public readonly struct BondEventDefinition
    {
        public BondEventDefinition(BondEventKind kind, string npcId, string title, params string[] lines)
        {
            Kind = kind;
            NpcId = npcId;
            Title = title;
            Lines = lines;
        }

        public BondEventKind Kind { get; }
        public string NpcId { get; }
        public string Title { get; }
        public string[] Lines { get; }
    }

    public static class BondEventService
    {
        public const int DogCompanionThreshold = 10;
        public const int DogEndingThreshold = 50;

        public static BondEventDefinition? GetPendingNpcEnding(GameSaveData data, string npcId)
        {
            var friendship = NpcFriendshipService.GetOrCreate(data, npcId);
            if (friendship == null || friendship.points < NpcFriendshipService.MaxPoints || friendship.endingSeen) return null;
            return CreateNpcEnding(npcId);
        }

        public static BondEventDefinition? GetNpcEndingForReplay(GameSaveData data, string npcId)
        {
            var friendship = NpcFriendshipService.GetOrCreate(data, npcId);
            return friendship != null && friendship.points >= NpcFriendshipService.MaxPoints ? CreateNpcEnding(npcId) : null;
        }

        public static BondEventDefinition? GetMarriageProposal(GameSaveData data, string npcId)
        {
            var friendship = NpcFriendshipService.GetOrCreate(data, npcId);
            if (friendship == null || friendship.points < NpcFriendshipService.MaxPoints || !string.IsNullOrEmpty(data.spouseNpcId)) return null;
            return new BondEventDefinition(BondEventKind.MarriageProposal, npcId, "함께할래요?",
                "우리가 함께 보낸 시간이 참 소중해요. 앞으로도 당신과 더 많은 계절을 맞고 싶어요.",
                "저와 결혼해 줄래요? 천천히 생각해도 괜찮아요.");
        }

        public static bool TryAcceptMarriage(GameSaveData data, string npcId)
        {
            if (!GetMarriageProposal(data, npcId).HasValue) return false;
            data.spouseNpcId = npcId;
            data.weddingSeen = false;
            return true;
        }

        public static BondEventDefinition? GetPendingWedding(GameSaveData data, string npcId)
        {
            return IsSpouse(data, npcId) && !data.weddingSeen ? CreateWedding(npcId) : null;
        }

        public static BondEventDefinition? GetWeddingForReplay(GameSaveData data, string npcId)
        {
            return IsSpouse(data, npcId) && data.weddingSeen ? CreateWedding(npcId) : null;
        }

        public static BondEventDefinition? GetPendingDogEvent(GameSaveData data)
        {
            if (data == null) return null;
            if (data.dogAffection >= DogEndingThreshold && !data.dogEndingSeen)
                return new BondEventDefinition(BondEventKind.DogEnding, string.Empty, "언제나 함께",
                    "처음 만났을 때는 조심스레 다가오던 강아지가 이제는 당신의 발소리만 들어도 달려옵니다.",
                    "당신이 밭을 돌보고 마을을 거닐던 모든 날에, 이 작은 친구도 곁에 있었습니다.",
                    "강아지가 손바닥에 머리를 기대고 꼬리를 흔듭니다. 앞으로도 함께할 거예요. — 강아지 우정 엔딩");
            if (data.dogAffection >= DogCompanionThreshold && !data.dogCompanionEventSeen && !data.dogEndingSeen)
                return new BondEventDefinition(BondEventKind.DogCompanion, string.Empty, "친구가 된 날",
                    "강아지가 당신을 보자마자 신나게 달려와 빙글빙글 돕니다.",
                    "이제 강아지는 당신을 믿고 따릅니다. 마을에서 가장 든든한 작은 친구가 생겼어요.");
            return null;
        }

        public static void Complete(GameSaveData data, BondEventDefinition bondEvent)
        {
            if (data == null) return;
            switch (bondEvent.Kind)
            {
                case BondEventKind.NpcEnding:
                    var friendship = NpcFriendshipService.GetOrCreate(data, bondEvent.NpcId);
                    if (friendship != null && friendship.points >= NpcFriendshipService.MaxPoints) friendship.endingSeen = true;
                    break;
                case BondEventKind.DogCompanion:
                    data.dogCompanionEventSeen = true;
                    break;
                case BondEventKind.DogEnding:
                    data.dogCompanionEventSeen = true;
                    data.dogEndingSeen = true;
                    break;
                case BondEventKind.Wedding:
                    if (IsSpouse(data, bondEvent.NpcId)) data.weddingSeen = true;
                    break;
            }
        }

        internal static bool IsSpouse(GameSaveData data, string npcId)
        {
            return data != null && !string.IsNullOrWhiteSpace(npcId) &&
                   string.Equals(data.spouseNpcId, npcId, StringComparison.Ordinal);
        }

        private static BondEventDefinition CreateWedding(string npcId)
        {
            var name = npcId switch
            {
                "Guide" => "하람", "Elder" => "순자", "Young Villager" => "도윤",
                "Fisher Minseo" => "민서", "Florist Taeo" => "태오",
                "Baker Yuna" => "유나", "Rancher Sora" => "소라", _ => "소중한 사람"
            };
            return new BondEventDefinition(BondEventKind.Wedding, npcId, "우리의 결혼식",
                $"마을 사람들의 축하 속에서 당신과 {name}가 나란히 섰습니다.",
                $"{name}가 당신의 손을 잡습니다. 함께 기쁜 날도 어려운 날도 나누기로 약속합니다.",
                "두 사람은 부부가 되었습니다. 이 마을에서 새로운 이야기가 시작됩니다. — 결혼 엔딩");
        }

        private static BondEventDefinition CreateNpcEnding(string npcId)
        {
            var lines = npcId switch
            {
                "Guide" => new[] { "처음 이 마을에 왔던 날을 기억하나요? 제가 길을 알려 드렸죠.", "지금은 오히려 당신 덕분에 마을이 더 따뜻해졌어요. 언제든 돌아올 수 있는 곳이 여기 있어요.", "서로를 믿는 친구가 되었습니다. — 안내인의 우정 엔딩" },
                "Elder" => new[] { "오래 살다 보면 계절마다 남는 얼굴이 있단다. 너는 내게 그런 사람이 되었어.", "네가 가꾼 밭처럼 우리 사이도 천천히 자랐지. 이 마을은 언제나 네 집이란다.", "마을 어르신이 당신의 손을 꼭 잡습니다. — 어르신의 우정 엔딩" },
                "Young Villager" => new[] { "처음에는 그냥 스쳐 지나가는 이웃일 줄 알았어요.", "그런데 어느새 기쁜 일이 생기면 제일 먼저 말하고 싶은 사람이 당신이네요.", "둘은 다음 계절에도 함께 놀러 가기로 약속합니다. — 마을 친구의 우정 엔딩" },
                "Fisher Minseo" => new[] { "물고기를 기다리는 시간은 길지만, 당신과 함께라면 금방 지나가요.", "다음에 좋은 자리를 발견하면 제일 먼저 알려 줄게요. 우리만의 낚시터가 생긴 셈이죠.", "민서가 오래 아껴 둔 낚시 이야기를 들려줍니다. — 민서의 우정 엔딩" },
                "Florist Taeo" => new[] { "꽃은 돌봐 주는 사람의 마음을 알고 피어난다고 믿어요.", "당신이 이 마을에 와서, 제 정원에도 전에 없던 색이 더해졌어요.", "태오가 가장 곱게 핀 꽃 한 송이를 건넵니다. — 태오의 우정 엔딩" },
                "Baker Yuna" => new[] { "새벽마다 빵을 굽다 보면 누가 첫 손님일지 생각하게 돼요.", "요즘은 당신이 들를까 봐 가장 따뜻한 빵을 남겨 두게 되네요.", "유나와 갓 구운 빵을 나눠 먹으며 긴 이야기를 합니다. — 유나의 우정 엔딩" },
                "Rancher Sora" => new[] { "동물들은 믿을 만한 사람을 금방 알아봐요. 당신 곁에선 모두 편안해 보여요.", "우리도 그렇게 시간을 보내며 서로를 믿게 됐네요. 힘든 날엔 목장으로 와요.", "소라와 함께 해 질 녘 목장을 바라봅니다. — 소라의 우정 엔딩" },
                _ => new[] { "함께 보낸 시간이 어느새 이렇게 쌓였네요.", "당신은 이제 이 마을에서 소중한 친구예요.", "앞으로도 함께 좋은 추억을 만들기로 약속합니다. — 우정 엔딩" }
            };
            return new BondEventDefinition(BondEventKind.NpcEnding, npcId, "우리의 인연", lines);
        }
    }
}
