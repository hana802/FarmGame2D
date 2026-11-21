using System;
using UnityEngine;

namespace FarmGame2D.Audio
{
    public static class ProceduralFarmAudio
    {
        public const int SampleRate = 22050;

        public static float[] GenerateCueSamples(GameAudioCue cue, int sampleRate = SampleRate)
        {
            return cue switch
            {
                GameAudioCue.TillSoil => Generate(sampleRate, 0.28f, (t, i) =>
                    Envelope(t, 0.28f, 10f) * (0.38f * Sine(84f - 120f * t, t) + 0.22f * Noise(i, 11))),
                GameAudioCue.PlantSeed => Generate(sampleRate, 0.2f, (t, i) =>
                    Envelope(t, 0.2f, 14f) * (0.32f * Sine(440f + 900f * t, t) + 0.09f * Noise(i, 23))),
                GameAudioCue.WaterCrop => Generate(sampleRate, 0.52f, (t, i) =>
                    Envelope(t, 0.52f, 5f) * (0.13f * Noise(i, 37) + 0.2f * Sine(620f + 80f * Mathf.Sin(t * 24f), t))),
                GameAudioCue.HarvestCrop => Generate(sampleRate, 0.46f, (t, i) =>
                    Envelope(t, 0.46f, 7f) * 0.32f * Sine(HarvestFrequency(t), t)),
                GameAudioCue.FishingCast => Generate(sampleRate, 0.48f, (t, i) =>
                    Envelope(t, 0.48f, 5f) * (0.16f * Noise(i, 51) + 0.2f * Sine(240f + 520f * t, t))),
                GameAudioCue.FishingCatch => Generate(sampleRate, 0.62f, (t, i) =>
                    Envelope(t, 0.62f, 6f) * (0.2f * Noise(i, 73) + 0.28f * Sine(t < 0.25f ? 520f : 780f, t))),
                GameAudioCue.WeddingChime => Generate(sampleRate, 1.6f, (t, i) =>
                    Envelope(t, 1.6f, 1.4f) * 0.18f *
                    (Sine(523.25f, t) + Sine(659.25f, t) + Sine(783.99f, t) + 0.5f * Sine(1046.5f, t))),
                _ => Array.Empty<float>()
            };
        }

        public static float[] GenerateMusicLoop(int sampleRate = SampleRate)
        {
            const float duration = 8f;
            var notes = new[] { 261.63f, 329.63f, 392f, 523.25f, 440f, 392f, 329.63f, 293.66f };
            return Generate(sampleRate, duration, (t, i) =>
            {
                var step = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t));
                var beat = t - Mathf.Floor(t);
                var noteEnvelope = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, beat * 12f))
                                   * Mathf.SmoothStep(1f, 0f, Mathf.Max(0f, (beat - 0.72f) / 0.28f));
                var melody = Triangle(notes[step], t) * 0.12f * noteEnvelope;
                var bass = Sine(notes[step] * 0.25f, t) * 0.06f;
                var loopFade = Mathf.Min(1f, Mathf.Min(t, duration - t) * 40f);
                return (melody + bass) * loopFade;
            });
        }

        private static float[] Generate(int sampleRate, float duration, Func<float, int, float> sampler)
        {
            var length = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[length];
            for (var index = 0; index < length; index++)
            {
                samples[index] = Mathf.Clamp(sampler(index / (float)sampleRate, index), -1f, 1f);
            }

            return samples;
        }

        private static float Envelope(float time, float duration, float decay)
        {
            var attack = Mathf.Min(1f, time * 80f);
            var release = Mathf.Clamp01((duration - time) * 16f);
            return attack * release * Mathf.Exp(-decay * time);
        }

        private static float Sine(float frequency, float time) => Mathf.Sin(2f * Mathf.PI * frequency * time);

        private static float Triangle(float frequency, float time)
        {
            return 2f * Mathf.Abs(2f * (frequency * time - Mathf.Floor(frequency * time + 0.5f))) - 1f;
        }

        private static float HarvestFrequency(float time)
        {
            if (time < 0.14f) return 523.25f;
            if (time < 0.28f) return 659.25f;
            return 783.99f;
        }

        private static float Noise(int sample, uint seed)
        {
            var value = (uint)sample * 747796405u + seed * 2891336453u;
            value = ((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737u;
            value = (value >> 22) ^ value;
            return value / (float)uint.MaxValue * 2f - 1f;
        }
    }
}
