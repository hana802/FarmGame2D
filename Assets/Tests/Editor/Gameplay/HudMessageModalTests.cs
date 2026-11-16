using FarmGame2D.UI;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class HudMessageModalTests
    {
        [Test]
        public void NewMessage_WaitsUntilModalClosesBeforeStartingDisplayTimer()
        {
            var visibility = new HudMessageVisibility();

            Assert.That(visibility.ShouldShow(7, 0f, 4f, modalOpen: true), Is.False);
            Assert.That(visibility.ShouldShow(7, 10f, 4f, modalOpen: false), Is.True);
            Assert.That(visibility.ShouldShow(7, 13.9f, 4f, modalOpen: false), Is.True);
        }
    }
}
