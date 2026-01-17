using UnityEngine;

namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// ScriptableObject containing all tunable parameters for a rider type.
    /// Create separate assets for Bike, Horse, etc.
    /// 
    /// HOW TO CREATE:
    /// 1. Right-click in Project window.
    /// 2. Select: Create > EdgeAbyss > Rider Tuning
    /// 3. Name it (e.g., "BikeTuning" or "HorseTuning").
    /// 4. Configure values in the Inspector.
    /// 5. Assign to the rider prefab or RiderManager.
    /// </summary>
    [CreateAssetMenu(fileName = "RiderTuning", menuName = "EdgeAbyss/Rider Tuning", order = 1)]
    public class RiderTuning : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Display name for this rider type.")]
        public string riderName = "Rider";

        [Header("Speed")]
        [Tooltip("Maximum forward speed (units/second).")]
        [Range(5f, 100f)] public float maxSpeed = 30f;

        [Tooltip("Acceleration rate (units/second²).")]
        [Range(1f, 50f)] public float acceleration = 15f;

        [Tooltip("Deceleration when braking (units/second²).")]
        [Range(5f, 100f)] public float brakeDeceleration = 25f;

        [Tooltip("Natural deceleration when no input (units/second²).")]
        [Range(0.5f, 10f)] public float drag = 2f;

        [Header("Steering")]
        [Tooltip("Maximum turn rate (degrees/second).")]
        [Range(30f, 180f)] public float maxTurnRate = 90f;

        [Tooltip("How quickly steering input is applied (higher = snappier).")]
        [Range(1f, 20f)] public float steerResponse = 8f;

        [Tooltip("Steering effectiveness at max speed (0 = no steering, 1 = full).")]
        [Range(0.2f, 1f)] public float highSpeedSteerFactor = 0.5f;

        [Header("Stability")]
        [Tooltip("Base stability recovery rate per second.")]
        [Range(0.1f, 2f)] public float stabilityRecoveryRate = 0.5f;

        [Tooltip("Stability threshold below which the rider falls.")]
        [Range(0f, 0.3f)] public float fallThreshold = 0.1f;

        [Tooltip("How much steering affects stability (higher = more unstable when turning).")]
        [Range(0f, 1f)] public float steerStabilityCost = 0.3f;

        [Tooltip("Stability bonus when focus is held.")]
        [Range(0f, 0.5f)] public float focusStabilityBonus = 0.2f;

        [Header("Lean (Visual)")]
        [Tooltip("Maximum lean angle when turning (degrees).")]
        [Range(5f, 45f)] public float maxLeanAngle = 25f;

        [Tooltip("How quickly the rider leans into turns.")]
        [Range(1f, 15f)] public float leanSpeed = 6f;

        [Header("Physics")]
        [Tooltip("Gravity multiplier (1 = normal gravity).")]
        [Range(0.5f, 3f)] public float gravityMultiplier = 1f;

        [Tooltip("Ground check distance.")]
        [Range(0.1f, 1f)] public float groundCheckDistance = 0.3f;

        [Header("Type-Specific")]
        [Tooltip("For horses: automatic micro-correction strength at low wobble.")]
        [Range(0f, 1f)] public float autoCorrection = 0f;

        [Tooltip("For horses: momentum inertia (higher = slower to change direction).")]
        [Range(0f, 1f)] public float momentumInertia = 0f;

        [Tooltip("For bikes: lean influence on turning.")]
        [Range(0f, 1f)] public float leanTurnInfluence = 0.5f;
    }
}
