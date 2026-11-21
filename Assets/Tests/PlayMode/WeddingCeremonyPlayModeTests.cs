using System;
using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class WeddingCeremonyPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeddingCeremony_OpensForExistingSpouseAndBlocksWorldInput()
        {
            Assert.That(Resources.Load<Texture2D>("Art/Wedding/wedding_character_bride_player_v2"), Is.Not.Null);
            Assert.That(Resources.Load<Texture2D>("Art/Wedding/wedding_character_bride_spouse_v2"), Is.Not.Null);
            Assert.That(Resources.Load<Texture2D>("Art/Wedding/wedding_character_groom_player_v2"), Is.Not.Null);
            Assert.That(Resources.Load<Texture2D>("Art/Wedding/wedding_character_groom_spouse_v2"), Is.Not.Null);

            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var session = GameSession.Instance;
            Assert.That(session, Is.Not.Null);
            session.Data.characterChoice = CharacterChoice.Female;
            NpcFriendshipService.GetOrCreate(session.Data, "Baker Yuna").points = 100;
            NpcFriendshipService.GetOrCreate(session.Data, "Baker Yuna").endingSeen = true;
            session.Data.spouseNpcId = "Baker Yuna";
            session.Data.weddingSeen = false;

            NpcInteractionOverlay.Show("Baker Yuna", "제빵사 유나", 6, () => "좋은 아침이에요.");
            Assert.That(WeddingCeremonyOverlay.IsPlaying, Is.True);
            Assert.That(NpcInteractionOverlay.IsOpen, Is.True);
            Assert.That(BondEventService.GetPendingWedding(session.Data, "Baker Yuna").HasValue, Is.True);

            var screenshotPath = Environment.GetEnvironmentVariable("FARM_GAME_WEDDING_SCREENSHOT");
            if (!string.IsNullOrWhiteSpace(screenshotPath))
            {
                yield return new WaitForSecondsRealtime(9f);
                ScreenCapture.CaptureScreenshot(screenshotPath);
                yield return new WaitForSecondsRealtime(0.5f);
            }

            var ceremony = UnityEngine.Object.FindAnyObjectByType<WeddingCeremonyOverlay>();
            UnityEngine.Object.Destroy(ceremony.gameObject);
            yield return null;
            Assert.That(WeddingCeremonyOverlay.IsPlaying, Is.False);
            Assert.That(session.Data.weddingSeen, Is.False, "Leaving early must not mark the wedding complete.");
        }
    }
}
