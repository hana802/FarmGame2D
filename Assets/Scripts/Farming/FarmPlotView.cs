using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.Farming
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class FarmPlotView : InteractableBehaviour
    {
        private int plotIndex;
        private SpriteRenderer plotRenderer;

        public override string Prompt => GetPrompt();

        public void Configure(int index)
        {
            plotIndex = index;
            plotRenderer = GetComponent<SpriteRenderer>();
            GameSession.Instance.StateChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.StateChanged -= Refresh;
            }
        }

        public override void Interact(PrototypePlayerController player)
        {
            GameSession.Instance?.UsePlot(plotIndex);
        }

        private string GetPrompt()
        {
            var session = GameSession.Instance;
            if (session == null || plotIndex >= session.Data.farmPlots.Count)
            {
                return "밭 확인";
            }

            var plot = session.Data.farmPlots[plotIndex];
            if (session.SelectedTool == FarmTool.Shovel)
            {
                return plot.state == SoilState.Tilled
                    ? "삽으로 밭 없애기"
                    : "작물이 있어 밭을 없앨 수 없음";
            }

            return plot.state switch
            {
                SoilState.Empty => "호미로 새 밭 만들기",
                SoilState.Tilled => $"{ItemCatalog.GetLabel(session.SelectedSeed)} 심기",
                SoilState.Planted => $"{GetCropLabel(plot)}에 물 주기",
                SoilState.Watered => "물이 충분합니다",
                SoilState.Ready => $"{GetCropLabel(plot)} 수확",
                _ => "밭 확인"
            };
        }

        private void Refresh()
        {
            if (plotRenderer == null || GameSession.Instance == null)
            {
                return;
            }

            if (plotIndex >= GameSession.Instance.Data.farmPlots.Count)
            {
                return;
            }

            var state = GameSession.Instance.Data.farmPlots[plotIndex].state;
            var stateSprite = FarmPlotArtCatalog.GetSprite(state);
            if (stateSprite != null)
            {
                plotRenderer.sprite = stateSprite;
                plotRenderer.color = Color.white;
                return;
            }

            plotRenderer.color = GetFallbackColor(state);
        }

        private static string GetCropLabel(FarmPlotData plot)
        {
            return CropCatalog.TryGetBySeed(plot.plantedSeed, out var crop) ? crop.Label : "작물";
        }

        private static Color GetFallbackColor(SoilState state)
        {
            return state switch
            {
                SoilState.Empty => new Color(0.55f, 0.38f, 0.2f),
                SoilState.Tilled => new Color(0.35f, 0.2f, 0.12f),
                SoilState.Planted => new Color(0.3f, 0.32f, 0.12f),
                SoilState.Watered => new Color(0.2f, 0.32f, 0.2f),
                SoilState.Ready => new Color(0.88f, 0.7f, 0.18f),
                _ => Color.magenta
            };
        }
    }
}
