using UnityEngine;

namespace FarmGame2D.TimeSystem
{
    public sealed class GameClockOverlay : MonoBehaviour
    {
        private const float PanelWidth = 180f;
        private const float PanelHeight = 66f;
        private const float ScreenMargin = 18f;

        private GUIStyle labelStyle;

        private void OnGUI()
        {
            var controller = GameClockController.Instance;
            if (controller == null)
            {
                return;
            }

            labelStyle ??= CreateLabelStyle();

            var panel = new Rect(
                Screen.width - PanelWidth - ScreenMargin,
                ScreenMargin,
                PanelWidth,
                PanelHeight);

            GUI.Box(panel, GUIContent.none);
            GUI.Label(panel, controller.CurrentTime.ToString(), labelStyle);
        }

        private static GUIStyle CreateLabelStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }
    }
}
