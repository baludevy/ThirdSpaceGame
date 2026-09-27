using System;
using UnityEngine;

namespace Game
{
    public class GameTime : MonoBehaviour
    {
        public static GameTime Instance { get; private set; }

        public bool paused;
        public long tick { get; private set; }
        public int day { get; private set; }

        public float DayProgress { get; private set; }
        public event Action<long> OnTick;
        public event Action<int> OnNewDay;

        private long lastTick;
        private int lastDay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UpdateFromRealTime(notify: false);
        }

        private void Update()
        {
            if (!paused)
            {
                UpdateFromRealTime(notify: true);
            }
        }

        private void UpdateFromRealTime(bool notify)
        {
            DateTime now = DateTime.UtcNow;
            long currentTick = new DateTimeOffset(now).ToUnixTimeSeconds();
            
            int currentDay = (int)(now.Date - new DateTime(1970, 1, 1)).TotalDays;

            tick = currentTick;
            day = currentDay;
            DayProgress = (float)now.TimeOfDay.TotalSeconds / 86400f;

            if (!notify)
            {
                lastTick = currentTick;
                lastDay = currentDay;
                return;
            }
            if (currentDay != lastDay)
            {
                lastDay = currentDay;
                OnNewDay?.Invoke(day);
            }
            if (currentTick != lastTick)
            {
                lastTick = currentTick;
                OnTick?.Invoke(tick);
            }
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
