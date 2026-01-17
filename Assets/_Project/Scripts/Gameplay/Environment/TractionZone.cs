using UnityEngine;

namespace EdgeAbyss.Gameplay.Environment
{
    /// <summary>
    /// Trigger zone that modifies traction when rider is inside.
    /// Use for surface types like gravel, wet stone, ice, etc.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject with Box/Sphere Collider (Is Trigger = true).
    /// 2. Attach this component.
    /// 3. Configure traction modifier for surface type.
    /// 4. Position zone to cover the track surface area.
    /// 
    /// RECOMMENDED VALUES:
    /// - Normal Road: 1.0
    /// - Wet Stone: 0.7
    /// - Loose Gravel: 0.5
    /// - Ice: 0.3
    /// - Mud: 0.6
    /// - Boost Pad: 1.2 (extra grip)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TractionZone : MonoBehaviour
    {
        [Header("Traction Settings")]
        [Tooltip("Traction multiplier (1.0 = normal, <1 = slippery, >1 = extra grip).")]
        [SerializeField] [Range(0.1f, 2f)] private float tractionMultiplier = 0.5f;

        [Tooltip("Steering response modifier (affects how quickly steering responds).")]
        [SerializeField] [Range(0.3f, 1.5f)] private float steeringModifier = 0.8f;

        [Tooltip("Stability modifier (affects stability drain/recovery).")]
        [SerializeField] [Range(0.5f, 1.5f)] private float stabilityModifier = 0.9f;

        [Header("Surface Type")]
        [Tooltip("Surface type name for audio/visual feedback.")]
        [SerializeField] private string surfaceType = "Gravel";

        [Tooltip("Color tint for debug visualization.")]
        [SerializeField] private Color debugColor = new Color(0.8f, 0.6f, 0.3f, 0.5f);

        [Header("Transition")]
        [Tooltip("How quickly traction changes when entering/exiting zone.")]
        [SerializeField] [Range(1f, 20f)] private float transitionSpeed = 8f;

        [Header("Audio/Visual")]
        [Tooltip("Optional particle system for surface effects (dust, spray, etc.).")]
        [SerializeField] private ParticleSystem surfaceParticles;

        [Tooltip("Play particles only when moving.")]
        [SerializeField] private bool particlesRequireMovement = true;

        [Tooltip("Minimum speed for particle effects.")]
        [SerializeField] [Range(0f, 10f)] private float particleMinSpeed = 2f;

        // State
        private bool _hasRiderInside;
        private Riders.IRiderController _riderInZone;
        private MonoBehaviour _riderMono;

        /// <summary>Traction multiplier for this zone.</summary>
        public float TractionMultiplier => tractionMultiplier;

        /// <summary>Steering modifier for this zone.</summary>
        public float SteeringModifier => steeringModifier;

        /// <summary>Stability modifier for this zone.</summary>
        public float StabilityModifier => stabilityModifier;

        /// <summary>Surface type identifier.</summary>
        public string SurfaceType => surfaceType;

        /// <summary>True if a rider is currently in this zone.</summary>
        public bool HasRiderInside => _hasRiderInside;

        /// <summary>Speed of traction transition.</summary>
        public float TransitionSpeed => transitionSpeed;

        private void Awake()
        {
            // Ensure collider is trigger
            var collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (!_hasRiderInside || _riderInZone == null) return;

            UpdateParticles();
        }

        private void UpdateParticles()
        {
            if (surfaceParticles == null) return;

            if (particlesRequireMovement)
            {
                bool shouldPlay = _riderInZone.Speed >= particleMinSpeed;

                if (shouldPlay && !surfaceParticles.isPlaying)
                {
                    surfaceParticles.Play();
                }
                else if (!shouldPlay && surfaceParticles.isPlaying)
                {
                    surfaceParticles.Stop();
                }

                // Modulate emission based on speed
                if (shouldPlay)
                {
                    var emission = surfaceParticles.emission;
                    float speedFactor = Mathf.Clamp01(_riderInZone.Speed / 20f);
                    emission.rateOverTimeMultiplier = speedFactor;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var rider = other.GetComponentInParent<Riders.IRiderController>();
            if (rider != null)
            {
                _hasRiderInside = true;
                _riderInZone = rider;
                _riderMono = rider as MonoBehaviour;

                // Notify the traction manager
                if (TractionManager.HasInstance)
                {
                    TractionManager.Instance.EnterZone(this);
                }

                if (surfaceParticles != null && !particlesRequireMovement)
                {
                    surfaceParticles.Play();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var rider = other.GetComponentInParent<Riders.IRiderController>();
            if (rider != null && rider == _riderInZone)
            {
                _hasRiderInside = false;
                _riderInZone = null;
                _riderMono = null;

                // Notify the traction manager
                if (TractionManager.HasInstance)
                {
                    TractionManager.Instance.ExitZone(this);
                }

                if (surfaceParticles != null)
                {
                    surfaceParticles.Stop();
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = debugColor;

            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }

            var sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw zone outline and label
            Gizmos.color = debugColor;

            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }

            var sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }

            // Label
            UnityEditor.Handles.Label(transform.position + Vector3.up, 
                $"{surfaceType}\nTraction: {tractionMultiplier:F1}x");
        }
#endif
    }
}
