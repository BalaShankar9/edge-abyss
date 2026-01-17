using UnityEngine;
using EdgeAbyss.Gameplay.Score;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Input;

namespace EdgeAbyss.UI.HUD
{
    /// <summary>
    /// Binds HUD to game systems (RiderManager, ScoreManager).
    /// Updates HUD each frame with current values.
    /// 
    /// SETUP:
    /// 1. Attach to same GameObject as HUDView (or nearby).
    /// 2. Assign HUDView reference.
    /// 3. Assign RiderManager reference (or leave null to auto-find).
    /// 4. ScoreManager is auto-found via singleton.
    /// </summary>
    [RequireComponent(typeof(HUDView))]
    public class HUDBinder : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The HUD view to update. Auto-assigned if null.")]
        [SerializeField] private HUDView hudView;

        [Tooltip("The rider manager. Auto-found if null.")]
        [SerializeField] private RiderManager riderManager;

        [Header("Settings")]
        [Tooltip("Hide HUD when no active rider.")]
        [SerializeField] private bool hideWhenNoRider = true;

        private ScoreManager _scoreManager;
        private IRiderController _currentRider;
        private bool _isSubscribedToScore;

        private void Awake()
        {
            if (hudView == null)
            {
                hudView = GetComponent<HUDView>();
            }
        }

        private void Start()
        {
            // Find references if not assigned
            if (riderManager == null)
            {
                riderManager = FindFirstObjectByType<RiderManager>();
            }

            _scoreManager = ScoreManager.Instance;

            // Subscribe to events
            SubscribeToRiderManager();
            SubscribeToScoreManager();

            // Initialize display
            if (hudView != null)
            {
                hudView.ResetDisplay();
            }

            // Check if rider already exists
            if (riderManager != null && riderManager.ActiveRider != null)
            {
                OnRiderSpawned(riderManager.ActiveRider);
            }
            else if (hideWhenNoRider && hudView != null)
            {
                hudView.SetVisible(false);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromRiderManager();
            UnsubscribeFromScoreManager();
        }

        private void Update()
        {
            UpdateHUD();
            UpdateBrakeInput();
        }

        private void UpdateHUD()
        {
            if (hudView == null || _currentRider == null) return;

            // Update speed
            hudView.SetSpeed(_currentRider.Speed);

            // Update stability
            hudView.SetStability(_currentRider.Stability);
        }

        private void UpdateBrakeInput()
        {
            // Pass brake input to score manager for streak detection
            if (_scoreManager != null && InputReader.Instance != null)
            {
                _scoreManager.SetBrakeInput(InputReader.Instance.Brake);
            }
        }

        private void SubscribeToRiderManager()
        {
            if (riderManager == null) return;

            riderManager.OnRiderSpawned += OnRiderSpawned;
            riderManager.OnRiderDespawned += OnRiderDespawned;
        }

        private void UnsubscribeFromRiderManager()
        {
            if (riderManager == null) return;

            riderManager.OnRiderSpawned -= OnRiderSpawned;
            riderManager.OnRiderDespawned -= OnRiderDespawned;
        }

        private void SubscribeToScoreManager()
        {
            if (_scoreManager == null || _isSubscribedToScore) return;

            _scoreManager.OnScoreChanged += OnScoreChanged;
            _scoreManager.OnStreakChanged += OnStreakChanged;
            _isSubscribedToScore = true;

            // Initialize with current values
            if (hudView != null)
            {
                hudView.SetScore(_scoreManager.CurrentScore);
                hudView.SetStreak(_scoreManager.CurrentStreak);
            }
        }

        private void UnsubscribeFromScoreManager()
        {
            if (_scoreManager == null || !_isSubscribedToScore) return;

            _scoreManager.OnScoreChanged -= OnScoreChanged;
            _scoreManager.OnStreakChanged -= OnStreakChanged;
            _isSubscribedToScore = false;
        }

        private void OnRiderSpawned(IRiderController rider)
        {
            _currentRider = rider;

            if (hudView != null)
            {
                hudView.SetVisible(true);
                hudView.ResetDisplay();
            }

            // Start scoring when rider spawns
            if (_scoreManager != null)
            {
                _scoreManager.StartScoring();
            }
        }

        private void OnRiderDespawned(IRiderController rider)
        {
            if (_currentRider == rider)
            {
                _currentRider = null;

                if (hideWhenNoRider && hudView != null)
                {
                    hudView.SetVisible(false);
                }

                // Stop scoring when rider despawns
                if (_scoreManager != null)
                {
                    _scoreManager.StopScoring();
                }
            }
        }

        private void OnScoreChanged(int newScore)
        {
            if (hudView != null)
            {
                hudView.SetScore(newScore);
            }
        }

        private void OnStreakChanged(int newStreak)
        {
            if (hudView != null)
            {
                hudView.SetStreak(newStreak);
            }
        }

        /// <summary>
        /// Manually refresh score manager reference (e.g., after scene load).
        /// </summary>
        public void RefreshScoreManager()
        {
            UnsubscribeFromScoreManager();
            _scoreManager = ScoreManager.Instance;
            SubscribeToScoreManager();
        }
    }
}
