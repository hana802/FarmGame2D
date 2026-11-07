using System;
using UnityEngine;

namespace FarmGame2D.TimeSystem
{
    public sealed class GameClockController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float realSecondsPerStep = 3f;
        [SerializeField, Min(1)] private int gameMinutesPerStep = 1;

        private float elapsedSeconds;

        public static GameClockController Instance { get; private set; }
        public GameClock Clock { get; private set; }
        public GameTimeSnapshot CurrentTime => Clock.Current;

        public event Action<GameTimeSnapshot> TimeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Clock = new GameClock();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            TimeChanged?.Invoke(CurrentTime);
        }

        private void Update()
        {
            elapsedSeconds += Time.deltaTime;

            while (elapsedSeconds >= realSecondsPerStep)
            {
                elapsedSeconds -= realSecondsPerStep;
                Clock.AdvanceMinutes(gameMinutesPerStep);
                TimeChanged?.Invoke(CurrentTime);
            }
        }

        public void SetTime(int day, int hour, int minute)
        {
            Clock.Set(day, hour, minute);
            elapsedSeconds = 0f;
            TimeChanged?.Invoke(CurrentTime);
        }
    }
}
