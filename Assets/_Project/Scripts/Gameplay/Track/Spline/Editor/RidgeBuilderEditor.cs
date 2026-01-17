#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace EdgeAbyss.Gameplay.Track.Spline
{
    /// <summary>
    /// Custom editor for RidgeMeshBuilder with rebuild button and tools.
    /// </summary>
    [CustomEditor(typeof(RidgeMeshBuilder))]
    public class RidgeBuilderEditor : Editor
    {
        private RidgeMeshBuilder _builder;
        private SplinePath _splinePath;
        private bool _showStats;

        private void OnEnable()
        {
            _builder = (RidgeMeshBuilder)target;
            _splinePath = _builder.GetComponent<SplinePath>();
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            // Build buttons
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Rebuild Mesh", GUILayout.Height(30)))
            {
                Undo.RecordObject(_builder, "Rebuild Ridge Mesh");
                _builder.RebuildMesh();
                EditorUtility.SetDirty(_builder);
            }

            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
            if (GUILayout.Button("Clear Mesh", GUILayout.Height(30)))
            {
                Undo.RecordObject(_builder, "Clear Ridge Mesh");
                _builder.ClearMesh();
                EditorUtility.SetDirty(_builder);
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Stats foldout
            _showStats = EditorGUILayout.Foldout(_showStats, "Mesh Statistics");
            if (_showStats && _builder.GeneratedMesh != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Vertices", _builder.GeneratedMesh.vertexCount.ToString());
                EditorGUILayout.LabelField("Triangles", (_builder.GeneratedMesh.triangles.Length / 3).ToString());
                EditorGUILayout.LabelField("Submeshes", _builder.GeneratedMesh.subMeshCount.ToString());

                if (_splinePath != null)
                {
                    EditorGUILayout.LabelField("Spline Length", $"{_splinePath.ApproximateLength:F2} units");
                    EditorGUILayout.LabelField("Control Points", _splinePath.PointCount.ToString());
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            // Quick tools
            EditorGUILayout.LabelField("Quick Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Control Point"))
            {
                AddControlPoint();
            }

            if (GUILayout.Button("Select Spline"))
            {
                if (_splinePath != null)
                {
                    Selection.activeGameObject = _splinePath.gameObject;
                }
            }

            EditorGUILayout.EndHorizontal();

            // Presets
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Narrow Path"))
            {
                ApplyPreset(2f, 0.3f, false);
            }

            if (GUILayout.Button("Wide Ridge"))
            {
                ApplyPreset(5f, 0.8f, false);
            }

            if (GUILayout.Button("Cracked Stone"))
            {
                ApplyPreset(3f, 0.5f, true);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddControlPoint()
        {
            if (_splinePath == null) return;

            // Create new control point at end of spline
            Vector3 endPos = _splinePath.Evaluate(1f);
            Vector3 tangent = _splinePath.EvaluateTangent(1f);

            var newPoint = new GameObject($"Point_{_splinePath.PointCount}");
            newPoint.transform.SetParent(_splinePath.transform);
            newPoint.transform.position = endPos + tangent * 5f;

            Undo.RegisterCreatedObjectUndo(newPoint, "Add Control Point");
            Selection.activeGameObject = newPoint;

            _splinePath.RefreshControlPoints();
        }

        private void ApplyPreset(float width, float height, bool cracks)
        {
            Undo.RecordObject(_builder, "Apply Preset");

            var serializedObj = new SerializedObject(_builder);

            serializedObj.FindProperty("baseWidth").floatValue = width;
            serializedObj.FindProperty("heightScale").floatValue = height;
            serializedObj.FindProperty("enableCracks").boolValue = cracks;

            if (cracks)
            {
                serializedObj.FindProperty("crackFrequency").floatValue = 5f;
                serializedObj.FindProperty("crackDepth").floatValue = 0.15f;
            }

            serializedObj.ApplyModifiedProperties();

            _builder.RebuildMesh();
            EditorUtility.SetDirty(_builder);
        }

        private void OnSceneGUI()
        {
            if (_splinePath == null) return;

            // Draw handles for control points
            var points = _splinePath.GetControlPoints();
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 pos = points[i];

                // Size handle
                float handleSize = HandleUtility.GetHandleSize(pos) * 0.1f;
                
                Handles.color = Color.yellow;
                if (Handles.Button(pos, Quaternion.identity, handleSize, handleSize * 1.5f, Handles.SphereHandleCap))
                {
                    // Select the control point transform
                    if (_splinePath.transform.childCount > i)
                    {
                        Selection.activeTransform = _splinePath.transform.GetChild(i);
                    }
                }

                // Label
                Handles.Label(pos + Vector3.up * 0.5f, $"P{i}", EditorStyles.boldLabel);
            }
        }
    }

    /// <summary>
    /// Custom editor for SplinePath with control point tools.
    /// </summary>
    [CustomEditor(typeof(SplinePath))]
    public class SplinePathEditor : Editor
    {
        private SplinePath _spline;

        private void OnEnable()
        {
            _spline = (SplinePath)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Point at End"))
            {
                AddPointAtEnd();
            }

            if (GUILayout.Button("Reverse Points"))
            {
                ReversePoints();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Distribute Evenly"))
            {
                DistributeEvenly();
            }

            if (GUILayout.Button("Smooth Path"))
            {
                SmoothPath();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddPointAtEnd()
        {
            _spline.RefreshControlPoints();

            Vector3 endPos = _spline.Evaluate(1f);
            Vector3 tangent = _spline.EvaluateTangent(1f);

            var newPoint = new GameObject($"Point_{_spline.transform.childCount}");
            newPoint.transform.SetParent(_spline.transform);
            newPoint.transform.position = endPos + tangent * 5f;

            Undo.RegisterCreatedObjectUndo(newPoint, "Add Spline Point");
            Selection.activeGameObject = newPoint;
        }

        private void ReversePoints()
        {
            int count = _spline.transform.childCount;
            for (int i = 0; i < count / 2; i++)
            {
                int j = count - 1 - i;
                _spline.transform.GetChild(j).SetSiblingIndex(i);
            }

            _spline.RefreshControlPoints();
            EditorUtility.SetDirty(_spline);
        }

        private void DistributeEvenly()
        {
            int count = _spline.transform.childCount;
            if (count < 2) return;

            Vector3 start = _spline.transform.GetChild(0).position;
            Vector3 end = _spline.transform.GetChild(count - 1).position;

            for (int i = 1; i < count - 1; i++)
            {
                float t = i / (float)(count - 1);
                Undo.RecordObject(_spline.transform.GetChild(i), "Distribute Points");
                _spline.transform.GetChild(i).position = Vector3.Lerp(start, end, t);
            }

            _spline.RefreshControlPoints();
            EditorUtility.SetDirty(_spline);
        }

        private void SmoothPath()
        {
            int count = _spline.transform.childCount;
            if (count < 3) return;

            var positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = _spline.transform.GetChild(i).position;
            }

            // Simple averaging
            for (int i = 1; i < count - 1; i++)
            {
                Vector3 avg = (positions[i - 1] + positions[i] + positions[i + 1]) / 3f;
                Undo.RecordObject(_spline.transform.GetChild(i), "Smooth Path");
                _spline.transform.GetChild(i).position = Vector3.Lerp(positions[i], avg, 0.5f);
            }

            _spline.RefreshControlPoints();
            EditorUtility.SetDirty(_spline);
        }

        private void OnSceneGUI()
        {
            // Allow moving control points with position handles
            for (int i = 0; i < _spline.transform.childCount; i++)
            {
                Transform child = _spline.transform.GetChild(i);

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(child.position, child.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(child, "Move Spline Point");
                    child.position = newPos;
                    _spline.RefreshControlPoints();
                }
            }
        }
    }
}
#endif
