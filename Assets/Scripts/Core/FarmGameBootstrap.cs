using FarmGame2D.Player;
using FarmGame2D.UI;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed class FarmGameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (FindAnyObjectByType<FarmGameBootstrap>() != null)
            {
                return;
            }

            new GameObject("Farm Game Bootstrap").AddComponent<FarmGameBootstrap>();
        }

        private void Start()
        {
            var session = GameSession.Instance;
            if (session == null)
            {
                session = new GameObject("Game Session").AddComponent<GameSession>();
            }

            var world = FindAnyObjectByType<PrototypeWorldBuilder>();
            if (world == null)
            {
                world = new GameObject("Prototype World").AddComponent<PrototypeWorldBuilder>();
                world.Build(session);
            }

            var player = FindAnyObjectByType<PrototypePlayerController>();
            if (player == null)
            {
                player = new GameObject("Player").AddComponent<PrototypePlayerController>();
            }

            var playerObject = player.gameObject;
            playerObject.GetComponent<SpriteRenderer>().sortingOrder = 10;

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            var cameraFollow = camera.GetComponent<PixelCameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = camera.gameObject.AddComponent<PixelCameraFollow>();
            }

            cameraFollow.Configure(player.transform);

            if (FindAnyObjectByType<PrototypeHud>() == null)
            {
                new GameObject("Prototype HUD").AddComponent<PrototypeHud>().Configure(player);
            }
        }
    }
}
