using UnityEngine;

namespace EdgeAbyss.Gameplay.Score
{
    /// <summary>
    /// ScriptableObject containing all tunable parameters for the scoring system.
    /// 
    /// HOW TO CREATE:
    /// 1. Right-click in Project window.
    /// 2. Select: Create > EdgeAbyss > Score Tuning
    /// 3. Name it "ScoreTuning" and save in Assets/_Project/Configs/
    /// 4. Assign to ScoreManager component.
    /// </summary>
    [CreateAssetMenu(fileName = "ScoreTuning", menuName = "EdgeAbyss/Score Tuning", order = 3)]
    public class ScoreTuning : ScriptableObject
    {
        [Header("Distance Scoring")]
        [Tooltip("Base points awarded per unit of distance traveled.")]
        [Range(0.1f, 10f)] public float pointsPerUnit = 1f;

        [Tooltip("Minimum speed (units/sec) required to earn distance points.")]
        [Range(0f, 5f)] public float minimumScoringSpeed = 2f;

        [Header("Speed Multiplier")]
        [Tooltip("Enable speed-based score multiplier.")]
        public bool enableSpeedMultiplier = true;

        [Tooltip("Reference speed for 2x multiplier (units/sec).")]
        [Range(10f, 100f)] public float referenceSpeed = 30f;

        [Tooltip("Maximum speed multiplier achievable.")]
        [Range(1f, 5f)] public float maxSpeedMultiplier = 3f;

        [Tooltip("Minimum stability required for speed multiplier to apply.")]
        [Range(0.3f, 0.9f)] public float speedMultiplierStabilityThreshold = 0.5f;

        [Header("Streak System")]
        [Tooltip("Enable streak bonus system.")]
        public bool enableStreaks = true;

        [Tooltip("Time of clean riding required to gain a streak level (seconds).")]
        [Range(1f, 10f)] public float streakBuildTime = 3f;

        [Tooltip("Maximum streak level.")]
        [Range(1, 20)] public int maxStreakLevel = 10;

        [Tooltip("Bonus multiplier per streak level.")]
        [Range(0.05f, 0.5f)] public float streakBonusPerLevel = 0.1f;

        [Tooltip("Minimum stability to maintain streak (lower = more forgiving).")]
        [Range(0.2f, 0.8f)] public float streakStabilityThreshold = 0.4f;

        [Tooltip("Brake input threshold that breaks streak (0-1).")]
        [Range(0.3f, 1f)] public float hardBrakeThreshold = 0.7f;

        [Tooltip("Grace period after minor instability before streak breaks (seconds).")]
        [Range(0f, 1f)] public float streakGracePeriod = 0.3f;

        [Header("Edge Proximity Bonus")]
        [Tooltip("Enable near-edge bonus scoring.")]
        public bool enableEdgeBonus = true;

        [Tooltip("Points per second when riding near edge.")]
        [Range(1f, 50f)] public float edgeBonusPointsPerSecond = 20f;

        [Tooltip("Proximity factor threshold to start earning edge bonus (0-1).")]
        [Range(0.3f, 0.9f)] public float edgeBonusThreshold = 0.5f;

        [Tooltip("Multiplier applied to edge bonus based on proximity factor.")]
        [Range(1f, 5f)] public float edgeBonusProximityMultiplier = 2f;

        [Header("Penalties")]
        [Tooltip("Points lost per fall/respawn.")]
        [Range(0, 1000)] public int fallPenalty = 100;

        [Tooltip("Streak levels lost per fall.")]
        [Range(0, 10)] public int streakLossOnFall = 3;

        [Header("Display")]
        [Tooltip("Smooth score display changes over this duration.")]
        [Range(0f, 1f)] public float scoreDisplaySmoothTime = 0.2f;
    }
}
