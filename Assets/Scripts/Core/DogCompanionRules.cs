namespace FarmGame2D.Core
{
    public readonly struct DogMorningCarePlan
    {
        public DogMorningCarePlan(bool chickensFed, bool cowsFed, int feedSpent)
        {
            ChickensFed = chickensFed;
            CowsFed = cowsFed;
            FeedSpent = feedSpent;
        }

        public bool ChickensFed { get; }
        public bool CowsFed { get; }
        public int FeedSpent { get; }
        public bool Helped => ChickensFed || CowsFed;
    }

    public static class DogCompanionRules
    {
        public static DogMorningCarePlan PlanMorningCare(int availableFeed, int chickenCount, int cowCount)
        {
            var normalizedFeed = System.Math.Max(0, availableFeed);
            var remainingFeed = normalizedFeed;
            var chickens = System.Math.Max(0, chickenCount);
            var cows = System.Math.Max(0, cowCount);
            var chickensFed = chickens > 0 && remainingFeed >= chickens;
            if (chickensFed)
            {
                remainingFeed -= chickens;
            }

            var cowsFed = cows > 0 && remainingFeed >= cows;
            if (cowsFed)
            {
                remainingFeed -= cows;
            }

            return new DogMorningCarePlan(chickensFed, cowsFed, normalizedFeed - remainingFeed);
        }
    }
}
