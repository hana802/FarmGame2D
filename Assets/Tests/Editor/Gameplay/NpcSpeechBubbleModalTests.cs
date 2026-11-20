using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;

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

        [Test]
        public void SpeechBubble_HeightFitsWrappedText()
        {
            var style = NpcSpeechBubblePresentation.CreateStyle(new GUIStyle(),
                Resources.Load<Font>("Fonts/NotoSansKR-VF"));
            const string line = "안녕하세요. 오늘도 밭일이 잘 풀렸으면 좋겠어요. 잠깐 쉬어 가는 건 어때요?";

            Assert.That(NpcSpeechBubblePresentation.CalculateHeight(style, line, 128f),
                Is.GreaterThanOrEqualTo(style.CalcHeight(new GUIContent(line), 128f)));
        }
    }
}
