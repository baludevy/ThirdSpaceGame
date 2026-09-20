using System;
using UnityEngine;

public class GameTime : MonoBehaviour {
    public static GameTime Instance { get; private set; }

    public event Action<long> OnTick;
    public event Action<int> OnNewDay;

    [SerializeField] private float secondsPerDay = 600f;
    private float secondsPerTick = 1f;

    [NonSerialized] public int day = 1;
    [NonSerialized] public long tick;
    [NonSerialized] public bool paused;

    private float dayTimer;
    private float tickTimer;

    public float DayProgress => dayTimer / secondsPerDay;

    private void Awake() {
        dayTimer = 300;
        
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        secondsPerDay = Mathf.Max(1f, secondsPerDay);
        secondsPerTick = Mathf.Max(0.01f, secondsPerDay);

        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if (paused) {
            return;
        }

        dayTimer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        while (dayTimer >= secondsPerDay) {
            dayTimer -= secondsPerDay;
            day++;

            if (OnNewDay != null) {
                OnNewDay(day);
            }
        }

        while (tickTimer >= secondsPerTick) {
            tickTimer -= secondsPerTick;
            AdvanceTick();
        }
    }

    public void AdvanceTick() {
        tick++;

        if (OnTick != null) {
            OnTick(tick);
        }
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }
}