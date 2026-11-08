using System;
using System.Collections.Generic;
using FarmGame2D.Farming;
using FarmGame2D.World;

namespace FarmGame2D.Core
{
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentVersion = 4;

        public int version = CurrentVersion;
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
        public int readyEggs;
        public int readyMilk;
        public int ownedChickens = 1;
        public int ownedCows;
        public List<int> chickenJuvenileDays = new();
        public List<int> cowJuvenileDays = new();
        public bool cowFedToday;
        public bool hasPickaxe;
        public int wheatHarvested;
        public bool harvestQuestCompleted;
        public bool eggQuestCompleted;
        public bool weedQuestCompleted;
        public bool fishingQuestCompleted;
        public bool miningQuestCompleted;
        public int activeQuestId = -1;
        public int fishCaught;
        public int oreMined;
        public bool firstHarvestAchievement;
        public bool firstEggAchievement;
        public List<int> pulledWeedIds = new();
        public List<MiningNodeData> miningNodes = new();

        public static GameSaveData CreateNew(int plotCount, int worldSeed)
        {
            var data = new GameSaveData { worldSeed = worldSeed };
            data.inventory.Add(GameItem.PotatoSeed, 6);
            data.inventory.Add(GameItem.StrawberrySeed, 3);
            data.inventory.Add(GameItem.Feed, 3);

            for (var index = 0; index < plotCount; index++)
            {
                data.farmPlots.Add(new FarmPlotData());
            }

            return data;
        }
    }
}
