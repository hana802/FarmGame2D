using System;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public static class SleepTransitionTiming
    {
        public const float FadeOutSeconds = 0.55f;
        public const float HoldSeconds = 0.45f;
        public const float FadeInSeconds = 0.5f;

        public static float TotalSeconds => FadeOutSeconds + HoldSeconds + FadeInSeconds;

        public static float Darkness(float elapsed)
        {
            if (elapsed <= 0f)
            {
                return 0f;
            }

            if (elapsed < FadeOutSeconds)
            {
                return Mathf.Clamp01(elapsed / FadeOutSeconds);
            }

            if (elapsed < FadeOutSeconds + HoldSeconds)
            {
                return 1f;
            }

            var intoDawn = elapsed - FadeOutSeconds - HoldSeconds;
            if (intoDawn >= FadeInSeconds)
            {
                return 0f;
            }

            return 1f - Mathf.Clamp01(intoDawn / FadeInSeconds);
        }

        public static bool HasReachedNight(float elapsed)
        {
            return elapsed >= FadeOutSeconds;
        }
    }

    public sealed class SleepTransitionOverlay : MonoBehaviour
    {
        private static SleepTransitionOverlay instance;
        private float elapsed;
        private bool advanced;
        private Action onNight;
        private GUIStyle style;

        public static bool IsPlaying { get; private set; }

        public static void Play(Action onNight)
        {
            if (IsPlaying)
            {
                return;
            }

            if (instance == null)
            {
                var host = new GameObject("Sleep Transition");
                DontDestroyOnLoad(host);
                instance = host.AddComponent<SleepTransitionOverlay>();
            }

            instance.onNight = onNight;
            instance.elapsed = 0f;
            instance.advanced = false;
            instance.enabled = true;
            IsPlaying = true;
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            if (!advanced && SleepTransitionTiming.HasReachedNight(elapsed))
            {
                advanced = true;
                onNight?.Invoke();
                onNight = null;
            }

            if (elapsed >= SleepTransitionTiming.TotalSeconds)
            {
                IsPlaying = false;
                enabled = false;
            }
        }

        private void OnGUI()
        {
            if (!IsPlaying)
            {
                return;
            }

            var darkness = SleepTransitionTiming.Darkness(elapsed);
            if (darkness <= 0.001f)
            {
                return;
            }

            var previousColor = GUI.color;
            var previousDepth = GUI.depth;
            GUI.depth = -300;
            GUI.color = new Color(0.04f, 0.05f, 0.1f, darkness);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            if (darkness > 0.65f)
            {
                style ??= new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 42,
                    fontStyle = FontStyle.Bold
                };
                var alpha = Mathf.Clamp01((darkness - 0.65f) / 0.35f);
                GUI.color = new Color(0.93f, 0.9f, 0.82f, alpha);
                GUI.Label(new Rect(0f, Screen.height * 0.42f, Screen.width, 72f), "Zzz", style);
            }

            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }
    }
}
