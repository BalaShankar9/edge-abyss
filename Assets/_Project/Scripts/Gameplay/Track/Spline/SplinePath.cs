using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeAbyss.Gameplay.Track.Spline
{
    /// <summary>
    /// Catmull-Rom spline path defined by control points.
    /// Provides smooth interpolation and tangent calculation.
    /// 
    /// SETUP:
    /// 1. Attach to empty GameObject.
    /// 2. Add child Transform objects as control points, OR
    /// 3. Use serialized points list for procedural paths.
    /// 4. Enable "Use Child Transforms" to auto-detect children.
    /// </summary>
    public class SplinePath : MonoBehaviour
    {
        [Header("Control Points")]
        [Tooltip("Use child transforms as control points.")]
        [SerializeField] private bool useChildTransforms = true;

        [Tooltip("Manual control points (used if not using child transforms).")]
        [SerializeField] private List<SplinePoint> points = new List<SplinePoint>();

        [Header("Spline Settings")]
        [Tooltip("Tension parameter for Catmull-Rom (0.5 = standard).")]
        [Range(0f, 1f)]
        [SerializeField] private float tension = 0.5f;

        [Tooltip("Close the spline into a loop.")]
        [SerializeField] private bool isLoop;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color splineColor = Color.cyan;
        [SerializeField] private Color pointColor = Color.yellow;
        [SerializeField] private Color tangentColor = Color.green;
        [SerializeField] private int gizmoSegments = 50;
        [SerializeField] private float pointSize = 0.3f;

        // Cached points for runtime
        private List<Vector3> _cachedWorldPoints = new List<Vector3>();
        private bool _isDirty = true;

        /// <summary>Number of control points.</summary>
        public int PointCount => GetControlPoints().Count;

        /// <summary>Is this a closed loop?</summary>
        public bool IsLoop => isLoop;

        /// <summary>Total approximate length of the spline.</summary>
        public float ApproximateLength { get; private set; }

        private void OnValidate()
        {
            _isDirty = true;
        }

        private void OnTransformChildrenChanged()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Gets the world-space control points.
        /// </summary>
        public List<Vector3> GetControlPoints()
        {
            if (_isDirty || _cachedWorldPoints.Count == 0)
            {
                RefreshControlPoints();
            }
            return _cachedWorldPoints;
        }

        /// <summary>
        /// Refreshes the cached control points from source.
        /// </summary>
        public void RefreshControlPoints()
        {
            _cachedWorldPoints.Clear();

            if (useChildTransforms)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child.gameObject.activeInHierarchy)
                    {
                        _cachedWorldPoints.Add(child.position);
                    }
                }
            }
            else
            {
                foreach (var point in points)
                {
                    _cachedWorldPoints.Add(transform.TransformPoint(point.localPosition));
                }
            }

            CalculateApproximateLength();
            _isDirty = false;
        }

        /// <summary>
        /// Evaluates the spline at parameter t [0, 1].
        /// </summary>
        public Vector3 Evaluate(float t)
        {
            var pts = GetControlPoints();
            if (pts.Count < 2) return pts.Count > 0 ? pts[0] : transform.position;

            t = Mathf.Clamp01(t);

            int numSegments = isLoop ? pts.Count : pts.Count - 1;
            float segmentT = t * numSegments;
            int segment = Mathf.Min((int)segmentT, numSegments - 1);
            float localT = segmentT - segment;

            return EvaluateSegment(segment, localT);
        }

        /// <summary>
        /// Evaluates the tangent (forward direction) at parameter t [0, 1].
        /// </summary>
        public Vector3 EvaluateTangent(float t)
        {
            var pts = GetControlPoints();
            if (pts.Count < 2) return Vector3.forward;

            t = Mathf.Clamp01(t);

            int numSegments = isLoop ? pts.Count : pts.Count - 1;
            float segmentT = t * numSegments;
            int segment = Mathf.Min((int)segmentT, numSegments - 1);
            float localT = segmentT - segment;

            return EvaluateSegmentTangent(segment, localT).normalized;
        }

        /// <summary>
        /// Evaluates position and rotation at parameter t.
        /// </summary>
        public void Evaluate(float t, out Vector3 position, out Quaternion rotation)
        {
            position = Evaluate(t);
            Vector3 tangent = EvaluateTangent(t);
            
            // Calculate up vector (world up projected onto plane perpendicular to tangent)
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, tangent).normalized;
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.Cross(Vector3.forward, tangent).normalized;
            }
            up = Vector3.Cross(tangent, right).normalized;

            rotation = Quaternion.LookRotation(tangent, up);
        }

        /// <summary>
        /// Samples the spline at regular intervals.
        /// </summary>
        public void SamplePoints(int sampleCount, List<Vector3> positions, List<Vector3> tangents = null, List<Quaternion> rotations = null)
        {
            positions.Clear();
            tangents?.Clear();
            rotations?.Clear();

            if (sampleCount < 2) sampleCount = 2;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);

                Evaluate(t, out Vector3 pos, out Quaternion rot);
                positions.Add(pos);
                tangents?.Add(EvaluateTangent(t));
                rotations?.Add(rot);
            }
        }

        /// <summary>
        /// Gets the closest point on the spline to a world position.
        /// </summary>
        public float GetClosestT(Vector3 worldPosition, int searchIterations = 20)
        {
            float bestT = 0f;
            float bestDist = float.MaxValue;

            // Coarse search
            for (int i = 0; i <= searchIterations; i++)
            {
                float t = i / (float)searchIterations;
                Vector3 pos = Evaluate(t);
                float dist = (pos - worldPosition).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = t;
                }
            }

            // Fine search around best
            float range = 1f / searchIterations;
            for (int i = 0; i < searchIterations; i++)
            {
                float t = bestT + UnityEngine.Random.Range(-range, range);
                t = Mathf.Clamp01(t);
                Vector3 pos = Evaluate(t);
                float dist = (pos - worldPosition).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = t;
                }
            }

            return bestT;
        }

        private Vector3 EvaluateSegment(int segment, float t)
        {
            var pts = _cachedWorldPoints;
            int count = pts.Count;

            // Get four points for Catmull-Rom
            int p0 = GetPointIndex(segment - 1, count);
            int p1 = GetPointIndex(segment, count);
            int p2 = GetPointIndex(segment + 1, count);
            int p3 = GetPointIndex(segment + 2, count);

            return CatmullRom(pts[p0], pts[p1], pts[p2], pts[p3], t, tension);
        }

        private Vector3 EvaluateSegmentTangent(int segment, float t)
        {
            var pts = _cachedWorldPoints;
            int count = pts.Count;

            int p0 = GetPointIndex(segment - 1, count);
            int p1 = GetPointIndex(segment, count);
            int p2 = GetPointIndex(segment + 1, count);
            int p3 = GetPointIndex(segment + 2, count);

            return CatmullRomDerivative(pts[p0], pts[p1], pts[p2], pts[p3], t, tension);
        }

        private int GetPointIndex(int index, int count)
        {
            if (isLoop)
            {
                return ((index % count) + count) % count;
            }
            return Mathf.Clamp(index, 0, count - 1);
        }

        private void CalculateApproximateLength()
        {
            ApproximateLength = 0f;
            if (_cachedWorldPoints.Count < 2) return;

            Vector3 prevPos = Evaluate(0f);
            int steps = 100;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 pos = Evaluate(t);
                ApproximateLength += Vector3.Distance(prevPos, pos);
                prevPos = pos;
            }
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float alpha = tension;
            Vector3 m1 = alpha * (p2 - p0);
            Vector3 m2 = alpha * (p3 - p1);

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            return h00 * p1 + h10 * m1 + h01 * p2 + h11 * m2;
        }

        private static Vector3 CatmullRomDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension)
        {
            float t2 = t * t;

            float alpha = tension;
            Vector3 m1 = alpha * (p2 - p0);
            Vector3 m2 = alpha * (p3 - p1);

            float h00 = 6f * t2 - 6f * t;
            float h10 = 3f * t2 - 4f * t + 1f;
            float h01 = -6f * t2 + 6f * t;
            float h11 = 3f * t2 - 2f * t;

            return h00 * p1 + h10 * m1 + h01 * p2 + h11 * m2;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            DrawSplineGizmos(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            DrawSplineGizmos(true);
        }

        private void DrawSplineGizmos(bool selected)
        {
            RefreshControlPoints();
            var pts = _cachedWorldPoints;

            if (pts.Count < 2) return;

            // Draw control points
            Gizmos.color = pointColor;
            foreach (var pt in pts)
            {
                Gizmos.DrawSphere(pt, pointSize);
            }

            // Draw spline curve
            Gizmos.color = splineColor;
            Vector3 prevPos = Evaluate(0f);

            for (int i = 1; i <= gizmoSegments; i++)
            {
                float t = i / (float)gizmoSegments;
                Vector3 pos = Evaluate(t);
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }

            // Draw tangents when selected
            if (selected)
            {
                Gizmos.color = tangentColor;
                for (int i = 0; i <= 10; i++)
                {
                    float t = i / 10f;
                    Vector3 pos = Evaluate(t);
                    Vector3 tangent = EvaluateTangent(t);
                    Gizmos.DrawLine(pos, pos + tangent * 1f);
                }
            }
        }
#endif
    }

    /// <summary>
    /// Serializable spline control point.
    /// </summary>
    [Serializable]
    public struct SplinePoint
    {
        public Vector3 localPosition;
        public float width; // Optional: per-point width override

        public SplinePoint(Vector3 position, float width = 1f)
        {
            this.localPosition = position;
            this.width = width;
        }
    }
}
