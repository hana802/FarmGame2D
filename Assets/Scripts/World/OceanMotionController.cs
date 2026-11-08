using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame2D.World
{
    public sealed class OceanMotionController : MonoBehaviour
    {
        private Tilemap tilemap;
        private Vector3 origin;
        private Color baseColor;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            origin = transform.localPosition;
            baseColor = tilemap.color;
        }

        private void Update()
        {
            var wave = Mathf.Sin(Time.time * 0.75f);
            var ripple = Mathf.Sin(Time.time * 1.35f + 0.8f);
            transform.localPosition = origin + new Vector3(wave * 0.035f, ripple * 0.025f, 0f);
            tilemap.color = Color.Lerp(baseColor, baseColor * new Color(0.92f, 1f, 1.08f, 1f), (wave + 1f) * 0.25f);
        }
    }
}
