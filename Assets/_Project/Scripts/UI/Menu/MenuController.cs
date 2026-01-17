using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using EdgeAbyss.Core;
using EdgeAbyss.Persistence;
using EdgeAbyss.Gameplay.Run;

namespace EdgeAbyss.UI.Menu
{
    /// <summary>
    /// Main menu controller handling navigation and game mode selection.
    /// 
    /// SETUP:
    /// 1. Attach to Menu Canvas root.
    /// 2. Create UI panels for Main, Mode Select, Settings.
    /// 3. Hook up button OnClick events to public methods.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject modeSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject levelSelectPanel;

        [Header("Scene Names")]
        [SerializeField] private string storyLevelScene = "Level_01";
        #pragma warning disable CS0414 // Planned for future use
        [SerializeField] private string timeTrialScene = "Level_01_TimeTrial";
        #pragma warning restore CS0414
        [SerializeField] private string endlessScene = "Endless";

        [Header("Settings")]
        [SerializeField] private SettingsMenu settingsMenu;

        // State
        private MenuPanel _currentPanel = MenuPanel.Main;

        /// <summary>Current selected game mode.</summary>
        public GameMode SelectedMode { get; private set; } = GameMode.Story;

        /// <summary>Current selected level.</summary>
        public string SelectedLevel { get; private set; } = "Level_01";

        // Events
        public event Action<MenuPanel> OnPanelChanged;

        private void Start()
        {
            ShowPanel(MenuPanel.Main);

            // Ensure settings are loaded
            if (SaveSystem.Instance != null)
            {
                ApplySettings(SaveSystem.Instance.Settings);
            }
        }

        #region Panel Navigation

        /// <summary>
        /// Shows the main menu panel.
        /// </summary>
        public void ShowMainMenu()
        {
            ShowPanel(MenuPanel.Main);
        }

        /// <summary>
        /// Shows the mode selection panel.
        /// </summary>
        public void ShowModeSelect()
        {
            ShowPanel(MenuPanel.ModeSelect);
        }

        /// <summary>
        /// Shows the settings panel.
        /// </summary>
        public void ShowSettings()
        {
            ShowPanel(MenuPanel.Settings);
            settingsMenu?.RefreshUI();
        }

        /// <summary>
        /// Shows the level selection panel.
        /// </summary>
        public void ShowLevelSelect()
        {
            ShowPanel(MenuPanel.LevelSelect);
        }

        /// <summary>
        /// Goes back to previous panel.
        /// </summary>
        public void GoBack()
        {
            switch (_currentPanel)
            {
                case MenuPanel.ModeSelect:
                case MenuPanel.Settings:
                    ShowPanel(MenuPanel.Main);
                    break;
                case MenuPanel.LevelSelect:
                    ShowPanel(MenuPanel.ModeSelect);
                    break;
                default:
                    ShowPanel(MenuPanel.Main);
                    break;
            }
        }

        private void ShowPanel(MenuPanel panel)
        {
            // Hide all panels
            if (mainPanel != null) mainPanel.SetActive(false);
            if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);

            // Show requested panel
            switch (panel)
            {
                case MenuPanel.Main:
                    if (mainPanel != null) mainPanel.SetActive(true);
                    break;
                case MenuPanel.ModeSelect:
                    if (modeSelectPanel != null) modeSelectPanel.SetActive(true);
                    break;
                case MenuPanel.Settings:
                    if (settingsPanel != null) settingsPanel.SetActive(true);
                    break;
                case MenuPanel.LevelSelect:
                    if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
                    break;
            }

            _currentPanel = panel;
            OnPanelChanged?.Invoke(panel);
        }

        #endregion

        #region Game Mode Selection

        /// <summary>
        /// Selects Story mode and starts Level 01.
        /// </summary>
        public void StartStoryMode()
        {
            SelectedMode = GameMode.Story;
            SelectedLevel = "Level_01";
            LoadGameScene(storyLevelScene);
        }

        /// <summary>
        /// Selects Time Trial mode and shows level select.
        /// </summary>
        public void SelectTimeTrial()
        {
            SelectedMode = GameMode.TimeTrial;
            ShowLevelSelect();
        }

        /// <summary>
        /// Selects Endless mode (placeholder).
        /// </summary>
        public void SelectEndless()
        {
            SelectedMode = GameMode.Endless;
            Debug.Log("[MenuController] Endless mode selected (coming soon!)");
            // TODO: Load endless scene when implemented
            // LoadGameScene(endlessScene);
        }

        /// <summary>
        /// Starts the selected level.
        /// </summary>
        public void StartLevel(string levelId)
        {
            SelectedLevel = levelId;

            string sceneName = SelectedMode switch
            {
                GameMode.Story => levelId,
                GameMode.TimeTrial => $"{levelId}_TimeTrial",
                GameMode.Endless => endlessScene,
                _ => levelId
            };

            LoadGameScene(sceneName);
        }

        /// <summary>
        /// Quick start Level 01 in current mode.
        /// </summary>
        public void QuickStartLevel01()
        {
            StartLevel("Level_01");
        }

        #endregion

        #region Settings

        /// <summary>
        /// Applies current settings to game systems.
        /// </summary>
        public void ApplySettings(GameSettings settings)
        {
            // Apply FOV
            if (Camera.main != null)
            {
                Camera.main.fieldOfView = settings.fieldOfView;
            }

            // Apply quality
            QualitySettings.SetQualityLevel(settings.qualityLevel);

            // Apply VSync
            QualitySettings.vSyncCount = settings.enableVSync ? 1 : 0;

            // Apply audio
            AudioListener.volume = settings.masterVolume;
        }

        #endregion

        #region App Control

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            SaveSystem.Instance?.SaveAll();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Scene Loading

        private void LoadGameScene(string sceneName)
        {
            // Store selected mode for RunManager to pick up
            PlayerPrefs.SetString("SelectedLevel", SelectedLevel);
            PlayerPrefs.SetInt("SelectedMode", (int)SelectedMode);
            PlayerPrefs.Save();

            // Direct scene load
            SceneManager.LoadScene(sceneName);
        }

        #endregion
    }

    /// <summary>
    /// Menu panel types.
    /// </summary>
    public enum MenuPanel
    {
        Main,
        ModeSelect,
        Settings,
        LevelSelect
    }
}
