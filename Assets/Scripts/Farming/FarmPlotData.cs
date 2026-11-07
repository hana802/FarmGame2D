using System;

namespace FarmGame2D.Farming
{
    public enum SoilState
    {
        Empty,
        Tilled,
        Planted,
        Watered,
        Ready
    }

    [Serializable]
    public sealed class FarmPlotData
    {
        public SoilState state;
        public int growthDays;

        public void AdvanceDay()
        {
            if (state != SoilState.Watered)
            {
                return;
            }

            growthDays++;
            state = growthDays >= 2 ? SoilState.Ready : SoilState.Planted;
        }

        public void Reset()
        {
            state = SoilState.Empty;
            growthDays = 0;
        }
    }
}
