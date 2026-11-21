using FarmGame2D.Core;
using NUnit.Framework;

namespace FarmGame2D.Tests.Editor.Gameplay
{
    public sealed class BondEventTests
    {
        [Test]
        public void NpcEnding_UnlocksAtOneHundredAndStaysReplayable()
        {
            var save = GameSaveData.CreateNew(0, 1);
            var friend = NpcFriendshipService.GetOrCreate(save, "Baker Yuna");
            friend.points = 99;
            Assert.That(BondEventService.GetPendingNpcEnding(save, "Baker Yuna"), Is.Null);

            friend.points = 100;
            var ending = BondEventService.GetPendingNpcEnding(save, "Baker Yuna");
            Assert.That(ending.HasValue, Is.True);
            Assert.That(ending.Value.Lines.Length, Is.GreaterThanOrEqualTo(2));
            BondEventService.Complete(save, ending.Value);
            Assert.That(BondEventService.GetPendingNpcEnding(save, "Baker Yuna"), Is.Null);
            Assert.That(BondEventService.GetNpcEndingForReplay(save, "Baker Yuna").HasValue, Is.True);
        }

        [Test]
        public void ExistingMaxedSave_ReceivesNpcEndingOnNextInteraction()
        {
            var save = GameSaveData.CreateNew(0, 1);
            NpcFriendshipService.GetOrCreate(save, "Florist Taeo").points = 100;
            Assert.That(BondEventService.GetPendingNpcEnding(save, "Florist Taeo").HasValue, Is.True);
        }

        [Test]
        public void DogEvents_UnlockAtTenAndFiftyWithoutRepeating()
        {
            var save = GameSaveData.CreateNew(0, 1);
            save.dogAffection = 9;
            Assert.That(BondEventService.GetPendingDogEvent(save), Is.Null);
            save.dogAffection = 10;
            var first = BondEventService.GetPendingDogEvent(save);
            Assert.That(first.HasValue, Is.True);
            BondEventService.Complete(save, first.Value);
            Assert.That(BondEventService.GetPendingDogEvent(save), Is.Null);

            save.dogAffection = 50;
            var final = BondEventService.GetPendingDogEvent(save);
            Assert.That(final.HasValue, Is.True);
            BondEventService.Complete(save, final.Value);
            Assert.That(BondEventService.GetPendingDogEvent(save), Is.Null);
            Assert.That(BondEventService.GetDogEventForReplay(save)?.Kind, Is.EqualTo(BondEventKind.DogEnding));
        }

        [Test]
        public void SeenDogEnding_CanBeReplayedWithoutResettingProgress()
        {
            var save = GameSaveData.CreateNew(0, 1);
            save.dogAffection = 361;
            save.dogCompanionEventSeen = true;
            save.dogEndingSeen = true;

            Assert.That(BondEventService.GetPendingDogEvent(save), Is.Null);
            Assert.That(BondEventService.GetDogEventForReplay(save)?.Title, Is.EqualTo("언제나 함께"));
            Assert.That(save.dogAffection, Is.EqualTo(361));
            Assert.That(save.dogEndingSeen, Is.True);
        }

        [Test]
        public void ExistingHighAffectionSave_GoesDirectlyToFinalDogEvent()
        {
            var save = GameSaveData.CreateNew(0, 1);
            save.dogAffection = 100;
            var ending = BondEventService.GetPendingDogEvent(save);
            Assert.That(ending?.Kind, Is.EqualTo(BondEventKind.DogEnding));
            BondEventService.Complete(save, ending.Value);
            Assert.That(save.dogCompanionEventSeen, Is.True);
            Assert.That(save.dogEndingSeen, Is.True);
        }

        [Test]
        public void CompletedEvents_SurviveSaveRoundTrip()
        {
            var save = GameSaveData.CreateNew(0, 1);
            var friend = NpcFriendshipService.GetOrCreate(save, "Guide");
            friend.points = 100;
            friend.endingSeen = true;
            save.dogAffection = 50;
            save.dogEndingSeen = true;
            var service = new SaveService();
            Assert.That(service.TrySerialize(save, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            Assert.That(BondEventService.GetPendingNpcEnding(restored, "Guide"), Is.Null);
            Assert.That(BondEventService.GetPendingDogEvent(restored), Is.Null);
        }

        [Test]
        public void Marriage_RequiresOneHundredAndOnePartner()
        {
            var save = GameSaveData.CreateNew(0, 1);
            var yuna = NpcFriendshipService.GetOrCreate(save, "Baker Yuna");
            var sora = NpcFriendshipService.GetOrCreate(save, "Rancher Sora");
            yuna.points = 99;
            sora.points = 100;
            Assert.That(BondEventService.GetMarriageProposal(save, "Baker Yuna"), Is.Null);
            Assert.That(BondEventService.TryAcceptMarriage(save, "Baker Yuna"), Is.False);

            yuna.points = 100;
            Assert.That(BondEventService.GetMarriageProposal(save, "Baker Yuna").HasValue, Is.True);
            Assert.That(BondEventService.TryAcceptMarriage(save, "Baker Yuna"), Is.True);
            Assert.That(save.spouseNpcId, Is.EqualTo("Baker Yuna"));
            Assert.That(BondEventService.TryAcceptMarriage(save, "Rancher Sora"), Is.False);
            Assert.That(BondEventService.GetMarriageProposal(save, "Rancher Sora"), Is.Null);
        }

        [Test]
        public void Wedding_RetriggersUntilCompletedAndCanBeReplayed()
        {
            var save = GameSaveData.CreateNew(0, 1);
            NpcFriendshipService.GetOrCreate(save, "Florist Taeo").points = 100;
            Assert.That(BondEventService.TryAcceptMarriage(save, "Florist Taeo"), Is.True);
            var wedding = BondEventService.GetPendingWedding(save, "Florist Taeo");
            Assert.That(wedding.HasValue, Is.True);
            Assert.That(BondEventService.GetPendingWedding(save, "Florist Taeo").HasValue, Is.True);
            BondEventService.Complete(save, wedding.Value);
            Assert.That(BondEventService.GetPendingWedding(save, "Florist Taeo"), Is.Null);
            Assert.That(BondEventService.GetWeddingForReplay(save, "Florist Taeo").HasValue, Is.True);

            var service = new SaveService();
            Assert.That(service.TrySerialize(save, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            Assert.That(restored.spouseNpcId, Is.EqualTo("Florist Taeo"));
            Assert.That(restored.weddingSeen, Is.True);
        }
    }
}
