using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private static readonly Color32 Pine = new(40, 64, 57, 255);
        private static readonly Color32 DarkEdge = new(23, 37, 33, 255);
        private static readonly Color32 MidPine = new(62, 91, 77, 255);
        private static readonly Color32 Ink = new(48, 43, 31, 255);
        private static readonly Color32 Paper = new(245, 231, 190, 255);
        private static readonly Color32 PaperEdge = new(137, 106, 70, 255);
        private static readonly Color32 Cream = new(252, 241, 209, 255);
        private static readonly Color32 Clay = new(161, 84, 53, 255);
        private static readonly Color32 Amber = new(243, 195, 109, 255);
        private static readonly Color32 CanvasScrim = new(23, 34, 30, 191);

        private GUIStyle phoneTitleStyle;
        private GUIStyle phoneHeadingStyle;
        private GUIStyle phoneSectionStyle;
        private GUIStyle phoneBodyStyle;
        private GUIStyle phoneLightStyle;
        private GUIStyle warmLightStyle;
        private GUIStyle seasonStampStyle;
        private GUIStyle phoneInputStyle;
        private GUIStyle keyHintStyle;
        private FarmPhoneSkin phoneSkin;
        private string phoneSaveNotice = string.Empty;

        private void EnsureSurfaceStyles()
        {
            if (phoneTitleStyle != null) return;
            phoneSkin = new FarmPhoneSkin(koreanFont);
            phoneTitleStyle = CreateSurfaceStyle(30, new Color32(101, 48, 27, 255), FontStyle.Bold);
            phoneHeadingStyle = CreateSurfaceStyle(23, new Color32(101, 48, 27, 255), FontStyle.Bold);
            phoneSectionStyle = CreateSurfaceStyle(18, Ink, FontStyle.Bold);
            phoneBodyStyle = CreateSurfaceStyle(17, Ink);
            phoneLightStyle = CreateSurfaceStyle(16, Cream);
            warmLightStyle = CreateSurfaceStyle(16, Amber, FontStyle.Bold);
            seasonStampStyle = CreateSurfaceStyle(23, Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            keyHintStyle = CreateSurfaceStyle(13, Amber, FontStyle.Bold, TextAnchor.MiddleCenter);
            phoneInputStyle = new GUIStyle(GUI.skin.textField)
            {
                font = koreanFont,
                fontSize = 17,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = new RectOffset(10, 10, 2, 2),
                normal = { background = null, textColor = Ink },
                hover = { background = null, textColor = Ink },
                active = { background = null, textColor = Ink },
                focused = { background = null, textColor = Ink }
            };
        }

        private GUIStyle CreateSurfaceStyle(int size, Color color, FontStyle weight = FontStyle.Normal,
            TextAnchor align = TextAnchor.MiddleLeft)
        {
            return new GUIStyle(GUI.skin.label)
            {
                font = koreanFont,
                fontSize = size,
                fontStyle = weight,
                alignment = align,
                wordWrap = true,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = color }
            };
        }

        private float GetKeyHintWidth(string key, string label)
        {
            var keyWidth = string.IsNullOrEmpty(key) ? 0f : Mathf.Max(28f, keyHintStyle.CalcSize(new GUIContent(key)).x + 12f);
            return keyWidth + Mathf.Ceil(phoneLightStyle.CalcSize(new GUIContent(label)).x) + 28f;
        }

        private void DrawKeyHint(Rect rect, string key, string label)
        {
            farmSkin.DrawPixelPanel(rect, Pine, DarkEdge, false);
            if (string.IsNullOrEmpty(key))
            {
                GUI.Label(HudLayout.InsetRow(rect, 14f, 0f, rect.height), label, phoneLightStyle);
                return;
            }
            var keyWidth = Mathf.Max(28f, keyHintStyle.CalcSize(new GUIContent(key)).x + 12f);
            var keyRect = new Rect(rect.x + 5f, rect.y + 5f, keyWidth, rect.height - 10f);
            farmSkin.DrawPixelPanel(keyRect, DarkEdge, MidPine, false);
            GUI.Label(keyRect, key, keyHintStyle);
            var textX = keyRect.xMax + 9f;
            GUI.Label(new Rect(textX, rect.y, rect.xMax - textX - 8f, rect.height), label, phoneLightStyle);
        }

        private string DrawCloudField(Rect row, string label, string name, string value, int maxLength, bool password)
        {
            GUI.Label(new Rect(row.x, row.y, 72f, row.height), label, phoneSectionStyle);
            var field = new Rect(row.x + 76f, row.y, row.width - 76f, row.height);
            var focused = GUI.GetNameOfFocusedControl() == name;
            phoneSkin.DrawField(field, focused);
            GUI.SetNextControlName(name);
            return password
                ? GUI.PasswordField(field, value, '●', maxLength, phoneInputStyle)
                : GUI.TextField(field, value, maxLength, phoneInputStyle);
        }
    }
}
