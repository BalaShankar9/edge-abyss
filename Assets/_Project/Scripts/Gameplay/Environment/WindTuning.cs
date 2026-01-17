using UnityEngine;

namespace EdgeAbyss.Gameplay.Environment
{
    /// <summary>
    /// ScriptableObject containing all tunable parameters for the wind system.
    /// 
    /// HOW TO CREATE:
    /// 1. Right-click in Project window.
    /// 2. Select: Create > EdgeAbyss > Wind Tuning
    /// 3. Name it "WindTuning" and save in Assets/_Project/Configs/
    /// 4. Assign to WindSystem component.
    /// </summary>
    [CreateAssetMenu(fileName = "WindTuning", menuName = "EdgeAbyss/Wind Tuning", order = 4)]
    public class WindTuning : ScriptableObject
    {
        [Header("Base Wind")]
        [Tooltip("Enable global ambient wind.")]
        public bool enableAmbientWind = true;

        [Tooltip("Base wind direction (normalized automatically).")]
        public Vector3 baseWindDirection = Vector3.right;

        [Tooltip("Base wind intensity (force units).")]
        [Range(0f, 10f)] public float baseWindIntensity = 2f;

        [Tooltip("How much the wind direction varies over time (0 = constant).")]
        [Range(0f, 45f)] public float directionVariance = 15f;

        [Tooltip("Speed of direction variance oscillation.")]
        [Range(0.1f, 2f)] public float varianceSpeed = 0.3f;

        [Header("Wind Gusts")]
        [Tooltip("Enable random wind gusts.")]
        public bool enableGusts = true;

        [Tooltip("Time between gusts (seconds).")]
        [Range(2f, 20f)] public float gustInterval = 8f;

        [Tooltip("Randomness in gust timing (± seconds).")]
        [Range(0f, 10f)] public float gustIntervalVariance = 3f;

        [Tooltip("Gust duration (seconds).")]
        [Range(0.5f, 5f)] public float gustDuration = 1.5f;

        [Tooltip("Gust intensity multiplier (applied on top of base).")]
        [Range(1f, 5f)] public float gustIntensityMultiplier = 2.5f;

        [Header("Rider Effects")]
        [Tooltip("Lateral force applied to rider per wind intensity unit.")]
        [Range(0f, 5f)] public float lateralForceMultiplier = 1f;

        [Tooltip("Stability reduction per wind intensity unit.")]
        [Range(0f, 0.1f)] public float stabilityImpactPerIntensity = 0.02f;

        [Tooltip("How quickly wind effects ramp up/down.")]
        [Range(1f, 10f)] public float effectSmoothSpeed = 4f;

        [Header("Gust Zone Settings")]
        [Tooltip("Default pulse frequency for gust zones (cycles per second).")]
        [Range(0.1f, 2f)] public float defaultPulseFrequency = 0.5f;

        [Tooltip("Default intensity for gust zone pulses.")]
        [Range(1f, 10f)] public float defaultZoneIntensity = 5f;

        [Header("Audio/Visual Cues")]
        [Tooltip("Intensity threshold to trigger strong wind audio/visual.")]
        [Range(2f, 10f)] public float strongWindThreshold = 5f;
    }
}
