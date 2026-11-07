using UnityEngine;
using UnityEngine.U2D;

namespace FarmGame2D.Player
{
    [RequireComponent(typeof(Camera))]
    public sealed class PixelCameraFollow : MonoBehaviour
    {
        private Transform target;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
            var camera = GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.75f;
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.1f);

            var pixelPerfect = GetComponent<PixelPerfectCamera>();
            if (pixelPerfect == null)
            {
                pixelPerfect = gameObject.AddComponent<PixelPerfectCamera>();
            }

            pixelPerfect.assetsPPU = 32;
            pixelPerfect.refResolutionX = 480;
            pixelPerfect.refResolutionY = 270;
            pixelPerfect.upscaleRT = true;
            pixelPerfect.pixelSnapping = true;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            const float pixelsPerUnit = 32f;
            var x = Mathf.Round(target.position.x * pixelsPerUnit) / pixelsPerUnit;
            var y = Mathf.Round(target.position.y * pixelsPerUnit) / pixelsPerUnit;
            transform.position = new Vector3(x, y, -10f);
        }
    }
}
