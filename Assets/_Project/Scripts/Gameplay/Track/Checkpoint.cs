using UnityEngine;

namespace EdgeAbyss.Gameplay.Track
{
    /// <summary>
    /// Trigger volume that stores a respawn point when the rider passes through.
    /// Place checkpoints along the track at safe locations.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject at checkpoint location.
    /// 2. Add Box/Sphere Collider, set as Trigger.
    /// 3. Attach this Checkpoint component.
    /// 4. Create child "RespawnPoint" transform positioned on track with correct heading.
    /// 5. Set checkpoint index (order along track).
    /// 6. Set Layer to "Checkpoint" for organization.
    /// 7. Ensure rider has Rigidbody and correct layer for trigger detection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Unique index of this checkpoint (0 = start, higher = further along).")]
        [SerializeField] private int checkpointIndex;

        [Tooltip("The transform where the rider will respawn. Should be on track, facing correct direction.")]
        [SerializeField] private Transform respawnPoint;

        [Tooltip("Optional name/description for debugging.")]
        [SerializeField] private string checkpointName;

        [Header("Respawn Adjustments")]
        [Tooltip("Height offset above respawn point to prevent ground clipping.")]
        [SerializeField] private float respawnHeightOffset = 0.5f;

        [Tooltip("Stability value to set on respawn [0-1].")]
        [SerializeField] [Range(0.5f, 1f)] private float respawnStability = 0.8f;

        /// <summary>Index of this checkpoint along the track.</summary>
        public int CheckpointIndex => checkpointIndex;

        /// <summary>Display name of the checkpoint.</summary>
        public string CheckpointName => string.IsNullOrEmpty(checkpointName) ? $"Checkpoint {checkpointIndex}" : checkpointName;

        /// <summary>World position for respawning (with height offset applied).</summary>
        public Vector3 RespawnPosition
        {
            get
            {
                Transform point = respawnPoint != null ? respawnPoint : transform;
                return point.position + Vector3.up * respawnHeightOffset;
            }
        }

        /// <summary>Rotation for respawning (facing direction of respawn point).</summary>
        public Quaternion RespawnRotation
        {
            get
            {
                Transform point = respawnPoint != null ? respawnPoint : transform;
                return point.rotation;
            }
        }

        /// <summary>Stability value to apply on respawn.</summary>
        public float RespawnStability => respawnStability;

        private Collider _triggerCollider;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();

            if (!_triggerCollider.isTrigger)
            {
                Debug.LogWarning($"[{nameof(Checkpoint)}] Collider should be set as Trigger.", this);
                _triggerCollider.isTrigger = true;
            }

            // Auto-create respawn point if not assigned
            if (respawnPoint == null)
            {
                respawnPoint = transform.Find("RespawnPoint");
                
                if (respawnPoint == null)
                {
                    // Use this transform as respawn point
                    Debug.LogWarning($"[{nameof(Checkpoint)}] No RespawnPoint child found. Using checkpoint transform.", this);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw checkpoint zone
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
                Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }

            // Draw respawn point
            Transform point = respawnPoint != null ? respawnPoint : transform;
            Vector3 spawnPos = point.position + Vector3.up * respawnHeightOffset;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPos, 0.3f);
            Gizmos.DrawLine(spawnPos, spawnPos + point.forward * 1.5f);

            // Draw arrow for direction
            Vector3 arrowEnd = spawnPos + point.forward * 1.5f;
            Vector3 arrowLeft = arrowEnd - point.forward * 0.3f + point.right * 0.2f;
            Vector3 arrowRight = arrowEnd - point.forward * 0.3f - point.right * 0.2f;
            Gizmos.DrawLine(arrowEnd, arrowLeft);
            Gizmos.DrawLine(arrowEnd, arrowRight);

            // Label
            UnityEditor.Handles.Label(spawnPos + Vector3.up * 0.5f, CheckpointName);
        }

        private void OnDrawGizmosSelected()
        {
            // Brighter when selected
            Gizmos.color = Color.yellow;
            Transform point = respawnPoint != null ? respawnPoint : transform;
            Vector3 spawnPos = point.position + Vector3.up * respawnHeightOffset;
            Gizmos.DrawSphere(spawnPos, 0.2f);
        }
#endif
    }
}
