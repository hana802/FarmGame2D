using System.Collections;
using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class SeasonalTreeAndFishingPresentationTests
    {
        [UnityTest]
        public IEnumerator Tree_ChangesCanopyForAllSeasonsAndKeepsTrunkAndGroundPlants()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var session = GameSession.Instance;
            var renderer = GameObject.Find("Seeded Tree 1").GetComponent<SpriteRenderer>();
            var original = WorldArtCatalog.GetSprite(WorldArtKind.Tree).texture;
            var seasonColors = new HashSet<Color32>();
            var originalDay = session.Data.day;
            try
            {
                for (var seasonIndex = 0; seasonIndex < 4; seasonIndex++)
                {
                    session.Data.day = 1 + seasonIndex * SeasonCalendar.DaysPerSeason;
                    session.ShowMessage(string.Empty);
                    var texture = renderer.sprite.texture;
                    Assert.That(renderer.color, Is.EqualTo(Color.white));
                    Assert.That((Color32)texture.GetPixel(640, 469), Is.EqualTo((Color32)original.GetPixel(640, 469)));
                    Assert.That((Color32)texture.GetPixel(575, 714), Is.EqualTo((Color32)original.GetPixel(575, 714)));
                    Assert.That((Color32)texture.GetPixel(370, 198), Is.EqualTo((Color32)original.GetPixel(370, 198)));
                    var foliage = (Color32)texture.GetPixel(620, 1028);
                    Assert.That(foliage, Is.Not.EqualTo((Color32)original.GetPixel(620, 1028)));
                    Assert.That(seasonColors.Add(foliage), Is.True);
                }
            }
            finally
            {
                session.Data.day = originalDay;
                session.ShowMessage(string.Empty);
            }
        }

        [UnityTest]
        public IEnumerator Fishing_PresentsOneCastAndClearsItOnCancellationOrCatch()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var session = GameSession.Instance;
            var player = Object.FindAnyObjectByType<PrototypePlayerController>();
            var pond = GameObject.Find("Pond Fishing Spot").GetComponent<FishingSpotInteractable>();
            var coast = GameObject.Find("West Coast Fishing Spot").GetComponent<FishingSpotInteractable>();
            if (session.Data.characterChoice == CharacterChoice.None)
            {
                session.ChooseCharacter(CharacterChoice.Female);
            }
            session.SelectTool(FarmTool.FishingRod);
            var before = session.Data.fishCaught;

            player.Teleport((Vector2)pond.transform.position + Vector2.down);
            pond.Interact(player);
            coast.Interact(player);
            Assert.That(player.IsFishing, Is.True);
            Assert.That(pond.transform.Find("Fishing Cast"), Is.Not.Null);
            Assert.That(coast.transform.Find("Fishing Cast"), Is.Null);
            pond.enabled = false;
            Assert.That(player.IsFishing, Is.False);
            yield return null;
            Assert.That(pond.transform.Find("Fishing Cast"), Is.Null);
            Assert.That(session.Data.fishCaught, Is.EqualTo(before));

            player.Teleport((Vector2)coast.transform.position + Vector2.right);
            coast.Interact(player);
            var cast = coast.transform.Find("Fishing Cast");
            Assert.That(cast, Is.Not.Null);
            var bobber = cast.Find("Fishing Bobber").GetComponent<SpriteRenderer>();
            Assert.That(cast.Find("Fishing Outer Ripple"), Is.Not.Null);
            var lowestBobberOpacity = 1f;
            var deadline = Time.realtimeSinceStartup + 5f;
            while (player.IsFishing && Time.realtimeSinceStartup < deadline)
            {
                lowestBobberOpacity = Mathf.Min(lowestBobberOpacity, bobber.color.a);
                yield return null;
            }
            Assert.That(lowestBobberOpacity, Is.LessThan(0.5f));
            Assert.That(player.IsFishing, Is.False);
            yield return null;
            Assert.That(coast.transform.Find("Fishing Cast"), Is.Null);
            Assert.That(session.Data.fishCaught, Is.EqualTo(before + 1));
        }
    }
}
