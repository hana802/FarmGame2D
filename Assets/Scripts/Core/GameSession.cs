using System;
using FarmGame2D.Farming;
using FarmGame2D.TimeSystem;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed class GameSession : MonoBehaviour
    {
        private const int PlotCount = 6;
        private const int HarvestQuestTarget = 3;

        private readonly SaveService saveService = new();
        private Transform playerTransform;
        private int processedDay;
        private string message = "안내자를 찾아가 농장 생활을 시작하세요.";

        public static GameSession Instance { get; private set; }
        public GameSaveData Data { get; private set; }
        public FarmTool SelectedTool { get; private set; } = FarmTool.Hoe;
        public bool PhoneOpen { get; private set; }
        public bool ShopOpen { get; private set; }
        public string Message
        {
            get => message;
            private set
            {
                message = value;
                MessageRevision++;
            }
        }
        public int MessageRevision { get; private set; }
        public Season CurrentSeason => SeasonCalendar.GetSeason(Data.day);

        public event Action StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Data = saveService.TryLoad(out var loaded)
                ? loaded
                : GameSaveData.CreateNew(PlotCount, UnityEngine.Random.Range(10000, 999999));
            EnsurePlotCount();
            processedDay = Data.day;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            var clock = GameClockController.Instance;
            if (clock == null)
            {
                return;
            }

            clock.SetTime(Data.day, Data.hour, Data.minute);
            clock.TimeChanged += HandleTimeChanged;
        }

        private void OnDestroy()
        {
            if (GameClockController.Instance != null)
            {
                GameClockController.Instance.TimeChanged -= HandleTimeChanged;
            }
        }

        private void Update()
        {
            HandleToolInput();

            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePhone();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Save(false);
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                Load();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                AdvanceToNextSeason();
            }
        }

        public void RegisterPlayer(Transform player)
        {
            playerTransform = player;
            player.position = new Vector3(Data.playerX, Data.playerY, 0f);
        }

        public void ChooseCharacter(CharacterChoice choice)
        {
            if (choice == CharacterChoice.None)
            {
                return;
            }

            Data.characterChoice = choice;
            Message = choice == CharacterChoice.Male ? "남성형 농부를 선택했습니다." : "여성형 농부를 선택했습니다.";
            NotifyChanged();
        }

        public void UsePlot(int index)
        {
            if (index < 0 || index >= Data.farmPlots.Count)
            {
                return;
            }

            var plot = Data.farmPlots[index];
            if (plot.state == SoilState.Ready)
            {
                Harvest(plot);
                return;
            }

            switch (SelectedTool)
            {
                case FarmTool.Hoe when plot.state == SoilState.Empty:
                    plot.state = SoilState.Tilled;
                    Message = "땅을 갈았습니다.";
                    break;
                case FarmTool.Seeds when plot.state == SoilState.Tilled:
                    Plant(plot);
                    break;
                case FarmTool.WateringCan when plot.state == SoilState.Planted:
                    plot.state = SoilState.Watered;
                    Message = "밀에 물을 주었습니다.";
                    break;
                default:
                    Message = "지금 선택한 도구로는 할 수 없습니다.";
                    break;
            }

            NotifyChanged();
        }

        public void InteractChicken()
        {
            if (Data.eggReady)
            {
                Data.eggReady = false;
                Data.inventory.Add(GameItem.Egg, 1);
                Data.firstEggAchievement = true;
                Message = "달걀을 1개 모았습니다. 도전 과제: 첫 달걀!";
                NotifyChanged();
                return;
            }

            if (Data.chickenFedToday)
            {
                Message = "닭은 오늘 이미 먹이를 먹었습니다.";
                return;
            }

            if (!Data.inventory.TrySpend(GameItem.Feed, 1))
            {
                Message = "사료가 없습니다. 마을 상점에서 구입하세요.";
                return;
            }

            Data.chickenFedToday = true;
            Message = "닭에게 먹이를 주었습니다.";
            NotifyChanged();
        }

        public void PurchaseSeeds()
        {
            Purchase(GameItem.WheatSeed, 5, 20, "밀 씨앗 5개");
        }

        public void PurchaseFeed()
        {
            Purchase(GameItem.Feed, 3, 15, "사료 3개");
        }

        public void SellAllProduce()
        {
            var wheat = Data.inventory.Count(GameItem.Wheat);
            var eggs = Data.inventory.Count(GameItem.Egg);
            if (wheat == 0 && eggs == 0)
            {
                Message = "판매할 밀이나 달걀이 없습니다.";
                return;
            }

            Data.inventory.TrySpend(GameItem.Wheat, wheat);
            Data.inventory.TrySpend(GameItem.Egg, eggs);
            var income = (wheat * 15) + (eggs * 25);
            Data.money += income;
            Message = $"판매 상자에 넣어 {income}G를 받았습니다.";
            NotifyChanged();
        }

        public void Sleep()
        {
            var clock = GameClockController.Instance;
            if (clock == null)
            {
                return;
            }

            clock.SetTime(Data.day + 1, 6, 0);
            Message = "새로운 아침입니다. 자동 저장했습니다.";
            Save(true);
        }

        public void TogglePhone()
        {
            PhoneOpen = !PhoneOpen;
            ShopOpen = false;
            NotifyChanged();
        }

        public void ToggleShop()
        {
            ShopOpen = !ShopOpen;
            PhoneOpen = false;
            NotifyChanged();
        }

        public void ShowMessage(string message)
        {
            Message = message;
            NotifyChanged();
        }

        public void Save(bool automatic)
        {
            CapturePlayerPosition();
            var saved = saveService.TrySave(Data);
            Message = saved
                ? automatic ? "자동 저장했습니다." : "휴대폰 저장을 완료했습니다."
                : "저장에 실패했습니다. 저장 폴더 권한을 확인하세요.";
            NotifyChanged();
        }

        public void Load()
        {
            if (!saveService.TryLoad(out var loaded))
            {
                Message = "불러올 저장 데이터가 없습니다.";
                return;
            }

            Data = loaded;
            EnsurePlotCount();
            processedDay = Data.day;
            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(Data.playerX, Data.playerY, 0f);
            }

            GameClockController.Instance?.SetTime(Data.day, Data.hour, Data.minute);
            Message = "저장 데이터를 불러왔습니다.";
            NotifyChanged();
        }

        private void HandleToolInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectedTool = FarmTool.Hoe;
                NotifyChanged();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectedTool = FarmTool.Seeds;
                NotifyChanged();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectedTool = FarmTool.WateringCan;
                NotifyChanged();
            }
        }

        private void HandleTimeChanged(GameTimeSnapshot snapshot)
        {
            Data.day = snapshot.Day;
            Data.hour = snapshot.Hour;
            Data.minute = snapshot.Minute;

            while (processedDay < snapshot.Day)
            {
                processedDay++;
                ProcessNewDay();
            }

            NotifyChanged();
        }

        private void ProcessNewDay()
        {
            foreach (var plot in Data.farmPlots)
            {
                plot.AdvanceDay();
            }

            Data.eggReady = Data.chickenFedToday;
            Data.chickenFedToday = false;
        }

        private void Plant(FarmPlotData plot)
        {
            if (!Data.inventory.TrySpend(GameItem.WheatSeed, 1))
            {
                Message = "밀 씨앗이 없습니다.";
                return;
            }

            plot.state = SoilState.Planted;
            plot.growthDays = 0;
            Message = "밀 씨앗을 심었습니다.";
        }

        private void Harvest(FarmPlotData plot)
        {
            plot.Reset();
            Data.inventory.Add(GameItem.Wheat, 1);
            Data.wheatHarvested++;
            Data.firstHarvestAchievement = true;
            Message = "밀을 수확했습니다. 도전 과제: 첫 수확!";

            if (!Data.harvestQuestCompleted && Data.wheatHarvested >= HarvestQuestTarget)
            {
                Data.harvestQuestCompleted = true;
                Data.money += 100;
                Message = "퀘스트 완료: 밀 3개 수확! 보상 100G";
            }

            NotifyChanged();
        }

        private void Purchase(GameItem item, int quantity, int price, string label)
        {
            if (Data.money < price)
            {
                Message = "돈이 부족합니다.";
                return;
            }

            Data.money -= price;
            Data.inventory.Add(item, quantity);
            Message = $"{label}를 구입했습니다.";
            NotifyChanged();
        }

        private void AdvanceToNextSeason()
        {
            var nextSeasonDay = (((Data.day - 1) / SeasonCalendar.DaysPerSeason) + 1) * SeasonCalendar.DaysPerSeason + 1;
            GameClockController.Instance?.SetTime(nextSeasonDay, Data.hour, Data.minute);
            Message = "계절 전환 디버그 기능을 사용했습니다.";
        }

        private void EnsurePlotCount()
        {
            Data.inventory ??= new InventoryModel();
            Data.farmPlots ??= new System.Collections.Generic.List<FarmPlotData>();
            while (Data.farmPlots.Count < PlotCount)
            {
                Data.farmPlots.Add(new FarmPlotData());
            }
        }

        private void CapturePlayerPosition()
        {
            if (playerTransform == null)
            {
                return;
            }

            Data.playerX = playerTransform.position.x;
            Data.playerY = playerTransform.position.y;
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
