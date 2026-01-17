using System;
using UnityEngine;
using EdgeAbyss.Utils;

namespace EdgeAbyss.Gameplay
{
    /// <summary>
    /// DEPRECATED: Use EdgeAbyss.Gameplay.Score.ScoreManager instead.
    /// This placeholder is kept for backward compatibility.
    /// </summary>
    [System.Obsolete("Use EdgeAbyss.Gameplay.Score.ScoreManager instead")]
    public class ScoreManagerLegacy : Singleton<ScoreManagerLegacy>
    {
        [Header("Settings")]
        [Tooltip("Points awarded per second of clean riding.")]
        [SerializeField] private int pointsPerSecond = 10;

        [Tooltip("Streak bonus multiplier per streak level.")]
        [SerializeField] private float streakMultiplier = 0.1f;

        [Tooltip("Maximum streak level.")]
        [SerializeField] private int maxStreak = 10;

        // State
        private int _currentScore;
        private int _currentStreak;
        private float _streakTimer;
        private bool _isScoring;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnStreakChanged;
        public event Action OnStreakBroken;

        /// <summary>Current total score.</summary>
        public int CurrentScore => _currentScore;

        /// <summary>Current streak level.</summary>
        public int CurrentStreak => _currentStreak;

        /// <summary>Current score multiplier based on streak.</summary>
        public float CurrentMultiplier => 1f + (_currentStreak * streakMultiplier);

        /// <summary>True if actively scoring.</summary>
        public bool IsScoring => _isScoring;

        private void Update()
        {
            if (!_isScoring) return;

            _streakTimer += Time.deltaTime;

            // Award points every second
            if (_streakTimer >= 1f)
            {
                _streakTimer -= 1f;
                int points = Mathf.RoundToInt(pointsPerSecond * CurrentMultiplier);
                AddScoreInternal(points);
            }
        }

        /// <summary>
        /// Starts scoring (call when rider is moving).
        /// </summary>
        public void StartScoring()
        {
            _isScoring = true;
        }

        /// <summary>
        /// Stops scoring (call when rider stops/falls).
        /// </summary>
        public void StopScoring()
        {
            _isScoring = false;
        }

        /// <summary>
        /// Adds points to the score.
        /// </summary>
        public void AddScore(int points)
        {
            int finalPoints = Mathf.RoundToInt(points * CurrentMultiplier);
            AddScoreInternal(finalPoints);
        }

        /// <summary>
        /// Increments the streak counter.
        /// </summary>
        public void AddStreak()
        {
            if (_currentStreak < maxStreak)
            {
                _currentStreak++;
                OnStreakChanged?.Invoke(_currentStreak);
            }
        }

        /// <summary>
        /// Breaks the current streak.
        /// </summary>
        public void BreakStreak()
        {
            if (_currentStreak > 0)
            {
                _currentStreak = 0;
                OnStreakChanged?.Invoke(_currentStreak);
                OnStreakBroken?.Invoke();
            }
        }

        /// <summary>
        /// Resets all scoring state.
        /// </summary>
        public void ResetScore()
        {
            _currentScore = 0;
            _currentStreak = 0;
            _streakTimer = 0f;
            _isScoring = false;

            OnScoreChanged?.Invoke(_currentScore);
            OnStreakChanged?.Invoke(_currentStreak);
        }

        private void AddScoreInternal(int points)
        {
            _currentScore += points;
            OnScoreChanged?.Invoke(_currentScore);
        }
    }
}
