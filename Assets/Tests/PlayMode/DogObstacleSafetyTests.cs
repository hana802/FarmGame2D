using System.Collections;
using System.Reflection;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class DogObstacleSafetyTests
    {
        [UnityTest]
        public IEnumerator CelebrationOrbit_DoesNotLeaveDogInsideSolidObstacle()
        {
            if (!SceneManager.GetActiveScene().isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(0);
            }

            DogCompanionController dog = null;
            for (var i = 0; i < 120 && dog == null; i++)
            {
                dog = Object.FindFirstObjectByType<DogCompanionController>();
                yield return null;
            }

            Assert.That(dog, Is.Not.Null, "The farm scene did not create the dog companion.");

            var playerField = typeof(DogCompanionController).GetField(
                "player",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(playerField, Is.Not.Null);
            var player = playerField.GetValue(dog) as Transform;
            Assert.That(player, Is.Not.Null);

            var blocker = new GameObject("Dog orbit obstacle regression blocker");
            blocker.transform.position = player.position + Vector3.right * 1.35f;
            var blockerCollider = blocker.AddComponent<BoxCollider2D>();
            blockerCollider.size = new Vector2(0.55f, 2.2f);

            var dogBody = dog.GetComponent<Rigidbody2D>();
            var dogCollider = dog.GetComponent<BoxCollider2D>();
            dogBody.position = (Vector2)player.position + Vector2.left * 1.12f;
            Physics2D.SyncTransforms();

            dog.Celebrate();
            for (var frame = 0; frame < 90; frame++)
            {
                yield return new WaitForFixedUpdate();
                Physics2D.SyncTransforms();
                var distance = Physics2D.Distance(dogCollider, blockerCollider);
                Assert.That(distance.isOverlapped, Is.False,
                    $"The dog entered a solid obstacle during its celebration on frame {frame}.");
            }

            Object.Destroy(blocker);
        }
    }
}
