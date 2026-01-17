using UnityEngine;
using EdgeAbyss.Gameplay.Track;

namespace EdgeAbyss.Gameplay.Track
{
    /// <summary>
    /// Trigger component that registers checkpoint when rider passes through.
    /// Attach to the same GameObject as Checkpoint component.
    /// 
    /// SETUP:
    /// 1. Ensure rider prefab has a Rigidbody.
    /// 2. Set rider layer to "Player" (or similar).
    /// 3. Configure Physics settings to allow trigger detection between layers.
    /// </summary>
    [RequireComponent(typeof(Checkpoint))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Layer mask for detecting the rider.")]
        [SerializeField] private LayerMask riderLayerMask = ~0;

        [Tooltip("Tag to identify rider (optional, leave empty to use layer only).")]
        [SerializeField] private string riderTag = "Player";

        private Checkpoint _checkpoint;
        private RespawnManager _respawnManager;
        private bool _hasBeenTriggered;

        private void Awake()
        {
            _checkpoint = GetComponent<Checkpoint>();
        }

        private void Start()
        {
            // Find respawn manager in scene
            _respawnManager = FindFirstObjectByType<RespawnManager>();
            
            if (_respawnManager == null)
            {
                Debug.LogWarning($"[{nameof(CheckpointTrigger)}] No RespawnManager found in scene.", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsRider(other)) return;

            if (_respawnManager != null)
            {
                _respawnManager.RegisterCheckpoint(_checkpoint);
            }

            // Visual/audio feedback could be triggered here
            if (!_hasBeenTriggered)
            {
                _hasBeenTriggered = true;
                OnFirstTimeTriggered();
            }
        }

        private bool IsRider(Collider other)
        {
            // Check layer
            if ((riderLayerMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return false;
            }

            // Check tag if specified
            if (!string.IsNullOrEmpty(riderTag) && !other.CompareTag(riderTag))
            {
                return false;
            }

            return true;
        }

        private void OnFirstTimeTriggered()
        {
            // Could trigger checkpoint reached effects here
            // e.g., sound, particle effect, UI notification
        }

        /// <summary>
        /// Resets the triggered state (for race restarts).
        /// </summary>
        public void ResetTrigger()
        {
            _hasBeenTriggered = false;
        }
    }
}
