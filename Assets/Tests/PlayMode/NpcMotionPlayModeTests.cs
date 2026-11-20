using System.Collections;
using FarmGame2D.Art;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class NpcMotionPlayModeTests
    {
        [UnityTest]
        public IEnumerator Villagers_UseSideWalkFramesAndKeepFacingWhenTheyStop()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var residents = new[]
            {
                "Guide", "Elder", "Young Villager", "Fisher Minseo", "Florist Taeo", "Baker Yuna", "Rancher Sora"
            };
            foreach (var name in residents)
            {
                Assert.That(GameObject.Find(name).GetComponent<NpcMotionPresenter>(), Is.Not.Null, name);
            }
            Assert.That(GameObject.Find("Fisher Minseo").GetComponent<SpriteRenderer>().sprite.texture.name,
                Is.EqualTo("minseo_world_v1"));
            Assert.That(GameObject.Find("Florist Taeo").GetComponent<SpriteRenderer>().sprite.texture.name,
                Is.EqualTo("taeo_world_v1"));

            const int soraWorldArt = 7;
            var sora = GameObject.Find("Rancher Sora").GetComponent<SpriteRenderer>();
            Assert.That(sora.sprite.texture.name, Is.EqualTo("sora_world_v1"));
            Assert.That(GameArtCatalog.GetNpcPortrait(7).texture.name, Is.EqualTo("rancher_sora_portrait_v2"));

            var rightWalker = CreateWalker(new Vector2(0f, 100f), Vector2.right, 0);
            var leftWalker = CreateWalker(new Vector2(0f, 104f), Vector2.left, soraWorldArt);
            var rightRenderer = rightWalker.GetComponent<SpriteRenderer>();
            var leftRenderer = leftWalker.GetComponent<SpriteRenderer>();
            var rightStride = GameArtCatalog.GetNpcSprite(0, 6);
            var leftStride = GameArtCatalog.GetNpcDirectionalSprite(soraWorldArt, 2, true);
            var sawRightStride = false;
            var sawLeftStride = false;
            try
            {
                var deadline = Time.realtimeSinceStartup + 3f;
                while ((!sawRightStride || !sawLeftStride) && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                    sawRightStride |= rightRenderer.sprite == rightStride;
                    sawLeftStride |= leftRenderer.sprite == leftStride;
                }

                Assert.That(sawRightStride, Is.True);
                Assert.That(sawLeftStride, Is.True);
                rightWalker.Pause(1f);
                leftWalker.Pause(1f);
                yield return null;
                yield return null;

                Assert.That(rightRenderer.sprite, Is.EqualTo(GameArtCatalog.GetNpcSprite(0, 2)));
                Assert.That(leftRenderer.sprite, Is.EqualTo(GameArtCatalog.GetNpcDirectionalSprite(soraWorldArt, 2, false)));
            }
            finally
            {
                Object.Destroy(rightWalker.gameObject);
                Object.Destroy(leftWalker.gameObject);
            }
        }

        private static WanderingMover CreateWalker(Vector2 start, Vector2 direction, int sheetRow)
        {
            var actor = new GameObject("Walk presentation test resident");
            actor.transform.position = start;
            actor.AddComponent<SpriteRenderer>();
            actor.AddComponent<NpcMotionPresenter>().Configure(sheetRow);
            var mover = actor.AddComponent<WanderingMover>();
            mover.Configure(new WanderArea(start, new Vector2(10f, 1f)), 1f, 0, minIdle: 0f, maxIdle: 0f);
            mover.ConfigureRoute(new VillageWaypointNetwork(
                new[] { start, start + direction * 5f },
                new[] { new[] { 1 }, new[] { 0 } }), 0);
            return mover;
        }
    }
}
