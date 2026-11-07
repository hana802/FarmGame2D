using System;
using System.Collections.Generic;
using FarmGame2D.Farming;

namespace FarmGame2D.Core
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int version = 1;
        public int worldSeed;
        public CharacterChoice characterChoice;
        public int money = 200;
        public int day = 1;
        public int hour = 6;
        public int minute;
        public float playerX;
        public float playerY = -12f;
        public InventoryModel inventory = new();
        public List<FarmPlotData> farmPlots = new();
        public bool chickenFedToday;
        public bool eggReady;
        public int wheatHarvested;
        public bool harvestQuestCompleted;
        public bool firstHarvestAchievement;
        public bool firstEggAchievement;
        public List<int> pulledWeedIds = new();

        public static GameSaveData CreateNew(int plotCount, int worldSeed)
        {
            var data = new GameSaveData { worldSeed = worldSeed };
            data.inventory.Add(GameItem.WheatSeed, 8);
            data.inventory.Add(GameItem.Feed, 3);

            for (var index = 0; index < plotCount; index++)
            {
                data.farmPlots.Add(new FarmPlotData());
            }

            return data;
        }
    }
}
