using UnityEngine;

namespace EdgeAbyss.Gameplay.Score
{
    /// <summary>
    /// Detects proximity to track edges using raycasts.
    /// Provides a smooth proximity factor for near-miss bonus scoring.
    /// 
    /// SETUP:
    /// 1. Attach to rider prefab (or as child of rider).
    /// 2. Configure raycast parameters.
    /// 3. Set layer mask to detect track boundaries.
    /// 4. ScoreManager queries ProximityFactor each frame.
    /// </summary>
    public class EdgeProximitySensor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [Tooltip("Maximum distance to check for edges.")]
        [SerializeField] private float maxRayDistance = 5f;

        [Tooltip("Height offset for raycast origin.")]
        [SerializeField] private float rayHeightOffset = 0.5f;

        [Tooltip("Number of rays per side for averaging.")]
        [SerializeField] [Range(1, 5)] private int raysPerSide = 3;

        [Tooltip("Angle spread for multiple rays (degrees).")]
        [SerializeField] [Range(0f, 45f)] private float rayAngleSpread = 15f;

        [Header("Detection")]
        [Tooltip("Layers to detect as track edge/boundary.")]
        [SerializeField] private LayerMask edgeLayerMask = ~0;

        [Tooltip("Distance considered 'danger zone' for near-miss.")]
        [SerializeField] private float dangerZoneDistance = 1.5f;

        [Header("Smoothing")]
        [Tooltip("How quickly proximity factor changes.")]
        [SerializeField] private float smoothSpeed = 8f;

        [Tooltip("Minimum change to update (reduces jitter).")]
        [SerializeField] private float changeThreshold = 0.02f;

        // Results
        private float _leftDistance;
        private float _rightDistance;
        private float _rawProximityFactor;
        private float _smoothedProximityFactor;
        private float _nearestEdgeDistance;

        /// <summary>
        /// Smoothed proximity factor [0-1]. 
        /// 0 = far from edges, 1 = extremely close to edge.
        /// </summary>
        public float ProximityFactor => _smoothedProximityFactor;

        /// <summary>
        /// Distance to the nearest edge detected.
        /// Returns maxRayDistance if no edge detected.
        /// </summary>
        public float NearestEdgeDistance => _nearestEdgeDistance;

        /// <summary>
        /// Distance to left edge. Returns maxRayDistance if none detected.
        /// </summary>
        public float LeftEdgeDistance => _leftDistance;

        /// <summary>
        /// Distance to right edge. Returns maxRayDistance if none detected.
        /// </summary>
        public float RightEdgeDistance => _rightDistance;

        /// <summary>
        /// True if currently in the danger zone near an edge.
        /// </summary>
        public bool IsInDangerZone => _nearestEdgeDistance < dangerZoneDistance;

        /// <summary>
        /// Which side is closer: -1 = left, 0 = balanced, 1 = right.
        /// </summary>
        public int NearerSide
        {
            get
            {
                float diff = _rightDistance - _leftDistance;
                if (Mathf.Abs(diff) < 0.5f) return 0;
                return diff > 0 ? -1 : 1;
            }
        }

        private void FixedUpdate()
        {
            UpdateEdgeDetection();
            SmoothProximityFactor();
        }

        private void UpdateEdgeDetection()
        {
            Vector3 origin = transform.position + Vector3.up * rayHeightOffset;

            // Cast rays to the left
            _leftDistance = CastRaysInDirection(-transform.right, origin);

            // Cast rays to the right
            _rightDistance = CastRaysInDirection(transform.right, origin);

            // Nearest edge
            _nearestEdgeDistance = Mathf.Min(_leftDistance, _rightDistance);

            // Calculate raw proximity factor
            _rawProximityFactor = CalculateProximityFactor(_nearestEdgeDistance);
        }

        private float CastRaysInDirection(Vector3 baseDirection, Vector3 origin)
        {
            if (raysPerSide <= 1)
            {
                return CastSingleRay(origin, baseDirection);
            }

            float totalDistance = 0f;
            float minDistance = maxRayDistance;
            int hitCount = 0;

            float angleStep = rayAngleSpread / (raysPerSide - 1);
            float startAngle = -rayAngleSpread * 0.5f;

            for (int i = 0; i < raysPerSide; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * baseDirection;

                float distance = CastSingleRay(origin, direction);
                if (distance < maxRayDistance)
                {
                    hitCount++;
                    totalDistance += distance;
                    minDistance = Mathf.Min(minDistance, distance);
                }
            }

            // Return minimum distance (most conservative/dangerous)
            return minDistance;
        }

        private float CastSingleRay(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRayDistance, edgeLayerMask))
            {
                return hit.distance;
            }
            return maxRayDistance;
        }

        private float CalculateProximityFactor(float distance)
        {
            if (distance >= maxRayDistance)
            {
                return 0f;
            }

            // Inverse relationship: closer = higher factor
            // Uses danger zone as the high-value region
            if (distance <= dangerZoneDistance)
            {
                // In danger zone: 0.5 to 1.0
                float dangerFactor = 1f - (distance / dangerZoneDistance);
                return 0.5f + (dangerFactor * 0.5f);
            }
            else
            {
                // Outside danger zone: 0.0 to 0.5
                float safeFactor = (distance - dangerZoneDistance) / (maxRayDistance - dangerZoneDistance);
                return 0.5f * (1f - safeFactor);
            }
        }

        private void SmoothProximityFactor()
        {
            float delta = _rawProximityFactor - _smoothedProximityFactor;

            // Apply threshold to reduce jitter
            if (Mathf.Abs(delta) < changeThreshold)
            {
                return;
            }

            // Smooth interpolation
            _smoothedProximityFactor = Mathf.Lerp(
                _smoothedProximityFactor,
                _rawProximityFactor,
                smoothSpeed * Time.fixedDeltaTime
            );

            // Clamp to valid range
            _smoothedProximityFactor = Mathf.Clamp01(_smoothedProximityFactor);
        }

        /// <summary>
        /// Resets the sensor state (call on respawn).
        /// </summary>
        public void Reset()
        {
            _leftDistance = maxRayDistance;
            _rightDistance = maxRayDistance;
            _nearestEdgeDistance = maxRayDistance;
            _rawProximityFactor = 0f;
            _smoothedProximityFactor = 0f;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * rayHeightOffset;

            // Draw left rays
            DrawRayGizmos(origin, -transform.right, _leftDistance, Color.red);

            // Draw right rays
            DrawRayGizmos(origin, transform.right, _rightDistance, Color.blue);

            // Draw danger zone
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(origin, dangerZoneDistance);
        }

        private void DrawRayGizmos(Vector3 origin, Vector3 baseDirection, float hitDistance, Color color)
        {
            Gizmos.color = color;

            if (raysPerSide <= 1)
            {
                Gizmos.DrawLine(origin, origin + baseDirection * hitDistance);
                return;
            }

            float angleStep = rayAngleSpread / (raysPerSide - 1);
            float startAngle = -rayAngleSpread * 0.5f;

            for (int i = 0; i < raysPerSide; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * baseDirection;
                Gizmos.DrawLine(origin, origin + direction * maxRayDistance);
            }

            // Draw hit indicator
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin + baseDirection * hitDistance, 0.1f);
        }
#endif
    }
}
