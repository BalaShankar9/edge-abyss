using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace EdgeAbyss.UI.Menu
{
    /// <summary>
    /// Handles pause functionality during gameplay.
    /// Press Escape to toggle pause.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Pause Settings")]
        [SerializeField] private Key pauseKey = Key.Escape;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string currentSceneName = ""; // Leave empty to reload current

        [Header("UI References (Optional - auto-finds by tag)")]
        [SerializeField] private GameObject pauseCanvas;

        private bool _isPaused = false;
        private Button _resumeButton;
        private Button _restartButton;
        private Button _mainMenuButton;

        public bool IsPaused => _isPaused;

        public static PauseManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Find pause canvas if not assigned
            if (pauseCanvas == null)
            {
                var found = GameObject.FindWithTag("PauseMenu");
                if (found != null)
                {
                    pauseCanvas = found;
                }
            }

            // Auto-wire buttons
            if (pauseCanvas != null)
            {
                _resumeButton = FindButtonInChildren(pauseCanvas, "ResumeButton");
                _restartButton = FindButtonInChildren(pauseCanvas, "RestartButton");
                _mainMenuButton = FindButtonInChildren(pauseCanvas, "MainMenuButton");

                if (_resumeButton != null)
                    _resumeButton.onClick.AddListener(Resume);

                if (_restartButton != null)
                    _restartButton.onClick.AddListener(Restart);

                if (_mainMenuButton != null)
                    _mainMenuButton.onClick.AddListener(GoToMainMenu);

                // Ensure hidden at start
                pauseCanvas.SetActive(false);
            }

            // Ensure game is running
            _isPaused = false;
            Time.timeScale = 1f;
        }

        private Button FindButtonInChildren(GameObject parent, string name)
        {
            var transforms = parent.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == name)
                {
                    return t.GetComponent<Button>();
                }
            }
            return null;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[pauseKey].wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;

            if (pauseCanvas != null)
            {
                pauseCanvas.SetActive(true);
            }

            Debug.Log("[Pause] Game paused");
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;

            if (pauseCanvas != null)
            {
                pauseCanvas.SetActive(false);
            }

            Debug.Log("[Pause] Game resumed");
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            _isPaused = false;

            string sceneName = string.IsNullOrEmpty(currentSceneName) 
                ? SceneManager.GetActiveScene().name 
                : currentSceneName;

            Debug.Log($"[Pause] Restarting scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            _isPaused = false;

            Debug.Log("[Pause] Going to main menu...");
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnDestroy()
        {
            // Clean up singleton
            if (Instance == this)
            {
                Instance = null;
            }

            // Ensure time scale is normal
            Time.timeScale = 1f;
        }
    }
}
