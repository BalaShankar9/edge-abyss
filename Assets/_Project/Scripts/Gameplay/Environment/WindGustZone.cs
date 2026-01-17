using UnityEngine;

namespace EdgeAbyss.Gameplay.Environment
{
    /// <summary>
    /// Trigger zone that applies timed wind pulses to riders.
    /// Uses predictable sine-wave or pulse patterns.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject with Box/Sphere Collider (Is Trigger = true).
    /// 2. Attach this component.
    /// 3. Configure pulse settings and direction.
    /// 4. Position zone along track where gusts should occur.
    /// 
    /// LAYER SETUP:
    /// - Zone should be on a layer that collides with the rider layer.
    /// - Use a dedicated "WindZone" layer if needed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WindGustZone : MonoBehaviour
    {
        public enum PulsePattern
        {
            /// <summary>Smooth sine wave oscillation.</summary>
            SineWave,
            /// <summary>Sharp on/off pulses.</summary>
            SquareWave,
            /// <summary>Constant intensity while in zone.</summary>
            Constant,
            /// <summary>Random pulses.</summary>
            Random
        }

        [Header("Wind Settings")]
        [Tooltip("Direction of wind in this zone (local space).")]
        [SerializeField] private Vector3 windDirection = Vector3.right;

        [Tooltip("Use world space direction instead of local.")]
        [SerializeField] private bool useWorldSpace;

        [Tooltip("Maximum wind intensity.")]
        [SerializeField] [Range(1f, 20f)] private float maxIntensity = 5f;

        [Header("Pulse Pattern")]
        [Tooltip("Pattern of wind pulses.")]
        [SerializeField] private PulsePattern pattern = PulsePattern.SineWave;

        [Tooltip("Pulse frequency (cycles per second).")]
        [SerializeField] [Range(0.1f, 5f)] private float pulseFrequency = 0.5f;

        [Tooltip("Minimum intensity during pulse (for non-constant patterns).")]
        [SerializeField] [Range(0f, 1f)] private float minIntensityRatio = 0.2f;

        [Header("Random Pattern")]
        [Tooltip("Minimum time between random pulses.")]
        [SerializeField] [Range(0.5f, 5f)] private float randomMinInterval = 1f;

        [Tooltip("Maximum time between random pulses.")]
        [SerializeField] [Range(1f, 10f)] private float randomMaxInterval = 3f;

        [Header("Visual Feedback")]
        [Tooltip("Optional particle system to show wind.")]
        [SerializeField] private ParticleSystem windParticles;

        // State
        private bool _hasRiderInside;
        private float _currentIntensity;
        private float _phaseOffset;
        private float _nextRandomPulse;
        private bool _randomPulseActive;

        private void Awake()
        {
            // Ensure collider is trigger
            var collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }

            // Random phase offset so zones don't sync up
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
            ScheduleNextRandomPulse();
        }

        private void Update()
        {
            if (!_hasRiderInside) return;

            UpdatePulse();
            ApplyWindToSystem();
        }

        private void UpdatePulse()
        {
            float time = Time.time;

            switch (pattern)
            {
                case PulsePattern.SineWave:
                    // Smooth sine oscillation between min and max
                    float sine = Mathf.Sin((time * pulseFrequency * Mathf.PI * 2f) + _phaseOffset);
                    float normalized = (sine + 1f) * 0.5f; // 0 to 1
                    _currentIntensity = Mathf.Lerp(maxIntensity * minIntensityRatio, maxIntensity, normalized);
                    break;

                case PulsePattern.SquareWave:
                    // Sharp on/off pulses
                    float phase = ((time * pulseFrequency) + (_phaseOffset / (Mathf.PI * 2f))) % 1f;
                    _currentIntensity = phase < 0.5f ? maxIntensity : maxIntensity * minIntensityRatio;
                    break;

                case PulsePattern.Constant:
                    _currentIntensity = maxIntensity;
                    break;

                case PulsePattern.Random:
                    UpdateRandomPulse(time);
                    break;
            }

            // Update particles if present
            if (windParticles != null)
            {
                var emission = windParticles.emission;
                float normalizedIntensity = _currentIntensity / maxIntensity;
                emission.rateOverTimeMultiplier = normalizedIntensity;
            }
        }

        private void UpdateRandomPulse(float time)
        {
            if (time >= _nextRandomPulse)
            {
                _randomPulseActive = !_randomPulseActive;
                ScheduleNextRandomPulse();
            }

            _currentIntensity = _randomPulseActive ? maxIntensity : maxIntensity * minIntensityRatio;
        }

        private void ScheduleNextRandomPulse()
        {
            _nextRandomPulse = Time.time + Random.Range(randomMinInterval, randomMaxInterval);
        }

        private void ApplyWindToSystem()
        {
            if (WindSystem.HasInstance)
            {
                Vector3 worldDirection = useWorldSpace ? windDirection : transform.TransformDirection(windDirection);
                WindSystem.Instance.AddZoneWind(worldDirection.normalized, _currentIntensity);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if it's a rider
            if (other.GetComponentInParent<Riders.IRiderController>() != null)
            {
                _hasRiderInside = true;

                if (windParticles != null && !windParticles.isPlaying)
                {
                    windParticles.Play();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<Riders.IRiderController>() != null)
            {
                _hasRiderInside = false;

                if (WindSystem.HasInstance)
                {
                    WindSystem.Instance.ClearZoneWind();
                }

                if (windParticles != null)
                {
                    windParticles.Stop();
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw wind direction arrow
            Vector3 worldDir = useWorldSpace ? windDirection : transform.TransformDirection(windDirection);
            worldDir = worldDir.normalized;

            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + worldDir * 3f);

            // Arrow head
            Vector3 arrowEnd = transform.position + worldDir * 3f;
            Vector3 right = Vector3.Cross(worldDir, Vector3.up).normalized;
            Gizmos.DrawLine(arrowEnd, arrowEnd - worldDir * 0.5f + right * 0.3f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - worldDir * 0.5f - right * 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw zone bounds
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }

            var sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
                Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }
#endif
    }
}
