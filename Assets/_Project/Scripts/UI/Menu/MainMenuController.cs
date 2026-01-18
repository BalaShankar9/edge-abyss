using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EdgeAbyss.UI.Menu
{
    /// <summary>
    /// Simple main menu controller with Play, Settings, and Quit functionality.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "TestTrack";

        [Header("Button References (Optional - auto-finds if not set)")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            // Auto-find buttons if not assigned
            if (playButton == null)
            {
                var found = FindButton("PlayButton");
                if (found != null) playButton = found;
            }

            if (settingsButton == null)
            {
                var found = FindButton("SettingsButton");
                if (found != null) settingsButton = found;
            }

            if (quitButton == null)
            {
                var found = FindButton("QuitButton");
                if (found != null) quitButton = found;
            }

            // Hook up listeners
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Ensure normal time scale
            Time.timeScale = 1f;

            // Hide settings panel initially
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private Button FindButton(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null) return obj.GetComponent<Button>();
            return null;
        }

        public void OnPlayClicked()
        {
            Debug.Log("[MainMenu] Starting game...");
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Opening settings...");
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }

        public void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Quitting game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
