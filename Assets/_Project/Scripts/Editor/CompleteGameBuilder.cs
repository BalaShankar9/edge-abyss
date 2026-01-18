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
using EdgeAbyss.UI.Menu;
using EdgeAbyss.Debug;
using TMPro;
using UnityEngine.UI;

namespace EdgeAbyss.Editor
{
    /// <summary>
    /// Complete game builder that creates ALL required scenes, prefabs, and assets.
    /// </summary>
    public static class CompleteGameBuilder
    {
        private const string TUNING_PATH = "Assets/_Project/Tuning";
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";
        private const string SCENES_PATH = "Assets/_Project/Scenes";
        private const string MATERIALS_PATH = "Assets/_Project/Materials";

        private static RiderTuning s_bikeTuning;
        private static RiderTuning s_horseTuning;
        private static ScoreTuning s_scoreTuning;
        private static CameraTuning s_cameraTuning;
        private static WindTuning s_windTuning;
        private static GameObject s_bikePrefab;
        private static GameObject s_horsePrefab;

        [MenuItem("EdgeAbyss/Build Complete Game", false, 0)]
        public static void BuildCompleteGame()
        {
            if (!EditorUtility.DisplayDialog(
                "Build Complete EdgeAbyss Game",
                "This will create:\n" +
                "• All tuning assets\n" +
                "• All rider prefabs\n" +
                "• Boot scene\n" +
                "• MainMenu scene\n" +
                "• TestTrack scene\n" +
                "• All UI elements\n\n" +
                "Existing assets will be preserved.\n\n" +
                "Continue?",
                "Build", "Cancel"))
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating directories...", 0.05f);
            CreateDirectories();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating tuning assets...", 0.1f);
            CreateTuningAssets();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating materials...", 0.2f);
            CreateMaterials();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating prefabs...", 0.3f);
            CreatePrefabs();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating Boot scene...", 0.4f);
            CreateBootScene();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating MainMenu scene...", 0.5f);
            CreateMainMenuScene();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating TestTrack scene...", 0.7f);
            CreateTestTrackScene();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Configuring build settings...", 0.9f);
            ConfigureBuildSettings();

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Build Complete!",
                "EdgeAbyss game built successfully!\n\n" +
                "To play:\n" +
                "1. Open Assets/_Project/Scenes/Boot.unity\n" +
                "2. Press Play\n\n" +
                "Or open TestTrack.unity directly for quick testing.\n\n" +
                "Controls:\n" +
                "• W/S - Accelerate/Brake\n" +
                "• A/D - Steer\n" +
                "• 1/2 - Switch Bike/Horse\n" +
                "• R - Reset\n" +
                "• Escape - Pause",
                "OK");
        }

        [MenuItem("EdgeAbyss/Quick Open/Boot Scene", false, 100)]
        public static void OpenBootScene()
        {
            OpenOrCreateScene($"{SCENES_PATH}/Boot.unity");
        }

        [MenuItem("EdgeAbyss/Quick Open/MainMenu Scene", false, 101)]
        public static void OpenMainMenuScene()
        {
            OpenOrCreateScene($"{SCENES_PATH}/MainMenu.unity");
        }

        [MenuItem("EdgeAbyss/Quick Open/TestTrack Scene", false, 102)]
        public static void OpenTestTrackScene()
        {
            OpenOrCreateScene($"{SCENES_PATH}/TestTrack.unity");
        }

        private static void OpenOrCreateScene(string path)
        {
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Scene Not Found",
                    $"{Path.GetFileName(path)} not found.\n\nRun 'Build Complete Game' first.",
                    "OK");
            }
        }

        private static void CreateDirectories()
        {
            string[] folders = {
                "Assets/_Project",
                TUNING_PATH,
                PREFABS_PATH,
                $"{PREFABS_PATH}/Riders",
                $"{PREFABS_PATH}/UI",
                SCENES_PATH,
                MATERIALS_PATH,
                "Assets/_Project/Audio",
                "Assets/_Project/Art/Placeholders"
            };

            foreach (var folder in folders)
            {
                var parts = folder.Split('/');
                var current = "";
                for (int i = 0; i < parts.Length; i++)
                {
                    var parent = current;
                    current = i == 0 ? parts[i] : current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(current))
                    {
                        AssetDatabase.CreateFolder(parent, parts[i]);
                    }
                }
            }
        }

        private static void CreateTuningAssets()
        {
            s_bikeTuning = CreateOrLoadAsset<RiderTuning>($"{TUNING_PATH}/RiderTuning_Bike.asset", tuning => {
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
            });

            s_horseTuning = CreateOrLoadAsset<RiderTuning>($"{TUNING_PATH}/RiderTuning_Horse.asset", tuning => {
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
            });

            s_scoreTuning = CreateOrLoadAsset<ScoreTuning>($"{TUNING_PATH}/ScoreTuning.asset", tuning => {
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
                tuning.enableEdgeBonus = true;
                tuning.edgeBonusPointsPerSecond = 20f;
                tuning.edgeBonusThreshold = 0.5f;
                tuning.edgeBonusProximityMultiplier = 2f;
                tuning.fallPenalty = 100;
                tuning.streakLossOnFall = 3;
                tuning.scoreDisplaySmoothTime = 0.2f;
            });

            s_cameraTuning = CreateOrLoadAsset<CameraTuning>($"{TUNING_PATH}/CameraTuning.asset", tuning => {
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
            });

            s_windTuning = CreateOrLoadAsset<WindTuning>($"{TUNING_PATH}/WindTuning.asset", tuning => {
                tuning.enableAmbientWind = true;
                tuning.baseWindDirection = Vector3.right;
                tuning.baseWindIntensity = 1.5f;
                tuning.directionVariance = 15f;
                tuning.varianceSpeed = 0.3f;
                tuning.enableGusts = true;
                tuning.gustInterval = 8f;
                tuning.gustIntervalVariance = 3f;
                tuning.gustDuration = 1.5f;
                tuning.gustIntensityMultiplier = 2f;
                tuning.lateralForceMultiplier = 0.8f;
                tuning.stabilityImpactPerIntensity = 0.015f;
                tuning.effectSmoothSpeed = 4f;
                tuning.defaultPulseFrequency = 0.5f;
                tuning.defaultZoneIntensity = 4f;
                tuning.strongWindThreshold = 5f;
            });
        }

        private static T CreateOrLoadAsset<T>(string path, System.Action<T> configure) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            configure(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void CreateMaterials()
        {
            CreateMaterial($"{MATERIALS_PATH}/Track.mat", new Color(0.25f, 0.25f, 0.3f));
            CreateMaterial($"{MATERIALS_PATH}/Wall.mat", new Color(0.5f, 0.2f, 0.2f));
            CreateMaterial($"{MATERIALS_PATH}/Ground.mat", new Color(0.1f, 0.2f, 0.1f));
            CreateMaterial($"{MATERIALS_PATH}/BikeRider.mat", new Color(0.2f, 0.6f, 1f));
            CreateMaterial($"{MATERIALS_PATH}/HorseRider.mat", new Color(0.6f, 0.4f, 0.2f));
            CreateMaterial($"{MATERIALS_PATH}/Sky.mat", new Color(0.4f, 0.6f, 0.9f));
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void CreatePrefabs()
        {
            s_bikePrefab = CreateBikeRiderPrefab();
            s_horsePrefab = CreateHorseRiderPrefab();
        }

        private static GameObject CreateBikeRiderPrefab()
        {
            string path = $"{PREFABS_PATH}/Riders/BikeRider.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var rider = new GameObject("BikeRider");

            // Body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(rider.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/BikeRider.mat");

            // Front indicator
            var front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "FrontIndicator";
            front.transform.SetParent(rider.transform);
            front.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
            front.transform.localScale = new Vector3(0.2f, 0.2f, 0.5f);
            Object.DestroyImmediate(front.GetComponent<Collider>());

            // Wheels (visual only)
            var frontWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frontWheel.name = "FrontWheel";
            frontWheel.transform.SetParent(rider.transform);
            frontWheel.transform.localPosition = new Vector3(0f, 0.3f, 0.6f);
            frontWheel.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            frontWheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Object.DestroyImmediate(frontWheel.GetComponent<Collider>());

            var backWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            backWheel.name = "BackWheel";
            backWheel.transform.SetParent(rider.transform);
            backWheel.transform.localPosition = new Vector3(0f, 0.3f, -0.4f);
            backWheel.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            backWheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Object.DestroyImmediate(backWheel.GetComponent<Collider>());

            // Physics
            var rb = rider.AddComponent<Rigidbody>();
            rb.mass = 80f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var col = rider.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1f, 0f);
            col.radius = 0.4f;
            col.height = 2f;

            // Controller
            rider.AddComponent<BikeRiderController>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(rider, path);
            Object.DestroyImmediate(rider);
            return prefab;
        }

        private static GameObject CreateHorseRiderPrefab()
        {
            string path = $"{PREFABS_PATH}/Riders/HorseRider.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var rider = new GameObject("HorseRider");

            // Body (horizontal capsule for horse body)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(rider.transform);
            body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            body.transform.localScale = new Vector3(0.8f, 0.6f, 1.4f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/HorseRider.mat");

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(rider.transform);
            head.transform.localPosition = new Vector3(0f, 1.5f, 0.9f);
            head.transform.localScale = new Vector3(0.3f, 0.4f, 0.6f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/HorseRider.mat");

            // Legs
            for (int i = 0; i < 4; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = $"Leg{i + 1}";
                leg.transform.SetParent(rider.transform);
                float x = (i % 2 == 0) ? -0.25f : 0.25f;
                float z = (i < 2) ? 0.4f : -0.4f;
                leg.transform.localPosition = new Vector3(x, 0.5f, z);
                leg.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
                Object.DestroyImmediate(leg.GetComponent<Collider>());
                leg.GetComponent<MeshRenderer>().sharedMaterial = 
                    AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/HorseRider.mat");
            }

            // Rider on top
            var riderBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            riderBody.name = "Rider";
            riderBody.transform.SetParent(rider.transform);
            riderBody.transform.localPosition = new Vector3(0f, 2f, -0.1f);
            riderBody.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
            Object.DestroyImmediate(riderBody.GetComponent<Collider>());

            // Physics
            var rb = rider.AddComponent<Rigidbody>();
            rb.mass = 500f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var col = rider.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.2f, 0f);
            col.radius = 0.5f;
            col.height = 2.5f;

            // Controller
            rider.AddComponent<HorseRiderController>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(rider, path);
            Object.DestroyImmediate(rider);
            return prefab;
        }

        private static void CreateBootScene()
        {
            string scenePath = $"{SCENES_PATH}/Boot.unity";
            if (File.Exists(scenePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var cam = new GameObject("Camera");
            cam.tag = "MainCamera";
            var camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cam.AddComponent<AudioListener>();

            // Boot Manager
            var bootManager = new GameObject("BootManager");
            bootManager.AddComponent<BootManager>();

            // Loading text
            var canvas = new GameObject("Canvas");
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            var loadingText = new GameObject("LoadingText");
            loadingText.transform.SetParent(canvas.transform, false);
            var rect = loadingText.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var tmp = loadingText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Loading...";
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateMainMenuScene()
        {
            string scenePath = $"{SCENES_PATH}/MainMenu.unity";
            if (File.Exists(scenePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Light
            var light = new GameObject("Directional Light");
            var lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Camera
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.2f, 0.3f, 0.5f);
            cam.AddComponent<AudioListener>();

            // Canvas
            var canvas = new GameObject("MainMenu Canvas");
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();

            // Title
            var title = CreateUIText(canvas.transform, "Title", "EDGE ABYSS", 72, 
                new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f));
            title.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Subtitle
            CreateUIText(canvas.transform, "Subtitle", "Ride the Edge. Master the Fall.", 24,
                new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f));

            // Buttons panel
            var buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(canvas.transform, false);
            var panelRect = buttonPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(300f, 300f);
            panelRect.anchoredPosition = new Vector2(0f, -50f);

            var vlg = buttonPanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            CreateMenuButton(buttonPanel.transform, "PlayButton", "PLAY");
            CreateMenuButton(buttonPanel.transform, "SettingsButton", "SETTINGS");
            CreateMenuButton(buttonPanel.transform, "QuitButton", "QUIT");

            // Menu Controller
            var menuController = new GameObject("MenuController");
            menuController.AddComponent<MainMenuController>();

            // Event System
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static GameObject CreateUIText(Transform parent, string name, string text, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = new Vector2(800f, 100f);
            rect.anchoredPosition = Vector2.zero;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return obj;
        }

        private static void CreateMenuButton(Transform parent, string name, string text)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            var rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250f, 50f);

            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

            var button = buttonObj.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.4f, 0.4f, 0.6f);
            button.colors = colors;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void CreateTestTrackScene()
        {
            string scenePath = $"{SCENES_PATH}/TestTrack.unity";
            if (File.Exists(scenePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f);

            var light = new GameObject("Directional Light");
            var lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(1f, 0.95f, 0.85f);
            lightComp.intensity = 1.5f;
            lightComp.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // === TRACK ===
            var trackParent = new GameObject("--- TRACK ---");

            // Main track
            var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "Track";
            track.transform.SetParent(trackParent.transform);
            track.transform.position = new Vector3(0f, -0.5f, 125f);
            track.transform.localScale = new Vector3(10f, 1f, 300f);
            track.isStatic = true;
            track.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/Track.mat");

            // Walls
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall";
            leftWall.transform.SetParent(trackParent.transform);
            leftWall.transform.position = new Vector3(-5.5f, 0.25f, 125f);
            leftWall.transform.localScale = new Vector3(1f, 1.5f, 300f);
            leftWall.isStatic = true;
            leftWall.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/Wall.mat");

            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall";
            rightWall.transform.SetParent(trackParent.transform);
            rightWall.transform.position = new Vector3(5.5f, 0.25f, 125f);
            rightWall.transform.localScale = new Vector3(1f, 1.5f, 300f);
            rightWall.isStatic = true;
            rightWall.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/Wall.mat");

            // Ground backdrop
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(trackParent.transform);
            ground.transform.position = new Vector3(0f, -10f, 125f);
            ground.transform.localScale = new Vector3(100f, 1f, 100f);
            ground.isStatic = true;
            ground.GetComponent<MeshRenderer>().sharedMaterial = 
                AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_PATH}/Ground.mat");
            Object.DestroyImmediate(ground.GetComponent<Collider>());

            // === SPAWN POINT ===
            var spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = new Vector3(0f, 0.5f, 5f);

            // === MANAGERS ===
            var managers = new GameObject("--- MANAGERS ---");

            // Input Reader
            var inputReader = new GameObject("InputReader");
            inputReader.transform.SetParent(managers.transform);
            inputReader.AddComponent<InputReader>();

            // Rider Manager
            var riderManagerObj = new GameObject("RiderManager");
            riderManagerObj.transform.SetParent(managers.transform);
            var riderManager = riderManagerObj.AddComponent<RiderManager>();

            var rmSO = new SerializedObject(riderManager);
            rmSO.FindProperty("_bikePrefab").objectReferenceValue = s_bikePrefab;
            rmSO.FindProperty("_horsePrefab").objectReferenceValue = s_horsePrefab;
            rmSO.FindProperty("_bikeTuning").objectReferenceValue = s_bikeTuning;
            rmSO.FindProperty("_horseTuning").objectReferenceValue = s_horseTuning;
            rmSO.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
            rmSO.FindProperty("_initialRiderType").enumValueIndex = 0;
            rmSO.FindProperty("_spawnOnStart").boolValue = true;
            rmSO.ApplyModifiedPropertiesWithoutUndo();

            // Score Manager
            var scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.transform.SetParent(managers.transform);
            var scoreManager = scoreManagerObj.AddComponent<ScoreManager>();

            var smSO = new SerializedObject(scoreManager);
            var tuningProp = smSO.FindProperty("tuning");
            if (tuningProp != null) tuningProp.objectReferenceValue = s_scoreTuning;
            var rmProp = smSO.FindProperty("riderManager");
            if (rmProp != null) rmProp.objectReferenceValue = riderManager;
            smSO.ApplyModifiedPropertiesWithoutUndo();

            // Rider Switcher
            var switcherObj = new GameObject("RiderSwitcher");
            switcherObj.transform.SetParent(managers.transform);
            var switcher = switcherObj.AddComponent<RiderSwitcher>();

            var switchSO = new SerializedObject(switcher);
            var rmRefProp = switchSO.FindProperty("riderManager");
            if (rmRefProp != null) rmRefProp.objectReferenceValue = riderManager;
            switchSO.ApplyModifiedPropertiesWithoutUndo();

            // Pause Manager
            var pauseObj = new GameObject("PauseManager");
            pauseObj.transform.SetParent(managers.transform);
            pauseObj.AddComponent<PauseManager>();

            // === CAMERA ===
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cameraObj.AddComponent<AudioListener>();

            var follow = cameraObj.AddComponent<SimpleFollowCamera>();
            var followSO = new SerializedObject(follow);
            var rmFollowProp = followSO.FindProperty("riderManager");
            if (rmFollowProp != null) rmFollowProp.objectReferenceValue = riderManager;
            followSO.ApplyModifiedPropertiesWithoutUndo();

            // === HUD ===
            CreateGameHUD(riderManager);

            // === PAUSE MENU ===
            CreatePauseMenu();

            // === DEBUG OVERLAY ===
            var debugOverlay = new GameObject("DebugOverlay");
            debugOverlay.AddComponent<DebugOverlay>();

            // Event System
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateGameHUD(RiderManager riderManager)
        {
            var canvas = new GameObject("HUD Canvas");
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.sortingOrder = 10;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();

            // HUD Panel (top-left)
            var panel = new GameObject("HUD Panel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(20f, -20f);
            panelRect.sizeDelta = new Vector2(350f, 180f);

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.6f);

            // HUD texts
            CreateHUDText(panel.transform, "SpeedText", "Speed: 0 m/s", new Vector2(15f, -15f));
            CreateHUDText(panel.transform, "ScoreText", "Score: 0", new Vector2(15f, -45f));
            CreateHUDText(panel.transform, "StreakText", "Streak: x0", new Vector2(15f, -75f));
            CreateHUDText(panel.transform, "StabilityText", "Stability: 100%", new Vector2(15f, -105f));
            CreateHUDText(panel.transform, "RiderText", "Rider: Bike", new Vector2(15f, -135f));

            // Controls hint
            var controlsPanel = new GameObject("ControlsPanel");
            controlsPanel.transform.SetParent(canvas.transform, false);
            var ctrlRect = controlsPanel.AddComponent<RectTransform>();
            ctrlRect.anchorMin = new Vector2(1f, 0f);
            ctrlRect.anchorMax = new Vector2(1f, 0f);
            ctrlRect.pivot = new Vector2(1f, 0f);
            ctrlRect.anchoredPosition = new Vector2(-20f, 20f);
            ctrlRect.sizeDelta = new Vector2(250f, 80f);

            var ctrlBg = controlsPanel.AddComponent<Image>();
            ctrlBg.color = new Color(0f, 0f, 0f, 0.5f);

            CreateHUDText(controlsPanel.transform, "Controls1", "W/S: Accel/Brake  A/D: Steer", new Vector2(10f, -10f), 14);
            CreateHUDText(controlsPanel.transform, "Controls2", "[1] Bike  [2] Horse  [R] Reset", new Vector2(10f, -35f), 14);
            CreateHUDText(controlsPanel.transform, "Controls3", "[Esc] Pause", new Vector2(10f, -55f), 14);

            // HUD Updater
            var updater = canvas.AddComponent<MinimalHUDUpdater>();
            var so = new SerializedObject(updater);
            so.FindProperty("riderManager").objectReferenceValue = riderManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHUDText(Transform parent, string name, string text, Vector2 position, int fontSize = 20)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(-30f, 28f);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
        }

        private static void CreatePauseMenu()
        {
            var canvas = new GameObject("Pause Canvas");
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.sortingOrder = 100;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();

            // Overlay
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvas.transform, false);
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.7f);

            // Panel
            var panel = new GameObject("PausePanel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400f, 350f);

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title
            var title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -30f);
            titleRect.sizeDelta = new Vector2(300f, 50f);

            var titleTmp = title.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "PAUSED";
            titleTmp.fontSize = 36;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;

            // Buttons
            var buttonParent = new GameObject("Buttons");
            buttonParent.transform.SetParent(panel.transform, false);
            var btnRect = buttonParent.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = new Vector2(250f, 200f);

            var vlg = buttonParent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            CreateMenuButton(buttonParent.transform, "ResumeButton", "RESUME");
            CreateMenuButton(buttonParent.transform, "RestartButton", "RESTART");
            CreateMenuButton(buttonParent.transform, "MainMenuButton", "MAIN MENU");

            // Start hidden
            canvas.SetActive(false);

            // Tag for PauseManager to find
            canvas.tag = "PauseMenu";
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene($"{SCENES_PATH}/Boot.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/TestTrack.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
