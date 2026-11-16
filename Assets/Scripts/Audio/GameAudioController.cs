using System.Collections.Generic;
using UnityEngine;

namespace FarmGame2D.Audio
{
    [DefaultExecutionOrder(-50)]
    public sealed class GameAudioController : MonoBehaviour
    {
        private static GameAudioController instance;
        private readonly Dictionary<GameAudioCue, AudioClip> cueClips = new();
        private AudioSource musicSource;
        private AudioSource effectSource;
        private bool musicStarted;

        public static void Play(GameAudioCue cue)
        {
            if (instance != null && instance.cueClips.TryGetValue(cue, out var clip))
            {
                instance.effectSource.PlayOneShot(clip, GetCueVolume(cue));
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = CreateSource("Farm Music", 0.22f, true);
            effectSource = CreateSource("Farm Effects", 0.72f, false);

            var musicSamples = ProceduralFarmAudio.GenerateMusicLoop();
            musicSource.clip = CreateClip("Morning Farm Loop", musicSamples);
            foreach (GameAudioCue cue in System.Enum.GetValues(typeof(GameAudioCue)))
            {
                cueClips[cue] = CreateClip(cue.ToString(), ProceduralFarmAudio.GenerateCueSamples(cue));
            }
        }

        private void Update()
        {
            if (!musicStarted && Input.anyKeyDown)
            {
                musicStarted = true;
                musicSource.Play();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private AudioSource CreateSource(string sourceName, float volume, bool loop)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        private static AudioClip CreateClip(string clipName, float[] samples)
        {
            var clip = AudioClip.Create(clipName, samples.Length, 1, ProceduralFarmAudio.SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float GetCueVolume(GameAudioCue cue)
        {
            return cue == GameAudioCue.WaterCrop || cue == GameAudioCue.FishingCast ? 0.72f : 1f;
        }
    }
}
