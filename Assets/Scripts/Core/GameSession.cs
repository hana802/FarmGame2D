using System;
using FarmGame2D.Farming;
using FarmGame2D.TimeSystem;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed class GameSession : MonoBehaviour
    {
        private readonly SaveService saveService = new();
        private readonly CloudSaveClient cloudSaveClient = new();
        private Transform playerTransform;
        private int processedDay;
        private string message = "안내자를 찾아가 농장 생활을 시작하세요.";
        private string activeSpeaker = string.Empty;
        private int activePortraitRow = -1;
        private float messageDisplaySeconds = 1f;
        private bool cloudSyncBusy;
        private bool hadLocalSaveAtStartup;
        private bool cloudLoadRequired;
        private string lastMorningEvent = string.Empty;

        public static GameSession Instance { get; private set; }
        public GameSaveData Data { get; private set; }
        public FarmTool SelectedTool { get; private set; } = FarmTool.Hoe;
        public GameItem SelectedSeed { get; private set; } = GameItem.PotatoSeed;
        public ShopKind ActiveShop { get; private set; } = ShopKind.SeedsAndAnimals;
        public bool PhoneOpen { get; private set; }
        public bool ShopOpen { get; private set; }
        public bool InventoryOpen { get; private set; }
        public bool QuestBoardOpen { get; private set; }
        public Vector2 PlayerPosition => playerTransform == null ? Vector2.zero : playerTransform.position;
        public bool CloudSyncBusy => cloudSyncBusy;
        public bool CloudSyncLinked => cloudSaveClient.IsLinked;
        public string CloudSyncCode => cloudSaveClient.SyncCode;
        public string NewlyCreatedCloudPin { get; private set; } = string.Empty;
        public bool CloudLoadRequired => cloudLoadRequired;
        public string Message
        {
            get => message;
            private set
            {
                message = value;
                activeSpeaker = string.Empty;
                activePortraitRow = -1;
                messageDisplaySeconds = 1f;
                MessageRevision++;
            }
        }
        public int MessageRevision { get; private set; }
        public string ActiveSpeaker => activeSpeaker;
        public int ActivePortraitRow => activePortraitRow;
        public float MessageDisplaySeconds => messageDisplaySeconds;
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
            hadLocalSaveAtStartup = saveService.TryLoad(out var loaded);
            Data = hadLocalSaveAtStartup
                ? loaded
                : GameSaveData.CreateNew(FarmFieldLayout.PlotCount, UnityEngine.Random.Range(10000, 999999));
            EnsurePlotCount();
            SelectFirstOwnedSeed();
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
            if (!Application.isBatchMode)
            {
                BeginAutomaticDeviceSync();
            }
        }

        private void OnDestroy()
        {
            if (GameClockController.Instance != null)
            {
                GameClockController.Instance.TimeChanged -= HandleTimeChanged;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && Data != null)
            {
                SaveSilently();
            }
        }

        private void OnApplicationQuit()
        {
            if (Data != null)
            {
                SaveSilently();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePhone();
            }

            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.B))
            {
                ToggleInventory();
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
                case FarmTool.Shovel:
                    RemoveFreeformPlot(index);
                    return;
                case FarmTool.Hoe when plot.state == SoilState.Empty:
                    plot.state = SoilState.Tilled;
                    Message = "호미로 새 밭을 만들었습니다.";
                    break;
                case FarmTool.Seeds when plot.state == SoilState.Tilled:
                    Plant(plot);
                    break;
                case FarmTool.WateringCan when plot.state == SoilState.Planted:
                    plot.state = SoilState.Watered;
                    Message = $"{GetCropLabel(plot)}에 물을 주었습니다.";
                    break;
                default:
                    Message = "지금 선택한 도구로는 할 수 없습니다.";
                    break;
            }

            NotifyChanged();
        }

        public void InteractChicken()
        {
            if (Data.readyEggs > 0)
            {
                var collected = Data.readyEggs;
                Data.readyEggs = 0;
                Data.inventory.Add(GameItem.Egg, collected);
                Data.firstEggAchievement = true;
                Message = $"달걀을 {collected}개 모았습니다. 도전 과제: 첫 달걀!";
                CompleteCurrentStarterQuest();
                NotifyChanged();
                return;
            }

            if (Data.chickenFedToday)
            {
                Message = "닭은 오늘 이미 먹이를 먹었습니다.";
                return;
            }

            if (!Data.inventory.TrySpend(GameItem.Feed, Data.ownedChickens))
            {
                Message = $"닭 {Data.ownedChickens}마리에게 줄 사료가 부족합니다.";
                return;
            }

            Data.chickenFedToday = true;
            Message = "닭에게 먹이를 주었습니다.";
            NotifyChanged();
        }

        public void InteractCow()
        {
            if (Data.ownedCows <= 0)
            {
                Message = "아직 소가 없습니다. 씨앗·동물 상점에서 구입할 수 있습니다.";
                NotifyChanged();
                return;
            }

            if (Data.readyMilk > 0)
            {
                var collected = Data.readyMilk;
                Data.readyMilk = 0;
                Data.inventory.Add(GameItem.Milk, collected);
                Message = $"우유를 {collected}개 모았습니다.";
                NotifyChanged();
                return;
            }

            if (Data.cowFedToday)
            {
                Message = "소들은 오늘 이미 먹이를 먹었습니다.";
                return;
            }

            if (!Data.inventory.TrySpend(GameItem.Feed, Data.ownedCows))
            {
                Message = $"소 {Data.ownedCows}마리에게 줄 사료가 부족합니다.";
                return;
            }

            Data.cowFedToday = true;
            Message = "소들에게 사료를 주었습니다. 내일 우유를 얻을 수 있습니다.";
            NotifyChanged();
        }

        public void PetDog()
        {
            Data.dogAffection = Mathf.Min(999, Data.dogAffection + 1);
            Message = Data.dogAffection switch
            {
                1 => "강아지를 쓰다듬었습니다. 꼬리를 세차게 흔듭니다!",
                5 => "강아지가 아주 가까운 친구가 되었습니다!",
                _ => $"강아지가 기뻐합니다. 친밀도 {Data.dogAffection}"
            };
            NotifyChanged();
            SaveSilently();
        }

        public void PurchaseSeed(GameItem seed)
        {
            if (!CropCatalog.TryGetBySeed(seed, out var crop) || crop.Season != CurrentSeason)
            {
                Message = "지금 계절에는 판매하지 않는 씨앗입니다.";
                return;
            }

            Purchase(seed, 5, crop.PackPrice, $"{crop.Label} 씨앗 5개");
        }

        public void PurchaseFeed()
        {
            Purchase(GameItem.Feed, 3, 15, "사료 3개");
        }

        public void PurchaseChicken()
        {
            if (PurchaseWithoutItem(150, "닭 1마리"))
            {
                Data.ownedChickens++;
                NotifyChanged();
                SaveSilently();
            }
        }

        public bool SellChicken()
        {
            const int salePrice = 100;
            if (Data.ownedChickens <= 1)
            {
                Message = "마지막 닭은 판매할 수 없습니다.";
                NotifyChanged();
                return false;
            }

            var adultCount = Data.ownedChickens - Data.chickenJuvenileDays.Count;
            if (adultCount <= 0 && Data.chickenJuvenileDays.Count > 0)
            {
                Data.chickenJuvenileDays.RemoveAt(Data.chickenJuvenileDays.Count - 1);
            }

            Data.ownedChickens--;
            Data.money += salePrice;
            Message = $"닭 1마리를 판매해 {salePrice}G를 받았습니다.";
            NotifyChanged();
            SaveSilently();
            return true;
        }

        public void PurchaseCow()
        {
            if (PurchaseWithoutItem(350, "소 1마리"))
            {
                Data.ownedCows++;
                NotifyChanged();
                SaveSilently();
            }
        }

        public void PurchasePickaxe()
        {
            if (Data.hasPickaxe)
            {
                Message = "이미 곡괭이를 가지고 있습니다.";
                return;
            }

            if (PurchaseWithoutItem(120, "곡괭이"))
            {
                Data.hasPickaxe = true;
                NotifyChanged();
                SaveSilently();
            }
        }

        public void PurchaseOre(GameItem ore, int quantity, int price)
        {
            if (ore != GameItem.Stone && ore != GameItem.CopperOre && ore != GameItem.IronOre)
            {
                return;
            }

            Purchase(ore, quantity, price, $"{ItemCatalog.GetLabel(ore)} {quantity}개");
        }

        public void SellAllProduce()
        {
            var income = 0;
            foreach (GameItem item in Enum.GetValues(typeof(GameItem)))
            {
                var price = ItemCatalog.GetSellPrice(item);
                var quantity = Data.inventory.Count(item);
                if (price <= 0 || quantity <= 0)
                {
                    continue;
                }

                Data.inventory.TrySpend(item, quantity);
                income += quantity * price;
            }

            if (income == 0)
            {
                Message = "판매할 농산물이나 광물이 없습니다.";
                return;
            }

            Data.money += income;
            Message = $"농산물과 광물을 판매해 {income}G를 받았습니다.";
            NotifyChanged();
            SaveSilently();
        }

        public void Sleep()
        {
            var clock = GameClockController.Instance;
            if (clock == null)
            {
                return;
            }

            clock.SetTime(Data.day + 1, 6, 0);
            Message = string.IsNullOrEmpty(lastMorningEvent)
                ? "새로운 아침입니다. 자동 저장했습니다."
                : $"새로운 아침입니다. {lastMorningEvent} 자동 저장했습니다.";
            SaveSilently();
            NotifyChanged();
        }

        public void TogglePhone()
        {
            PhoneOpen = !PhoneOpen;
            ShopOpen = false;
            InventoryOpen = false;
            QuestBoardOpen = false;
            NotifyChanged();
        }

        public void ToggleShop()
        {
            OpenShop(ShopKind.SeedsAndAnimals);
        }

        public void OpenShop(ShopKind shop)
        {
            var shouldOpen = !ShopOpen || ActiveShop != shop;
            ActiveShop = shop;
            ShopOpen = shouldOpen;
            PhoneOpen = false;
            InventoryOpen = false;
            QuestBoardOpen = false;
            NotifyChanged();
        }

        public void ToggleInventory()
        {
            InventoryOpen = !InventoryOpen;
            PhoneOpen = false;
            ShopOpen = false;
            QuestBoardOpen = false;
            NotifyChanged();
        }

        public void OpenQuestBoard()
        {
            QuestBoardOpen = true;
            PhoneOpen = false;
            ShopOpen = false;
            InventoryOpen = false;
            NotifyChanged();
        }

        public void CloseQuestBoard()
        {
            QuestBoardOpen = false;
            NotifyChanged();
        }

        public void AcceptQuest(StarterQuestId questId)
        {
            if (!StarterQuestProgress.TryAccept(Data, questId))
            {
                Message = Data.activeQuestId >= 0 ? "먼저 진행 중인 의뢰를 완료하세요." : "이미 완료한 의뢰입니다.";
                NotifyChanged();
                return;
            }

            var quest = StarterQuestProgress.GetCurrent(Data);
            QuestBoardOpen = false;
            Message = $"의뢰를 수락했습니다: {quest.Title}";
            CompleteCurrentStarterQuest();
            NotifyChanged();
            SaveSilently();
        }

        public bool AddFreeformPlot(Vector2 position)
        {
            if (FarmFieldLayout.HasPlotNear(Data.farmPlots, position))
            {
                Message = "이미 밭이 있는 자리입니다.";
                NotifyChanged();
                return false;
            }

            Data.farmPlots.Add(new FarmPlotData
            {
                state = SoilState.Tilled,
                freeform = true,
                worldX = position.x,
                worldY = position.y
            });
            Message = "호미로 새 밭을 만들었습니다.";
            NotifyChanged();
            SaveSilently();
            return true;
        }

        public bool RemoveFreeformPlot(int index)
        {
            if (index < 0 || index >= Data.farmPlots.Count)
            {
                return false;
            }

            var plot = Data.farmPlots[index];
            if (!plot.freeform)
            {
                Message = "기본 지형은 삽으로 없앨 수 없습니다.";
                NotifyChanged();
                return false;
            }

            if (plot.state != SoilState.Tilled)
            {
                Message = "작물이 있는 밭은 먼저 수확해야 없앨 수 있습니다.";
                NotifyChanged();
                return false;
            }

            Data.farmPlots.RemoveAt(index);
            Message = "삽으로 밭을 다시 잔디로 되돌렸습니다.";
            NotifyChanged();
            SaveSilently();
            return true;
        }

        public void CatchFish()
        {
            var roll = Mathf.Abs(unchecked((Data.worldSeed * 31) + (Data.day * 17) + (Data.fishCaught * 13))) % 100;
            var fish = roll < 8 ? GameItem.GoldenCarp : roll < 43 ? GameItem.SeaBass : GameItem.RiverFish;
            Data.inventory.Add(fish, 1);
            Data.fishCaught++;
            Message = $"{ItemCatalog.GetLabel(fish)}을(를) 낚았습니다!";
            CompleteCurrentStarterQuest();
            NotifyChanged();
            SaveSilently();
        }

        public void ShowMessage(string message)
        {
            Message = message;
            NotifyChanged();
        }

        public void ShowDialogue(string speaker, string dialogue, int portraitRow)
        {
            Message = dialogue;
            activeSpeaker = speaker;
            activePortraitRow = portraitRow;
            messageDisplaySeconds = 4f;
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

            if (saved && cloudSaveClient.IsLinked)
            {
                UploadCloudSave();
            }
        }

        public void Load()
        {
            if (!saveService.TryLoad(out var loaded))
            {
                Message = "불러올 저장 데이터가 없습니다.";
                return;
            }

            ApplyLoadedData(loaded, "저장 데이터를 불러왔습니다.");
        }

        public void CreateCloudSave()
        {
            if (cloudSyncBusy)
            {
                ShowCloudMessage("동기화 작업이 진행 중입니다.");
                return;
            }

            CapturePlayerPosition();
            if (!saveService.TrySave(Data) || !saveService.TrySerialize(Data, out var json))
            {
                ShowCloudMessage("현재 저장 데이터를 준비하지 못했습니다.");
                return;
            }

            cloudSyncBusy = true;
            ShowCloudMessage("새 동기화 코드를 만드는 중입니다…");
            StartCoroutine(cloudSaveClient.Create(json, result =>
            {
                cloudSyncBusy = false;
                if (!result.Success)
                {
                    ShowCloudMessage(result.Message);
                    return;
                }

                NewlyCreatedCloudPin = result.Pin;
                ShowCloudMessage("동기화 코드와 PIN을 만들었습니다. 오른쪽 패널에서 확인하세요.", 6f);
            }));
        }

        public void ConnectCloudSave(string code, string pin)
        {
            if (cloudSyncBusy)
            {
                ShowCloudMessage("동기화 작업이 진행 중입니다.");
                return;
            }

            cloudSyncBusy = true;
            ShowCloudMessage("클라우드 저장을 확인하는 중입니다…");
            StartCoroutine(cloudSaveClient.LinkAndLoad(code, pin, result =>
            {
                cloudSyncBusy = false;
                if (!result.Success)
                {
                    ShowCloudMessage(result.Message);
                    return;
                }

                NewlyCreatedCloudPin = string.Empty;
                ApplyCloudSave(result.SaveJson, "이 브라우저에 클라우드 저장을 연결했습니다.");
            }));
        }

        public void LoadCloudSave()
        {
            if (cloudSyncBusy)
            {
                ShowCloudMessage("동기화 작업이 진행 중입니다.");
                return;
            }

            cloudSyncBusy = true;
            ShowCloudMessage("클라우드 저장을 불러오는 중입니다…");
            if (!saveService.TrySerialize(Data, out var fallbackJson))
            {
                cloudSyncBusy = false;
                ShowCloudMessage("현재 저장 데이터를 준비하지 못했습니다.");
                return;
            }

            StartCoroutine(cloudSaveClient.Load(fallbackJson, result =>
            {
                cloudSyncBusy = false;
                if (!result.Success)
                {
                    ShowCloudMessage(result.Message);
                    return;
                }

                ApplyCloudSave(result.SaveJson, "클라우드의 최신 저장을 불러왔습니다.");
            }));
        }

        public void DisconnectCloudSave()
        {
            if (cloudSyncBusy)
            {
                ShowCloudMessage("동기화 작업이 끝난 뒤 연결을 해제해 주세요.");
                return;
            }

            cloudSaveClient.Disconnect();
            NewlyCreatedCloudPin = string.Empty;
            ShowCloudMessage("이 브라우저의 클라우드 연결만 해제했습니다. 저장 데이터는 지워지지 않습니다.");
        }

        public void SelectTool(FarmTool tool)
        {
            if (tool == FarmTool.Pickaxe && !Data.hasPickaxe)
            {
                Message = "광물 상점에서 곡괭이를 먼저 구입하세요.";
                return;
            }

            SelectedTool = tool;
            InventoryOpen = false;
            Message = $"가방에서 {GetToolLabel(tool)}를 꺼냈습니다.";
            NotifyChanged();
        }

        public void SelectSeed(GameItem seed)
        {
            if (!ItemCatalog.IsSeed(seed) || Data.inventory.Count(seed) <= 0)
            {
                Message = "가방에 그 씨앗이 없습니다.";
                return;
            }

            SelectedSeed = seed;
            SelectedTool = FarmTool.Seeds;
            InventoryOpen = false;
            Message = $"{ItemCatalog.GetLabel(seed)}을 꺼냈습니다.";
            NotifyChanged();
        }

        public void MineNode(int nodeId)
        {
            if (SelectedTool != FarmTool.Pickaxe || !Data.hasPickaxe)
            {
                Message = "곡괭이를 선택해야 광맥을 캘 수 있습니다.";
                NotifyChanged();
                return;
            }

            if (nodeId < 0 || nodeId >= Data.miningNodes.Count)
            {
                return;
            }

            var node = Data.miningNodes[nodeId];
            if (!node.IsAvailable)
            {
                Message = $"광맥이 다시 생기려면 {node.regrowDaysRemaining}일 남았습니다.";
                NotifyChanged();
                return;
            }

            node.Mine();
            Data.inventory.Add(node.ore, 1);
            Data.oreMined++;
            Message = $"{ItemCatalog.GetLabel(node.ore)}을 채굴했습니다.";
            CompleteCurrentStarterQuest();
            NotifyChanged();
        }

        public bool PullWeed(int weedId)
        {
            Data.pulledWeedIds ??= new System.Collections.Generic.List<int>();
            if (Data.pulledWeedIds.Contains(weedId))
            {
                return false;
            }

            Data.pulledWeedIds.Add(weedId);
            Data.inventory.Add(GameItem.Weed, 1);
            Message = "잡초를 뽑아 가방에 넣었습니다.";
            CompleteCurrentStarterQuest();
            NotifyChanged();
            return true;
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
            lastMorningEvent = string.Empty;
            foreach (var plot in Data.farmPlots)
            {
                plot.AdvanceDay();
            }

            if (Data.chickenFedToday)
            {
                Data.readyEggs += Mathf.Max(0, Data.ownedChickens - Data.chickenJuvenileDays.Count);
            }

            if (Data.cowFedToday)
            {
                Data.readyMilk += Mathf.Max(0, Data.ownedCows - Data.cowJuvenileDays.Count);
            }

            Data.chickenFedToday = false;
            Data.cowFedToday = false;
            ProcessDogMorningCare();

            AnimalBreedingRules.AdvanceJuveniles(Data.chickenJuvenileDays);
            AnimalBreedingRules.AdvanceJuveniles(Data.cowJuvenileDays);
            TryBreedAnimals(FarmAnimalKind.Chicken);
            TryBreedAnimals(FarmAnimalKind.Cow);

            foreach (var node in Data.miningNodes)
            {
                node.AdvanceDay();
            }
        }

        private void TryBreedAnimals(FarmAnimalKind kind)
        {
            var chickens = kind == FarmAnimalKind.Chicken;
            var total = chickens ? Data.ownedChickens : Data.ownedCows;
            var juveniles = chickens ? Data.chickenJuvenileDays : Data.cowJuvenileDays;
            if (!AnimalBreedingRules.ShouldBreed(Data.worldSeed, Data.day, kind, total - juveniles.Count, total))
            {
                return;
            }

            if (chickens)
            {
                Data.ownedChickens++;
            }
            else
            {
                Data.ownedCows++;
            }

            juveniles.Add(AnimalBreedingRules.JuvenileDays);
            AppendMorningEvent(chickens ? "병아리가 태어났습니다!" : "송아지가 태어났습니다!");
        }

        private void ProcessDogMorningCare()
        {
            if (Data.dogLastCareDay >= Data.day)
            {
                return;
            }

            var plan = DogCompanionRules.PlanMorningCare(
                Data.inventory.Count(GameItem.Feed),
                Data.ownedChickens,
                Data.ownedCows);
            if (plan.FeedSpent > 0)
            {
                Data.inventory.TrySpend(GameItem.Feed, plan.FeedSpent);
            }

            Data.chickenFedToday = plan.ChickensFed;
            Data.cowFedToday = plan.CowsFed;
            Data.dogLastCareDay = Data.day;
            if (plan.ChickensFed && plan.CowsFed)
            {
                AppendMorningEvent("강아지가 닭과 소에게 아침 사료를 주었습니다.");
            }
            else if (plan.ChickensFed)
            {
                AppendMorningEvent("강아지가 닭에게 아침 사료를 주었습니다.");
            }
            else if (plan.CowsFed)
            {
                AppendMorningEvent("강아지가 소에게 아침 사료를 주었습니다.");
            }
            else
            {
                AppendMorningEvent("강아지가 사료가 부족하다고 알려줍니다.");
            }
        }

        private void AppendMorningEvent(string eventMessage)
        {
            lastMorningEvent = string.IsNullOrEmpty(lastMorningEvent)
                ? eventMessage
                : $"{lastMorningEvent} {eventMessage}";
        }

        private void Plant(FarmPlotData plot)
        {
            if (!CropCatalog.TryGetBySeed(SelectedSeed, out var crop))
            {
                Message = "심을 씨앗을 가방에서 선택하세요.";
                return;
            }

            if (crop.Season != CurrentSeason)
            {
                Message = $"{crop.Label}은(는) {GetSeasonLabel(crop.Season)}에 심을 수 있습니다.";
                return;
            }

            if (!Data.inventory.TrySpend(SelectedSeed, 1))
            {
                Message = $"{ItemCatalog.GetLabel(SelectedSeed)}이 없습니다.";
                return;
            }

            plot.Plant(SelectedSeed);
            Message = $"{crop.Label} 씨앗을 심었습니다.";
        }

        private void Harvest(FarmPlotData plot)
        {
            var produce = CropCatalog.TryGetBySeed(plot.plantedSeed, out var crop)
                ? crop.Produce
                : GameItem.Wheat;
            plot.ResetToTilled();
            Data.inventory.Add(produce, 1);
            Data.wheatHarvested++;
            Data.firstHarvestAchievement = true;
            Message = $"{ItemCatalog.GetLabel(produce)}을 수확했습니다. 도전 과제: 첫 수확!";

            CompleteCurrentStarterQuest();

            NotifyChanged();
        }

        private void CompleteCurrentStarterQuest()
        {
            var completion = StarterQuestProgress.TryCompleteCurrent(Data);
            if (!completion.HasCompletion)
            {
                return;
            }

            Message = $"퀘스트 완료: {completion.Title}! 보상 {completion.Reward}G";
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
            SaveSilently();
        }

        private bool PurchaseWithoutItem(int price, string label)
        {
            if (Data.money < price)
            {
                Message = "돈이 부족합니다.";
                return false;
            }

            Data.money -= price;
            Message = $"{label}을(를) 구입했습니다.";
            return true;
        }

        private void AdvanceToNextSeason()
        {
            var nextSeasonDay = (((Data.day - 1) / SeasonCalendar.DaysPerSeason) + 1) * SeasonCalendar.DaysPerSeason + 1;
            GameClockController.Instance?.SetTime(nextSeasonDay, Data.hour, Data.minute);
            Message = "계절 전환 디버그 기능을 사용했습니다.";
        }

        private void EnsurePlotCount()
        {
            GameSaveDataNormalizer.Normalize(Data);
        }

        private void SelectFirstOwnedSeed()
        {
            foreach (var crop in CropCatalog.GetAll())
            {
                if (Data.inventory.Count(crop.Seed) <= 0)
                {
                    continue;
                }

                SelectedSeed = crop.Seed;
                return;
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

        private void SaveSilently()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            CapturePlayerPosition();
            if (saveService.TrySave(Data) && cloudSaveClient.IsLinked)
            {
                UploadCloudSave(false);
            }
        }

        private void UploadCloudSave(bool showProgress = true)
        {
            if (cloudLoadRequired)
            {
                if (showProgress)
                {
                    ShowCloudMessage("다른 브라우저의 저장이 있습니다. 먼저 최신 저장을 받아 주세요.");
                }
                return;
            }

            if (cloudSyncBusy || !cloudSaveClient.IsLinked || !saveService.TrySerialize(Data, out var json))
            {
                return;
            }

            cloudSyncBusy = true;
            if (showProgress)
            {
                ShowCloudMessage("기기에 저장했습니다. 클라우드에도 올리는 중입니다…");
            }

            StartCoroutine(cloudSaveClient.Save(json, result =>
            {
                cloudSyncBusy = false;
                if (result.Success)
                {
                    if (showProgress)
                    {
                        ShowCloudMessage("기기와 클라우드에 모두 저장했습니다.");
                    }
                    return;
                }

                ShowCloudMessage(result.Message);
            }));
        }

        private void BeginAutomaticDeviceSync()
        {
            if (cloudSyncBusy || !saveService.TrySerialize(Data, out var json))
            {
                return;
            }

            cloudSyncBusy = true;
            StartCoroutine(cloudSaveClient.AutoConnect(json, result =>
            {
                cloudSyncBusy = false;
                if (!result.Success)
                {
                    Debug.LogWarning($"Automatic cloud sync unavailable: {result.Message}");
                    return;
                }

                NewlyCreatedCloudPin = result.Pin ?? string.Empty;
                if (!hadLocalSaveAtStartup && !string.IsNullOrWhiteSpace(result.SaveJson))
                {
                    ApplyCloudSave(result.SaveJson, "같은 기기의 저장을 자동으로 불러왔습니다.");
                }
                else if (hadLocalSaveAtStartup && !string.IsNullOrWhiteSpace(result.SaveJson))
                {
                    cloudLoadRequired = true;
                }
            }));
        }

        private void ApplyCloudSave(string json, string successMessage)
        {
            if (!saveService.TryDeserialize(json, out var loaded))
            {
                ShowCloudMessage("클라우드 저장 데이터가 손상되어 불러오지 못했습니다.");
                return;
            }

            ApplyLoadedData(loaded, successMessage);
            cloudLoadRequired = false;
            saveService.TrySave(Data);
        }

        private void ApplyLoadedData(GameSaveData loaded, string successMessage)
        {
            Data = loaded;
            EnsurePlotCount();
            SelectFirstOwnedSeed();
            processedDay = Data.day;
            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(Data.playerX, Data.playerY, 0f);
            }

            GameClockController.Instance?.SetTime(Data.day, Data.hour, Data.minute);
            ShowCloudMessage(successMessage);
        }

        private void ShowCloudMessage(string text, float seconds = 5f)
        {
            Message = text;
            messageDisplaySeconds = seconds;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke();
        }

        private static string GetToolLabel(FarmTool tool)
        {
            return tool switch
            {
                FarmTool.Hoe => "호미",
                FarmTool.Seeds => "씨앗",
                FarmTool.WateringCan => "물뿌리개",
                FarmTool.Pickaxe => "곡괭이",
                FarmTool.FishingRod => "낚싯대",
                FarmTool.Shovel => "삽",
                _ => tool.ToString()
            };
        }

        private static string GetCropLabel(FarmPlotData plot)
        {
            return CropCatalog.TryGetBySeed(plot.plantedSeed, out var crop) ? crop.Label : "작물";
        }

        private static string GetSeasonLabel(Season season)
        {
            return season switch
            {
                Season.Spring => "봄",
                Season.Summer => "여름",
                Season.Autumn => "가을",
                Season.Winter => "겨울",
                _ => season.ToString()
            };
        }
    }
}
