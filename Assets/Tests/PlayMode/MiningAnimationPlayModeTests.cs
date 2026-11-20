using System.Collections;
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
    public sealed class MiningAnimationPlayModeTests
    {
        [UnityTest]
        public IEnumerator Mining_SwingsBeforeGrantingOneOreAndClearsTheImpact()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var session = GameSession.Instance;
            if (session.Data.characterChoice == CharacterChoice.None)
                session.ChooseCharacter(CharacterChoice.Female);
            session.Data.hasPickaxe = true;
            session.SelectTool(FarmTool.Pickaxe);
            var node = session.Data.miningNodes[0];
            node.regrowDaysRemaining = 0;
            var before = session.Data.inventory.Count(node.ore);
            var player = Object.FindAnyObjectByType<PrototypePlayerController>();
            var mine = GameObject.Find("Mining Node 1").GetComponent<MiningNodeInteractable>();
            var rock = mine.GetComponent<MiningNodeDepletionPresentation>();
            player.Teleport((Vector2)mine.transform.position + Vector2.right);

            mine.Interact(player);
            Assert.That(player.IsMining, Is.True);
            Assert.That(session.Data.inventory.Count(node.ore), Is.EqualTo(before));
            mine.enabled = false;
            Assert.That(player.IsMining, Is.False);
            Assert.That(node.IsAvailable, Is.True);
            mine.enabled = true;

            mine.Interact(player);
            mine.Interact(player);
            var pickaxe = player.HeldItemPresenter.ItemRenderer;
            Assert.That(pickaxe.sprite.name, Is.EqualTo("held_pickaxe"));
            var initialPose = pickaxe.transform.localRotation;
            var largestSwing = 0f;
            var sawImpact = false;
            var deadline = Time.realtimeSinceStartup + 3f;
            while (player.IsMining && Time.realtimeSinceStartup < deadline)
            {
                largestSwing = Mathf.Max(largestSwing, Quaternion.Angle(initialPose, pickaxe.transform.localRotation));
                var chips = mine.transform.Find("Mining Impact Chips");
                sawImpact |= chips != null && chips.GetChild(0).GetComponent<SpriteRenderer>().enabled;
                yield return null;
            }

            Assert.That(player.IsMining, Is.False);
            Assert.That(largestSwing, Is.GreaterThan(30f));
            Assert.That(sawImpact, Is.True);
            Assert.That(rock.CracksVisible, Is.True);
            Assert.That(session.Data.inventory.Count(node.ore), Is.EqualTo(before + 1));
            Assert.That(pickaxe.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(mine.transform.Find("Mining Impact Chips").GetChild(0).GetComponent<SpriteRenderer>().enabled,
                Is.False);
            mine.Interact(player);
            Assert.That(player.IsMining, Is.False);
        }
    }
}
