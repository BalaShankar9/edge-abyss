using System;

namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Contract for all rider controllers (Bike, Horse, etc.).
    /// Defines the core locomotion and stability interface.
    /// 
    /// ARCHITECTURE RULES:
    /// - Movement logic belongs ONLY in concrete implementations.
    /// - Riders must NOT reference UI, ScoreManager, or Camera directly.
    /// - All falls must emit OnFall with an appropriate FallReason.
    /// - Only RiderManager may call Initialize/ResetRider/TickInput/TickPhysics.
    /// </summary>
    public interface IRiderController
    {
        #region Constants

        /// <summary>Minimum stability value.</summary>
        public const float STABILITY_MIN = 0f;

        /// <summary>Maximum stability value.</summary>
        public const float STABILITY_MAX = 1f;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the rider with tuning parameters.
        /// Called once when the rider is spawned.
        /// </summary>
        /// <param name="tuning">The tuning asset for this rider type.</param>
        void Initialize(RiderTuning tuning);

        /// <summary>
        /// Processes input for this frame.
        /// Called every frame before physics by RiderManager only.
        /// </summary>
        /// <param name="input">Current input state.</param>
        void TickInput(InputState input);

        /// <summary>
        /// Advances physics simulation.
        /// Called in FixedUpdate by RiderManager only.
        /// </summary>
        /// <param name="deltaTime">Fixed delta time.</param>
        void TickPhysics(float deltaTime);

        /// <summary>
        /// Resets the rider to a valid state at the given position.
        /// Called by RiderManager only.
        /// </summary>
        void ResetRider(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation);

        #endregion

        #region Read-Only State

        /// <summary>
        /// Current forward speed in units per second (read-only).
        /// </summary>
        float Speed { get; }

        /// <summary>
        /// Current stability value [0..1] (read-only).
        /// 0 = about to fall, 1 = perfectly stable.
        /// </summary>
        float Stability { get; }

        /// <summary>
        /// True if the rider is currently grounded (read-only).
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// True if the rider has fallen and needs respawn (read-only).
        /// </summary>
        bool HasFallen { get; }

        /// <summary>
        /// Current lean angle in degrees (read-only).
        /// </summary>
        float LeanAngle { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when the rider falls. Must include a valid FallReason.
        /// </summary>
        event Action<FallReason> OnFall;

        #endregion
    }
}
