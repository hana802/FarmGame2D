using UnityEngine;

namespace FarmGame2D.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WorldDepthSorter : MonoBehaviour
    {
        private const int BaseOrder = 1000;
        private const float OrdersPerUnit = 10f;
        public const int BuildingDepthOffset = 6;

        private SpriteRenderer spriteRenderer;
        private int offset;

        public int CurrentOrder => spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Refresh();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        public void Configure(int sortingOffset = 0)
        {
            offset = sortingOffset;
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            Refresh();
        }

        public static int CalculateOrder(float worldY, int sortingOffset = 0)
        {
            return BaseOrder - Mathf.RoundToInt(worldY * OrdersPerUnit) + sortingOffset;
        }

        private void Refresh()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sortingOrder = CalculateOrder(transform.position.y, offset);
        }
    }
}
