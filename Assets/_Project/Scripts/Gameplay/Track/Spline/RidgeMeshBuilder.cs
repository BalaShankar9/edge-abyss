using System.Collections.Generic;
using UnityEngine;

namespace EdgeAbyss.Gameplay.Track.Spline
{
    /// <summary>
    /// Generates a ridge mesh along a SplinePath.
    /// Supports variable width, noise-based roughness, and collider generation.
    /// 
    /// SETUP:
    /// 1. Attach to same GameObject as SplinePath.
    /// 2. Configure width curve, resolution, and materials.
    /// 3. Click "Rebuild Mesh" in editor or call RebuildMesh() at runtime.
    /// 
    /// PERFORMANCE:
    /// - Use box colliders for better physics performance on long tracks.
    /// - Reduce resolution for distant LODs.
    /// </summary>
    [RequireComponent(typeof(SplinePath))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RidgeMeshBuilder : MonoBehaviour
    {
        [Header("Mesh Settings")]
        [Tooltip("Number of segments along the spline.")]
        [Range(10, 500)]
        [SerializeField] private int lengthSegments = 100;

        [Tooltip("Number of segments across the width.")]
        [Range(1, 20)]
        [SerializeField] private int widthSegments = 4;

        [Header("Width")]
        [Tooltip("Base width of the ridge in units.")]
        [SerializeField] private float baseWidth = 3f;

        [Tooltip("Width multiplier over spline distance (0 to 1).")]
        [SerializeField] private AnimationCurve widthCurve = AnimationCurve.Constant(0f, 1f, 1f);

        [Header("Height/Profile")]
        [Tooltip("Cross-section height profile (0 = edge, 1 = center).")]
        [SerializeField] private AnimationCurve heightProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.5f);

        [Tooltip("Height scale multiplier.")]
        [SerializeField] private float heightScale = 0.5f;

        [Header("Noise/Roughness")]
        [Tooltip("Enable noise-based surface roughness.")]
        [SerializeField] private bool enableNoise = true;

        [Tooltip("Noise frequency (higher = more detail).")]
        [SerializeField] private float noiseFrequency = 2f;

        [Tooltip("Noise amplitude (height variation).")]
        [SerializeField] private float noiseAmplitude = 0.1f;

        [Tooltip("Secondary noise for cracks.")]
        [SerializeField] private bool enableCracks;

        [Tooltip("Crack frequency.")]
        [SerializeField] private float crackFrequency = 5f;

        [Tooltip("Crack depth.")]
        [SerializeField] private float crackDepth = 0.2f;

        [Header("UV Mapping")]
        [Tooltip("UV tiling along length.")]
        [SerializeField] private float uvTilingLength = 1f;

        [Tooltip("UV tiling along width.")]
        [SerializeField] private float uvTilingWidth = 1f;

        [Header("Collider")]
        [Tooltip("Collider generation mode.")]
        [SerializeField] private ColliderMode colliderMode = ColliderMode.MeshCollider;

        [Tooltip("Number of box colliders (if using boxes).")]
        [SerializeField] private int boxColliderCount = 20;

        [Tooltip("Physics material for colliders.")]
        [SerializeField] private PhysicsMaterial physicsMaterial;

        [Header("Editor")]
        [Tooltip("Auto-rebuild when values change.")]
        [SerializeField] private bool autoRebuild = false;

        // Components
        private SplinePath _splinePath;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;

        // Generated data
        private Mesh _generatedMesh;
        private List<BoxCollider> _boxColliders = new List<BoxCollider>();

        // Noise seed for consistent results
        private int _noiseSeed;

        public enum ColliderMode
        {
            None,
            MeshCollider,
            BoxColliders
        }

        /// <summary>The generated mesh.</summary>
        public Mesh GeneratedMesh => _generatedMesh;

        /// <summary>The spline path used for generation.</summary>
        public SplinePath SplinePath => _splinePath;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (autoRebuild && Application.isEditor && !Application.isPlaying)
            {
                // Delay to avoid editor issues
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        RebuildMesh();
                    }
                };
            }
#endif
        }

        private void CacheComponents()
        {
            if (_splinePath == null) _splinePath = GetComponent<SplinePath>();
            if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Rebuilds the ridge mesh from the spline.
        /// </summary>
        public void RebuildMesh()
        {
            CacheComponents();

            if (_splinePath == null)
            {
                Debug.LogError("[RidgeMeshBuilder] SplinePath component not found.");
                return;
            }

            _splinePath.RefreshControlPoints();

            if (_splinePath.PointCount < 2)
            {
                Debug.LogWarning("[RidgeMeshBuilder] Need at least 2 control points.");
                return;
            }

            // Generate seed for noise
            _noiseSeed = gameObject.GetInstanceID();

            // Create mesh
            if (_generatedMesh == null)
            {
                _generatedMesh = new Mesh();
                _generatedMesh.name = "RidgeMesh";
            }
            else
            {
                _generatedMesh.Clear();
            }

            GenerateMeshData();

            _meshFilter.sharedMesh = _generatedMesh;

            // Generate collider
            GenerateCollider();

            Debug.Log($"[RidgeMeshBuilder] Generated mesh: {_generatedMesh.vertexCount} verts, {_generatedMesh.triangles.Length / 3} tris");
        }

        /// <summary>
        /// Clears the generated mesh and colliders.
        /// </summary>
        public void ClearMesh()
        {
            if (_generatedMesh != null)
            {
                _generatedMesh.Clear();
            }

            if (_meshFilter != null)
            {
                _meshFilter.sharedMesh = null;
            }

            ClearBoxColliders();

            if (_meshCollider != null)
            {
                _meshCollider.sharedMesh = null;
            }
        }

        private void GenerateMeshData()
        {
            int vertCountLength = lengthSegments + 1;
            int vertCountWidth = widthSegments + 1;
            int totalVerts = vertCountLength * vertCountWidth;

            var vertices = new Vector3[totalVerts];
            var normals = new Vector3[totalVerts];
            var uvs = new Vector2[totalVerts];
            var triangles = new int[lengthSegments * widthSegments * 6];

            // Generate vertices
            for (int l = 0; l < vertCountLength; l++)
            {
                float t = l / (float)lengthSegments;

                _splinePath.Evaluate(t, out Vector3 center, out Quaternion rotation);
                Vector3 right = rotation * Vector3.right;
                Vector3 up = rotation * Vector3.up;

                float widthMult = widthCurve.Evaluate(t);
                float currentWidth = baseWidth * widthMult;

                for (int w = 0; w < vertCountWidth; w++)
                {
                    float widthT = w / (float)widthSegments;
                    float widthOffset = (widthT - 0.5f) * currentWidth;

                    // Height from profile (0 at edges, peak at center)
                    float profileT = 1f - Mathf.Abs(widthT * 2f - 1f); // 0 at edges, 1 at center
                    float height = heightProfile.Evaluate(profileT) * heightScale;

                    // Add noise
                    if (enableNoise)
                    {
                        float noiseValue = PerlinNoise3D(
                            center.x * noiseFrequency + _noiseSeed,
                            center.z * noiseFrequency,
                            widthT * noiseFrequency * 2f
                        );
                        height += (noiseValue - 0.5f) * 2f * noiseAmplitude;
                    }

                    // Add cracks
                    if (enableCracks)
                    {
                        float crackNoise = PerlinNoise3D(
                            center.x * crackFrequency,
                            center.z * crackFrequency,
                            widthT * crackFrequency
                        );
                        if (crackNoise > 0.7f)
                        {
                            height -= crackDepth * ((crackNoise - 0.7f) / 0.3f);
                        }
                    }

                    Vector3 vertex = center + right * widthOffset + up * height;

                    int index = l * vertCountWidth + w;
                    vertices[index] = transform.InverseTransformPoint(vertex);
                    uvs[index] = new Vector2(widthT * uvTilingWidth, t * uvTilingLength * _splinePath.ApproximateLength);
                }
            }

            // Generate triangles
            int triIndex = 0;
            for (int l = 0; l < lengthSegments; l++)
            {
                for (int w = 0; w < widthSegments; w++)
                {
                    int bl = l * vertCountWidth + w;
                    int br = bl + 1;
                    int tl = bl + vertCountWidth;
                    int tr = tl + 1;

                    // First triangle
                    triangles[triIndex++] = bl;
                    triangles[triIndex++] = tl;
                    triangles[triIndex++] = tr;

                    // Second triangle
                    triangles[triIndex++] = bl;
                    triangles[triIndex++] = tr;
                    triangles[triIndex++] = br;
                }
            }

            // Apply to mesh
            _generatedMesh.vertices = vertices;
            _generatedMesh.triangles = triangles;
            _generatedMesh.uv = uvs;

            // Calculate normals
            _generatedMesh.RecalculateNormals();
            _generatedMesh.RecalculateBounds();
            _generatedMesh.RecalculateTangents();
        }

        private void GenerateCollider()
        {
            ClearBoxColliders();

            switch (colliderMode)
            {
                case ColliderMode.None:
                    if (_meshCollider != null)
                    {
                        _meshCollider.sharedMesh = null;
                    }
                    break;

                case ColliderMode.MeshCollider:
                    if (_meshCollider == null)
                    {
                        _meshCollider = GetComponent<MeshCollider>();
                        if (_meshCollider == null)
                        {
                            _meshCollider = gameObject.AddComponent<MeshCollider>();
                        }
                    }
                    _meshCollider.sharedMesh = _generatedMesh;
                    _meshCollider.sharedMaterial = physicsMaterial;
                    break;

                case ColliderMode.BoxColliders:
                    if (_meshCollider != null)
                    {
                        _meshCollider.sharedMesh = null;
                    }
                    GenerateBoxColliders();
                    break;
            }
        }

        private void GenerateBoxColliders()
        {
            for (int i = 0; i < boxColliderCount; i++)
            {
                float t0 = i / (float)boxColliderCount;
                float t1 = (i + 1) / (float)boxColliderCount;
                float tMid = (t0 + t1) * 0.5f;

                _splinePath.Evaluate(tMid, out Vector3 center, out Quaternion rotation);

                Vector3 p0 = _splinePath.Evaluate(t0);
                Vector3 p1 = _splinePath.Evaluate(t1);
                float segmentLength = Vector3.Distance(p0, p1);

                float widthMult = widthCurve.Evaluate(tMid);
                float currentWidth = baseWidth * widthMult;

                // Create child with box collider
                var boxObj = new GameObject($"BoxCollider_{i}");
                boxObj.transform.SetParent(transform);
                boxObj.transform.position = center;
                boxObj.transform.rotation = rotation;
                boxObj.layer = gameObject.layer;

                var boxCollider = boxObj.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(currentWidth, heightScale * 2f, segmentLength * 1.1f);
                boxCollider.center = new Vector3(0f, heightScale * 0.25f, 0f);
                boxCollider.sharedMaterial = physicsMaterial;

                _boxColliders.Add(boxCollider);
            }
        }

        private void ClearBoxColliders()
        {
            foreach (var col in _boxColliders)
            {
                if (col != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(col.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(col.gameObject);
                    }
                }
            }
            _boxColliders.Clear();
        }

        private static float PerlinNoise3D(float x, float y, float z)
        {
            // Combine 2D Perlin samples for pseudo-3D noise
            float xy = Mathf.PerlinNoise(x, y);
            float xz = Mathf.PerlinNoise(x + 100f, z);
            float yz = Mathf.PerlinNoise(y + 200f, z);
            return (xy + xz + yz) / 3f;
        }

#if UNITY_EDITOR
        [ContextMenu("Rebuild Mesh")]
        private void EditorRebuildMesh()
        {
            RebuildMesh();
        }

        [ContextMenu("Clear Mesh")]
        private void EditorClearMesh()
        {
            ClearMesh();
        }

        private void OnDrawGizmosSelected()
        {
            if (_splinePath == null) return;

            // Draw width preview
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);

            int previewSegments = 20;
            for (int i = 0; i < previewSegments; i++)
            {
                float t = i / (float)previewSegments;
                _splinePath.Evaluate(t, out Vector3 center, out Quaternion rotation);
                Vector3 right = rotation * Vector3.right;

                float widthMult = widthCurve.Evaluate(t);
                float currentWidth = baseWidth * widthMult * 0.5f;

                Vector3 left = center - right * currentWidth;
                Vector3 rightPt = center + right * currentWidth;

                Gizmos.DrawLine(left, rightPt);
            }
        }
#endif
    }
}
