using UnityEngine;

namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Arcade-style precision bike controller.
    /// Features responsive steering, lean-based turning, and tight control.
    /// Integrates with WindSystem and TractionManager for environmental effects.
    /// 
    /// MOVEMENT LOGIC: All bike-specific physics handled here.
    /// </summary>
    public sealed class BikeRiderController : RiderBase
    {
        #region Private State

        private float _targetSteer;
        private float _effectiveAcceleration;
        private float _effectiveBraking;

        #endregion

        #region Movement Implementation

        protected override void ProcessInput(InputState input)
        {
            // Immediate target steer from input
            _targetSteer = input.Steer;

            // Steering response modified by surface traction
            float effectiveSteerResponse = Tuning.steerResponse * SteeringModifier;
            currentSteer = Mathf.Lerp(currentSteer, _targetSteer, effectiveSteerResponse * Time.deltaTime);

            // Calculate target speed based on throttle/brake
            // Traction affects how much power transfers to ground
            _effectiveAcceleration = input.Throttle * Tuning.acceleration * TractionFactor;
            _effectiveBraking = input.Brake * Tuning.brakeDeceleration * TractionFactor;
            float drag = Tuning.drag;

            // Net acceleration
            float netAcceleration = _effectiveAcceleration - _effectiveBraking - drag;
            currentSpeed += netAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, Tuning.maxSpeed);
        }

        protected override void ApplyMovement(float deltaTime)
        {
            if (!IsGrounded)
            {
                // Apply gravity when airborne
                Rb.AddForce(Physics.gravity * (Tuning.gravityMultiplier - 1f), ForceMode.Acceleration);
                return;
            }

            // Calculate turn rate (lean influences turning for bikes)
            float turnRate = GetSpeedAdjustedTurnRate();
            float leanInfluence = Mathf.Abs(currentLean) / Tuning.maxLeanAngle * Tuning.leanTurnInfluence;
            float effectiveTurnRate = turnRate * (1f + leanInfluence);

            // Apply traction to turn rate - low traction = can turn but may slide
            effectiveTurnRate *= Mathf.Lerp(1f, TractionFactor, 0.7f);

            // Apply rotation
            float turnAmount = currentSteer * effectiveTurnRate * deltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            Rb.MoveRotation(Rb.rotation * turnRotation);

            // Apply forward velocity with lateral slip on low traction
            Vector3 forwardVelocity = transform.forward * currentSpeed;
            Vector3 currentVel = Rb.linearVelocity;

            // Lateral slip: on low traction, some momentum carries in old direction
            if (TractionFactor < 1f)
            {
                float slipFactor = 1f - TractionFactor;
                Vector3 lateralVel = Vector3.ProjectOnPlane(currentVel, Vector3.up);
                lateralVel.y = 0f;

                // Blend forward velocity with existing lateral momentum
                forwardVelocity = Vector3.Lerp(forwardVelocity, lateralVel.normalized * currentSpeed, slipFactor * 0.3f);
            }

            // Preserve vertical velocity for slopes/jumps
            forwardVelocity.y = currentVel.y;
            Rb.linearVelocity = forwardVelocity;
        }

        protected override void ApplyLean(float deltaTime)
        {
            // Bikes lean more aggressively and lean is tied to speed
            float speedFactor = Mathf.Clamp01(currentSpeed / (Tuning.maxSpeed * 0.5f));
            float targetLean = -currentSteer * Tuning.maxLeanAngle * speedFactor;

            // Wind pushes the lean
            if (WindForce.sqrMagnitude > 0.01f)
            {
                float windLean = Vector3.Dot(WindForce, transform.right) * 0.5f;
                targetLean += windLean;
            }

            currentLean = Mathf.Lerp(currentLean, targetLean, Tuning.leanSpeed * deltaTime);

            // Clamp lean angle
            currentLean = Mathf.Clamp(currentLean, -Tuning.maxLeanAngle * 1.2f, Tuning.maxLeanAngle * 1.2f);

            // Apply lean to visual (Z rotation)
            Vector3 euler = transform.localEulerAngles;
            euler.z = currentLean;
            transform.localEulerAngles = euler;
        }

        #endregion
    }
}
