using System.Collections;
using FarmGame2D.Audio;
using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class FishingSpotInteractable : InteractableBehaviour
    {
        private bool fishing;
        private PrototypePlayerController angler;

        public override string Prompt => fishing ? "입질을 기다리는 중" : "낚싯대 던지기";

        public override void Interact(PrototypePlayerController player)
        {
            var session = GameSession.Instance;
            if (fishing || session == null || player.IsMining)
            {
                return;
            }

            if (session.SelectedTool != FarmTool.FishingRod)
            {
                session.ShowMessage("가방에서 낚싯대를 꺼내세요.");
                return;
            }

            StartCoroutine(PlayFishingAnimation(player));
        }

        private IEnumerator PlayFishingAnimation(PrototypePlayerController player)
        {
            fishing = true;
            angler = player;
            var castTarget = FishingCastTarget.Resolve(transform.position, player.transform.position);
            player.BeginMining(castTarget);
            GameObject bobber = null;
            LineRenderer line = null;
            var released = false;
            var elapsed = 0f;
            while (elapsed < FishingCastMotion.ReleaseTime + 1.6f && player != null)
            {
                elapsed += Time.deltaTime;
                var tip = player.PresentFishingCast(castTarget, FishingCastMotion.Pullback(elapsed));
                if (!released && elapsed >= FishingCastMotion.ReleaseTime)
                {
                    released = true;
                    GameSession.Instance.ShowMessage("낚싯줄을 던졌습니다…");
                    GameAudioController.Play(GameAudioCue.FishingCast);
                    bobber = PrototypeSpriteFactory.CreateBlock("Fishing Bobber", castTarget,
                        new Vector2(0.16f, 0.16f), new Color(0.95f, 0.35f, 0.25f));
                    line = bobber.AddComponent<LineRenderer>();
                    line.positionCount = 2;
                    line.startWidth = line.endWidth = 0.025f;
                    line.sortingOrder = 180;
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.startColor = line.endColor = new Color(0.85f, 0.85f, 0.78f);
                }

                if (bobber != null)
                {
                    var bob = Mathf.Sin(elapsed * 10f) * 0.07f;
                    bobber.transform.position = castTarget + new Vector2(0f, bob);
                    line.SetPosition(0, tip);
                    line.SetPosition(1, bobber.transform.position);
                }

                yield return null;
            }

            if (bobber != null)
            {
                Destroy(bobber);
            }

            if (player != null)
            {
                player.EndMining();
            }

            angler = null;

            if (released && GameSession.Instance != null)
            {
                GameAudioController.Play(GameAudioCue.FishingCatch);
                GameSession.Instance.CatchFish();
            }

            fishing = false;
        }

        private void OnDestroy()
        {
            if (angler != null)
            {
                angler.EndMining();
            }
        }
    }
}
