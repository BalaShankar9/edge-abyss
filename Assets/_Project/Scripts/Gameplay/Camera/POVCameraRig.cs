using UnityEngine;

namespace EdgeAbyss.Gameplay.Camera
{
    /// <summary>
    /// GoPro-style POV camera rig with dynamic FOV, lean roll, procedural shake, and head bob.
    /// Works with Unity's standard Camera. Cinemachine optional (detected automatically).
    /// 
    /// SETUP INSTRUCTIONS:
    /// 
    /// 1. CAMERA SETUP:
    ///    - Create empty GameObject "POVCameraRig" in scene
    ///    - Attach this POVCameraRig component
    ///    - Make Main Camera a child of POVCameraRig (or reference it)
    ///    - Create CameraTuning asset and assign it
    /// 
    /// 2. BIKE PREFAB SETUP:
    ///    - Create empty child GameObject on bike called "POVMount"
    ///    - Position it at rider head level (e.g., Y=1.6, Z=0.1 forward of center)
    ///    - This is where the camera will be positioned
    /// 
    /// 3. CONNECT AT RUNTIME:
    ///    - Call SetTargets(bikeTransform, povMountTransform) when rider spawns
    ///    - Or assign in Inspector for testing
    /// 
    /// 4. EXTERNAL INPUTS (optional hooks):
    ///    - Set SurfaceRoughness (0-1) from ground detection
    ///    - Set WindIntensity (0-1) from weather/speed system
    ///    - Set CurrentSpeed for FOV/shake calculations
    ///    - Set CurrentLean for roll following
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class POVCameraRig : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The camera to control. Auto-assigned if not set.")]
        [SerializeField] private UnityEngine.Camera targetCamera;

        [Tooltip("Camera tuning parameters.")]
        [SerializeField] private CameraTuning tuning;

        [Header("Targets (can be set at runtime)")]
        [Tooltip("The bike/rider root transform.")]
        [SerializeField] private Transform bikeTransform;

        [Tooltip("The POV mount point on the bike (head level).")]
        [SerializeField] private Transform povMount;

        // External input properties (set by gameplay systems)
        private float _currentSpeed;
        private float _currentLean;
        private float _currentStability = 1f;
        private float _surfaceRoughness;
        private float _windIntensity;
        private bool _riderGrounded = true;
        private bool _riderFallen;

        /// <summary>Current rider speed in units/second. Set externally.</summary>
        public float CurrentSpeed { get => _currentSpeed; set => _currentSpeed = Mathf.Max(0f, value); }

        /// <summary>Current bike lean angle in degrees. Set externally.</summary>
        public float CurrentLean { get => _currentLean; set => _currentLean = value; }

        /// <summary>Current rider stability [0-1]. Set externally.</summary>
        public float CurrentStability { get => _currentStability; set => _currentStability = Mathf.Clamp01(value); }

        /// <summary>Surface roughness [0-1]. 0 = smooth, 1 = very rough. Set externally.</summary>
        public float SurfaceRoughness { get => _surfaceRoughness; set => _surfaceRoughness = Mathf.Clamp01(value); }

        /// <summary>Wind intensity [0-1]. Set externally.</summary>
        public float WindIntensity { get => _windIntensity; set => _windIntensity = Mathf.Clamp01(value); }

        /// <summary>
        /// Updates camera with rider data struct. Preferred method for decoupled data flow.
        /// Call this each frame from RiderCameraConnector.
        /// </summary>
        public void UpdateRiderData(RiderCameraData data)
        {
            _currentSpeed = data.Speed;
            _currentLean = data.LeanAngle;
            _currentStability = data.Stability;
            _riderGrounded = data.IsGrounded;
            _riderFallen = data.HasFallen;
        }

        // Internal state
        private float _currentFOV;
        private float _currentRoll;
        private Vector3 _shakeOffset;
        private Vector3 _shakeRotation;
        private float _headBobPhase;
        private Vector3 _smoothedPosition;
        private Quaternion _smoothedRotation;

        // Noise offsets for procedural shake
        private float _noiseOffsetX;
        private float _noiseOffsetY;
        private float _noiseOffsetZ;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<UnityEngine.Camera>();
            }

            if (tuning == null)
            {
                Debug.LogError($"[{nameof(POVCameraRig)}] CameraTuning is not assigned. Camera effects will not work.");
            }

            // Initialize noise offsets with random values for variety
            _noiseOffsetX = Random.Range(0f, 100f);
            _noiseOffsetY = Random.Range(0f, 100f);
            _noiseOffsetZ = Random.Range(0f, 100f);

            if (tuning != null)
            {
                _currentFOV = tuning.baseFOV;
            }
        }

        private void Start()
        {
            // Initialize smoothed position if mount exists
            if (povMount != null)
            {
                _smoothedPosition = povMount.position;
                _smoothedRotation = povMount.rotation;
            }
        }

        private void LateUpdate()
        {
            if (tuning == null || targetCamera == null) return;

            float dt = Time.deltaTime;

            UpdateFOV(dt);
            UpdatePosition(dt);
            UpdateRoll(dt);
            UpdateShake(dt);
            UpdateHeadBob(dt);
            ApplyTransform();
        }

        /// <summary>
        /// Sets the target transforms for the camera to follow.
        /// Call this when spawning a new rider.
        /// </summary>
        /// <param name="bike">The bike/rider root transform.</param>
        /// <param name="mount">The POV mount point (should be at head level).</param>
        public void SetTargets(Transform bike, Transform mount)
        {
            bikeTransform = bike;
            povMount = mount;

            if (povMount != null)
            {
                _smoothedPosition = povMount.position;
                _smoothedRotation = povMount.rotation;
            }

            // Reset state
            _shakeOffset = Vector3.zero;
            _shakeRotation = Vector3.zero;
            _currentRoll = 0f;
            _headBobPhase = 0f;
        }

        /// <summary>
        /// Clears the target references.
        /// </summary>
        public void ClearTargets()
        {
            bikeTransform = null;
            povMount = null;
        }

        private void UpdateFOV(float dt)
        {
            float speedRatio = Mathf.Clamp01(_currentSpeed / tuning.referenceMaxSpeed);
            float targetFOV = tuning.baseFOV + (tuning.maxSpeedFOVBoost * speedRatio);

            _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, tuning.fovLerpSpeed * dt);
            targetCamera.fieldOfView = _currentFOV;
        }

        private void UpdatePosition(float dt)
        {
            if (povMount == null) return;

            // Smooth follow position
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, povMount.position, tuning.positionFollowSpeed * dt);

            // Smooth follow rotation (base rotation before roll)
            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, povMount.rotation, tuning.rotationFollowSpeed * dt);
        }

        private void UpdateRoll(float dt)
        {
            // Calculate target roll from bike lean
            float targetRoll = -_currentLean * tuning.rollMultiplier;
            targetRoll = Mathf.Clamp(targetRoll, -tuning.maxRollAngle, tuning.maxRollAngle);

            _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, tuning.rollLerpSpeed * dt);
        }

        private void UpdateShake(float dt)
        {
            float time = Time.time;

            // Speed-based shake
            float speedRatio = Mathf.Clamp01(_currentSpeed / tuning.speedShakeMaxSpeed);
            float speedShakeAmount = tuning.speedShakeIntensity * speedRatio * speedRatio; // Quadratic for more effect at high speed

            Vector3 speedShake = new Vector3(
                PerlinNoise(time * tuning.speedShakeFrequency + _noiseOffsetX) * speedShakeAmount,
                PerlinNoise(time * tuning.speedShakeFrequency + _noiseOffsetY) * speedShakeAmount * 0.7f,
                PerlinNoise(time * tuning.speedShakeFrequency + _noiseOffsetZ) * speedShakeAmount * 0.3f
            );

            // Roughness-based shake
            float roughnessShakeAmount = tuning.roughnessShakeIntensity * _surfaceRoughness;

            Vector3 roughnessShake = new Vector3(
                PerlinNoise(time * tuning.roughnessShakeFrequency + _noiseOffsetX + 50f) * roughnessShakeAmount,
                PerlinNoise(time * tuning.roughnessShakeFrequency + _noiseOffsetY + 50f) * roughnessShakeAmount,
                PerlinNoise(time * tuning.roughnessShakeFrequency + _noiseOffsetZ + 50f) * roughnessShakeAmount * 0.5f
            );

            // Wind-based shake
            float windShakeAmount = tuning.windShakeIntensity * _windIntensity;

            Vector3 windShake = new Vector3(
                PerlinNoise(time * tuning.windShakeFrequency + _noiseOffsetX + 100f) * windShakeAmount,
                PerlinNoise(time * tuning.windShakeFrequency + _noiseOffsetY + 100f) * windShakeAmount * 0.5f,
                0f // Wind doesn't affect forward shake much
            );

            // Stability-based shake (subtle camera nervousness when unstable)
            // Only applies when stability is below 0.5 for readability
            float instabilityFactor = Mathf.Clamp01((0.5f - _currentStability) * 2f);
            float stabilityShakeAmount = tuning.speedShakeIntensity * instabilityFactor * 0.5f;

            Vector3 stabilityShake = new Vector3(
                PerlinNoise(time * 20f + _noiseOffsetX + 150f) * stabilityShakeAmount,
                PerlinNoise(time * 20f + _noiseOffsetY + 150f) * stabilityShakeAmount * 0.8f,
                PerlinNoise(time * 15f + _noiseOffsetZ + 150f) * stabilityShakeAmount * 0.3f
            );

            // Combine all shakes
            _shakeOffset = speedShake + roughnessShake + windShake + stabilityShake;

            // Clamp total shake
            _shakeOffset = Vector3.ClampMagnitude(_shakeOffset, tuning.maxShakeOffset);

            // Rotational shake (subtle pitch/yaw from shake)
            _shakeRotation = new Vector3(
                _shakeOffset.y * tuning.maxShakeRotation * 10f,  // Pitch from vertical shake
                _shakeOffset.x * tuning.maxShakeRotation * 5f,   // Yaw from horizontal shake
                0f  // Roll is handled separately
            );

            _shakeRotation = Vector3.ClampMagnitude(_shakeRotation, tuning.maxShakeRotation);
        }

        private void UpdateHeadBob(float dt)
        {
            if (_currentSpeed < 0.5f)
            {
                // Reduce bob when nearly stationary
                _headBobPhase = 0f;
                return;
            }

            // Advance phase based on speed
            float speedRatio = Mathf.Clamp01(_currentSpeed / tuning.referenceMaxSpeed);
            _headBobPhase += dt * tuning.headBobFrequency * (0.5f + speedRatio * 0.5f);

            // Add bob to shake offset (subtle)
            float verticalBob = Mathf.Sin(_headBobPhase * Mathf.PI * 2f) * tuning.headBobAmplitude * speedRatio;
            float horizontalSway = Mathf.Sin(_headBobPhase * Mathf.PI) * tuning.headSwayAmplitude * speedRatio;

            _shakeOffset.y += verticalBob;
            _shakeOffset.x += horizontalSway;
        }

        private void ApplyTransform()
        {
            if (povMount == null)
            {
                // No mount - just apply shake to current position
                transform.localPosition = _shakeOffset;
                transform.localEulerAngles = new Vector3(_shakeRotation.x, _shakeRotation.y, _currentRoll);
                return;
            }

            // Apply smoothed position + local shake offset
            Vector3 finalPosition = _smoothedPosition + _smoothedRotation * _shakeOffset;
            transform.position = finalPosition;

            // Apply smoothed rotation + roll + shake rotation
            Quaternion rollRotation = Quaternion.Euler(0f, 0f, _currentRoll);
            Quaternion shakeRotation = Quaternion.Euler(_shakeRotation.x, _shakeRotation.y, 0f);
            transform.rotation = _smoothedRotation * shakeRotation * rollRotation;
        }

        /// <summary>
        /// Returns Perlin noise in range [-1, 1].
        /// </summary>
        private float PerlinNoise(float t)
        {
            return Mathf.PerlinNoise(t, 0f) * 2f - 1f;
        }

        /// <summary>
        /// Triggers an impact shake (e.g., landing from a jump).
        /// </summary>
        /// <param name="intensity">Impact intensity [0-1].</param>
        public void TriggerImpactShake(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);

            // Add immediate offset that will decay naturally
            _shakeOffset.y -= tuning.maxShakeOffset * intensity * 0.5f;
            _shakeRotation.x -= tuning.maxShakeRotation * intensity;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<UnityEngine.Camera>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (povMount != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(povMount.position, 0.1f);
                Gizmos.DrawLine(povMount.position, povMount.position + povMount.forward * 0.5f);
            }
        }
#endif
    }
}
