using UnityEngine;
using UnityEditor;
using System.IO;

namespace EdgeAbyss.Editor
{
    /// <summary>
    /// Auto-builds the game on first load if required assets are missing.
    /// This ensures the game is always playable without manual intervention.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoGameBuilder
    {
        private const string AUTO_BUILD_KEY = "EdgeAbyss_AutoBuildComplete";
        private const string SCENES_PATH = "Assets/_Project/Scenes";
        private const string TUNING_PATH = "Assets/_Project/Tuning";

        static AutoGameBuilder()
        {
            // Delay to let Unity finish loading
            EditorApplication.delayCall += CheckAndBuild;
        }

        private static void CheckAndBuild()
        {
            // Check if essential assets exist
            bool scenesExist = File.Exists($"{SCENES_PATH}/TestTrack.unity");
            bool tuningExists = File.Exists($"{TUNING_PATH}/RiderTuning_Bike.asset");
            bool prefabsExist = File.Exists("Assets/_Project/Prefabs/Riders/BikeRider.prefab");

            if (!scenesExist || !tuningExists || !prefabsExist)
            {
                Debug.Log("[EdgeAbyss] Missing game assets detected. Auto-building...");
                
                // Show a non-blocking notification
                EditorUtility.DisplayDialog(
                    "EdgeAbyss Auto-Build",
                    "Game assets are missing. Building now...\n\n" +
                    "This will create all required scenes, prefabs, and tuning assets.",
                    "OK");

                // Build the game
                BuildGameSilent();
            }
            else
            {
                Debug.Log("[EdgeAbyss] Game assets found. Ready to play!");
            }
        }

        /// <summary>
        /// Builds the game without confirmation dialog.
        /// </summary>
        [MenuItem("EdgeAbyss/Force Rebuild (No Confirm)", false, 10)]
        public static void BuildGameSilent()
        {
            Debug.Log("[EdgeAbyss] Starting silent build...");

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating directories...", 0.05f);
            CreateDirectories();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating tuning assets...", 0.1f);
            CreateAllTuningAssets();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating materials...", 0.2f);
            CreateAllMaterials();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating prefabs...", 0.3f);
            CreateAllPrefabs();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Creating scenes...", 0.5f);
            CreateAllScenes();

            EditorUtility.DisplayProgressBar("Building EdgeAbyss", "Configuring build settings...", 0.9f);
            ConfigureBuildSettings();

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EdgeAbyss] Build complete! Open TestTrack.unity and press Play.");
            
            // Auto-open TestTrack scene
            if (File.Exists($"{SCENES_PATH}/TestTrack.unity"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene($"{SCENES_PATH}/TestTrack.unity");
            }
        }

        private static void CreateDirectories()
        {
            string[] folders = {
                "Assets/_Project",
                "Assets/_Project/Tuning",
                "Assets/_Project/Prefabs",
                "Assets/_Project/Prefabs/Riders",
                "Assets/_Project/Prefabs/UI",
                "Assets/_Project/Scenes",
                "Assets/_Project/Materials",
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

        private static void CreateAllTuningAssets()
        {
            CompleteGameBuilder.CreateTuningAssets();
        }

        private static void CreateAllMaterials()
        {
            CompleteGameBuilder.CreateMaterials();
        }

        private static void CreateAllPrefabs()
        {
            CompleteGameBuilder.CreatePrefabs();
        }

        private static void CreateAllScenes()
        {
            CompleteGameBuilder.CreateBootScene();
            CompleteGameBuilder.CreateMainMenuScene();
            CompleteGameBuilder.CreateTestTrackScene();
        }

        private static void ConfigureBuildSettings()
        {
            CompleteGameBuilder.ConfigureBuildSettings();
        }
    }
}
