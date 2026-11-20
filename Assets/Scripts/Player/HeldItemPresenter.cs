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
        private bool showingCastPose;

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

            if (displayedTool != tool || showingCastPose)
            {
                itemRenderer.sprite = GameArtCatalog.GetHeldToolSprite(tool);
                displayedTool = tool;
            }
            showingCastPose = false;

            itemRenderer.enabled = itemRenderer.sprite != null;
            itemRenderer.flipX = facingRow == 2;
            itemRenderer.flipY = false;
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

        public Vector3 PresentFishing(int facingRow, Vector2 direction, float swing, float bite)
        {
            if (!showingCastPose)
            {
                itemRenderer.sprite = GameArtCatalog.GetCastingRodSprite();
                displayedTool = FarmTool.FishingRod;
                showingCastPose = true;
            }

            var left = direction.x < -0.05f;
            var tip = GameArtCatalog.CastingRodTip;
            if (left) tip.y = -tip.y;
            var aim = Vector2.SignedAngle(tip, direction);
            var pullback = Mathf.Lerp(68f, 4f, swing) + Mathf.SmoothStep(0f, 59f, bite);
            var rod = itemRenderer.transform;
            rod.localPosition = GetLocalPosition(facingRow);
            rod.localRotation = Quaternion.Euler(0f, 0f, aim + (left ? -pullback : pullback));
            rod.localScale = Vector3.one * 0.85f;
            itemRenderer.enabled = true;
            itemRenderer.flipX = false;
            itemRenderer.flipY = left;
            itemRenderer.sortingOrder = ownerRenderer.sortingOrder + (facingRow == 0 ? -1 : 1);
            return rod.TransformPoint(tip);
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
