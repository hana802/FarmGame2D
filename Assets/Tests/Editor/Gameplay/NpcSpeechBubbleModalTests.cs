using FarmGame2D.World;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class NpcSpeechBubbleModalTests
    {
        [Test]
        public void OpenModal_HidesNpcSpeechBubble()
        {
            Assert.That(NpcSpeechBubblePresentation.ShouldDraw("잠깐 쉬어 가는 건 어때요?", modalOpen: false), Is.True);
            Assert.That(NpcSpeechBubblePresentation.ShouldDraw("잠깐 쉬어 가는 건 어때요?", modalOpen: true), Is.False);
        }
    }
}
