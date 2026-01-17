using UnityEngine;

namespace EdgeAbyss.Gameplay.Track
{
    /// <summary>
    /// Defines the safe riding area using a trigger collider.
    /// The rider must stay within this volume to be considered "on track".
    /// 
    /// SETUP:
    /// 1. Create empty GameObject "TrackBounds" in scene.
    /// 2. Add Box Collider (or multiple) sized to cover entire track.
    /// 3. Set collider as Trigger.
    /// 4. Attach this component.
    /// 5. Set Layer to "TrackBounds" (create this layer).
    /// 6. Configure rider's FallDetector to reference this.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TrackBounds : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Grace period before triggering out-of-bounds (seconds).")]
        [SerializeField] private float gracePeriod = 0.5f;

        /// <summary>Grace period before out-of-bounds triggers.</summary>
        public float GracePeriod => gracePeriod;

        private Collider _boundsCollider;

        /// <summary>The collider defining the bounds.</summary>
        public Collider BoundsCollider => _boundsCollider;

        private void Awake()
        {
            _boundsCollider = GetComponent<Collider>();
            
            if (!_boundsCollider.isTrigger)
            {
                Debug.LogWarning($"[{nameof(TrackBounds)}] Collider should be set as Trigger.", this);
                _boundsCollider.isTrigger = true;
            }
        }

        /// <summary>
        /// Checks if a world position is within the track bounds.
        /// </summary>
        public bool ContainsPoint(Vector3 worldPosition)
        {
            return _boundsCollider.bounds.Contains(worldPosition);
        }

        /// <summary>
        /// Gets the closest point on the bounds to the given position.
        /// </summary>
        public Vector3 GetClosestPoint(Vector3 worldPosition)
        {
            return _boundsCollider.ClosestPoint(worldPosition);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(0f, 1f, 0.5f, 0.1f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
                Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
#endif
    }
}
