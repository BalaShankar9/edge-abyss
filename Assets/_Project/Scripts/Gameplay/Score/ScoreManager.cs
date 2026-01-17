using System;
using UnityEngine;
using EdgeAbyss.Utils;
using EdgeAbyss.Gameplay.Riders;

namespace EdgeAbyss.Gameplay.Score
{
    /// <summary>
    /// Comprehensive scoring system with distance, speed multipliers, streaks, and edge bonuses.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject "ScoreManager" in scene.
    /// 2. Attach this component.
    /// 3. Create ScoreTuning asset and assign it.
    /// 4. Assign RiderManager reference.
    /// 5. EdgeProximitySensor should be on the rider prefab.
    /// </summary>
    public class ScoreManager : Singleton<ScoreManager>
    {
        [Header("Configuration")]
        [Tooltip("Scoring tuning parameters.")]
        [SerializeField] private ScoreTuning tuning;

        [Header("References")]
        [Tooltip("The rider manager.")]
        [SerializeField] private RiderManager riderManager;

        // Core state
        private float _currentScore;
        private int _displayScore;
        private int _currentStreak;
        private bool _isScoring;

        // Distance tracking
        private Vector3 _lastPosition;
        private float _totalDistance;

        // Streak tracking
        private float _cleanRideTimer;
        private float _streakGraceTimer;
        private bool _inStreakGrace;

        // Edge proximity
        private EdgeProximitySensor _edgeSensor;
        private float _edgeBonusAccumulator;

        // Rider reference
        private IRiderController _currentRider;
        private Transform _currentRiderTransform;

        // Cached input state
        private float _lastBrakeInput;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnStreakChanged;
        public event Action OnStreakBroken;
        public event Action<float> OnEdgeBonusEarned;

        // Public properties
        /// <summary>Current total score (integer for display).</summary>
        public int CurrentScore => _displayScore;

        /// <summary>Current streak level.</summary>
        public int CurrentStreak => _currentStreak;

        /// <summary>True if actively scoring.</summary>
        public bool IsScoring => _isScoring;

        /// <summary>Total distance traveled this run.</summary>
        public float TotalDistance => _totalDistance;

        /// <summary>Current combined multiplier (speed + streak).</summary>
        public float CurrentMultiplier
        {
            get
            {
                float multiplier = 1f;

                // Speed multiplier
                if (tuning != null && tuning.enableSpeedMultiplier && _currentRider != null)
                {
                    if (_currentRider.Stability >= tuning.speedMultiplierStabilityThreshold)
                    {
                        float speedRatio = _currentRider.Speed / tuning.referenceSpeed;
                        float speedMult = 1f + Mathf.Clamp(speedRatio, 0f, tuning.maxSpeedMultiplier - 1f);
                        multiplier *= speedMult;
                    }
                }

                // Streak multiplier
                if (tuning != null && tuning.enableStreaks && _currentStreak > 0)
                {
                    multiplier *= (1f + (_currentStreak * tuning.streakBonusPerLevel));
                }

                return multiplier;
            }
        }

        /// <summary>Current edge proximity factor [0-1].</summary>
        public float EdgeProximityFactor => _edgeSensor != null ? _edgeSensor.ProximityFactor : 0f;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            if (tuning == null)
            {
                Debug.LogError($"[{nameof(ScoreManager)}] ScoreTuning is not assigned.");
            }
        }

        private void Start()
        {
            // Find rider manager if not assigned
            if (riderManager == null)
            {
                riderManager = FindFirstObjectByType<RiderManager>();
            }

            // Subscribe to rider events
            if (riderManager != null)
            {
                riderManager.OnRiderSpawned += HandleRiderSpawned;
                riderManager.OnRiderDespawned += HandleRiderDespawned;
                riderManager.OnRiderFell += HandleRiderFell;

                // Check if rider already exists
                if (riderManager.ActiveRider != null)
                {
                    HandleRiderSpawned(riderManager.ActiveRider);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (riderManager != null)
            {
                riderManager.OnRiderSpawned -= HandleRiderSpawned;
                riderManager.OnRiderDespawned -= HandleRiderDespawned;
                riderManager.OnRiderFell -= HandleRiderFell;
            }
        }

        private void Update()
        {
            if (!_isScoring || _currentRider == null || tuning == null) return;

            float deltaTime = Time.deltaTime;

            UpdateDistanceScore(deltaTime);
            UpdateStreakSystem(deltaTime);
            UpdateEdgeBonus(deltaTime);
            UpdateDisplayScore(deltaTime);
        }

        private void UpdateDistanceScore(float deltaTime)
        {
            if (_currentRiderTransform == null) return;

            Vector3 currentPos = _currentRiderTransform.position;
            float distance = Vector3.Distance(currentPos, _lastPosition);
            _lastPosition = currentPos;

            // Only score if moving above minimum speed
            if (_currentRider.Speed < tuning.minimumScoringSpeed) return;

            // Filter out teleportation (respawn)
            if (distance > 10f) return;

            _totalDistance += distance;

            // Calculate distance points with multiplier
            float points = distance * tuning.pointsPerUnit * CurrentMultiplier;
            _currentScore += points;
        }

        private void UpdateStreakSystem(float deltaTime)
        {
            if (!tuning.enableStreaks) return;

            bool isCleanRiding = IsCleanRiding();

            if (isCleanRiding)
            {
                // Reset grace timer
                _inStreakGrace = false;
                _streakGraceTimer = 0f;

                // Build streak
                _cleanRideTimer += deltaTime;

                if (_cleanRideTimer >= tuning.streakBuildTime)
                {
                    _cleanRideTimer -= tuning.streakBuildTime;
                    IncrementStreak();
                }
            }
            else
            {
                // Start or continue grace period
                if (!_inStreakGrace)
                {
                    _inStreakGrace = true;
                    _streakGraceTimer = 0f;
                }

                _streakGraceTimer += deltaTime;

                if (_streakGraceTimer >= tuning.streakGracePeriod)
                {
                    // Grace period expired - break streak
                    BreakStreak();
                }
            }
        }

        private bool IsCleanRiding()
        {
            if (_currentRider == null) return false;

            // Check stability threshold
            if (_currentRider.Stability < tuning.streakStabilityThreshold)
            {
                return false;
            }

            // Check for hard braking
            if (_lastBrakeInput >= tuning.hardBrakeThreshold)
            {
                return false;
            }

            // Must be moving
            if (_currentRider.Speed < tuning.minimumScoringSpeed)
            {
                return false;
            }

            return true;
        }

        private void UpdateEdgeBonus(float deltaTime)
        {
            if (!tuning.enableEdgeBonus || _edgeSensor == null) return;

            float proximityFactor = _edgeSensor.ProximityFactor;

            if (proximityFactor >= tuning.edgeBonusThreshold)
            {
                // Calculate edge bonus
                float bonusMultiplier = (proximityFactor - tuning.edgeBonusThreshold) / (1f - tuning.edgeBonusThreshold);
                bonusMultiplier *= tuning.edgeBonusProximityMultiplier;

                float edgePoints = tuning.edgeBonusPointsPerSecond * bonusMultiplier * deltaTime;
                _currentScore += edgePoints;
                _edgeBonusAccumulator += edgePoints;

                // Fire event periodically
                if (_edgeBonusAccumulator >= 10f)
                {
                    OnEdgeBonusEarned?.Invoke(_edgeBonusAccumulator);
                    _edgeBonusAccumulator = 0f;
                }
            }
        }

        private void UpdateDisplayScore(float deltaTime)
        {
            int targetScore = Mathf.RoundToInt(_currentScore);

            if (targetScore != _displayScore)
            {
                if (tuning.scoreDisplaySmoothTime <= 0f)
                {
                    _displayScore = targetScore;
                }
                else
                {
                    // Smooth score display
                    float diff = targetScore - _displayScore;
                    float change = diff / tuning.scoreDisplaySmoothTime * deltaTime;
                    
                    if (Mathf.Abs(change) < 1f)
                    {
                        change = Mathf.Sign(diff);
                    }

                    _displayScore += Mathf.RoundToInt(change);
                    _displayScore = Mathf.Clamp(_displayScore, 0, targetScore);
                }

                OnScoreChanged?.Invoke(_displayScore);
            }
        }

        private void IncrementStreak()
        {
            if (_currentStreak < tuning.maxStreakLevel)
            {
                _currentStreak++;
                OnStreakChanged?.Invoke(_currentStreak);
            }
        }

        private void HandleRiderSpawned(IRiderController rider)
        {
            _currentRider = rider;

            if (rider is MonoBehaviour mb)
            {
                _currentRiderTransform = mb.transform;
                _lastPosition = _currentRiderTransform.position;

                // Find edge sensor on rider
                _edgeSensor = mb.GetComponentInChildren<EdgeProximitySensor>();
            }

            StartScoring();
        }

        private void HandleRiderDespawned(IRiderController rider)
        {
            if (_currentRider == rider)
            {
                StopScoring();
                _currentRider = null;
                _currentRiderTransform = null;
                _edgeSensor = null;
            }
        }

        private void HandleRiderFall(FallReason reason)
        {
            // Apply fall penalty
            if (tuning != null)
            {
                _currentScore = Mathf.Max(0, _currentScore - tuning.fallPenalty);

                // Reduce streak
                int newStreak = Mathf.Max(0, _currentStreak - tuning.streakLossOnFall);
                if (newStreak != _currentStreak)
                {
                    _currentStreak = newStreak;
                    OnStreakChanged?.Invoke(_currentStreak);
                    OnStreakBroken?.Invoke();
                }
            }

            // Reset streak building
            _cleanRideTimer = 0f;
            _inStreakGrace = false;
            _streakGraceTimer = 0f;
        }

        /// <summary>
        /// Sets the current brake input (call from input system).
        /// </summary>
        public void SetBrakeInput(float brakeValue)
        {
            _lastBrakeInput = Mathf.Clamp01(brakeValue);
        }

        /// <summary>
        /// Starts scoring.
        /// </summary>
        public void StartScoring()
        {
            _isScoring = true;

            if (_currentRiderTransform != null)
            {
                _lastPosition = _currentRiderTransform.position;
            }
        }

        /// <summary>
        /// Stops scoring.
        /// </summary>
        public void StopScoring()
        {
            _isScoring = false;
        }

        /// <summary>
        /// Adds bonus points directly.
        /// </summary>
        public void AddScore(int points)
        {
            _currentScore += points * CurrentMultiplier;
        }

        /// <summary>
        /// Manually breaks the current streak.
        /// </summary>
        public void BreakStreak()
        {
            if (_currentStreak > 0)
            {
                _currentStreak = 0;
                _cleanRideTimer = 0f;
                _inStreakGrace = false;
                
                OnStreakChanged?.Invoke(_currentStreak);
                OnStreakBroken?.Invoke();
            }
        }

        /// <summary>
        /// Resets all scoring state for a new run.
        /// </summary>
        public void ResetRun()
        {
            _currentScore = 0f;
            _displayScore = 0;
            _currentStreak = 0;
            _totalDistance = 0f;
            _cleanRideTimer = 0f;
            _inStreakGrace = false;
            _streakGraceTimer = 0f;
            _edgeBonusAccumulator = 0f;
            _lastBrakeInput = 0f;

            if (_currentRiderTransform != null)
            {
                _lastPosition = _currentRiderTransform.position;
            }

            if (_edgeSensor != null)
            {
                _edgeSensor.Reset();
            }

            OnScoreChanged?.Invoke(_displayScore);
            OnStreakChanged?.Invoke(_currentStreak);
        }

        /// <summary>
        /// Gets the current score without multipliers applied.
        /// </summary>
        public float GetRawScore()
        {
            return _currentScore;
        }
    }
}
