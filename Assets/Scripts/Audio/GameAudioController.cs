using UnityEngine;

namespace FarmGame2D.Audio
{
    [DefaultExecutionOrder(-50)]
    public sealed class GameAudioController : MonoBehaviour
    {
        private static GameAudioController instance;

        public static void Play(GameAudioCue cue)
        {
            // The generated cues, especially tilling, are the wrong sounds. Keep them silent until replaced.
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
