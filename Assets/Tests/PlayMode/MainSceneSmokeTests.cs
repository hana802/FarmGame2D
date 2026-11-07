using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.UI;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class MainSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator MainScene_CreatesPlayableFarmWorld()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<GameSession>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PrototypePlayerController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PrototypeWorldBuilder>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PixelCameraFollow>(), Is.Not.Null);
            var hud = Object.FindFirstObjectByType<PrototypeHud>();
            Assert.That(hud, Is.Not.Null);

            var player = Object.FindFirstObjectByType<PrototypePlayerController>();
            var playerTexture = player.GetComponent<SpriteRenderer>().sprite.texture;
            Assert.That(playerTexture.width, Is.GreaterThan(1), "Player must use the character art sheet, not a 1x1 block.");
            Assert.That(hud.HasArtSkin, Is.True, "HUD must load the illustrated UI atlas.");

            var guide = GameObject.Find("Guide");
            Assert.That(guide, Is.Not.Null);
            Assert.That(guide.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "NPC must use the village NPC art sheet, not a 1x1 block.");
        }
    }
}
