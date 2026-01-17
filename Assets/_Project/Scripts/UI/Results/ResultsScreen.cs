using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using EdgeAbyss.Gameplay.Run;
using EdgeAbyss.Core;

namespace EdgeAbyss.UI.Results
{
    /// <summary>
    /// Results screen controller displaying run statistics.
    /// Reads results from RunManager.LastResults.
    /// 
    /// SETUP:
    /// 1. Create Results scene with Canvas.
    /// 2. Attach this component to Canvas root.
    /// 3. Assign UI element references.
    /// 4. Set scene names for navigation.
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [Header("Time Display")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text bestTimeText;
        [SerializeField] private TMP_Text timeDifferenceText;
        [SerializeField] private GameObject newBestTimeIndicator;

        [Header("Score Display")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private GameObject newHighScoreIndicator;

        [Header("Grade Display")]
        [SerializeField] private TMP_Text gradeText;
        [SerializeField] private Image gradeBackground;

        [Header("Stats Display")]
        [SerializeField] private TMP_Text maxSpeedText;
        [SerializeField] private TMP_Text avgSpeedText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text maxStreakText;
        [SerializeField] private TMP_Text fallCountText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button menuButton;

        [Header("Scene Navigation")]
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private string currentLevelScene = "Level_01";
        [SerializeField] private string nextLevelScene = "Level_02";

        [Header("Grade Colors")]
        [SerializeField] private Color gradeS = new Color(1f, 0.84f, 0f); // Gold
        [SerializeField] private Color gradeA = new Color(0.4f, 0.8f, 0.4f); // Green
        [SerializeField] private Color gradeB = new Color(0.4f, 0.6f, 0.9f); // Blue
        [SerializeField] private Color gradeC = new Color(0.8f, 0.6f, 0.2f); // Orange
        [SerializeField] private Color gradeD = new Color(0.7f, 0.3f, 0.3f); // Red
        [SerializeField] private Color gradeF = new Color(0.4f, 0.4f, 0.4f); // Gray

        private RunResults _results;

        private void Start()
        {
            // Get results from static storage
            _results = RunManager.LastResults;

            if (_results == null)
            {
                Debug.LogWarning("[ResultsScreen] No results available. Using placeholder.");
                _results = CreatePlaceholderResults();
            }

            // Store current level for retry
            currentLevelScene = _results.trackId;

            DisplayResults();
            SetupButtons();
        }

        private void DisplayResults()
        {
            // Time
            if (timeText != null)
            {
                timeText.text = _results.FormattedTime;
            }

            if (bestTimeText != null)
            {
                bestTimeText.text = _results.bestTime > 0 ? _results.FormattedBestTime : "--:--.---";
            }

            if (timeDifferenceText != null)
            {
                timeDifferenceText.text = _results.FormattedTimeDifference;
                timeDifferenceText.color = _results.timeDifference < 0 ? Color.green : Color.red;
            }

            if (newBestTimeIndicator != null)
            {
                newBestTimeIndicator.SetActive(_results.isNewBestTime);
            }

            // Score
            if (scoreText != null)
            {
                scoreText.text = _results.score.ToString("N0");
            }

            if (highScoreText != null)
            {
                highScoreText.text = _results.highScore.ToString("N0");
            }

            if (newHighScoreIndicator != null)
            {
                newHighScoreIndicator.SetActive(_results.isNewHighScore);
            }

            // Grade
            if (gradeText != null)
            {
                gradeText.text = _results.Grade;
            }

            if (gradeBackground != null)
            {
                gradeBackground.color = GetGradeColor(_results.Grade);
            }

            // Stats
            if (maxSpeedText != null)
            {
                float speedKmh = _results.maxSpeed * 3.6f;
                maxSpeedText.text = $"{speedKmh:F0} km/h";
            }

            if (avgSpeedText != null)
            {
                float avgKmh = _results.averageSpeed * 3.6f;
                avgSpeedText.text = $"{avgKmh:F0} km/h";
            }

            if (distanceText != null)
            {
                distanceText.text = $"{_results.totalDistance:F0}m";
            }

            if (maxStreakText != null)
            {
                maxStreakText.text = $"x{_results.maxStreak}";
            }

            if (fallCountText != null)
            {
                fallCountText.text = _results.fallCount.ToString();
            }
        }

        private void SetupButtons()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryLevel);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(NextLevel);

                // Hide next level button if not completed or no next level
                if (!_results.completed || string.IsNullOrEmpty(nextLevelScene))
                {
                    nextLevelButton.gameObject.SetActive(false);
                }
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ReturnToMenu);
            }
        }

        /// <summary>
        /// Retries the current level.
        /// </summary>
        public void RetryLevel()
        {
            LoadScene(currentLevelScene);
        }

        /// <summary>
        /// Loads the next level.
        /// </summary>
        public void NextLevel()
        {
            if (!string.IsNullOrEmpty(nextLevelScene))
            {
                LoadScene(nextLevelScene);
            }
        }

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            LoadScene(menuSceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private Color GetGradeColor(string grade)
        {
            return grade switch
            {
                "S" => gradeS,
                "A" => gradeA,
                "B" => gradeB,
                "C" => gradeC,
                "D" => gradeD,
                _ => gradeF
            };
        }

        private RunResults CreatePlaceholderResults()
        {
            return new RunResults
            {
                trackId = "Level_01",
                gameMode = GameMode.Story,
                runTime = 45.678f,
                score = 12500,
                bestTime = 42.123f,
                highScore = 15000,
                isNewBestTime = false,
                isNewHighScore = false,
                timeDifference = 3.555f,
                maxSpeed = 25f,
                averageSpeed = 18f,
                maxStreak = 5,
                totalDistance = 850f,
                fallCount = 2,
                respawnCount = 2,
                completed = true,
                endReason = RunEndReason.FinishedTrack
            };
        }
    }
}
