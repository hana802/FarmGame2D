using System;
using FarmGame2D.Audio;
using FarmGame2D.Interactions;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests.Editor.Gameplay
{
    public sealed class FarmAudioAndFishingTests
    {
        [TestCase(0f, -1f, 0f, 0.85f)]
        [TestCase(1f, 0f, -0.85f, 0f)]
        [TestCase(0f, 1f, 0f, -0.85f)]
        public void FishingCastTarget_MovesFromShoreAwayFromPlayer(
            float playerX, float playerY, float expectedX, float expectedY)
        {
            var target = FishingCastTarget.Resolve(Vector2.zero, new Vector2(playerX, playerY));

            Assert.That(target.x, Is.EqualTo(expectedX).Within(0.001f));
            Assert.That(target.y, Is.EqualTo(expectedY).Within(0.001f));
            Assert.That(target.magnitude, Is.EqualTo(FishingCastTarget.DefaultCastDistance).Within(0.001f));
        }

        [Test]
        public void ProceduralAudio_GeneratesAudibleNormalizedCueSamples()
        {
            foreach (GameAudioCue cue in Enum.GetValues(typeof(GameAudioCue)))
            {
                var samples = ProceduralFarmAudio.GenerateCueSamples(cue, 22050);

                Assert.That(samples.Length, Is.GreaterThan(100), cue.ToString());
                Assert.That(MaxAbsolute(samples), Is.GreaterThan(0.01f), cue.ToString());
                Assert.That(AllFiniteAndNormalized(samples), Is.True, cue.ToString());
            }
        }

        [Test]
        public void ProceduralAudio_GeneratesAudibleMusicLoop()
        {
            var samples = ProceduralFarmAudio.GenerateMusicLoop(22050);

            Assert.That(samples.Length, Is.GreaterThan(22050 * 4));
            Assert.That(MaxAbsolute(samples), Is.GreaterThan(0.01f));
            Assert.That(AllFiniteAndNormalized(samples), Is.True);
        }

        private static float MaxAbsolute(float[] samples)
        {
            var maximum = 0f;
            foreach (var sample in samples)
            {
                maximum = Mathf.Max(maximum, Mathf.Abs(sample));
            }

            return maximum;
        }

        private static bool AllFiniteAndNormalized(float[] samples)
        {
            foreach (var sample in samples)
            {
                if (float.IsNaN(sample) || float.IsInfinity(sample) || Mathf.Abs(sample) > 1f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
