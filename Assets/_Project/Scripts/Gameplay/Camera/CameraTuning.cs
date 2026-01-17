using UnityEngine;

namespace EdgeAbyss.Gameplay.Camera
{
    /// <summary>
    /// ScriptableObject containing all tunable parameters for the POV camera rig.
    /// Create one asset and assign it to the POVCameraRig component.
    /// 
    /// HOW TO CREATE:
    /// 1. Right-click in Project window.
    /// 2. Select: Create > EdgeAbyss > Camera Tuning
    /// 3. Name it "CameraTuning" and save in Assets/_Project/Configs/
    /// 4. Assign to POVCameraRig component.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraTuning", menuName = "EdgeAbyss/Camera Tuning", order = 2)]
    public class CameraTuning : ScriptableObject
    {
        [Header("Field of View")]
        [Tooltip("Base FOV when stationary.")]
        [Range(60f, 100f)] public float baseFOV = 75f;

        [Tooltip("Maximum additional FOV at max speed.")]
        [Range(0f, 30f)] public float maxSpeedFOVBoost = 15f;

        [Tooltip("How quickly FOV changes with speed.")]
        [Range(1f, 10f)] public float fovLerpSpeed = 4f;

        [Tooltip("Reference speed for max FOV boost (units/second).")]
        [Range(10f, 100f)] public float referenceMaxSpeed = 35f;

        [Header("Roll (Lean Follow)")]
        [Tooltip("Maximum camera roll angle following bike lean.")]
        [Range(0f, 20f)] public float maxRollAngle = 8f;

        [Tooltip("How quickly camera roll follows bike lean.")]
        [Range(1f, 15f)] public float rollLerpSpeed = 5f;

        [Tooltip("Roll multiplier (1 = match lean exactly up to max).")]
        [Range(0.5f, 2f)] public float rollMultiplier = 0.8f;

        [Header("Shake - Speed Based")]
        [Tooltip("Base shake intensity at high speed.")]
        [Range(0f, 0.1f)] public float speedShakeIntensity = 0.02f;

        [Tooltip("Speed at which shake reaches full intensity.")]
        [Range(10f, 100f)] public float speedShakeMaxSpeed = 30f;

        [Tooltip("Frequency of speed-based shake (Hz).")]
        [Range(5f, 30f)] public float speedShakeFrequency = 15f;

        [Header("Shake - Surface Roughness")]
        [Tooltip("Shake intensity multiplier for rough surfaces.")]
        [Range(0f, 0.2f)] public float roughnessShakeIntensity = 0.05f;

        [Tooltip("Frequency of roughness shake (Hz).")]
        [Range(10f, 40f)] public float roughnessShakeFrequency = 25f;

        [Header("Shake - Wind")]
        [Tooltip("Shake intensity from wind.")]
        [Range(0f, 0.1f)] public float windShakeIntensity = 0.015f;

        [Tooltip("Frequency of wind shake (Hz).")]
        [Range(2f, 15f)] public float windShakeFrequency = 8f;

        [Header("Head Bob")]
        [Tooltip("Vertical head bob amplitude.")]
        [Range(0f, 0.05f)] public float headBobAmplitude = 0.008f;

        [Tooltip("Head bob frequency tied to speed.")]
        [Range(1f, 10f)] public float headBobFrequency = 3f;

        [Tooltip("Horizontal sway amplitude (subtle).")]
        [Range(0f, 0.02f)] public float headSwayAmplitude = 0.003f;

        [Header("Position Smoothing")]
        [Tooltip("How quickly camera follows the mount position.")]
        [Range(5f, 50f)] public float positionFollowSpeed = 20f;

        [Tooltip("How quickly camera follows the mount rotation.")]
        [Range(5f, 50f)] public float rotationFollowSpeed = 15f;

        [Header("Limits")]
        [Tooltip("Maximum total shake offset.")]
        [Range(0.01f, 0.3f)] public float maxShakeOffset = 0.1f;

        [Tooltip("Maximum total rotational shake (degrees).")]
        [Range(0.5f, 5f)] public float maxShakeRotation = 2f;
    }
}
