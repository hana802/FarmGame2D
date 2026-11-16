using System.Collections;
using System.Reflection;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class DogSitCommandTests
    {
        [UnityTest]
        public IEnumerator SitCommand_HoldsPositionUntilFollowCommand()
        {
            if (!SceneManager.GetActiveScene().isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(0);
            }

            DogCompanionController dog = null;
            PrototypePlayerController player = null;
            for (var frame = 0; frame < 120 && (dog == null || player == null); frame++)
            {
                dog = Object.FindAnyObjectByType<DogCompanionController>();
                player = Object.FindAnyObjectByType<PrototypePlayerController>();
                yield return null;
            }

            Assert.That(dog, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            var originalPlayerPosition = (Vector2)player.transform.position;
            var session = GameSession.Instance;
            if (session.Data.dogSitting) session.ToggleDogSit();

            session.ToggleDogSit();
            Assert.That(dog.IsSitting, Is.True);
            var sittingPosition = (Vector2)dog.transform.position;
            player.Teleport(originalPlayerPosition + Vector2.right * 3f);
            for (var frame = 0; frame < 25; frame++) yield return new WaitForFixedUpdate();

            Assert.That(Vector2.Distance(dog.transform.position, sittingPosition), Is.LessThan(0.05f));
            session.ToggleDogSit();
            Assert.That(dog.IsSitting, Is.False);

            for (var frame = 0; frame < 25; frame++) yield return new WaitForFixedUpdate();
            Assert.That(Vector2.Distance(dog.transform.position, sittingPosition), Is.GreaterThan(0.25f),
                "The dog should resume following after the command is toggled off.");

            player.Teleport(originalPlayerPosition);
        }

        [UnityTest]
        public IEnumerator Interacting_PrefersNpcWhenDogIsCloser()
        {
            var playerObject = new GameObject("Interaction priority test player");
            var dogObject = new GameObject("Interaction priority test dog");
            var npcObject = new GameObject("Interaction priority test npc");
            playerObject.transform.position = new Vector3(200f, 200f, 0f);
            dogObject.transform.position = new Vector3(200.2f, 200f, 0f);
            npcObject.transform.position = new Vector3(200.8f, 200f, 0f);
            var player = playerObject.AddComponent<PrototypePlayerController>();
            dogObject.AddComponent<BoxCollider2D>().isTrigger = true;
            npcObject.AddComponent<BoxCollider2D>().isTrigger = true;
            var dog = dogObject.AddComponent<DogInteractable>();
            var npc = npcObject.AddComponent<DialogueInteractable>();
            Physics2D.SyncTransforms();

            var findNearest = typeof(PrototypePlayerController).GetMethod(
                "FindNearestInteractable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(findNearest, Is.Not.Null);
            Assert.That(findNearest.Invoke(player, null), Is.SameAs(npc));
            Assert.That(Vector2.Distance(dog.transform.position, player.transform.position),
                Is.LessThan(Vector2.Distance(npc.transform.position, player.transform.position)));

            Object.Destroy(playerObject);
            Object.Destroy(dogObject);
            Object.Destroy(npcObject);
            yield return null;
        }

        [Test]
        public void SittingState_SurvivesSaveSerialization()
        {
            var save = GameSaveData.CreateNew(0, 1234);
            save.dogSitting = true;
            save.dogSitX = 4.25f;
            save.dogSitY = -3.5f;
            var service = new SaveService();

            Assert.That(service.TrySerialize(save, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            Assert.That(restored.dogSitting, Is.True);
            Assert.That(restored.dogSitX, Is.EqualTo(4.25f));
            Assert.That(restored.dogSitY, Is.EqualTo(-3.5f));
        }
    }
}
