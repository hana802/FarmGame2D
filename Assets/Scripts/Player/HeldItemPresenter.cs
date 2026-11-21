using FarmGame2D.Art;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Player
{
    public sealed class HeldItemPresenter : MonoBehaviour
    {
        private SpriteRenderer itemRenderer;
        private SpriteRenderer ownerRenderer;
        private FarmTool? displayedTool;

        public SpriteRenderer ItemRenderer => itemRenderer;
        public FarmTool? DisplayedTool => displayedTool;

        private void Awake()
        {
            ownerRenderer = GetComponent<SpriteRenderer>();
            var itemObject = new GameObject("Held Item");
            itemObject.transform.SetParent(transform, false);
            itemRenderer = itemObject.AddComponent<SpriteRenderer>();
            itemRenderer.enabled = false;
        }

        public void Present(FarmTool tool, int facingRow, bool moving, bool available)
        {
            if (!available)
            {
                itemRenderer.enabled = false;
                displayedTool = null;
                return;
            }

            if (displayedTool != tool)
            {
                itemRenderer.sprite = GameArtCatalog.GetHeldToolSprite(tool);
                displayedTool = tool;
            }

            itemRenderer.enabled = itemRenderer.sprite != null;
            itemRenderer.flipX = facingRow == 2;
            itemRenderer.sortingOrder = ownerRenderer.sortingOrder + (facingRow == 0 ? -1 : 1);
            itemRenderer.transform.localScale = Vector3.one * GetScale(tool);
            itemRenderer.transform.localRotation = Quaternion.identity;

            var position = GetLocalPosition(facingRow);
            if (moving)
            {
                position.y += Mathf.Sin(Time.time * 12f) * 0.025f;
            }

            itemRenderer.transform.localPosition = position;
        }

        internal void PresentMining(int facingRow, Vector2 target, float pullback, FarmTool tool)
        {
            Present(tool, facingRow, false, true);
            var pickaxe = itemRenderer.transform;
            var direction = target - (Vector2)pickaxe.position;
            var side = facingRow == 2 ? -1f : 1f;
            pickaxe.localScale = Vector3.one * 0.9f;
            pickaxe.localRotation = Quaternion.Euler(0f, 0f,
                Vector2.SignedAngle(Vector2.up, direction) + side * pullback);
        }

        internal Vector3 PresentFishingCast(int facingRow, Vector2 target, float pullback)
        {
            Present(FarmTool.FishingRod, facingRow, false, true);
            var rod = itemRenderer.transform;
            var direction = target - (Vector2)rod.position;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.up;
            }

            var side = facingRow == 2 ? -1f : 1f;
            rod.localRotation = Quaternion.Euler(0f, 0f,
                Vector2.SignedAngle(Vector2.up, direction) + side * pullback);
            var tip = itemRenderer.sprite == null ? 0.7f : itemRenderer.sprite.bounds.max.y;
            return rod.TransformPoint(new Vector3(0f, tip, 0f));
        }

        private static Vector3 GetLocalPosition(int facingRow)
        {
            return facingRow switch
            {
                0 => new Vector3(-0.28f, 0.46f, 0f),
                1 => new Vector3(0.34f, 0.42f, 0f),
                2 => new Vector3(-0.34f, 0.42f, 0f),
                _ => new Vector3(0.3f, 0.38f, 0f)
            };
        }

        private static float GetScale(FarmTool tool)
        {
            return tool switch
            {
                FarmTool.Hoe => 0.55f,
                FarmTool.Seeds => 0.5f,
                FarmTool.WateringCan => 0.58f,
                FarmTool.Pickaxe => 0.55f,
                _ => 0.55f
            };
        }
    }
}
