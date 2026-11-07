using FarmGame2D.Art;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Player
{
    public sealed class HeldItemPresenter : MonoBehaviour
    {
        private SpriteRenderer itemRenderer;
        private FarmTool? displayedTool;

        public SpriteRenderer ItemRenderer => itemRenderer;
        public FarmTool? DisplayedTool => displayedTool;

        private void Awake()
        {
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
            itemRenderer.sortingOrder = facingRow == 0 ? 9 : 11;
            itemRenderer.transform.localScale = Vector3.one * GetScale(tool);

            var position = GetLocalPosition(facingRow);
            if (moving)
            {
                position.y += Mathf.Sin(Time.time * 12f) * 0.025f;
            }

            itemRenderer.transform.localPosition = position;
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
                _ => 0.55f
            };
        }
    }
}
