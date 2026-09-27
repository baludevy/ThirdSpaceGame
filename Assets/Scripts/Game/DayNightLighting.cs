using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class DayNightLighting : MonoBehaviour
    {
        [SerializeField] private Color dayColor = new Color(1f, 0.9f, 0.8f);
        [SerializeField] private Color nightColor = new Color(0.35f, 0.45f, 0.8f);
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float nightIntensivity = 0.2f;

        private Light2D globalLight;

        private void Awake()
        {
            globalLight = GetComponent<Light2D>();
        }

        private void LateUpdate()
        {
            if (GameTime.Instance == null) return;

            float time = GameTime.Instance.DayProgress;
            float daylight = 0.5f - 0.5f * Mathf.Cos(time * Mathf.PI * 2f);
            daylight = Mathf.SmoothStep(0f, 1f, daylight);

            globalLight.color = Color.Lerp(nightColor, dayColor, daylight);
            globalLight.intensity = Mathf.Lerp(nightIntensivity, dayIntensity, daylight);
        }
    }
}
