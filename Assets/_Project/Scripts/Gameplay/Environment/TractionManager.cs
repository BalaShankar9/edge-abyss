using System.Collections.Generic;
using UnityEngine;
using EdgeAbyss.Utils;

namespace EdgeAbyss.Gameplay.Environment
{
    /// <summary>
    /// Manages active traction zones and provides combined traction values.
    /// Singleton for easy access from riders.
    /// 
    /// SETUP:
    /// 1. Auto-created when first accessed, or add manually to scene.
    /// 2. Riders query CurrentTraction, CurrentSteeringModifier, etc.
    /// 3. Supports overlapping zones with priority blending.
    /// </summary>
    public class TractionManager : Singleton<TractionManager>
    {
        [Header("Settings")]
        [Tooltip("Default traction when not in any zone.")]
        [SerializeField] private float defaultTraction = 1f;

        [Tooltip("Default steering modifier when not in any zone.")]
        [SerializeField] private float defaultSteering = 1f;

        [Tooltip("Default stability modifier when not in any zone.")]
        [SerializeField] private float defaultStability = 1f;

        // Active zones (stack for overlapping support)
        private readonly List<TractionZone> _activeZones = new List<TractionZone>();

        // Smoothed values
        private float _currentTraction;
        private float _currentSteering;
        private float _currentStability;
        private float _targetTraction;
        private float _targetSteering;
        private float _targetStability;
        private float _transitionSpeed;

        /// <summary>Current traction multiplier (smoothed).</summary>
        public float CurrentTraction => _currentTraction;

        /// <summary>Current steering modifier (smoothed).</summary>
        public float CurrentSteeringModifier => _currentSteering;

        /// <summary>Current stability modifier (smoothed).</summary>
        public float CurrentStabilityModifier => _currentStability;

        /// <summary>Currently active surface type (from topmost zone).</summary>
        public string CurrentSurfaceType => _activeZones.Count > 0 ? _activeZones[^1].SurfaceType : "Default";

        /// <summary>True if in any traction zone.</summary>
        public bool InTractionZone => _activeZones.Count > 0;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            _currentTraction = defaultTraction;
            _currentSteering = defaultSteering;
            _currentStability = defaultStability;
            _targetTraction = defaultTraction;
            _targetSteering = defaultSteering;
            _targetStability = defaultStability;
            _transitionSpeed = 8f;
        }

        private void Update()
        {
            SmoothTransition();
        }

        private void SmoothTransition()
        {
            float deltaTime = Time.deltaTime;
            float speed = _transitionSpeed * deltaTime;

            _currentTraction = Mathf.Lerp(_currentTraction, _targetTraction, speed);
            _currentSteering = Mathf.Lerp(_currentSteering, _targetSteering, speed);
            _currentStability = Mathf.Lerp(_currentStability, _targetStability, speed);
        }

        /// <summary>
        /// Called when rider enters a traction zone.
        /// </summary>
        public void EnterZone(TractionZone zone)
        {
            if (zone == null || _activeZones.Contains(zone)) return;

            _activeZones.Add(zone);
            UpdateTargetValues();
        }

        /// <summary>
        /// Called when rider exits a traction zone.
        /// </summary>
        public void ExitZone(TractionZone zone)
        {
            if (zone == null) return;

            _activeZones.Remove(zone);
            UpdateTargetValues();
        }

        private void UpdateTargetValues()
        {
            if (_activeZones.Count == 0)
            {
                // No zones - use defaults
                _targetTraction = defaultTraction;
                _targetSteering = defaultSteering;
                _targetStability = defaultStability;
                _transitionSpeed = 8f;
            }
            else
            {
                // Use topmost zone (last added)
                var topZone = _activeZones[^1];
                _targetTraction = topZone.TractionMultiplier;
                _targetSteering = topZone.SteeringModifier;
                _targetStability = topZone.StabilityModifier;
                _transitionSpeed = topZone.TransitionSpeed;
            }
        }

        /// <summary>
        /// Clears all active zones (call on respawn).
        /// </summary>
        public void ClearAllZones()
        {
            _activeZones.Clear();
            UpdateTargetValues();

            // Instant reset to defaults
            _currentTraction = defaultTraction;
            _currentSteering = defaultSteering;
            _currentStability = defaultStability;
        }
    }
}
