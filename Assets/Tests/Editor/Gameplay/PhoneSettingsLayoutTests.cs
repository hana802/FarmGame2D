using FarmGame2D.UI;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class PhoneSettingsLayoutTests
    {
        [Test]
        public void PhoneSettingsControls_StayInsidePhonePanel()
        {
            var layout = HudLayout.CalculatePhoneLayout();

            AssertInside(layout.Panel, layout.SettingsTabButton);
            AssertInside(layout.Panel, layout.VolumeSlider);
            AssertInside(layout.Panel, layout.QuitButton);
        }

        [Test]
        public void PhoneSettingsButtons_DoNotOverlap()
        {
            var layout = HudLayout.CalculatePhoneLayout();

            Assert.That(layout.MuteButton.Overlaps(layout.SaveButton), Is.False);
            Assert.That(layout.SaveButton.Overlaps(layout.QuitButton), Is.False);
            Assert.That(layout.QuitButton.Overlaps(layout.Notice), Is.False);
            Assert.That(layout.Notice.Overlaps(layout.CloseButton), Is.False);
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
