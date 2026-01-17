using UnityEngine;

namespace EdgeAbyss.Core
{
    /// <summary>
    /// Global game configuration ScriptableObject.
    /// Contains scene names and tuning values used throughout the game.
    /// 
    /// HOW TO CREATE THIS ASSET:
    /// 1. In Unity Editor, right-click in the Project window.
    /// 2. Select: Create > EdgeAbyss > Game Config
    /// 3. Name it "GameConfig" and place it in Assets/_Project/Configs/
    /// 4. Configure the values in the Inspector.
    /// 5. Assign this asset to the GameBootstrapper component in your Boot scene.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "EdgeAbyss/Game Config", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [Header("Scene Names")]
        [Tooltip("Name of the boot/initialization scene.")]
        [SerializeField] private string bootSceneName = "Boot";
        
        [Tooltip("Name of the main menu scene.")]
        [SerializeField] private string menuSceneName = "Menu";
        
        [Tooltip("Name of the gameplay scene.")]
        [SerializeField] private string gameplaySceneName = "Gameplay";

        [Header("Camera Settings")]
        [Tooltip("Default field of view for the POV camera.")]
        [SerializeField] [Range(60f, 120f)] private float defaultFov = 75f;
        
        [Tooltip("Intensity multiplier for camera shake effects.")]
        [SerializeField] [Range(0f, 2f)] private float cameraShakeIntensity = 1f;

        [Header("Gameplay Tuning")]
        [Tooltip("Duration of the fade effect when respawning (seconds).")]
        [SerializeField] [Range(0.1f, 3f)] private float respawnFadeDuration = 0.5f;

        // Public accessors (read-only)
        public string BootSceneName => bootSceneName;
        public string MenuSceneName => menuSceneName;
        public string GameplaySceneName => gameplaySceneName;
        public float DefaultFov => defaultFov;
        public float CameraShakeIntensity => cameraShakeIntensity;
        public float RespawnFadeDuration => respawnFadeDuration;

#if UNITY_EDITOR
        /// <summary>
        /// Validates configuration values in the editor.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(menuSceneName))
            {
                Debug.LogWarning($"[{nameof(GameConfig)}] Menu scene name is empty.", this);
            }
        }
#endif
    }
}
