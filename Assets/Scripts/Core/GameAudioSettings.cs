using UnityEngine;

namespace FarmGame2D.Core
{
    public static class GameAudioSettings
    {
        public const string MasterVolumePreferenceKey = "farmgame2d.settings.master-volume";
        public const float DefaultMasterVolume = 0.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedSettingsAtStartup()
        {
            LoadAndApply();
        }

        public static float LoadAndApply()
        {
            var savedVolume = PlayerPrefs.GetFloat(MasterVolumePreferenceKey, DefaultMasterVolume);
            var volume = ClampVolume(savedVolume);
            AudioListener.volume = volume;
            return volume;
        }

        public static float SetMasterVolume(float value)
        {
            var volume = ClampVolume(value);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(MasterVolumePreferenceKey, volume);
            PlayerPrefs.Save();
            return volume;
        }

        public static float ClampVolume(float value)
        {
            return Mathf.Clamp01(value);
        }
    }
}
