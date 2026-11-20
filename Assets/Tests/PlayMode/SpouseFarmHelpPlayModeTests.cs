using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class SpouseFarmHelpPlayModeTests
    {
        [UnityTest]
        public IEnumerator ChoresApplyOncePerMorningAndPersistAfterSuccessfulWork()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            const string spouse = "Florist Taeo";
            var session = GameSession.Instance;
            if (session.Data.characterChoice == CharacterChoice.None)
                session.ChooseCharacter(CharacterChoice.Female);
            var data = session.Data;
            data.day = 12;
            data.hour = 6;
            data.spouseNpcId = spouse;
            data.spouseHelpLastDay = 0;
            data.weddingSeen = false;
            Assert.That(session.HasSpouseFarmHelp(spouse), Is.False);

            data.weddingSeen = true;
            Assert.That(session.HasSpouseFarmHelp("Fisher Minseo"), Is.False);
            data.hour = 12;
            Assert.That(session.TryRequestSpouseFarmHelp(spouse, SpouseFarmChore.PullWeeds, out _), Is.False);
            data.hour = 6;
            data.chickenFedToday = false;
            data.cowFedToday = false;
            data.ownedChickens = 1;
            data.ownedCows = 1;
            data.inventory.TrySpend(GameItem.Feed, data.inventory.Count(GameItem.Feed));
            Assert.That(session.TryRequestSpouseFarmHelp(spouse, SpouseFarmChore.FeedAnimals, out _), Is.False);
            Assert.That(data.spouseHelpLastDay, Is.Zero);

            data.farmPlots.Clear();
            for (var i = 0; i < 5; i++)
                data.farmPlots.Add(new FarmPlotData { state = i == 4 ? SoilState.Ready : SoilState.Planted });

            Assert.That(session.TryRequestSpouseFarmHelp(spouse, SpouseFarmChore.WaterCrops, out _), Is.True);
            Assert.That(data.farmPlots.FindAll(plot => plot.state == SoilState.Watered), Has.Count.EqualTo(3));
            Assert.That(data.farmPlots[3].state, Is.EqualTo(SoilState.Planted));
            Assert.That(data.farmPlots[4].state, Is.EqualTo(SoilState.Ready));
            Assert.That(session.TryRequestSpouseFarmHelp(spouse, SpouseFarmChore.PullWeeds, out _), Is.False);
            var saveService = new SaveService();
            Assert.That(saveService.TrySerialize(data, out var json), Is.True);
            Assert.That(saveService.TryDeserialize(json, out var saved), Is.True);
            Assert.That(saved.spouseHelpLastDay, Is.EqualTo(12));

            data.day++;
            data.inventory.Add(GameItem.Feed, 2);
            Assert.That(session.TryRequestSpouseFarmHelp(spouse, SpouseFarmChore.FeedAnimals, out _), Is.True);
            Assert.That(data.chickenFedToday && data.cowFedToday, Is.True);
            Assert.That(data.inventory.Count(GameItem.Feed), Is.Zero);

            data.day++;
            data.pulledWeedIds.Clear();
            session.ShowMessage(string.Empty);
            var farmWeed = GameObject.Find("Weed 1");
            var villageWeed = GameObject.Find("Weed 5");
            var weedCount = data.inventory.Count(GameItem.Weed);
            Assert.That(farmWeed, Is.Not.Null);
            Assert.That(villageWeed, Is.Not.Null);
            Assert.That(session.TryRequestSpouseFarmHelp(spouse, SpouseFarmChore.PullWeeds, out _), Is.True);
            Assert.That(data.pulledWeedIds, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(data.inventory.Count(GameItem.Weed), Is.EqualTo(weedCount + 3));
            Assert.That(farmWeed.activeSelf, Is.False);
            Assert.That(villageWeed.activeSelf, Is.True);
        }
    }
}
