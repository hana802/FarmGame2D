using System;
using FarmGame2D.Core;

namespace FarmGame2D.World
{
    [Serializable]
    public sealed class MiningNodeData
    {
        public int id;
        public GameItem ore;
        public int regrowDaysRemaining;

        public bool IsAvailable => regrowDaysRemaining <= 0;

        public void Mine(int regrowDays = 2)
        {
            regrowDaysRemaining = regrowDays;
        }

        public void AdvanceDay()
        {
            if (regrowDaysRemaining > 0)
            {
                regrowDaysRemaining--;
            }
        }
    }
}
