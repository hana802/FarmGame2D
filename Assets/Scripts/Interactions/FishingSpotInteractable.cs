using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class FishingSpotInteractable : InteractableBehaviour
    {
        private bool fishing;

        public override string Prompt => fishing ? "입질을 기다리는 중" : "낚싯대 던지기";

        public override void Interact(PrototypePlayerController player)
        {
            var session = GameSession.Instance;
            if (fishing || session == null)
            {
                return;
            }

            if (session.SelectedTool != FarmTool.FishingRod)
            {
                session.ShowMessage("가방에서 낚싯대를 꺼내세요.");
                return;
            }

            StartCoroutine(PlayFishingAnimation(player.transform));
        }

        private IEnumerator PlayFishingAnimation(Transform player)
        {
            fishing = true;
            GameSession.Instance.ShowMessage("낚싯줄을 던졌습니다…");
            var bobber = PrototypeSpriteFactory.CreateBlock("Fishing Bobber", transform.position,
                new Vector2(0.16f, 0.16f), new Color(0.95f, 0.35f, 0.25f));
            var line = bobber.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.025f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = new Color(0.85f, 0.85f, 0.78f);

            var elapsed = 0f;
            while (elapsed < 1.6f)
            {
                elapsed += Time.deltaTime;
                var bob = Mathf.Sin(elapsed * 10f) * 0.07f;
                bobber.transform.position = transform.position + new Vector3(0f, bob, 0f);
                line.SetPosition(0, player.position + new Vector3(0f, 0.55f));
                line.SetPosition(1, bobber.transform.position);
                yield return null;
            }

            Destroy(bobber);
            GameSession.Instance.CatchFish();
            fishing = false;
        }
    }
}
