using FarmGame2D.Interactions;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class SleepAndDialogueTimingTests
    {
        [Test]
        public void Dialogue_StaysLongEnoughToReadOneLine()
        {
            Assert.That(VillagerDialogueDismiss.Seconds, Is.GreaterThan(2f).And.LessThan(3f));
            var visibleUntil = 10f + VillagerDialogueDismiss.Seconds;
            Assert.That(VillagerDialogueDismiss.ShouldClose(false, false, false, visibleUntil, visibleUntil - 0.1f), Is.False);
            Assert.That(VillagerDialogueDismiss.ShouldClose(false, false, false, visibleUntil, visibleUntil), Is.True);
            Assert.That(VillagerDialogueDismiss.ShouldClose(true, false, false, 10f, 20f), Is.False);
            Assert.That(VillagerDialogueDismiss.ShouldClose(false, true, false, 10f, 20f), Is.False);
            Assert.That(VillagerDialogueDismiss.ShouldClose(false, false, true, 10f, 20f), Is.False);
        }

        [Test]
        public void Sleep_FadesOutHoldsThenReturnsToMorning()
        {
            Assert.That(SleepTransitionTiming.Darkness(0f), Is.EqualTo(0f));
            Assert.That(SleepTransitionTiming.Darkness(SleepTransitionTiming.FadeOutSeconds * 0.5f), Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(SleepTransitionTiming.Darkness(SleepTransitionTiming.FadeOutSeconds + 0.1f), Is.EqualTo(1f));
            Assert.That(SleepTransitionTiming.HasReachedNight(SleepTransitionTiming.FadeOutSeconds), Is.True);
            Assert.That(SleepTransitionTiming.HasReachedNight(SleepTransitionTiming.FadeOutSeconds - 0.01f), Is.False);
            Assert.That(SleepTransitionTiming.Darkness(SleepTransitionTiming.TotalSeconds), Is.EqualTo(0f));
        }
    }
}
