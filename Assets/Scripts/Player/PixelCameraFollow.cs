using UnityEngine;
using UnityEngine.U2D;
using FarmGame2D.World;

namespace FarmGame2D.Player
{
    [RequireComponent(typeof(Camera))]
    public sealed class PixelCameraFollow : MonoBehaviour
    {
        private Transform target;
        private Camera worldCamera;
        private PixelPerfectCamera pixelPerfect;

        public bool IsInteriorView { get; private set; }
        public int ReferenceHeight => pixelPerfect == null ? 0 : pixelPerfect.refResolutionY;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            worldCamera = GetComponent<Camera>();
            worldCamera.orthographic = true;
            worldCamera.backgroundColor = new Color(0.08f, 0.12f, 0.1f);

            pixelPerfect = GetComponent<PixelPerfectCamera>();
            if (pixelPerfect == null)
            {
                pixelPerfect = gameObject.AddComponent<PixelPerfectCamera>();
            }

            pixelPerfect.assetsPPU = 32;
            pixelPerfect.upscaleRT = true;
            pixelPerfect.pixelSnapping = true;
            ApplyViewForPosition(target.position);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            ApplyViewForPosition(target.position);

            const float pixelsPerUnit = 32f;
            var focusPosition = IsInteriorView
                ? WorldAreaProfile.FarmhouseInteriorCenter
                : (Vector2)target.position;
            var x = Mathf.Round(focusPosition.x * pixelsPerUnit) / pixelsPerUnit;
            var y = Mathf.Round(focusPosition.y * pixelsPerUnit) / pixelsPerUnit;
            transform.position = new Vector3(x, y, -10f);
        }

        private void ApplyViewForPosition(Vector2 position)
        {
            IsInteriorView = WorldAreaProfile.IsInterior(position);
            worldCamera.orthographicSize = IsInteriorView
                ? WorldAreaProfile.InteriorCameraSize
                : WorldAreaProfile.OutdoorCameraSize;
            pixelPerfect.refResolutionX = IsInteriorView
                ? WorldAreaProfile.InteriorReferenceWidth
                : WorldAreaProfile.OutdoorReferenceWidth;
            pixelPerfect.refResolutionY = IsInteriorView
                ? WorldAreaProfile.InteriorReferenceHeight
                : WorldAreaProfile.OutdoorReferenceHeight;
        }
    }
}
