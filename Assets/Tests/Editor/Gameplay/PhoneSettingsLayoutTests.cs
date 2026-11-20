using FarmGame2D.UI;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class PhoneSettingsLayoutTests
    {
        [Test]
        public void PhoneSettingsControls_StayInsidePhonePanel()
        {
            var layout = HudLayout.CalculatePhoneLayout(settings: true);

            Assert.That(layout.Panel, Is.EqualTo(HudLayout.CalculatePhoneLayout().Panel));
            AssertInside(layout.Screen, layout.SettingsTabButton);
            AssertInside(layout.Content, layout.VolumeSlider);
            AssertInside(layout.Content, layout.MuteButton);
            AssertInside(layout.CloudPanel, layout.SaveButton);
            AssertInside(layout.CloudPanel, layout.QuitButton);
            AssertInside(layout.CloudPanel, layout.Notice);
        }

        [Test]
        public void PhoneSettingsButtons_DoNotOverlap()
        {
            var layout = HudLayout.CalculatePhoneLayout(settings: true);

            Assert.That(layout.MuteButton.Overlaps(layout.SaveButton), Is.False);
            Assert.That(layout.SaveButton.Overlaps(layout.QuitButton), Is.False);
            Assert.That(layout.QuitButton.Overlaps(layout.Notice), Is.False);
            Assert.That(layout.Notice.Overlaps(layout.CloseButton), Is.False);
        }

        [Test]
        public void FarmPhoneCardsAndHeader_StayInsidePanelWithoutOverlap()
        {
            var layout = HudLayout.CalculatePhoneLayout();
            AssertInside(new UnityEngine.Rect(0, 0, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight), layout.Panel);
            AssertInside(layout.Panel, layout.Screen);
            AssertInside(layout.Screen, layout.Content);
            AssertInside(layout.Screen, layout.CloudPanel);
            AssertInside(layout.Header, layout.Title);
            AssertInside(layout.Header, layout.CloseButton);
            AssertInside(layout.Content, layout.QuestBody);
            AssertInside(layout.Content, layout.SaveButton);
            AssertInside(layout.Content, layout.LoadButton);
            Assert.That(layout.Content.Overlaps(layout.CloudPanel), Is.False);
            Assert.That(layout.Title.Overlaps(layout.CloseButton), Is.False);
            Assert.That(layout.Title.Overlaps(layout.FarmTabButton), Is.False);
        }

        private static void AssertInside(UnityEngine.Rect panel, UnityEngine.Rect control)
        {
            Assert.That(control.xMin, Is.GreaterThanOrEqualTo(panel.xMin));
            Assert.That(control.xMax, Is.LessThanOrEqualTo(panel.xMax));
            Assert.That(control.yMin, Is.GreaterThanOrEqualTo(panel.yMin));
            Assert.That(control.yMax, Is.LessThanOrEqualTo(panel.yMax));
        }
    }
}
