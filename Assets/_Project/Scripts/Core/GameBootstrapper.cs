using UnityEngine;
using EdgeAbyss.Utils;

namespace EdgeAbyss.Core
{
    /// <summary>
    /// Entry point for the game. Attach to a GameObject in the Boot scene.
    /// Loads the Menu scene on start and persists across scene loads.
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Create a new scene called "Boot" and add it to Build Settings as index 0.
    /// 2. Create an empty GameObject, name it "GameBootstrapper".
    /// 3. Attach this script to the GameBootstrapper GameObject.
    /// 4. Create a GameConfig asset (see GameConfig.cs for instructions).
    /// 5. Assign the GameConfig asset to the "Config" field in the Inspector.
    /// 6. Create a "Menu" scene and add it to Build Settings.
    /// 7. Optionally, create a prefab from GameBootstrapper and save to Assets/_Project/Prefabs/.
    /// </summary>
    public class GameBootstrapper : Singleton<GameBootstrapper>
    {
        [Header("Configuration")]
        [Tooltip("Reference to the GameConfig ScriptableObject. Required.")]
        [SerializeField] private GameConfig config;

        /// <summary>
        /// Provides global access to the game configuration.
        /// </summary>
        public static GameConfig Config => Instance != null ? Instance.config : null;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            Initialize();
        }

        private void Initialize()
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            LoadMenuScene();
        }

        private bool ValidateConfiguration()
        {
            if (config == null)
            {
                Debug.LogError($"[{nameof(GameBootstrapper)}] GameConfig is not assigned. Please assign a GameConfig asset in the Inspector.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.MenuSceneName))
            {
                Debug.LogError($"[{nameof(GameBootstrapper)}] Menu scene name is not configured in GameConfig.");
                return false;
            }

            return true;
        }

        private void LoadMenuScene()
        {
            StartCoroutine(SceneLoader.LoadSceneAsync(
                config.MenuSceneName,
                additive: false,
                onProgress: null,
                onComplete: OnMenuSceneLoaded
            ));
        }

        private void OnMenuSceneLoaded()
        {
            // Menu scene loaded successfully - no log needed for normal operation
        }
    }
}
