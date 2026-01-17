using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Score;
using EdgeAbyss.Gameplay.Track;
using EdgeAbyss.Gameplay.Modes;
using EdgeAbyss.Persistence;

namespace EdgeAbyss.Gameplay.Run
{
    /// <summary>
    /// Manages a single gameplay run from start to finish.
    /// Coordinates between rider, score, time trial, and results.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject "RunManager" in gameplay scene.
    /// 2. Attach this component.
    /// 3. Assign references to RiderManager, ScoreManager, etc.
    /// 4. Configure track ID and game mode.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RiderManager riderManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private RespawnManager respawnManager;
        [SerializeField] private TimeTrialManager timeTrialManager;

        [Header("Run Settings")]
        [SerializeField] private string trackId = "Level_01";
        [SerializeField] private GameMode gameMode = GameMode.Story;

        [Header("Countdown")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private bool freezePlayerDuringCountdown = true;

        [Header("Results")]
        [SerializeField] private string resultsSceneName = "Results";
        [SerializeField] private float delayBeforeResults = 1.5f;

        // State
        private RunState _state = RunState.NotStarted;
        private float _runStartTime;
        private float _runEndTime;
        private int _fallCount;
        private int _respawnCount;
        private float _maxSpeed;
        private float _totalSpeedAccum;
        private int _speedSamples;
        private RunResults _results;

        // Events
        public event Action<RunState> OnStateChanged;
        public event Action<float> OnCountdownTick;
        public event Action OnRunStarted;
        public event Action<RunResults> OnRunFinished;

        /// <summary>Current run state.</summary>
        public RunState State => _state;

        /// <summary>Current run time in seconds.</summary>
        public float CurrentTime => _state == RunState.Running ? Time.time - _runStartTime : 0f;

        /// <summary>Current track ID.</summary>
        public string TrackId => trackId;

        /// <summary>Current game mode.</summary>
        public GameMode Mode => gameMode;

        /// <summary>Results from the last completed run.</summary>
        public RunResults Results => _results;

        /// <summary>Static reference for cross-scene access.</summary>
        public static RunResults LastResults { get; private set; }

        private void Start()
        {
            // Auto-find references if not assigned
            if (riderManager == null) riderManager = FindFirstObjectByType<RiderManager>();
            if (scoreManager == null) scoreManager = ScoreManager.Instance;
            if (respawnManager == null) respawnManager = FindFirstObjectByType<RespawnManager>();
            if (timeTrialManager == null) timeTrialManager = FindFirstObjectByType<TimeTrialManager>();

            // Subscribe to events
            if (riderManager != null)
            {
                riderManager.OnRiderFell += HandleRiderFell;
            }

            if (respawnManager != null)
            {
                respawnManager.OnRespawnCompleted += HandleRespawnCompleted;
            }
        }

        private void OnDestroy()
        {
            if (riderManager != null)
            {
                riderManager.OnRiderFell -= HandleRiderFell;
            }

            if (respawnManager != null)
            {
                respawnManager.OnRespawnCompleted -= HandleRespawnCompleted;
            }
        }

        private void Update()
        {
            if (_state == RunState.Running)
            {
                TrackPerformanceStats();
            }
        }

        /// <summary>
        /// Starts a new run with countdown.
        /// </summary>
        public void StartRun()
        {
            if (_state != RunState.NotStarted && _state != RunState.Finished)
            {
                Debug.LogWarning("[RunManager] Cannot start run in current state.");
                return;
            }

            StartCoroutine(RunSequence());
        }

        /// <summary>
        /// Finishes the run (call when crossing finish line).
        /// </summary>
        public void FinishRun()
        {
            if (_state != RunState.Running)
            {
                Debug.LogWarning("[RunManager] Cannot finish - not running.");
                return;
            }

            CompleteRun(RunEndReason.FinishedTrack);
        }

        /// <summary>
        /// Quits the run early.
        /// </summary>
        public void QuitRun()
        {
            if (_state == RunState.NotStarted || _state == RunState.Finished)
            {
                return;
            }

            CompleteRun(RunEndReason.PlayerQuit);
        }

        /// <summary>
        /// Restarts the current run.
        /// </summary>
        public void RestartRun()
        {
            StopAllCoroutines();
            SetState(RunState.NotStarted);
            ResetRunState();
            StartRun();
        }

        /// <summary>
        /// Sets the track and mode for this run.
        /// </summary>
        public void Configure(string trackId, GameMode mode)
        {
            this.trackId = trackId;
            this.gameMode = mode;
        }

        private IEnumerator RunSequence()
        {
            ResetRunState();
            SetState(RunState.Countdown);

            // Countdown
            float countdown = countdownDuration;
            while (countdown > 0)
            {
                OnCountdownTick?.Invoke(countdown);
                yield return new WaitForSeconds(1f);
                countdown -= 1f;
            }

            OnCountdownTick?.Invoke(0f);

            // Start!
            _runStartTime = Time.time;
            SetState(RunState.Running);

            // Initialize systems
            scoreManager?.ResetRun();
            scoreManager?.StartScoring();
            respawnManager?.ResetCheckpoints();

            // If time trial, let it handle its own timing
            if (gameMode == GameMode.TimeTrial && timeTrialManager != null)
            {
                timeTrialManager.StartTrial();
            }

            OnRunStarted?.Invoke();
        }

        private void CompleteRun(RunEndReason reason)
        {
            _runEndTime = Time.time;
            SetState(RunState.Finishing);

            // Stop scoring
            scoreManager?.StopScoring();

            // Build results
            _results = BuildResults(reason);
            LastResults = _results;

            // Save records
            SaveRecords(_results);

            SetState(RunState.Finished);
            OnRunFinished?.Invoke(_results);

            // Transition to results screen
            if (!string.IsNullOrEmpty(resultsSceneName))
            {
                StartCoroutine(TransitionToResults());
            }
        }

        private IEnumerator TransitionToResults()
        {
            yield return new WaitForSeconds(delayBeforeResults);
            SceneManager.LoadScene(resultsSceneName);
        }

        private RunResults BuildResults(RunEndReason reason)
        {
            float runTime = _runEndTime - _runStartTime;
            int score = scoreManager?.CurrentScore ?? 0;
            int streak = scoreManager?.CurrentStreak ?? 0;

            // Get previous bests
            float bestTime = SaveSystem.Instance?.GetBestTime(trackId) ?? 0f;
            int highScore = SaveSystem.Instance?.GetHighScore(trackId) ?? 0;

            var results = new RunResults
            {
                trackId = trackId,
                gameMode = gameMode,
                runTime = runTime,
                score = score,
                bestTime = bestTime,
                highScore = highScore,
                isNewBestTime = bestTime <= 0 || runTime < bestTime,
                isNewHighScore = score > highScore,
                timeDifference = bestTime > 0 ? runTime - bestTime : 0f,
                maxSpeed = _maxSpeed,
                averageSpeed = _speedSamples > 0 ? _totalSpeedAccum / _speedSamples : 0f,
                maxStreak = streak,
                totalDistance = scoreManager?.TotalDistance ?? 0f,
                fallCount = _fallCount,
                respawnCount = _respawnCount,
                completed = reason == RunEndReason.FinishedTrack,
                endReason = reason
            };

            return results;
        }

        private void SaveRecords(RunResults results)
        {
            if (SaveSystem.Instance == null) return;
            if (!results.completed) return;

            if (results.isNewBestTime)
            {
                SaveSystem.Instance.TrySetBestTime(trackId, results.runTime);
            }

            if (results.isNewHighScore)
            {
                SaveSystem.Instance.TrySetHighScore(trackId, results.score);
            }

            // Increment run counter
            SaveSystem.Instance.Progress.totalRuns++;
            SaveSystem.Instance.SaveProgress();
        }

        private void ResetRunState()
        {
            _fallCount = 0;
            _respawnCount = 0;
            _maxSpeed = 0f;
            _totalSpeedAccum = 0f;
            _speedSamples = 0;
            _results = null;
        }

        private void TrackPerformanceStats()
        {
            if (riderManager?.ActiveRider == null) return;

            float speed = riderManager.ActiveRider.Speed;
            _maxSpeed = Mathf.Max(_maxSpeed, speed);
            _totalSpeedAccum += speed;
            _speedSamples++;
        }

        private void HandleRiderFell(FallReason reason)
        {
            _fallCount++;
        }

        private void HandleRespawnCompleted()
        {
            _respawnCount++;
        }

        private void SetState(RunState newState)
        {
            if (_state == newState) return;
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }
    }

    /// <summary>
    /// Run state machine states.
    /// </summary>
    public enum RunState
    {
        NotStarted,
        Countdown,
        Running,
        Finishing,
        Finished
    }
}
