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
            var rod = player.HeldItemPresenter.ItemRenderer;
            var line = cast.GetComponent<LineRenderer>();
            var windup = rod.transform.localRotation;
            Assert.That(rod.sprite.name, Is.EqualTo("held_fishing_rod_cast"));
            Assert.That(line.enabled, Is.False);
            Assert.That(cast.Find("Fishing Outer Ripple"), Is.Not.Null);
            var lowestBobberOpacity = 1f;
            var furthestSwing = 0f;
            var lineFollowedRod = false;
            var deadline = Time.realtimeSinceStartup + 5f;
            while (player.IsFishing && Time.realtimeSinceStartup < deadline)
            {
                lowestBobberOpacity = Mathf.Min(lowestBobberOpacity, bobber.color.a);
                furthestSwing = Mathf.Max(furthestSwing, Quaternion.Angle(windup, rod.transform.localRotation));
                if (line.enabled)
                {
                    var tipOffset = rod.flipY ? new Vector3(20f, -3f) : new Vector3(20f, 3f);
                    var tip = rod.transform.TransformPoint(tipOffset / 18f);
                    lineFollowedRod |= Vector3.Distance(line.GetPosition(0), tip) < 0.01f;
                }
                yield return null;
            }
            Assert.That(lowestBobberOpacity, Is.LessThan(0.5f));
            Assert.That(furthestSwing, Is.GreaterThan(35f));
            Assert.That(lineFollowedRod, Is.True);
            Assert.That(player.IsFishing, Is.False);
            Assert.That(rod.sprite.texture.name, Is.EqualTo("held_fishing_rod_v1"));
            Assert.That(Quaternion.Angle(rod.transform.localRotation, Quaternion.identity), Is.LessThan(0.01f));
            yield return null;
            Assert.That(coast.transform.Find("Fishing Cast"), Is.Null);
            Assert.That(session.Data.fishCaught, Is.EqualTo(before + 1));
        }
    }
}
