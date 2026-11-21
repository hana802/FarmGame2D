using System.Reflection;
using FarmGame2D.UI;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests
{
    public sealed class DogSitPresentationTests
    {
        [Test]
        public void Sitting_PreservesDogSpriteProportions()
        {
            var dogObject = new GameObject("Sit scale dog");
            var dog = dogObject.AddComponent<DogCompanionController>();
            typeof(DogCompanionController)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(dog, null);

            dog.SetSitting(true);

            var visual = dogObject.transform.Find("Farm Dog Visual");
            Assert.That(visual, Is.Not.Null);
            Assert.That(visual.localScale.x, Is.EqualTo(0.84f).Within(0.001f));
            Assert.That(visual.localScale.y, Is.EqualTo(visual.localScale.x).Within(0.001f));
            Assert.That(visual.localPosition, Is.EqualTo(Vector3.zero));

            Object.DestroyImmediate(dogObject);
        }

        [Test]
        public void SitMessage_FitsInsideToastBanner()
        {
            var font = Resources.Load<Font>("Fonts/NotoSansKR-VF");
            Assert.That(font, Is.Not.Null);
            var style = new GUIStyle
            {
                font = font,
                fontSize = PrototypeHud.ToastMessageFontSize,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            var layout = HudLayout.CalculateToastLayout();
            var height = style.CalcHeight(
                new GUIContent("강아지가 그 자리에 앉았습니다. G를 다시 누르면 따라옵니다."),
                layout.Message.width);

            Assert.That(PrototypeHud.ToastMessageFontSize, Is.LessThanOrEqualTo(14),
                "Toast text should stay at the banner label size so it does not outgrow the ribbon.");
            Assert.That(height, Is.GreaterThan(0f));
            Assert.That(height, Is.LessThanOrEqualTo(layout.Message.height));
        }
    }
}
