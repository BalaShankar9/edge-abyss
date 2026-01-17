namespace EdgeAbyss.Gameplay.Camera
{
    /// <summary>
    /// Lightweight data struct containing all rider state needed by the camera.
    /// Decouples camera from specific rider implementations.
    /// 
    /// DATA FLOW:
    /// RiderManager -> RiderCameraData -> POVCameraRig
    /// 
    /// The camera never needs to know if it's following a bike, horse, or any
    /// other rider type. It only reacts to these universal values.
    /// </summary>
    public readonly struct RiderCameraData
    {
        /// <summary>Current forward speed in units/second.</summary>
        public readonly float Speed;

        /// <summary>Current lean angle in degrees (negative = left, positive = right).</summary>
        public readonly float LeanAngle;

        /// <summary>Current stability [0..1]. Can be used for camera effects.</summary>
        public readonly float Stability;

        /// <summary>True if the rider is grounded.</summary>
        public readonly bool IsGrounded;

        /// <summary>True if the rider has fallen (camera may need to detach or shake).</summary>
        public readonly bool HasFallen;

        /// <summary>Default empty data (stationary, stable).</summary>
        public static readonly RiderCameraData Empty = new RiderCameraData(0f, 0f, 1f, true, false);

        /// <summary>
        /// Creates camera data from rider state.
        /// </summary>
        public RiderCameraData(float speed, float leanAngle, float stability, bool isGrounded, bool hasFallen)
        {
            Speed = speed;
            LeanAngle = leanAngle;
            Stability = stability;
            IsGrounded = isGrounded;
            HasFallen = hasFallen;
        }
    }
}
