using System;
using UnityEngine;
using EdgeAbyss.Gameplay.Environment;

namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Abstract base class for all rider controllers.
    /// 
    /// RESPONSIBILITIES:
    /// - Stability clamping and management
    /// - Fall event dispatch (all falls go through TriggerFall)
    /// - Shared timing and physics helpers
    /// - Environment modifier integration (wind, traction)
    /// 
    /// RULES:
    /// - Movement logic belongs in derived classes ONLY
    /// - No direct references to UI, ScoreManager, or Camera
    /// - All public state is read-only (properties)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class RiderBase : MonoBehaviour, IRiderController
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundLayers = ~0;
        [SerializeField] private Transform _groundCheckOrigin;

        #endregion

        #region Protected State (for derived classes)

        /// <summary>Cached Rigidbody reference.</summary>
        protected Rigidbody Rb => _rigidbody;

        /// <summary>Ground layer mask.</summary>
        protected LayerMask GroundLayers => _groundLayers;

        /// <summary>Ground check origin transform.</summary>
        protected Transform GroundCheckOrigin => _groundCheckOrigin;

        /// <summary>Tuning asset for this rider.</summary>
        protected RiderTuning Tuning { get; private set; }

        /// <summary>Last received input state.</summary>
        protected InputState LastInput { get; private set; }

        // Mutable state - derived classes may read/write
        protected float currentSpeed;
        protected float currentSteer;
        protected float currentLean;

        #endregion

        #region Private State

        private float _stability = IRiderController.STABILITY_MAX;
        private bool _isGrounded;
        private bool _hasFallen;
        private bool _isRespawning;
        private float _respawnImmunityTimer;

        // Environment modifiers (updated each physics tick)
        private float _tractionFactor = 1f;
        private float _steeringModifier = 1f;
        private float _stabilityModifier = 1f;
        private Vector3 _windForce;
        private float _windStabilityDrain;

        // Fall diagnostics tracking
        private FallReason _lastFallContributor = FallReason.Unknown;
        private float _stabilityAtFall;
        private float _speedAtFall;

        #endregion

        #region Constants (Fairness)

        /// <summary>Maximum stability drop allowed per frame. Prevents instant deaths.</summary>
        private const float MAX_STABILITY_DROP_PER_FRAME = 0.4f;

        /// <summary>Immunity duration after respawn (seconds).</summary>
        private const float RESPAWN_IMMUNITY_DURATION = 0.5f;

        #endregion

        #region Debug Settings

        [Header("Debug (Fall Diagnostics)")]
        [SerializeField] private bool _enableFallDebugLogging = false;

        #endregion

        #region IRiderController Properties (Read-Only)

        public float Speed => currentSpeed;
        public float Stability => _stability;
        public bool IsGrounded => _isGrounded;
        public bool HasFallen => _hasFallen;
        public float LeanAngle => currentLean;

        /// <summary>True if rider is currently respawning (immune to falls).</summary>
        public bool IsRespawning => _isRespawning;

        #endregion

        #region Protected Read-Only Accessors (for derived classes)

        /// <summary>Current traction multiplier [0..1].</summary>
        protected float TractionFactor => _tractionFactor;

        /// <summary>Current steering modifier from surface.</summary>
        protected float SteeringModifier => _steeringModifier;

        /// <summary>Current stability modifier from surface.</summary>
        protected float StabilityModifier => _stabilityModifier;

        /// <summary>Current wind force vector.</summary>
        protected Vector3 WindForce => _windForce;

        /// <summary>Current wind stability drain rate.</summary>
        protected float WindStabilityDrain => _windStabilityDrain;

        #endregion

        #region Events

        public event Action<FallReason> OnFall;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            ValidateReferences();
        }

        private void ValidateReferences()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            if (_groundCheckOrigin == null)
            {
                _groundCheckOrigin = transform;
            }

            Debug.Assert(_rigidbody != null, $"[{GetType().Name}] Rigidbody is required.");
        }

        #endregion

        #region IRiderController Implementation

        public virtual void Initialize(RiderTuning tuningAsset)
        {
            Tuning = tuningAsset;
            _stability = IRiderController.STABILITY_MAX;
            currentSpeed = 0f;
            currentSteer = 0f;
            currentLean = 0f;
            _hasFallen = false;

            if (Tuning == null)
            {
                Debug.LogError($"[{GetType().Name}] RiderTuning is null. Rider will not function correctly.");
            }
        }

        public virtual void TickInput(InputState input)
        {
            if (_hasFallen || Tuning == null) return;

            LastInput = input;
            ProcessInput(input);
        }

        public virtual void TickPhysics(float deltaTime)
        {
            if (_hasFallen || Tuning == null) return;

            // Update respawn immunity
            if (_isRespawning)
            {
                _respawnImmunityTimer -= deltaTime;
                if (_respawnImmunityTimer <= 0f)
                {
                    _isRespawning = false;
                }
            }

            // Order matters: update environment -> ground -> stability -> check fall -> move
            QueryEnvironmentModifiers();
            UpdateGroundedState();
            UpdateStability(deltaTime);

            // Only check fall conditions if not respawning (immunity period)
            if (!_isRespawning)
            {
                CheckFallConditions();
            }

            if (!_hasFallen)
            {
                ApplyMovement(deltaTime);
                ApplyLean(deltaTime);
                ApplyWindForce(deltaTime);
            }
        }

        public virtual void ResetRider(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            currentSpeed = 0f;
            currentSteer = 0f;
            currentLean = 0f;
            _stability = IRiderController.STABILITY_MAX;
            _hasFallen = false;
            LastInput = InputState.Empty;

            // Enable respawn immunity to prevent immediate re-fall
            _isRespawning = true;
            _respawnImmunityTimer = RESPAWN_IMMUNITY_DURATION;

            // Reset environment modifiers
            _tractionFactor = 1f;
            _steeringModifier = 1f;
            _stabilityModifier = 1f;
            _windForce = Vector3.zero;
            _windStabilityDrain = 0f;

            // Reset fall diagnostics
            _lastFallContributor = FallReason.Unknown;
            _stabilityAtFall = 0f;
            _speedAtFall = 0f;

            // Clear traction zones (if manager exists)
            if (TractionManager.HasInstance)
            {
                TractionManager.Instance.ClearAllZones();
            }

            OnRiderReset();
        }

        #endregion

        #region Abstract Methods (Derived classes implement movement)

        /// <summary>
        /// Process input and update internal state.
        /// Movement logic goes here in derived classes.
        /// </summary>
        protected abstract void ProcessInput(InputState input);

        /// <summary>
        /// Apply physics-based movement.
        /// Movement logic goes here in derived classes.
        /// </summary>
        protected abstract void ApplyMovement(float deltaTime);

        #endregion

        #region Virtual Methods (Derived classes may override)

        /// <summary>
        /// Called after rider is reset. Override for custom cleanup.
        /// </summary>
        protected virtual void OnRiderReset() { }

        /// <summary>
        /// Update visual lean. Override for custom lean behavior.
        /// </summary>
        protected virtual void ApplyLean(float deltaTime)
        {
            float targetLean = -currentSteer * Tuning.maxLeanAngle;
            currentLean = Mathf.Lerp(currentLean, targetLean, Tuning.leanSpeed * deltaTime);

            // Apply lean rotation (local Z axis)
            Vector3 euler = transform.localEulerAngles;
            euler.z = currentLean;
            transform.localEulerAngles = euler;
        }

        /// <summary>
        /// Additional fall conditions. Base checks stability threshold.
        /// </summary>
        protected virtual void CheckFallConditions()
        {
            if (_stability <= Tuning.fallThreshold)
            {
                TriggerFall(FallReason.LostBalance);
            }
        }

        /// <summary>
        /// Called on collision. Override for custom collision handling.
        /// </summary>
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (_hasFallen || _isRespawning) return;

            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 10f)
            {
                _lastFallContributor = FallReason.Collision;
                TriggerFall(FallReason.Collision);
            }
            else if (impactForce > 5f)
            {
                // Reduce stability on moderate impacts (capped for fairness)
                ModifyStabilityCapped(-0.3f, FallReason.Collision);
            }
        }

        #endregion

        #region Stability Management (Centralized)

        /// <summary>
        /// Updates stability based on current state. Called each physics tick.
        /// All changes are gradual (per-frame) so no capping needed here.
        /// </summary>
        private void UpdateStability(float deltaTime)
        {
            // Base recovery (modified by surface)
            float recovery = Tuning.stabilityRecoveryRate * _stabilityModifier * deltaTime;

            // Focus bonus
            if (LastInput.FocusHeld)
            {
                recovery += Tuning.focusStabilityBonus * deltaTime;
            }

            // Wind stability drain (gradual, applied as impulse over time)
            float windDrain = _windStabilityDrain * deltaTime;

            // Track wind as contributor if it's significant
            if (windDrain > 0.05f * deltaTime)
            {
                _lastFallContributor = FallReason.ExternalForce;
            }

            // Steering cost (increased on low traction surfaces)
            float tractionPenalty = 1f + (1f - _tractionFactor) * 0.5f;
            float steerCost = Mathf.Abs(currentSteer) * Tuning.steerStabilityCost * tractionPenalty * deltaTime;

            // Speed penalty at high speeds
            float speedRatio = currentSpeed / Tuning.maxSpeed;
            float speedPenalty = speedRatio * Mathf.Abs(currentSteer) * 0.1f * deltaTime;

            // Apply delta (all gradual per-frame changes, no cap needed)
            float delta = recovery - steerCost - speedPenalty - windDrain;
            ModifyStability(delta);
        }

        /// <summary>
        /// Safely modifies stability with clamping. Use this for all stability changes.
        /// Does NOT apply frame cap - use for gradual per-frame changes only.
        /// </summary>
        protected void ModifyStability(float delta)
        {
            _stability = Mathf.Clamp(
                _stability + delta,
                IRiderController.STABILITY_MIN,
                IRiderController.STABILITY_MAX
            );
        }

        /// <summary>
        /// Modifies stability with fairness cap to prevent instant deaths.
        /// Stability cannot drop more than MAX_STABILITY_DROP_PER_FRAME from values above 0.5.
        /// Use this for impulse-style damage (collisions, sudden events).
        /// </summary>
        /// <param name="delta">The stability change (negative for damage).</param>
        /// <param name="contributor">The cause of this stability change for diagnostics.</param>
        protected void ModifyStabilityCapped(float delta, FallReason contributor)
        {
            if (delta >= 0f)
            {
                // Positive changes don't need capping
                ModifyStability(delta);
                return;
            }

            // Track contributor for diagnostics
            if (delta < -0.1f)
            {
                _lastFallContributor = contributor;
            }

            // Fairness rule: if stability is above 0.5, cap the drop
            // This prevents >0.5 to 0 in a single frame
            if (_stability > 0.5f)
            {
                float maxDrop = Mathf.Max(delta, -MAX_STABILITY_DROP_PER_FRAME);
                float newStability = _stability + maxDrop;

                // Ensure we don't drop below the safe threshold in one frame
                // Minimum after cap: stability - 0.4 (so 0.6 -> 0.2 minimum)
                _stability = Mathf.Max(newStability, _stability - MAX_STABILITY_DROP_PER_FRAME);
                _stability = Mathf.Clamp(_stability, IRiderController.STABILITY_MIN, IRiderController.STABILITY_MAX);
            }
            else
            {
                // Below 0.5, normal rules apply (player was already in danger)
                ModifyStability(delta);
            }
        }

        /// <summary>
        /// Sets stability to a specific value with clamping. Use sparingly.
        /// </summary>
        protected void SetStability(float value)
        {
            _stability = Mathf.Clamp(value, IRiderController.STABILITY_MIN, IRiderController.STABILITY_MAX);
        }

        #endregion

        #region Fall Dispatch (Centralized)

        /// <summary>
        /// Triggers a fall with the given reason. All falls MUST go through this method.
        /// Includes double-fall prevention and respawn immunity check.
        /// </summary>
        protected void TriggerFall(FallReason reason)
        {
            // Double-fall prevention
            if (_hasFallen) return;

            // Respawn immunity check
            if (_isRespawning) return;

            // Record diagnostics before fall
            _stabilityAtFall = _stability;
            _speedAtFall = currentSpeed;

            _hasFallen = true;

            // Debug logging (optional, disabled by default)
            if (_enableFallDebugLogging)
            {
                LogFallDiagnostics(reason);
            }

            OnFall?.Invoke(reason);
        }

        /// <summary>
        /// Logs detailed fall diagnostics for debugging fairness issues.
        /// </summary>
        private void LogFallDiagnostics(FallReason reason)
        {
            string primaryCause = DeterminePrimaryCause(reason);

            Debug.Log($"[FallDiagnostics] {GetType().Name} FELL\n" +
                      $"  Reason: {reason}\n" +
                      $"  Primary Cause: {primaryCause}\n" +
                      $"  Speed at Fall: {_speedAtFall:F1} ({(_speedAtFall / Tuning.maxSpeed * 100f):F0}% of max)\n" +
                      $"  Stability at Fall: {_stabilityAtFall:F3}\n" +
                      $"  Fall Threshold: {Tuning.fallThreshold:F3}\n" +
                      $"  Was Grounded: {_isGrounded}\n" +
                      $"  Wind Force: {_windForce.magnitude:F2}\n" +
                      $"  Traction: {_tractionFactor:F2}\n" +
                      $"  Steer Input: {currentSteer:F2}\n" +
                      $"  Lean Angle: {currentLean:F1}°");
        }

        /// <summary>
        /// Determines the primary cause of the fall for readable diagnostics.
        /// </summary>
        private string DeterminePrimaryCause(FallReason reason)
        {
            // If we have a tracked contributor, use it
            if (_lastFallContributor != FallReason.Unknown && _lastFallContributor != reason)
            {
                return $"{reason} (triggered by {_lastFallContributor})";
            }

            // Analyze state to determine cause
            if (reason == FallReason.LostBalance)
            {
                if (_windStabilityDrain > 0.1f)
                    return "Wind Gust";
                if (Mathf.Abs(currentLean) > Tuning.maxLeanAngle * 0.9f)
                    return "Over-Lean";
                if (_tractionFactor < 0.5f)
                    return "Low Traction";
                if (currentSpeed > Tuning.maxSpeed * 0.8f && Mathf.Abs(currentSteer) > 0.5f)
                    return "High-Speed Steering";

                return "Accumulated Instability";
            }

            return reason.ToString();
        }

        #endregion

        #region Ground Detection

        /// <summary>
        /// Updates grounded state via raycast.
        /// </summary>
        private void UpdateGroundedState()
        {
            Vector3 origin = _groundCheckOrigin.position;
            _isGrounded = Physics.Raycast(origin, Vector3.down, Tuning.groundCheckDistance, _groundLayers);
        }

        #endregion

        #region Environment Integration

        /// <summary>
        /// Queries environment systems for modifiers. Does not reference UI/Score/Camera.
        /// </summary>
        private void QueryEnvironmentModifiers()
        {
            // Traction from TractionManager
            if (TractionManager.HasInstance)
            {
                _tractionFactor = TractionManager.Instance.CurrentTraction;
                _steeringModifier = TractionManager.Instance.CurrentSteeringModifier;
                _stabilityModifier = TractionManager.Instance.CurrentStabilityModifier;
            }
            else
            {
                _tractionFactor = 1f;
                _steeringModifier = 1f;
                _stabilityModifier = 1f;
            }

            // Wind from WindSystem
            if (WindSystem.HasInstance)
            {
                _windForce = WindSystem.Instance.GetLateralForce();
                _windStabilityDrain = WindSystem.Instance.GetStabilityImpact();
            }
            else
            {
                _windForce = Vector3.zero;
                _windStabilityDrain = 0f;
            }
        }

        /// <summary>
        /// Applies wind lateral force to the rider.
        /// </summary>
        private void ApplyWindForce(float deltaTime)
        {
            if (_windForce.sqrMagnitude < 0.01f) return;
            if (!_isGrounded) return; // Less wind effect in air

            _rigidbody.AddForce(_windForce, ForceMode.Acceleration);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Calculates speed-adjusted turn rate.
        /// </summary>
        protected float GetSpeedAdjustedTurnRate()
        {
            float speedRatio = currentSpeed / Tuning.maxSpeed;
            float steerFactor = Mathf.Lerp(1f, Tuning.highSpeedSteerFactor, speedRatio);
            return Tuning.maxTurnRate * steerFactor;
        }

        #endregion
    }
}
