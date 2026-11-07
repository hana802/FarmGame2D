using UnityEngine;

namespace FarmGame2D.TimeSystem
{
    public static class GameTimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateGameClock()
        {
            if (Object.FindAnyObjectByType<GameClockController>() != null)
            {
                return;
            }

            var clockObject = new GameObject("Game Time");
            clockObject.AddComponent<GameClockController>();
        }
    }
}
