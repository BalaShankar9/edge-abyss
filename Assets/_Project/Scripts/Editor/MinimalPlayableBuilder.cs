using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Score;
using EdgeAbyss.Gameplay.Camera;
using EdgeAbyss.Gameplay.Environment;
using EdgeAbyss.Input;
using EdgeAbyss.UI.HUD;
using TMPro;

namespace EdgeAbyss.Editor
{
    /// <summary>
    /// One-click tool to create a minimal playable prototype.
    /// Creates tuning assets, prefabs, and a test scene.
    /// </summary>
    public static class MinimalPlayableBuilder
    {
        private const string TUNING_PATH = "Assets/_Project/Tuning";
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";
        private const string SCENES_PATH = "Assets/_Project/Scenes";

        [MenuItem("EdgeAbyss/Setup/Create Minimal Playable Prototype", false, 1)]
        public static void CreateMinimalPlayablePrototype()
        {
            if (!EditorUtility.DisplayDialog(
                "Create Minimal Playable Prototype",
                "This will create:\n" +
                "• Tuning assets (Bike, Horse, Score, Camera, Wind)\n" +
                "• Prefabs (BikeRider, HorseRider)\n" +
                "• TestTrack scene\n\n" +
                "Continue?",
                "Create", "Cancel"))
            {
                return;
            }

            CreateDirectories();
            var bikeTuning = CreateBikeTuning();
            var horseTuning = CreateHorseTuning();
            var scoreTuning = CreateScoreTuning();
            var cameraTuning = CreateCameraTuning();
            var windTuning = CreateWindTuning();

            var bikePrefab = CreateBikeRiderPrefab(bikeTuning);
            var horsePrefab = CreateHorseRiderPrefab(horseTuning);

            CreateTestTrackScene(bikePrefab, horsePrefab, bikeTuning, horseTuning, scoreTuning, cameraTuning, windTuning);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Success!",
                "Minimal playable prototype created!\n\n" +
                "Open: Assets/_Project/Scenes/TestTrack.unity\n" +
                "Press Play to test.\n\n" +
                "Controls:\n" +
                "• W/S - Throttle/Brake\n" +
                "• A/D - Steer\n" +
                "• 1 - Switch to Bike\n" +
                "• 2 - Switch to Horse\n" +
                "• R - Reset (after fall)",
                "OK");
        }

        [MenuItem("EdgeAbyss/Setup/Open Test Track Scene", false, 2)]
        public static void OpenTestTrackScene()
        {
            string scenePath = $"{SCENES_PATH}/TestTrack.unity";
            if (File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Scene Not Found",
                    "TestTrack.unity not found.\n\nRun 'Create Minimal Playable Prototype' first.",
                    "OK");
            }
        }

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder(TUNING_PATH))
                AssetDatabase.CreateFolder("Assets/_Project", "Tuning");
            if (!AssetDatabase.IsValidFolder(PREFABS_PATH))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            if (!AssetDatabase.IsValidFolder(SCENES_PATH))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        }

        private static RiderTuning CreateBikeTuning()
        {
            string path = $"{TUNING_PATH}/BikeTuning.asset";
            var existing = AssetDatabase.LoadAssetAtPath<RiderTuning>(path);
            if (existing != null) return existing;

            var tuning = ScriptableObject.CreateInstance<RiderTuning>();
            tuning.riderName = "Bike";
            tuning.maxSpeed = 35f;
            tuning.acceleration = 18f;
            tuning.brakeDeceleration = 30f;
            tuning.drag = 2.5f;
            tuning.maxTurnRate = 100f;
            tuning.steerResponse = 10f;
            tuning.highSpeedSteerFactor = 0.4f;
            tuning.stabilityRecoveryRate = 0.6f;
            tuning.fallThreshold = 0.1f;
            tuning.steerStabilityCost = 0.25f;
            tuning.focusStabilityBonus = 0.15f;
            tuning.maxLeanAngle = 30f;
            tuning.leanSpeed = 8f;
            tuning.gravityMultiplier = 1.2f;
            tuning.groundCheckDistance = 0.4f;
            tuning.autoCorrection = 0f;
            tuning.momentumInertia = 0f;
            tuning.leanTurnInfluence = 0.6f;

            AssetDatabase.CreateAsset(tuning, path);
            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return tuning;
        }

        private static RiderTuning CreateHorseTuning()
        {
            string path = $"{TUNING_PATH}/HorseTuning.asset";
            var existing = AssetDatabase.LoadAssetAtPath<RiderTuning>(path);
            if (existing != null) return existing;

            var tuning = ScriptableObject.CreateInstance<RiderTuning>();
            tuning.riderName = "Horse";
            tuning.maxSpeed = 28f;
            tuning.acceleration = 12f;
            tuning.brakeDeceleration = 20f;
            tuning.drag = 1.5f;
            tuning.maxTurnRate = 70f;
            tuning.steerResponse = 5f;
            tuning.highSpeedSteerFactor = 0.6f;
            tuning.stabilityRecoveryRate = 0.8f;
            tuning.fallThreshold = 0.08f;
            tuning.steerStabilityCost = 0.15f;
            tuning.focusStabilityBonus = 0.25f;
            tuning.maxLeanAngle = 18f;
            tuning.leanSpeed = 4f;
            tuning.gravityMultiplier = 1f;
            tuning.groundCheckDistance = 0.5f;
            tuning.autoCorrection = 0.4f;
            tuning.momentumInertia = 0.6f;
            tuning.leanTurnInfluence = 0.2f;

            AssetDatabase.CreateAsset(tuning, path);
            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return tuning;
        }

        private static ScoreTuning CreateScoreTuning()
        {
            string path = $"{TUNING_PATH}/ScoreTuning.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ScoreTuning>(path);
            if (existing != null) return existing;

            var tuning = ScriptableObject.CreateInstance<ScoreTuning>();
            tuning.pointsPerUnit = 1f;
            tuning.minimumScoringSpeed = 2f;
            tuning.enableSpeedMultiplier = true;
            tuning.referenceSpeed = 30f;
            tuning.maxSpeedMultiplier = 3f;
            tuning.speedMultiplierStabilityThreshold = 0.5f;
            tuning.enableStreaks = true;
            tuning.streakBuildTime = 3f;
            tuning.maxStreakLevel = 10;
            tuning.streakBonusPerLevel = 0.1f;
            tuning.streakStabilityThreshold = 0.4f;
            tuning.hardBrakeThreshold = 0.7f;
            tuning.streakGracePeriod = 0.3f;
            tuning.enableEdgeBonus = false; // Disable for now - no edge sensor in prototype
            tuning.fallPenalty = 100;
            tuning.streakLossOnFall = 3;
            tuning.scoreDisplaySmoothTime = 0.2f;

            AssetDatabase.CreateAsset(tuning, path);
            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return tuning;
        }

        private static CameraTuning CreateCameraTuning()
        {
            string path = $"{TUNING_PATH}/CameraTuning.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CameraTuning>(path);
            if (existing != null) return existing;

            var tuning = ScriptableObject.CreateInstance<CameraTuning>();
            tuning.baseFOV = 75f;
            tuning.maxSpeedFOVBoost = 12f;
            tuning.fovLerpSpeed = 4f;
            tuning.referenceMaxSpeed = 35f;
            tuning.maxRollAngle = 6f;
            tuning.rollLerpSpeed = 5f;
            tuning.rollMultiplier = 0.7f;
            tuning.speedShakeIntensity = 0.015f;
            tuning.speedShakeMaxSpeed = 30f;
            tuning.speedShakeFrequency = 15f;
            tuning.roughnessShakeIntensity = 0.03f;
            tuning.roughnessShakeFrequency = 25f;
            tuning.windShakeIntensity = 0.01f;
            tuning.windShakeFrequency = 8f;
            tuning.headBobAmplitude = 0.005f;
            tuning.headBobFrequency = 3f;
            tuning.headSwayAmplitude = 0.002f;
            tuning.positionFollowSpeed = 20f;
            tuning.rotationFollowSpeed = 15f;
            tuning.maxShakeOffset = 0.08f;
            tuning.maxShakeRotation = 1.5f;

            AssetDatabase.CreateAsset(tuning, path);
            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return tuning;
        }

        private static WindTuning CreateWindTuning()
        {
            string path = $"{TUNING_PATH}/WindTuning.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WindTuning>(path);
            if (existing != null) return existing;

            var tuning = ScriptableObject.CreateInstance<WindTuning>();
            tuning.enableAmbientWind = false; // Disable for prototype simplicity
            tuning.baseWindDirection = Vector3.right;
            tuning.baseWindIntensity = 1f;
            tuning.directionVariance = 10f;
            tuning.varianceSpeed = 0.2f;
            tuning.enableGusts = false;
            tuning.gustInterval = 10f;
            tuning.gustIntervalVariance = 3f;
            tuning.gustDuration = 1.5f;
            tuning.gustIntensityMultiplier = 2f;
            tuning.lateralForceMultiplier = 0.5f;
            tuning.stabilityImpactPerIntensity = 0.01f;
            tuning.effectSmoothSpeed = 4f;
            tuning.defaultPulseFrequency = 0.5f;
            tuning.defaultZoneIntensity = 3f;
            tuning.strongWindThreshold = 5f;

            AssetDatabase.CreateAsset(tuning, path);
            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return tuning;
        }

        private static GameObject CreateBikeRiderPrefab(RiderTuning tuning)
        {
            string path = $"{PREFABS_PATH}/BikeRider.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            // Create rider object
            var rider = new GameObject("BikeRider");

            // Add capsule mesh
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body";
            capsule.transform.SetParent(rider.transform);
            capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
            capsule.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            
            // Remove collider from visual (we'll add one to root)
            Object.DestroyImmediate(capsule.GetComponent<Collider>());

            // Set up a simple material color
            var renderer = capsule.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.6f, 1f); // Blue for bike
                renderer.sharedMaterial = mat;
                AssetDatabase.CreateAsset(mat, $"{PREFABS_PATH}/BikeRiderMaterial.mat");
            }

            // Add front indicator
            var front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "FrontIndicator";
            front.transform.SetParent(rider.transform);
            front.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
            front.transform.localScale = new Vector3(0.2f, 0.2f, 0.5f);
            Object.DestroyImmediate(front.GetComponent<Collider>());

            // Add Rigidbody
            var rb = rider.AddComponent<Rigidbody>();
            rb.mass = 80f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Add Capsule Collider
            var col = rider.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1f, 0f);
            col.radius = 0.4f;
            col.height = 2f;

            // Add BikeRiderController
            var controller = rider.AddComponent<BikeRiderController>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(rider, path);
            Object.DestroyImmediate(rider);

            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return prefab;
        }

        private static GameObject CreateHorseRiderPrefab(RiderTuning tuning)
        {
            string path = $"{PREFABS_PATH}/HorseRider.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            // Create rider object
            var rider = new GameObject("HorseRider");

            // Add capsule mesh (slightly larger for horse)
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body";
            capsule.transform.SetParent(rider.transform);
            capsule.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            capsule.transform.localScale = new Vector3(0.8f, 1.2f, 1.2f);
            Object.DestroyImmediate(capsule.GetComponent<Collider>());

            // Set up a simple material color
            var renderer = capsule.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.6f, 0.4f, 0.2f); // Brown for horse
                renderer.sharedMaterial = mat;
                AssetDatabase.CreateAsset(mat, $"{PREFABS_PATH}/HorseRiderMaterial.mat");
            }

            // Add front indicator (horse head)
            var front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "HeadIndicator";
            front.transform.SetParent(rider.transform);
            front.transform.localPosition = new Vector3(0f, 1.5f, 0.8f);
            front.transform.localScale = new Vector3(0.3f, 0.4f, 0.6f);
            Object.DestroyImmediate(front.GetComponent<Collider>());

            // Add Rigidbody
            var rb = rider.AddComponent<Rigidbody>();
            rb.mass = 500f; // Horse is heavier
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Add Capsule Collider
            var col = rider.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.2f, 0f);
            col.radius = 0.5f;
            col.height = 2.5f;

            // Add HorseRiderController
            var controller = rider.AddComponent<HorseRiderController>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(rider, path);
            Object.DestroyImmediate(rider);

            Debug.Log($"[MinimalPlayableBuilder] Created: {path}");
            return prefab;
        }

        private static void CreateTestTrackScene(
            GameObject bikePrefab, GameObject horsePrefab,
            RiderTuning bikeTuning, RiderTuning horseTuning,
            ScoreTuning scoreTuning, CameraTuning cameraTuning, WindTuning windTuning)
        {
            string scenePath = $"{SCENES_PATH}/TestTrack.unity";

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === LIGHTING ===
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1f;

            // Directional Light
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.5f;
            light.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // === TRACK ===
            // Main track plane (long and narrow - ridge style)
            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "Track";
            track.transform.position = new Vector3(0f, -0.5f, 100f);
            track.transform.localScale = new Vector3(8f, 1f, 250f);
            track.isStatic = true;
            var trackRenderer = track.GetComponent<MeshRenderer>();
            var trackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            trackMat.color = new Color(0.3f, 0.3f, 0.35f);
            trackRenderer.sharedMaterial = trackMat;

            // Left wall
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall";
            leftWall.transform.position = new Vector3(-4.5f, 0.5f, 100f);
            leftWall.transform.localScale = new Vector3(1f, 2f, 250f);
            leftWall.isStatic = true;
            var wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMat.color = new Color(0.5f, 0.2f, 0.2f);
            leftWall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Right wall
            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall";
            rightWall.transform.position = new Vector3(4.5f, 0.5f, 100f);
            rightWall.transform.localScale = new Vector3(1f, 2f, 250f);
            rightWall.isStatic = true;
            rightWall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Ground plane (visual backdrop)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundBackdrop";
            ground.transform.position = new Vector3(0f, -5f, 100f);
            ground.transform.localScale = new Vector3(50f, 1f, 50f);
            ground.isStatic = true;
            var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.15f, 0.25f, 0.15f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;
            Object.DestroyImmediate(ground.GetComponent<Collider>()); // Remove collider

            // === SPAWN POINT ===
            var spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = new Vector3(0f, 0.5f, 5f);
            spawnPoint.transform.rotation = Quaternion.identity;

            // === GAME MANAGERS ===
            var managers = new GameObject("--- MANAGERS ---");

            // Input Reader
            var inputReaderObj = new GameObject("InputReader");
            inputReaderObj.transform.SetParent(managers.transform);
            inputReaderObj.AddComponent<InputReader>();

            // Rider Manager
            var riderManagerObj = new GameObject("RiderManager");
            riderManagerObj.transform.SetParent(managers.transform);
            var riderManager = riderManagerObj.AddComponent<RiderManager>();

            // Set RiderManager properties via SerializedObject
            var rmSO = new SerializedObject(riderManager);
            rmSO.FindProperty("_bikePrefab").objectReferenceValue = bikePrefab;
            rmSO.FindProperty("_horsePrefab").objectReferenceValue = horsePrefab;
            rmSO.FindProperty("_bikeTuning").objectReferenceValue = bikeTuning;
            rmSO.FindProperty("_horseTuning").objectReferenceValue = horseTuning;
            rmSO.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
            rmSO.FindProperty("_initialRiderType").enumValueIndex = 0; // Bike
            rmSO.FindProperty("_spawnOnStart").boolValue = true;
            rmSO.ApplyModifiedPropertiesWithoutUndo();

            // Score Manager
            var scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.transform.SetParent(managers.transform);
            var scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
            var smSO = new SerializedObject(scoreManager);
            var tuningProp = smSO.FindProperty("tuning");
            if (tuningProp != null)
            {
                tuningProp.objectReferenceValue = scoreTuning;
            }
            var rmProp = smSO.FindProperty("riderManager");
            if (rmProp != null)
            {
                rmProp.objectReferenceValue = riderManager;
            }
            smSO.ApplyModifiedPropertiesWithoutUndo();

            // Rider Switcher (for 1/2 key switching)
            var switcherObj = new GameObject("RiderSwitcher");
            switcherObj.transform.SetParent(managers.transform);
            var switcher = switcherObj.AddComponent<RiderSwitcher>();
            var switchSO = new SerializedObject(switcher);
            var rmRefProp = switchSO.FindProperty("riderManager");
            if (rmRefProp != null)
            {
                rmRefProp.objectReferenceValue = riderManager;
            }
            switchSO.ApplyModifiedPropertiesWithoutUndo();

            // === CAMERA ===
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;
            cameraObj.AddComponent<AudioListener>();

            // Add simple follow camera script
            var follow = cameraObj.AddComponent<SimpleFollowCamera>();
            var followSO = new SerializedObject(follow);
            var rmFollowProp = followSO.FindProperty("riderManager");
            if (rmFollowProp != null)
            {
                rmFollowProp.objectReferenceValue = riderManager;
            }
            followSO.ApplyModifiedPropertiesWithoutUndo();

            // === HUD ===
            CreateMinimalHUD(riderManager);

            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MinimalPlayableBuilder] Created: {scenePath}");
        }

        private static void CreateMinimalHUD(RiderManager riderManager)
        {
            // Create Canvas
            var canvasObj = new GameObject("HUD Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create panel background
            var panel = new GameObject("HUD Panel");
            panel.transform.SetParent(canvasObj.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(20f, -20f);
            panelRect.sizeDelta = new Vector2(300f, 150f);

            var panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.5f);

            // Create texts
            CreateHUDText(panel.transform, "SpeedText", new Vector2(10f, -10f), "Speed: 0");
            CreateHUDText(panel.transform, "ScoreText", new Vector2(10f, -40f), "Score: 0");
            CreateHUDText(panel.transform, "StreakText", new Vector2(10f, -70f), "Streak: x0");
            CreateHUDText(panel.transform, "RiderText", new Vector2(10f, -100f), "Rider: Bike");
            CreateHUDText(panel.transform, "ControlsText", new Vector2(10f, -130f), "[1] Bike  [2] Horse  [R] Reset", 12);

            // Add minimal HUD updater
            var updater = canvasObj.AddComponent<MinimalHUDUpdater>();
            var so = new SerializedObject(updater);
            so.FindProperty("riderManager").objectReferenceValue = riderManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHUDText(Transform parent, string name, Vector2 position, string defaultText, int fontSize = 18)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(-20f, 25f);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
        }
    }
}
