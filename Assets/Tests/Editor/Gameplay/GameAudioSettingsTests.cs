using FarmGame2D.Core;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests
{
    public sealed class GameAudioSettingsTests
    {
        private float originalListenerVolume;
        private bool hadSavedVolume;
        private float originalSavedVolume;

        [SetUp]
        public void SetUp()
        {
            originalListenerVolume = AudioListener.volume;
            hadSavedVolume = PlayerPrefs.HasKey(GameAudioSettings.MasterVolumePreferenceKey);
            originalSavedVolume = PlayerPrefs.GetFloat(GameAudioSettings.MasterVolumePreferenceKey);
        }

        [TearDown]
        public void TearDown()
        {
            AudioListener.volume = originalListenerVolume;
            if (hadSavedVolume)
            {
                PlayerPrefs.SetFloat(GameAudioSettings.MasterVolumePreferenceKey, originalSavedVolume);
            }
            else
            {
                PlayerPrefs.DeleteKey(GameAudioSettings.MasterVolumePreferenceKey);
            }

            PlayerPrefs.Save();
        }

        [TestCase(-0.25f, 0f)]
        [TestCase(0.45f, 0.45f)]
        [TestCase(1.5f, 1f)]
        public void ClampVolume_RestrictsValueToSupportedRange(float input, float expected)
        {
            Assert.That(GameAudioSettings.ClampVolume(input), Is.EqualTo(expected).Within(0.001f));
        }

        [Test]
        public void LoadAndApply_UsesSavedVolumeForListener()
        {
            PlayerPrefs.SetFloat(GameAudioSettings.MasterVolumePreferenceKey, 0.35f);

            var loaded = GameAudioSettings.LoadAndApply();

            Assert.That(loaded, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(AudioListener.volume, Is.EqualTo(0.35f).Within(0.001f));
        }

        [Test]
        public void SetMasterVolume_PersistsClampedValue()
        {
            var applied = GameAudioSettings.SetMasterVolume(1.4f);

            Assert.That(applied, Is.EqualTo(1f));
            Assert.That(AudioListener.volume, Is.EqualTo(1f));
            Assert.That(PlayerPrefs.GetFloat(GameAudioSettings.MasterVolumePreferenceKey), Is.EqualTo(1f));
        }
    }
}
