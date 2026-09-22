using System;
using UnityEngine;

namespace Game
{
    public class GameTime : MonoBehaviour
    {

        [SerializeField] private float secondsPerDay = 600f;

        public int day = 1;
        private float dayTimer;
        public bool paused;
        private float secondsPerTick = 1f;
        public long tick;
        private float tickTimer;
        public static GameTime Instance { get; private set; }

        public float DayProgress => dayTimer / secondsPerDay;

        private void Awake()
        {
            dayTimer = 300;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            secondsPerDay = Mathf.Max(1f, secondsPerDay);
            secondsPerTick = Mathf.Max(0.01f, secondsPerDay);

            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            dayTimer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            while (dayTimer >= secondsPerDay)
            {
                dayTimer -= secondsPerDay;
                day++;

                if (OnNewDay != null)
                {
                    OnNewDay(day);
                }
            }

            while (tickTimer >= secondsPerTick)
            {
                tickTimer -= secondsPerTick;
                AdvanceTick();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public event Action<long> OnTick;
        public event Action<int> OnNewDay;

        public void AdvanceTick()
        {
            tick++;

            if (OnTick != null)
            {
                OnTick(tick);
            }
        }
    }
}
