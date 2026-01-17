using System;
using UnityEngine;
using EdgeAbyss.Utils;

namespace EdgeAbyss.Gameplay.Environment
{
    /// <summary>
    /// Global wind system providing ambient wind and gusts.
    /// Singleton for easy access from riders and environment.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject "WindSystem" in scene.
    /// 2. Attach this component.
    /// 3. Create WindTuning asset and assign it.
    /// 4. Riders query CurrentWindVector and Intensity for effects.
    /// </summary>
    public class WindSystem : Singleton<WindSystem>
    {
        [Header("Configuration")]
        [Tooltip("Wind tuning parameters.")]
        [SerializeField] private WindTuning tuning;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo;

        // Wind state
        private Vector3 _currentWindDirection;
        private float _currentIntensity;
        private float _ambientIntensity;
        private float _gustIntensity;
        private float _zoneIntensity;
        private Vector3 _zoneDirection;

        // Gust timing
        private float _nextGustTime;
        private float _gustTimer;
        private bool _isGusting;

        // Smoothing
        private Vector3 _smoothedWindVector;
        private float _smoothedIntensity;

        // Events
        public event Action OnGustStart;
        public event Action OnGustEnd;
        public event Action<float> OnIntensityChanged;

        /// <summary>Current wind direction (normalized).</summary>
        public Vector3 CurrentWindDirection => _currentWindDirection;

        /// <summary>Current wind intensity (smoothed).</summary>
        public float CurrentIntensity => _smoothedIntensity;

        /// <summary>Combined wind vector (direction × intensity).</summary>
        public Vector3 CurrentWindVector => _smoothedWindVector;

        /// <summary>True if currently in a gust.</summary>
        public bool IsGusting => _isGusting;

        /// <summary>True if wind is above strong threshold.</summary>
        public bool IsStrongWind => tuning != null && _smoothedIntensity >= tuning.strongWindThreshold;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            if (tuning == null)
            {
                Debug.LogWarning($"[{nameof(WindSystem)}] WindTuning not assigned. Using defaults.");
            }

            ScheduleNextGust();
        }

        private void Update()
        {
            if (tuning == null) return;

            float deltaTime = Time.deltaTime;

            UpdateAmbientWind(deltaTime);
            UpdateGusts(deltaTime);
            CalculateFinalWind(deltaTime);
        }

        private void UpdateAmbientWind(float deltaTime)
        {
            if (!tuning.enableAmbientWind)
            {
                _ambientIntensity = 0f;
                return;
            }

            // Base direction with variance
            Vector3 baseDir = tuning.baseWindDirection.normalized;
            if (tuning.directionVariance > 0f)
            {
                float variance = Mathf.Sin(Time.time * tuning.varianceSpeed) * tuning.directionVariance;
                Quaternion rotation = Quaternion.Euler(0f, variance, 0f);
                baseDir = rotation * baseDir;
            }

            _currentWindDirection = baseDir;
            _ambientIntensity = tuning.baseWindIntensity;
        }

        private void UpdateGusts(float deltaTime)
        {
            if (!tuning.enableGusts)
            {
                _gustIntensity = 0f;
                return;
            }

            if (_isGusting)
            {
                // Progress through gust
                _gustTimer += deltaTime;
                float progress = _gustTimer / tuning.gustDuration;

                if (progress >= 1f)
                {
                    // Gust ended
                    _isGusting = false;
                    _gustIntensity = 0f;
                    OnGustEnd?.Invoke();
                    ScheduleNextGust();
                }
                else
                {
                    // Sine-wave intensity curve: ramps up, peaks, ramps down
                    float curve = Mathf.Sin(progress * Mathf.PI);
                    _gustIntensity = tuning.baseWindIntensity * tuning.gustIntensityMultiplier * curve;
                }
            }
            else
            {
                // Check if it's time for a gust
                if (Time.time >= _nextGustTime)
                {
                    StartGust();
                }
            }
        }

        private void StartGust()
        {
            _isGusting = true;
            _gustTimer = 0f;
            OnGustStart?.Invoke();
        }

        private void ScheduleNextGust()
        {
            float interval = tuning.gustInterval;
            float variance = UnityEngine.Random.Range(-tuning.gustIntervalVariance, tuning.gustIntervalVariance);
            _nextGustTime = Time.time + interval + variance;
        }

        private void CalculateFinalWind(float deltaTime)
        {
            // Combine all sources
            _currentIntensity = _ambientIntensity + _gustIntensity + _zoneIntensity;

            // Direction: blend zone direction if active
            Vector3 targetDirection = _currentWindDirection;
            if (_zoneIntensity > 0f && _zoneDirection.sqrMagnitude > 0.01f)
            {
                float zoneWeight = _zoneIntensity / Mathf.Max(_currentIntensity, 0.01f);
                targetDirection = Vector3.Lerp(_currentWindDirection, _zoneDirection, zoneWeight).normalized;
            }

            // Smooth the final values
            _smoothedIntensity = Mathf.Lerp(_smoothedIntensity, _currentIntensity, tuning.effectSmoothSpeed * deltaTime);
            _smoothedWindVector = Vector3.Lerp(_smoothedWindVector, targetDirection * _smoothedIntensity, tuning.effectSmoothSpeed * deltaTime);

            // Fire intensity changed event (throttled)
            OnIntensityChanged?.Invoke(_smoothedIntensity);
        }

        /// <summary>
        /// Adds wind contribution from a zone.
        /// Call each frame while rider is in zone.
        /// </summary>
        public void AddZoneWind(Vector3 direction, float intensity)
        {
            _zoneDirection = direction.normalized;
            _zoneIntensity = intensity;
        }

        /// <summary>
        /// Clears zone wind contribution.
        /// Call when rider exits zone.
        /// </summary>
        public void ClearZoneWind()
        {
            _zoneDirection = Vector3.zero;
            _zoneIntensity = 0f;
        }

        /// <summary>
        /// Gets the lateral force to apply to a rider.
        /// </summary>
        public Vector3 GetLateralForce()
        {
            if (tuning == null) return Vector3.zero;
            return _smoothedWindVector * tuning.lateralForceMultiplier;
        }

        /// <summary>
        /// Gets the stability impact per second from current wind.
        /// </summary>
        public float GetStabilityImpact()
        {
            if (tuning == null) return 0f;
            return _smoothedIntensity * tuning.stabilityImpactPerIntensity;
        }

        /// <summary>
        /// Triggers an immediate gust (for scripted events).
        /// </summary>
        public void TriggerGust()
        {
            if (!_isGusting)
            {
                StartGust();
            }
        }

        /// <summary>
        /// Sets the base wind direction at runtime.
        /// </summary>
        public void SetWindDirection(Vector3 direction)
        {
            if (tuning != null)
            {
                // This creates a runtime copy behavior - modify tuning if needed
                _currentWindDirection = direction.normalized;
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 250, 120));
            GUILayout.Box("Wind System Debug");
            GUILayout.Label($"Direction: {_currentWindDirection:F2}");
            GUILayout.Label($"Intensity: {_smoothedIntensity:F2}");
            GUILayout.Label($"Gusting: {_isGusting}");
            GUILayout.Label($"Strong Wind: {IsStrongWind}");
            GUILayout.EndArea();
        }
#endif
    }
}
