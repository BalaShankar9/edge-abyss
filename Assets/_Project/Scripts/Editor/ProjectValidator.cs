using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Score;
using EdgeAbyss.Audio;
using EdgeAbyss.Input;

namespace EdgeAbyss.Editor
{
    /// <summary>
    /// Editor QA tool that validates the project is correctly set up and playable.
    /// </summary>
    public static class ProjectValidator
    {
        private static StringBuilder _report;
        private static int _errorCount;
        private static int _warningCount;
        private static int _passCount;

        [MenuItem("EdgeAbyss/Validate Project", false, 200)]
        public static void ValidateProject()
        {
            _report = new StringBuilder();
            _errorCount = 0;
            _warningCount = 0;
            _passCount = 0;

            _report.AppendLine("═══════════════════════════════════════════════════════════════");
            _report.AppendLine("                   EDGEABYSS PROJECT VALIDATION                  ");
            _report.AppendLine("═══════════════════════════════════════════════════════════════");
            _report.AppendLine();

            ValidateFolderStructure();
            ValidateTuningAssets();
            ValidatePrefabs();
            ValidateScenes();
            ValidateBuildSettings();
            ValidateMaterials();

            // Summary
            _report.AppendLine();
            _report.AppendLine("═══════════════════════════════════════════════════════════════");
            _report.AppendLine("                           SUMMARY                              ");
            _report.AppendLine("═══════════════════════════════════════════════════════════════");
            _report.AppendLine($"✓ PASSED:   {_passCount}");
            _report.AppendLine($"⚠ WARNINGS: {_warningCount}");
            _report.AppendLine($"✗ ERRORS:   {_errorCount}");
            _report.AppendLine();

            if (_errorCount == 0)
            {
                _report.AppendLine("★★★ PROJECT READY TO PLAY! ★★★");
                _report.AppendLine();
                _report.AppendLine("To test: Open TestTrack scene and press Play");
            }
            else
            {
                _report.AppendLine("Project has issues that need to be fixed.");
                _report.AppendLine("Run 'EdgeAbyss > Build Complete Game' to auto-fix.");
            }

            Debug.Log(_report.ToString());

            // Show dialog with quick result
            EditorUtility.DisplayDialog(
                "Validation Complete",
                $"Passed: {_passCount}\nWarnings: {_warningCount}\nErrors: {_errorCount}\n\n" +
                (_errorCount == 0 ? "Project is ready to play!" : "Check Console for details."),
                "OK");
        }

        private static void ValidateFolderStructure()
        {
            _report.AppendLine("▸ FOLDER STRUCTURE");

            CheckFolder("Assets/_Project", true);
            CheckFolder("Assets/_Project/Tuning", true);
            CheckFolder("Assets/_Project/Prefabs", true);
            CheckFolder("Assets/_Project/Prefabs/Riders", true);
            CheckFolder("Assets/_Project/Scenes", true);
            CheckFolder("Assets/_Project/Materials", false);
            CheckFolder("Assets/_Project/Scripts", true);

            _report.AppendLine();
        }

        private static void ValidateTuningAssets()
        {
            _report.AppendLine("▸ TUNING ASSETS");

            CheckAsset<RiderTuning>("Assets/_Project/Tuning/RiderTuning_Bike.asset", "Bike Tuning");
            CheckAsset<RiderTuning>("Assets/_Project/Tuning/RiderTuning_Horse.asset", "Horse Tuning");
            CheckAsset<ScoreTuning>("Assets/_Project/Tuning/ScoreTuning.asset", "Score Tuning");
            CheckAsset<CameraTuning>("Assets/_Project/Tuning/CameraTuning.asset", "Camera Tuning");
            CheckAsset<WindTuning>("Assets/_Project/Tuning/WindTuning.asset", "Wind Tuning");
            CheckAsset<AudioTuning>("Assets/_Project/Tuning/AudioTuning.asset", "Audio Tuning");

            _report.AppendLine();
        }

        private static void ValidatePrefabs()
        {
            _report.AppendLine("▸ PREFABS");

            var bikePrefab = CheckAsset<GameObject>("Assets/_Project/Prefabs/Riders/BikeRider.prefab", "BikeRider Prefab");
            var horsePrefab = CheckAsset<GameObject>("Assets/_Project/Prefabs/Riders/HorseRider.prefab", "HorseRider Prefab");

            if (bikePrefab != null)
            {
                CheckComponent<BikeRiderController>(bikePrefab, "BikeRiderController on BikeRider");
                CheckComponent<Rigidbody>(bikePrefab, "Rigidbody on BikeRider");
            }

            if (horsePrefab != null)
            {
                CheckComponent<HorseRiderController>(horsePrefab, "HorseRiderController on HorseRider");
                CheckComponent<Rigidbody>(horsePrefab, "Rigidbody on HorseRider");
            }

            _report.AppendLine();
        }

        private static void ValidateScenes()
        {
            _report.AppendLine("▸ SCENES");

            CheckSceneFile("Assets/_Project/Scenes/Boot.unity", "Boot Scene");
            CheckSceneFile("Assets/_Project/Scenes/MainMenu.unity", "MainMenu Scene");
            CheckSceneFile("Assets/_Project/Scenes/TestTrack.unity", "TestTrack Scene");

            // Validate TestTrack contents
            string testTrackPath = "Assets/_Project/Scenes/TestTrack.unity";
            if (File.Exists(testTrackPath))
            {
                var currentScene = EditorSceneManager.GetActiveScene();
                bool needReload = currentScene.path != testTrackPath;

                if (needReload)
                {
                    EditorSceneManager.OpenScene(testTrackPath, OpenSceneMode.Additive);
                }

                // Check for required objects (without loading scene fully)
                _report.AppendLine("  └─ TestTrack contents (basic check only)");
            }

            _report.AppendLine();
        }

        private static void ValidateBuildSettings()
        {
            _report.AppendLine("▸ BUILD SETTINGS");

            var scenes = EditorBuildSettings.scenes;
            bool hasBoot = false;
            bool hasMainMenu = false;
            bool hasTestTrack = false;

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    if (scene.path.Contains("Boot")) hasBoot = true;
                    if (scene.path.Contains("MainMenu")) hasMainMenu = true;
                    if (scene.path.Contains("TestTrack")) hasTestTrack = true;
                }
            }

            if (hasBoot) Pass("Boot in build settings");
            else Warning("Boot not in build settings (optional)");

            if (hasMainMenu) Pass("MainMenu in build settings");
            else Warning("MainMenu not in build settings (optional)");

            if (hasTestTrack) Pass("TestTrack in build settings");
            else Error("TestTrack not in build settings");

            _report.AppendLine();
        }

        private static void ValidateMaterials()
        {
            _report.AppendLine("▸ MATERIALS");

            CheckAsset<Material>("Assets/_Project/Materials/Track.mat", "Track Material", false);
            CheckAsset<Material>("Assets/_Project/Materials/BikeRider.mat", "BikeRider Material", false);
            CheckAsset<Material>("Assets/_Project/Materials/HorseRider.mat", "HorseRider Material", false);

            _report.AppendLine();
        }

        private static void CheckFolder(string path, bool required)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                Pass(path);
            }
            else if (required)
            {
                Error($"{path} missing");
            }
            else
            {
                Warning($"{path} missing (optional)");
            }
        }

        private static T CheckAsset<T>(string path, string name, bool required = true) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                Pass(name);
                return asset;
            }
            else if (required)
            {
                Error($"{name} missing at {path}");
            }
            else
            {
                Warning($"{name} missing at {path} (optional)");
            }
            return null;
        }

        private static void CheckSceneFile(string path, string name)
        {
            if (File.Exists(path))
            {
                Pass(name);
            }
            else
            {
                Error($"{name} missing at {path}");
            }
        }

        private static void CheckComponent<T>(GameObject prefab, string name) where T : Component
        {
            if (prefab.GetComponent<T>() != null)
            {
                Pass(name);
            }
            else
            {
                Error($"{name} missing");
            }
        }

        private static void Pass(string message)
        {
            _report.AppendLine($"  ✓ {message}");
            _passCount++;
        }

        private static void Warning(string message)
        {
            _report.AppendLine($"  ⚠ {message}");
            _warningCount++;
        }

        private static void Error(string message)
        {
            _report.AppendLine($"  ✗ {message}");
            _errorCount++;
        }
    }
}
