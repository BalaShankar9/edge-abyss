using UnityEngine;

namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Momentum-based horse controller.
    /// Features wider stability tolerance, automatic micro-correction,
    /// and lower response to sudden steering changes.
    /// 
    /// MOVEMENT LOGIC: All horse-specific physics handled here.
    /// </summary>
    public sealed class HorseRiderController : RiderBase
    {
        #region Private State

        private float _targetSteer;
        private float _momentumDirection;
        private float _wobble;

        #endregion

        #region Movement Implementation

        protected override void ProcessInput(InputState input)
        {
            // Slower response to steering input (momentum-based)
            _targetSteer = input.Steer;

            // Apply momentum inertia - horse resists sudden direction changes
            float inertiaFactor = 1f - Tuning.momentumInertia;
            float steerDelta = (_targetSteer - currentSteer) * inertiaFactor;
            currentSteer += steerDelta * Tuning.steerResponse * Time.deltaTime;
            currentSteer = Mathf.Clamp(currentSteer, -1f, 1f);

            // Momentum direction gradually follows steering
            _momentumDirection = Mathf.Lerp(_momentumDirection, currentSteer, 2f * Time.deltaTime);

            // Speed changes are also momentum-based
            float targetSpeed = input.Throttle * Tuning.maxSpeed;
            float brakeForce = input.Brake * Tuning.brakeDeceleration * Time.deltaTime;

            // Gradual acceleration (horse builds up speed)
            float speedDelta = (targetSpeed - currentSpeed) * (Tuning.acceleration / Tuning.maxSpeed) * Time.deltaTime;
            currentSpeed += speedDelta;
            currentSpeed -= brakeForce;
            currentSpeed -= Tuning.drag * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, Tuning.maxSpeed);
        }

        protected override void ApplyMovement(float deltaTime)
        {
            if (!IsGrounded)
            {
                Rb.AddForce(Physics.gravity * (Tuning.gravityMultiplier - 1f), ForceMode.Acceleration);
                return;
            }

            // Turn rate affected by momentum
            float turnRate = GetSpeedAdjustedTurnRate();
            float momentumFactor = 1f - Mathf.Abs(_momentumDirection - currentSteer) * Tuning.momentumInertia;
            float effectiveTurnRate = turnRate * Mathf.Max(0.3f, momentumFactor);

            // Apply rotation
            float turnAmount = currentSteer * effectiveTurnRate * deltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            Rb.MoveRotation(Rb.rotation * turnRotation);

            // Apply forward velocity
            Vector3 forwardVelocity = transform.forward * currentSpeed;
            Vector3 currentVel = Rb.linearVelocity;
            forwardVelocity.y = currentVel.y;
            Rb.linearVelocity = forwardVelocity;
        }

        protected override void ApplyLean(float deltaTime)
        {
            // Horse leans less dramatically and more smoothly
            float speedFactor = Mathf.Clamp01(currentSpeed / Tuning.maxSpeed);

            // Lean follows momentum rather than direct input
            float targetLean = -_momentumDirection * Tuning.maxLeanAngle * speedFactor * 0.7f;

            // Slower lean transitions for horse
            currentLean = Mathf.Lerp(currentLean, targetLean, Tuning.leanSpeed * 0.5f * deltaTime);

            Vector3 euler = transform.localEulerAngles;
            euler.z = currentLean;
            transform.localEulerAngles = euler;
        }

        protected override void CheckFallConditions()
        {
            // Call base which checks stability threshold
            base.CheckFallConditions();

            // Horse-specific: calculate wobble (difference between steering and momentum)
            _wobble = Mathf.Abs(currentSteer - _momentumDirection);

            // Auto-correction at low wobble (horse has wider tolerance)
            if (_wobble < 0.3f && Tuning.autoCorrection > 0f)
            {
                float correction = Tuning.autoCorrection * (1f - _wobble / 0.3f) * Time.deltaTime;
                ModifyStability(correction);
            }

            // High wobble at high speed drains stability (gradual, per-frame)
            if (_wobble > 0.8f && currentSpeed > Tuning.maxSpeed * 0.7f)
            {
                // Use gradual drain (already deltaTime scaled) - no cap needed
                ModifyStability(-0.1f * Time.deltaTime);
            }
        }

        protected override void OnRiderReset()
        {
            _targetSteer = 0f;
            _momentumDirection = 0f;
            _wobble = 0f;
        }

        #endregion
    }
}
