using UnityEngine;
using TMPro;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Score;

namespace EdgeAbyss.UI.HUD
{
    /// <summary>
    /// Minimal HUD updater for the test track prototype.
    /// Updates speed, score, streak, and rider type display.
    /// </summary>
    public class MinimalHUDUpdater : MonoBehaviour
    {
        [SerializeField] private RiderManager riderManager;

        private TextMeshProUGUI _speedText;
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _streakText;
        private TextMeshProUGUI _stabilityText;
        private TextMeshProUGUI _riderText;

        private ScoreManager _scoreManager;

        private void Start()
        {
            // Find text components
            _speedText = transform.Find("HUD Panel/SpeedText")?.GetComponent<TextMeshProUGUI>();
            _scoreText = transform.Find("HUD Panel/ScoreText")?.GetComponent<TextMeshProUGUI>();
            _streakText = transform.Find("HUD Panel/StreakText")?.GetComponent<TextMeshProUGUI>();
            _stabilityText = transform.Find("HUD Panel/StabilityText")?.GetComponent<TextMeshProUGUI>();
            _riderText = transform.Find("HUD Panel/RiderText")?.GetComponent<TextMeshProUGUI>();

            // Find managers
            if (riderManager == null)
            {
                riderManager = FindFirstObjectByType<RiderManager>();
            }

            _scoreManager = ScoreManager.Instance;

            // Subscribe to events
            if (riderManager != null)
            {
                riderManager.OnRiderSpawned += OnRiderChanged;
            }

            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged += OnScoreChanged;
                _scoreManager.OnStreakChanged += OnStreakChanged;
            }
        }

        private void OnDestroy()
        {
            if (riderManager != null)
            {
                riderManager.OnRiderSpawned -= OnRiderChanged;
            }

            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged -= OnScoreChanged;
                _scoreManager.OnStreakChanged -= OnStreakChanged;
            }
        }

        private void Update()
        {
            UpdateSpeedDisplay();
            UpdateStabilityDisplay();
            UpdateRiderDisplay();
        }

        private void UpdateSpeedDisplay()
        {
            if (_speedText == null || riderManager == null || riderManager.ActiveRider == null) return;

            float speed = riderManager.ActiveRider.Speed;
            _speedText.text = $"Speed: {speed:F1} m/s";
        }

        private void UpdateStabilityDisplay()
        {
            if (_stabilityText == null || riderManager == null || riderManager.ActiveRider == null) return;

            float stability = riderManager.ActiveRider.Stability;
            int percentage = Mathf.RoundToInt(stability * 100f);
            
            // Color code stability
            Color textColor = Color.white;
            if (stability < 0.3f)
                textColor = Color.red;
            else if (stability < 0.6f)
                textColor = Color.yellow;
            else
                textColor = Color.green;

            _stabilityText.color = textColor;
            _stabilityText.text = $"Stability: {percentage}%";
        }

        private void UpdateRiderDisplay()
        {
            if (_riderText == null || riderManager == null) return;

            string riderType = riderManager.CurrentRiderType.ToString();
            _riderText.text = $"Rider: {riderType}";
        }

        private void OnRiderChanged(IRiderController rider)
        {
            UpdateRiderDisplay();
        }

        private void OnScoreChanged(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }
        }

        private void OnStreakChanged(int streak)
        {
            if (_streakText != null)
            {
                _streakText.text = $"Streak: x{streak}";
            }
        }
    }
}
