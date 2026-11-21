using System.Collections;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed class ChoppableTree : InteractableBehaviour
    {
        private GameSession session;
        private SpriteRenderer canopy, stump;
        private Collider2D trunk, stumpCollider;
        private PrototypePlayerController chopper;
        private int treeId;

        public override string Prompt => session.SelectedTool == FarmTool.Axe ? "도끼로 나무 베기" : "나무를 베려면 도끼 장착";

        public void Configure(GameSession gameSession, int id)
        {
            session = gameSession;
            treeId = id;
            canopy = GetComponent<SpriteRenderer>();
            trunk = GetComponent<Collider2D>();
            var stumpObject = new GameObject("Tree Stump");
            stumpObject.transform.SetParent(transform, false);
            stump = stumpObject.AddComponent<SpriteRenderer>();
            stump.sprite = FarmEquipmentArt.Stump;
            stump.sortingOrder = canopy.sortingOrder;
            var blocker = stumpObject.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(0.45f, 0.4f);
            stumpCollider = blocker;
            session.StateChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (session != null) session.StateChanged -= Refresh;
            if (chopper != null) chopper.EndMining();
        }

        private void Refresh()
        {
            var cut = session.IsTreeCut(treeId);
            canopy.enabled = !cut;
            trunk.enabled = !cut;
            stump.enabled = cut;
            stumpCollider.enabled = cut;
        }

        public override void Interact(PrototypePlayerController player)
        {
            if (chopper != null || player.IsMining || session.IsTreeCut(treeId)) return;
            if (session.SelectedTool != FarmTool.Axe)
            {
                session.ShowMessage("가방에서 도끼를 장착하면 나무를 벨 수 있어요.");
                return;
            }

            chopper = player;
            StartCoroutine(Chop());
        }

        private IEnumerator Chop()
        {
            var contact = (Vector2)transform.position + Vector2.up * 0.35f;
            chopper.BeginMining(contact);
            var struck = false;
            for (var elapsed = 0f; elapsed < 0.5f && chopper != null; elapsed += Time.deltaTime)
            {
                var recoil = elapsed < 0.24f ? Mathf.Lerp(105f, 0f, elapsed / 0.24f)
                    : Mathf.Lerp(0f, 38f, (elapsed - 0.24f) / 0.26f);
                chopper.PresentMiningSwing(contact, recoil, FarmTool.Axe);
                if (!struck && elapsed >= 0.24f)
                {
                    session.ChopTree(treeId);
                    struck = true;
                }
                yield return null;
            }
            if (!struck && chopper != null) session.ChopTree(treeId);
            if (chopper != null) chopper.EndMining();
            chopper = null;
        }
    }
}
