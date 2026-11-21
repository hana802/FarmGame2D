using FarmGame2D.Farming;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed partial class GameSession
    {
        public FarmFacility OpenFacility { get; private set; }
        public string FacilityNotice { get; private set; } = string.Empty;

        public void OpenFarmFacility(FarmFacility facility)
        {
            if (facility == FarmFacility.Kitchen && Data.houseUpgradeLevel < 2) return;
            OpenFacility = facility;
            FacilityNotice = string.Empty;
        }

        public void CloseFarmFacility()
        {
            OpenFacility = FarmFacility.None;
            FacilityNotice = string.Empty;
        }

        public int GetBuildingLevel(FarmBuilding building) => building == FarmBuilding.House
            ? Data.houseUpgradeLevel : Data.greenhouseUpgradeLevel;

        public string GetUpgradeBlocker(FarmBuilding building)
        {
            if (building == FarmBuilding.Greenhouse && !Data.greenhouseBuilt) return "농장의 표지판에서 온실을 먼저 지으세요.";
            var level = GetBuildingLevel(building);
            if (level >= BuildingUpgradeCatalog.MaximumLevel) return "최종 단계까지 확장했어요.";

            var upgrade = BuildingUpgradeCatalog.Get(building, level);
            var inventory = Data.inventory;
            return Data.money < upgrade.Price || inventory.Count(GameItem.Wood) < upgrade.Wood
                || inventory.Count(GameItem.Stone) < upgrade.Stone || inventory.Count(upgrade.Metal) < upgrade.MetalCount
                ? "골드와 건축 재료를 모아 오세요." : string.Empty;
        }

        public bool TryUpgradeBuilding(FarmBuilding building)
        {
            FacilityNotice = GetUpgradeBlocker(building);
            if (FacilityNotice.Length > 0) return false;

            var level = GetBuildingLevel(building);
            var upgrade = BuildingUpgradeCatalog.Get(building, level);
            Data.money -= upgrade.Price;
            Data.inventory.TrySpend(GameItem.Wood, upgrade.Wood);
            Data.inventory.TrySpend(GameItem.Stone, upgrade.Stone);
            Data.inventory.TrySpend(upgrade.Metal, upgrade.MetalCount);
            if (building == FarmBuilding.House)
            {
                Data.houseUpgradeLevel = level + 1;
                Data.houseUpgraded = true;
            }
            else
            {
                Data.greenhouseUpgradeLevel = level + 1;
                GreenhouseLayout.EnsurePlots(Data);
            }

            var name = building == FarmBuilding.House ? "농가" : "온실";
            FacilityNotice = $"{name} {level + 1}단계 공사를 완료했어요.";
            Message = FacilityNotice;
            NotifyChanged();
            SaveSilently();
            return true;
        }

        public bool TryCook(GameItem result)
        {
            if (OpenFacility != FarmFacility.Kitchen || Data.houseUpgradeLevel < 2) return false;
            foreach (var recipe in BuildingUpgradeCatalog.Recipes)
            {
                if (recipe.Result != result) continue;
                var inventory = Data.inventory;
                if (inventory.Count(recipe.First) == 0 || inventory.Count(recipe.Second) == 0)
                {
                    FacilityNotice = "요리 재료가 부족해요.";
                    return false;
                }

                inventory.TrySpend(recipe.First, 1);
                inventory.TrySpend(recipe.Second, 1);
                inventory.Add(recipe.Result, 1);
                FacilityNotice = $"{ItemCatalog.GetLabel(recipe.Result)} 1개를 만들었어요.";
                Message = FacilityNotice;
                NotifyChanged();
                SaveSilently();
                return true;
            }

            return false;
        }
    }
}
