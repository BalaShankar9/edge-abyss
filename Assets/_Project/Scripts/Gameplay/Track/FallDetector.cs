using System;
using UnityEngine;
using EdgeAbyss.Gameplay.Riders;

namespace EdgeAbyss.Gameplay.Track
{
    /// <summary>
    /// Detects when the rider falls off the track or below a Y threshold.
    /// Attach to the rider prefab or manage externally.
    /// 
    /// SETUP:
    /// 1. Attach to rider prefab OR create as standalone manager.
    /// 2. Assign the TrackBounds reference.
    /// 3. Configure Y threshold (e.g., -50 for falling into void).
    /// 4. RespawnManager listens to OnFallDetected event.
    /// </summary>
    public class FallDetector : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The track bounds volume. If null, only Y threshold is checked.")]
        [SerializeField] private TrackBounds trackBounds;

        [Header("Detection Settings")]
        [Tooltip("Y position below which the rider is considered fallen.")]
        [SerializeField] private float fallYThreshold = -50f;

        [Tooltip("Time rider can be out of bounds before triggering fall.")]
        [SerializeField] private float outOfBoundsGracePeriod = 0.5f;

        [Tooltip("Check interval for performance (seconds).")]
        [SerializeField] private float checkInterval = 0.1f;

        // Target to monitor
        private Transform _targetTransform;
        private IRiderController _targetRider;
        private bool _isMonitoring;
        private float _outOfBoundsTimer;
        private float _checkTimer;
        private bool _isWithinBounds = true;
        private bool _hasFallBeenTriggered;

        /// <summary>
        /// Fired when a fall is detected. Provides the reason.
        /// </summary>
        public event Action<FallReason> OnFallDetected;

        /// <summary>
        /// True if currently monitoring a target.
        /// </summary>
        public bool IsMonitoring => _isMonitoring;

        /// <summary>
        /// True if target is currently within bounds.
        /// </summary>
        public bool IsWithinBounds => _isWithinBounds;

        private void Update()
        {
            if (!_isMonitoring || _targetTransform == null) return;

            _checkTimer -= Time.deltaTime;
            if (_checkTimer > 0f) return;
            _checkTimer = checkInterval;

            CheckFallConditions();
        }

        /// <summary>
        /// Starts monitoring a rider for falls.
        /// </summary>
        public void StartMonitoring(Transform target, IRiderController rider = null)
        {
            _targetTransform = target;
            _targetRider = rider;
            _isMonitoring = true;
            _outOfBoundsTimer = 0f;
            _isWithinBounds = true;
            _hasFallBeenTriggered = false;
            _checkTimer = 0f;

            // Subscribe to rider's fall event if available
            if (_targetRider != null)
            {
                _targetRider.OnFall += HandleRiderFall;
            }
        }

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        public void StopMonitoring()
        {
            if (_targetRider != null)
            {
                _targetRider.OnFall -= HandleRiderFall;
            }

            _targetTransform = null;
            _targetRider = null;
            _isMonitoring = false;
            _hasFallBeenTriggered = false;
        }

        /// <summary>
        /// Resets fall detection state (call after respawn).
        /// </summary>
        public void ResetFallState()
        {
            _outOfBoundsTimer = 0f;
            _isWithinBounds = true;
            _hasFallBeenTriggered = false;
        }

        private void CheckFallConditions()
        {
            if (_hasFallBeenTriggered) return;

            Vector3 position = _targetTransform.position;

            // Check Y threshold first (immediate)
            if (position.y < fallYThreshold)
            {
                TriggerFall(FallReason.FellOffEdge);
                return;
            }

            // Check track bounds
            if (trackBounds != null)
            {
                bool withinBounds = trackBounds.ContainsPoint(position);

                if (withinBounds)
                {
                    _isWithinBounds = true;
                    _outOfBoundsTimer = 0f;
                }
                else
                {
                    _isWithinBounds = false;
                    _outOfBoundsTimer += checkInterval;

                    float gracePeriod = Mathf.Max(outOfBoundsGracePeriod, trackBounds.GracePeriod);
                    if (_outOfBoundsTimer >= gracePeriod)
                    {
                        TriggerFall(FallReason.FellOffEdge);
                    }
                }
            }
        }

        private void HandleRiderFall(FallReason reason)
        {
            // Rider's internal fall detection triggered
            TriggerFall(reason);
        }

        private void TriggerFall(FallReason reason)
        {
            if (_hasFallBeenTriggered) return;
            _hasFallBeenTriggered = true;

            OnFallDetected?.Invoke(reason);
        }

        /// <summary>
        /// Sets the track bounds reference at runtime.
        /// </summary>
        public void SetTrackBounds(TrackBounds bounds)
        {
            trackBounds = bounds;
        }

        private void OnDestroy()
        {
            StopMonitoring();
        }
    }
}
