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

            return session.Data.farmPlots[plotIndex].state switch
            {
                SoilState.Empty => "밭 갈기",
                SoilState.Tilled => "밀 씨앗 심기",
                SoilState.Planted => "물 주기",
                SoilState.Watered => "물이 충분합니다",
                SoilState.Ready => "밀 수확",
                _ => "밭 확인"
            };
        }

        private void Refresh()
        {
            if (plotRenderer == null || GameSession.Instance == null)
            {
                return;
            }

            plotRenderer.color = GameSession.Instance.Data.farmPlots[plotIndex].state switch
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
