using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace EdgeAbyss.UI.HUD
{
    /// <summary>
    /// Displays HUD elements: speed, stability, score, and streak.
    /// Uses efficient text updates to avoid per-frame allocations.
    /// 
    /// SETUP:
    /// 1. Create UI Canvas for HUD.
    /// 2. Create child elements (see prefab setup below).
    /// 3. Attach this component to the Canvas.
    /// 4. Assign all UI references.
    /// 
    /// PREFAB STRUCTURE:
    /// HUDCanvas (Canvas, CanvasScaler, GraphicRaycaster)
    /// ├── SpeedPanel (anchored bottom-left)
    /// │   └── SpeedText (TMP_Text or Text)
    /// ├── StabilityPanel (anchored bottom-center)
    /// │   ├── StabilityFill (Image, type=Filled)
    /// │   └── StabilityBG (Image, behind fill)
    /// └── ScorePanel (anchored top-right)
    ///     ├── ScoreText (TMP_Text or Text)
    ///     └── StreakText (TMP_Text or Text)
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        [Header("Speed Display")]
        [Tooltip("Text component for speed display.")]
        [SerializeField] private TMP_Text speedText;
        
        [Tooltip("Fallback legacy Text if TMP not used.")]
        [SerializeField] private Text speedTextLegacy;

        [Header("Stability Display")]
        [Tooltip("Filled image for stability bar.")]
        [SerializeField] private Image stabilityFillImage;

        [Tooltip("Optional stability text display.")]
        [SerializeField] private TMP_Text stabilityText;

        [Header("Score Display")]
        [Tooltip("Text component for score.")]
        [SerializeField] private TMP_Text scoreText;
        
        [Tooltip("Fallback legacy Text if TMP not used.")]
        [SerializeField] private Text scoreTextLegacy;

        [Header("Streak Display")]
        [Tooltip("Text component for streak.")]
        [SerializeField] private TMP_Text streakText;
        
        [Tooltip("Fallback legacy Text if TMP not used.")]
        [SerializeField] private Text streakTextLegacy;

        [Header("Stability Colors")]
        [SerializeField] private Color stabilityHighColor = Color.green;
        [SerializeField] private Color stabilityMidColor = Color.yellow;
        [SerializeField] private Color stabilityLowColor = Color.red;

        [Header("Settings")]
        [Tooltip("Speed conversion factor (3.6 for m/s to km/h).")]
        [SerializeField] private float speedConversionFactor = 3.6f;

        [Tooltip("Speed unit suffix.")]
        [SerializeField] private string speedUnit = "km/h";

        // StringBuilder for efficient string building (no allocations after warmup)
        private StringBuilder _speedBuilder;
        private StringBuilder _scoreBuilder;
        private StringBuilder _streakBuilder;

        // Cached values to avoid redundant updates
        private int _lastSpeedInt = -1;
        private float _lastStability = -1f;
        private int _lastScore = -1;
        private int _lastStreak = -1;

        private void Awake()
        {
            // Pre-allocate StringBuilders with expected capacity
            _speedBuilder = new StringBuilder(16);
            _scoreBuilder = new StringBuilder(16);
            _streakBuilder = new StringBuilder(16);
        }

        /// <summary>
        /// Updates the speed display.
        /// </summary>
        /// <param name="speedUnitsPerSecond">Speed in units per second.</param>
        public void SetSpeed(float speedUnitsPerSecond)
        {
            int speedDisplay = Mathf.RoundToInt(speedUnitsPerSecond * speedConversionFactor);

            // Only update if value changed
            if (speedDisplay == _lastSpeedInt) return;
            _lastSpeedInt = speedDisplay;

            _speedBuilder.Clear();
            _speedBuilder.Append(speedDisplay);
            _speedBuilder.Append(' ');
            _speedBuilder.Append(speedUnit);

            string result = _speedBuilder.ToString();

            if (speedText != null)
            {
                speedText.SetText(result);
            }
            else if (speedTextLegacy != null)
            {
                speedTextLegacy.text = result;
            }
        }

        /// <summary>
        /// Updates the stability bar display.
        /// </summary>
        /// <param name="stability">Stability value [0-1].</param>
        public void SetStability(float stability)
        {
            stability = Mathf.Clamp01(stability);

            // Only update if value changed significantly
            if (Mathf.Abs(stability - _lastStability) < 0.01f) return;
            _lastStability = stability;

            if (stabilityFillImage != null)
            {
                stabilityFillImage.fillAmount = stability;

                // Color based on stability level
                Color targetColor;
                if (stability > 0.6f)
                {
                    targetColor = Color.Lerp(stabilityMidColor, stabilityHighColor, (stability - 0.6f) / 0.4f);
                }
                else if (stability > 0.3f)
                {
                    targetColor = Color.Lerp(stabilityLowColor, stabilityMidColor, (stability - 0.3f) / 0.3f);
                }
                else
                {
                    targetColor = stabilityLowColor;
                }

                stabilityFillImage.color = targetColor;
            }

            if (stabilityText != null)
            {
                // Use TMP's efficient SetText with format
                stabilityText.SetText("{0:0}%", stability * 100f);
            }
        }

        /// <summary>
        /// Updates the score display.
        /// </summary>
        /// <param name="score">Current score.</param>
        public void SetScore(int score)
        {
            if (score == _lastScore) return;
            _lastScore = score;

            _scoreBuilder.Clear();
            FormatNumberWithCommas(_scoreBuilder, score);

            string result = _scoreBuilder.ToString();

            if (scoreText != null)
            {
                scoreText.SetText(result);
            }
            else if (scoreTextLegacy != null)
            {
                scoreTextLegacy.text = result;
            }
        }

        /// <summary>
        /// Updates the streak display.
        /// </summary>
        /// <param name="streak">Current streak level.</param>
        public void SetStreak(int streak)
        {
            if (streak == _lastStreak) return;
            _lastStreak = streak;

            if (streak <= 0)
            {
                // Hide streak when zero
                if (streakText != null) streakText.gameObject.SetActive(false);
                if (streakTextLegacy != null) streakTextLegacy.gameObject.SetActive(false);
                return;
            }

            // Show streak
            if (streakText != null) streakText.gameObject.SetActive(true);
            if (streakTextLegacy != null) streakTextLegacy.gameObject.SetActive(true);

            _streakBuilder.Clear();
            _streakBuilder.Append('x');
            _streakBuilder.Append(streak);

            string result = _streakBuilder.ToString();

            if (streakText != null)
            {
                streakText.SetText(result);
            }
            else if (streakTextLegacy != null)
            {
                streakTextLegacy.text = result;
            }
        }

        /// <summary>
        /// Shows or hides the entire HUD.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Resets all displays to default values.
        /// </summary>
        public void ResetDisplay()
        {
            _lastSpeedInt = -1;
            _lastStability = -1f;
            _lastScore = -1;
            _lastStreak = -1;

            SetSpeed(0f);
            SetStability(1f);
            SetScore(0);
            SetStreak(0);
        }

        /// <summary>
        /// Formats a number with comma separators without allocation.
        /// </summary>
        private void FormatNumberWithCommas(StringBuilder sb, int number)
        {
            if (number < 0)
            {
                sb.Append('-');
                number = -number;
            }

            if (number == 0)
            {
                sb.Append('0');
                return;
            }

            // Count digits
            int temp = number;
            int digitCount = 0;
            while (temp > 0)
            {
                digitCount++;
                temp /= 10;
            }

            // Build string with commas
            int commaCount = (digitCount - 1) / 3;
            int totalLength = digitCount + commaCount;
            
            // Pre-size the builder
            int startIndex = sb.Length;
            for (int i = 0; i < totalLength; i++)
            {
                sb.Append('0');
            }

            int pos = startIndex + totalLength - 1;
            int digitIndex = 0;

            while (number > 0)
            {
                if (digitIndex > 0 && digitIndex % 3 == 0)
                {
                    sb[pos--] = ',';
                }
                sb[pos--] = (char)('0' + (number % 10));
                number /= 10;
                digitIndex++;
            }
        }
    }
}
