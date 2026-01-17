using UnityEngine;

namespace EdgeAbyss.Gameplay.Ghost
{
    /// <summary>
    /// Visual representation of the ghost bike.
    /// Receives position updates from GhostPlayback.
    /// 
    /// SETUP:
    /// 1. Create a ghost bike prefab (copy of bike, stripped of physics/input).
    /// 2. Attach this component to the root.
    /// 3. Assign optional translucent material.
    /// 4. TimeTrialManager spawns this and connects to GhostPlayback.
    /// 
    /// PREFAB SETUP:
    /// - Remove Rigidbody, Colliders (or set to trigger/non-colliding layer)
    /// - Apply translucent material to all renderers
    /// - Keep visual mesh and lean animations
    /// </summary>
    public class GhostRunner : MonoBehaviour
    {
        [Header("Visual Settings")]
        [Tooltip("Material to apply to all renderers (translucent ghost material).")]
        [SerializeField] private Material ghostMaterial;

        [Tooltip("Apply ghost material on start.")]
        [SerializeField] private bool applyMaterialOnStart = true;

        [Tooltip("Ghost color tint.")]
        [SerializeField] private Color ghostTint = new Color(0.5f, 0.7f, 1f, 0.4f);

        [Header("Animation")]
        [Tooltip("Smooth position interpolation.")]
        [SerializeField] private bool smoothMovement = true;

        [Tooltip("Position smoothing speed.")]
        [SerializeField] private float positionSmoothSpeed = 20f;

        [Tooltip("Rotation smoothing speed.")]
        [SerializeField] private float rotationSmoothSpeed = 15f;

        [Header("Lean")]
        [Tooltip("Transform to apply lean rotation to (usually the bike body).")]
        [SerializeField] private Transform leanTransform;

        // Playback reference
        private GhostPlayback _playback;

        // Target values from playback
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private float _targetLean;

        // Cached renderers
        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;

        private static readonly int TintColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int AlphaProperty = Shader.PropertyToID("_Alpha");

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();

            if (leanTransform == null)
            {
                leanTransform = transform;
            }

            // Ensure no collision
            SetupNonColliding();
        }

        private void Start()
        {
            if (applyMaterialOnStart)
            {
                ApplyGhostMaterial();
            }

            ApplyGhostTint();
        }

        private void Update()
        {
            if (_playback == null || !_playback.IsPlaying) return;

            // Get current values from playback
            _targetPosition = _playback.CurrentPosition;
            _targetRotation = _playback.CurrentRotation;
            _targetLean = _playback.CurrentLean;

            ApplyTransform();
        }

        /// <summary>
        /// Connects this runner to a GhostPlayback component.
        /// </summary>
        public void SetPlayback(GhostPlayback playback)
        {
            // Unsubscribe from old
            if (_playback != null)
            {
                _playback.OnFrameUpdated -= HandleFrameUpdated;
                _playback.OnPlaybackFinished -= HandlePlaybackFinished;
            }

            _playback = playback;

            // Subscribe to new
            if (_playback != null)
            {
                _playback.OnFrameUpdated += HandleFrameUpdated;
                _playback.OnPlaybackFinished += HandlePlaybackFinished;
            }
        }

        /// <summary>
        /// Sets the ghost to a specific position/rotation immediately.
        /// </summary>
        public void SetPositionImmediate(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _targetPosition = position;
            _targetRotation = rotation;
        }

        /// <summary>
        /// Shows or hides the ghost.
        /// </summary>
        public void SetVisible(bool visible)
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = visible;
            }
        }

        private void ApplyTransform()
        {
            if (smoothMovement)
            {
                // Smooth interpolation for visual polish
                transform.position = Vector3.Lerp(
                    transform.position,
                    _targetPosition,
                    positionSmoothSpeed * Time.deltaTime
                );

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    _targetRotation,
                    rotationSmoothSpeed * Time.deltaTime
                );
            }
            else
            {
                // Direct assignment
                transform.SetPositionAndRotation(_targetPosition, _targetRotation);
            }

            // Apply lean
            if (leanTransform != null)
            {
                Vector3 euler = leanTransform.localEulerAngles;
                euler.z = _targetLean;
                leanTransform.localEulerAngles = euler;
            }
        }

        private void SetupNonColliding()
        {
            // Disable all colliders or set to trigger
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Remove rigidbody if present
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }
        }

        private void ApplyGhostMaterial()
        {
            if (ghostMaterial == null) return;

            foreach (var renderer in _renderers)
            {
                // Create material instances to avoid modifying shared materials
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = ghostMaterial;
                }
                renderer.materials = materials;
            }
        }

        private void ApplyGhostTint()
        {
            foreach (var renderer in _renderers)
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(TintColorProperty, ghostTint);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void HandleFrameUpdated(Vector3 position, Quaternion rotation, float speed, float lean)
        {
            _targetPosition = position;
            _targetRotation = rotation;
            _targetLean = lean;
        }

        private void HandlePlaybackFinished()
        {
            // Optionally hide or fade out the ghost
            // SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_playback != null)
            {
                _playback.OnFrameUpdated -= HandleFrameUpdated;
                _playback.OnPlaybackFinished -= HandlePlaybackFinished;
            }
        }
    }
}
