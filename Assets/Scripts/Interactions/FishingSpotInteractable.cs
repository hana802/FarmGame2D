using System.Collections;
using FarmGame2D.Audio;
using FarmGame2D.Core;
using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class FishingSpotInteractable : InteractableBehaviour
    {
        private const float CastDuration = 1.6f;
        private const float BiteStart = 1.32f;
        private static Sprite bobberSprite;
        private static Sprite rippleSprite;
        private PrototypePlayerController angler;
        private GameObject castVisual;

        public override string Prompt => angler != null ? "입질을 기다리는 중" : "낚싯대 던지기";

        public override void Interact(PrototypePlayerController player)
        {
            var session = GameSession.Instance;
            if (!isActiveAndEnabled || angler != null || player == null || !player.isActiveAndEnabled
                || player.IsFishing || session == null)
            {
                return;
            }

            if (session.SelectedTool != FarmTool.FishingRod)
            {
                session.ShowMessage("가방에서 낚싯대를 꺼내세요.");
                return;
            }

            angler = player;
            angler.IsFishing = true;
            StartCoroutine(PlayFishingAnimation(session));
        }

        private IEnumerator PlayFishingAnimation(GameSession session)
        {
            GameAudioController.Play(GameAudioCue.FishingCast);
            var castTarget = FishingCastTarget.Resolve(transform.position, angler.transform.position);
            castVisual = new GameObject("Fishing Cast");
            castVisual.transform.position = castTarget;
            castVisual.transform.SetParent(transform, true);
            bobberSprite ??= CreateBobberSprite();
            rippleSprite ??= CreateRippleSprite();
            var bobber = CreateEffect("Fishing Bobber", bobberSprite, 7);
            var outerRipple = CreateEffect("Fishing Outer Ripple", rippleSprite, 5);
            var innerRipple = CreateEffect("Fishing Inner Ripple", rippleSprite, 6);
            var line = castVisual.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = 0.026f;
            line.endWidth = 0.014f;
            line.sharedMaterial = bobber.sharedMaterial;
            line.sortingOrder = 4;
            line.startColor = line.endColor = new Color(0.98f, 0.93f, 0.78f, 0.8f);

            var elapsed = 0f;
            while (elapsed < CastDuration && angler != null && angler.isActiveAndEnabled
                && GameSession.Instance == session)
            {
                var bite = Mathf.InverseLerp(BiteStart, CastDuration, elapsed);
                var dip = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(bite * 2.7f));
                var bob = Mathf.Sin(elapsed * 10f) * 0.05f;
                bobber.transform.localPosition = Vector3.up * (0.065f + bob - dip * 0.17f);
                bobber.color = new Color(1f, 1f, 1f, 1f - dip * 0.92f);
                line.SetPosition(0, angler.transform.position + new Vector3(0f, 0.55f));
                line.SetPosition(1, bobber.transform.position);
                if (bite > 0f)
                {
                    DrawRipple(outerRipple, bite, 0.96f, 1f);
                    DrawRipple(innerRipple, Mathf.Clamp01(bite * 1.5f - 0.15f), 0.68f, 0.9f);
                }
                else
                {
                    DrawRipple(outerRipple, Mathf.Repeat(elapsed * 1.65f + 0.48f, 1f), 0.64f, 0.78f);
                    DrawRipple(innerRipple, Mathf.Repeat(elapsed * 1.65f, 1f), 0.64f, 0.78f);
                }
                yield return null;
                elapsed += Time.deltaTime;
            }

            var caught = elapsed >= CastDuration && angler != null && angler.isActiveAndEnabled
                && GameSession.Instance == session;
            ClearCast();
            if (!caught) yield break;

            GameAudioController.Play(GameAudioCue.FishingCatch);
            session.CatchFish();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            ClearCast();
        }

        private void ClearCast()
        {
            if (angler != null) angler.IsFishing = false;
            angler = null;
            if (castVisual != null)
            {
                castVisual.SetActive(false);
                Destroy(castVisual);
            }
            castVisual = null;
        }

        private SpriteRenderer CreateEffect(string effectName, Sprite sprite, int order)
        {
            var effect = new GameObject(effectName);
            effect.transform.SetParent(castVisual.transform, false);
            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void DrawRipple(SpriteRenderer ripple, float progress, float maxWidth, float opacity)
        {
            var width = Mathf.Lerp(0.25f, maxWidth, progress);
            ripple.transform.localScale = new Vector3(width, width * 0.44f, 1f);
            ripple.color = new Color(0.78f, 0.96f, 1f, (1f - progress) * opacity);
        }

        private static Sprite CreateBobberSprite()
        {
            const int width = 12;
            const int height = 14;
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var distance = new Vector2((x - 5.5f) / 5.5f, (y - 5f) / 4.8f).sqrMagnitude;
                if (distance <= 1f)
                {
                    pixels[y * width + x] = distance > 0.72f
                        ? new Color32(91, 42, 46, 255)
                        : y < 5 ? new Color32(255, 236, 180, 255)
                        : x < 5 ? new Color32(250, 116, 78, 255) : new Color32(211, 58, 54, 255);
                }
                else if (y >= 9 && x >= 5 && x <= 6)
                {
                    pixels[y * width + x] = new Color32(255, 198, 94, 255);
                }
            }
            return CreateEffectSprite("Fishing Bobber", width, height, pixels, 46f);
        }

        private static Sprite CreateRippleSprite()
        {
            const int size = 48;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var radius = (new Vector2(x + 0.5f, y + 0.5f) - Vector2.one * (size * 0.5f)).magnitude;
                var alpha = Mathf.Clamp01(1f - Mathf.Abs(radius - 22f) / 1.75f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            return CreateEffectSprite("Fishing Ripple", size, size, pixels, size);
        }

        private static Sprite CreateEffectSprite(string spriteName, int width, int height, Color32[] pixels, float ppu)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = spriteName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
