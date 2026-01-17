using System;
using System.Collections;
using UnityEngine;
using EdgeAbyss.Gameplay.Ghost;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Track;
using EdgeAbyss.Gameplay.Score;

namespace EdgeAbyss.Gameplay.Modes
{
    /// <summary>
    /// Manages Time Trial mode including timing, ghost recording/playback, and results.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject "TimeTrialManager" in scene.
    /// 2. Attach this component.
    /// 3. Assign references to RiderManager, RespawnManager, ScoreManager.
    /// 4. Assign ghost prefab (visual ghost bike).
    /// 5. Set track ID for ghost file naming.
    /// 
    /// FLOW:
    /// 1. StartTrial() - Begins countdown and race
    /// 2. Player races through track
    /// 3. FinishTrial() - Called when crossing finish line
    /// 4. Results shown, ghost saved if personal best
    /// </summary>
    public class TimeTrialManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The rider manager.")]
        [SerializeField] private RiderManager riderManager;

        [Tooltip("The respawn manager.")]
        [SerializeField] private RespawnManager respawnManager;

        [Tooltip("The score manager (optional).")]
        [SerializeField] private ScoreManager scoreManager;

        [Header("Ghost System")]
        [Tooltip("Prefab for ghost bike visualization.")]
        [SerializeField] private GameObject ghostPrefab;

        [Tooltip("Parent transform for spawned ghost.")]
        [SerializeField] private Transform ghostParent;

        [Header("Track Settings")]
        [Tooltip("Unique identifier for this track (for ghost saves).")]
        [SerializeField] private string trackId = "Level_01";

        [Header("Countdown Settings")]
        [Tooltip("Duration of countdown before race starts.")]
        [SerializeField] private float countdownDuration = 3f;

        [Tooltip("Freeze rider during countdown.")]
        [SerializeField] private bool freezeDuringCountdown = true;

        [Header("Settings")]
        [Tooltip("Show ghost of previous best run.")]
        [SerializeField] private bool showGhost = true;

        [Tooltip("Auto-save ghost on personal best.")]
        [SerializeField] private bool autoSaveGhost = true;

        // Components
        private GhostRecorder _recorder;
        private GhostPlayback _playback;
        private GhostRunner _ghostRunner;
        private GhostData _bestGhost;

        // State
        private TrialState _state = TrialState.Idle;
        private float _trialStartTime;
        private float _currentTime;
        private float _bestTime = float.MaxValue;
        private int _lapCount;
        private bool _hasBestTime;

        // Events
        public event Action<TrialState> OnStateChanged;
        public event Action<float> OnCountdownTick;
        public event Action OnTrialStarted;
        public event Action<TrialResult> OnTrialFinished;
        public event Action<float> OnNewBestTime;

        /// <summary>Current trial state.</summary>
        public TrialState State => _state;

        /// <summary>Current run time in seconds.</summary>
        public float CurrentTime => _currentTime;

        /// <summary>Best recorded time.</summary>
        public float BestTime => _hasBestTime ? _bestTime : float.MaxValue;

        /// <summary>True if there's a previous best time.</summary>
        public bool HasBestTime => _hasBestTime;

        /// <summary>Difference from best time (negative = ahead).</summary>
        public float TimeDifference => _hasBestTime ? _currentTime - _bestTime : 0f;

        /// <summary>Track identifier.</summary>
        public string TrackId => trackId;

        private void Awake()
        {
            // Create recorder component
            _recorder = gameObject.AddComponent<GhostRecorder>();

            // Create playback component
            _playback = gameObject.AddComponent<GhostPlayback>();
        }

        private void Start()
        {
            // Wire up recorder references via reflection/serialized field copy
            // This would normally be done in inspector, but we can use Unity's injection
            SetupRecorder();

            // Load best time and ghost
            LoadBestData();

            // Spawn ghost if available
            if (showGhost && _bestGhost != null)
            {
                SpawnGhost();
            }
        }

        private void Update()
        {
            if (_state == TrialState.Racing)
            {
                _currentTime = Time.time - _trialStartTime;
            }
        }

        /// <summary>
        /// Starts a new time trial with optional countdown.
        /// </summary>
        public void StartTrial()
        {
            if (_state != TrialState.Idle && _state != TrialState.Finished)
            {
                Debug.LogWarning("[TimeTrialManager] Cannot start trial in current state.");
                return;
            }

            StartCoroutine(TrialSequence());
        }

        /// <summary>
        /// Called when rider crosses the finish line.
        /// </summary>
        public void FinishTrial()
        {
            if (_state != TrialState.Racing)
            {
                Debug.LogWarning("[TimeTrialManager] Cannot finish trial - not racing.");
                return;
            }

            SetState(TrialState.Finishing);

            // Stop recording
            var ghostData = _recorder.StopRecording();

            // Stop ghost playback
            _playback.StopPlayback();

            // Calculate result
            float finalTime = _currentTime;
            bool isNewBest = finalTime < _bestTime;

            var result = new TrialResult
            {
                time = finalTime,
                isNewBest = isNewBest,
                previousBest = _hasBestTime ? _bestTime : -1f,
                timeDifference = _hasBestTime ? finalTime - _bestTime : 0f
            };

            // Save if new best
            if (isNewBest && autoSaveGhost && ghostData != null)
            {
                _bestTime = finalTime;
                _hasBestTime = true;
                _bestGhost = ghostData;

                SaveBestData(ghostData);
                OnNewBestTime?.Invoke(finalTime);
            }

            SetState(TrialState.Finished);
            OnTrialFinished?.Invoke(result);
        }

        /// <summary>
        /// Cancels the current trial.
        /// </summary>
        public void CancelTrial()
        {
            StopAllCoroutines();

            _recorder.CancelRecording();
            _playback.StopPlayback();

            SetState(TrialState.Idle);
        }

        /// <summary>
        /// Resets and prepares for a new trial.
        /// </summary>
        public void ResetTrial()
        {
            CancelTrial();
            _currentTime = 0f;

            // Reset ghost to start
            if (_ghostRunner != null && _bestGhost != null && _bestGhost.FrameCount > 0)
            {
                var firstFrame = _bestGhost.GetFrame(0);
                _ghostRunner.SetPositionImmediate(firstFrame.position, firstFrame.rotation);
                _ghostRunner.SetVisible(showGhost);
            }

            // Reset checkpoints
            respawnManager?.ResetCheckpoints();

            // Reset score
            scoreManager?.ResetRun();
        }

        /// <summary>
        /// Toggles ghost visibility.
        /// </summary>
        public void SetGhostVisible(bool visible)
        {
            showGhost = visible;
            if (_ghostRunner != null)
            {
                _ghostRunner.SetVisible(visible && _state == TrialState.Racing);
            }
        }

        private IEnumerator TrialSequence()
        {
            // Reset
            ResetTrial();
            SetState(TrialState.Countdown);

            // Countdown
            float countdown = countdownDuration;
            while (countdown > 0)
            {
                OnCountdownTick?.Invoke(countdown);
                yield return new WaitForSeconds(1f);
                countdown -= 1f;
            }

            OnCountdownTick?.Invoke(0f);

            // GO!
            _trialStartTime = Time.time;
            _currentTime = 0f;

            // Start recording
            _recorder.StartRecording(trackId);

            // Start ghost playback
            if (_bestGhost != null && showGhost)
            {
                _playback.LoadGhost(_bestGhost);
                _playback.StartPlayback();
                _ghostRunner?.SetVisible(true);
            }

            // Start scoring
            scoreManager?.ResetRun();
            scoreManager?.StartScoring();

            SetState(TrialState.Racing);
            OnTrialStarted?.Invoke();
        }

        private void SetState(TrialState newState)
        {
            if (_state == newState) return;
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void SetupRecorder()
        {
            // Use serialized field injection workaround
            // In a real setup, these would be assigned in inspector
            var recorderType = _recorder.GetType();
            
            var riderManagerField = recorderType.GetField("riderManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            riderManagerField?.SetValue(_recorder, riderManager);

            var respawnManagerField = recorderType.GetField("respawnManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            respawnManagerField?.SetValue(_recorder, respawnManager);
        }

        private void SpawnGhost()
        {
            if (ghostPrefab == null)
            {
                Debug.LogWarning("[TimeTrialManager] Ghost prefab not assigned.");
                return;
            }

            // Spawn ghost
            var parent = ghostParent != null ? ghostParent : transform;
            var ghostObj = Instantiate(ghostPrefab, parent);
            ghostObj.name = "GhostRunner";

            _ghostRunner = ghostObj.GetComponent<GhostRunner>();
            if (_ghostRunner == null)
            {
                _ghostRunner = ghostObj.AddComponent<GhostRunner>();
            }

            _ghostRunner.SetPlayback(_playback);
            _ghostRunner.SetVisible(false); // Hidden until race starts

            // Set initial position
            if (_bestGhost != null && _bestGhost.FrameCount > 0)
            {
                var firstFrame = _bestGhost.GetFrame(0);
                _ghostRunner.SetPositionImmediate(firstFrame.position, firstFrame.rotation);
            }
        }

        private void LoadBestData()
        {
            // Load best time from PlayerPrefs
            string timeKey = $"BestTime_{trackId}";
            if (PlayerPrefs.HasKey(timeKey))
            {
                _bestTime = PlayerPrefs.GetFloat(timeKey);
                _hasBestTime = true;
            }

            // Load ghost from disk
            if (GhostSerializer.GhostExists(trackId))
            {
                _bestGhost = GhostSerializer.Load(trackId);
                _playback.LoadGhost(_bestGhost);
            }
        }

        private void SaveBestData(GhostData ghostData)
        {
            // Save best time
            string timeKey = $"BestTime_{trackId}";
            PlayerPrefs.SetFloat(timeKey, _bestTime);
            PlayerPrefs.Save();

            // Save ghost to disk
            GhostSerializer.Save(ghostData, trackId);

            Debug.Log($"[TimeTrialManager] New best time saved: {_bestTime:F3}s");
        }
    }

    /// <summary>
    /// Time trial state machine states.
    /// </summary>
    public enum TrialState
    {
        Idle,
        Countdown,
        Racing,
        Finishing,
        Finished
    }

    /// <summary>
    /// Result of a completed time trial.
    /// </summary>
    [Serializable]
    public struct TrialResult
    {
        public float time;
        public bool isNewBest;
        public float previousBest;
        public float timeDifference;
    }
}
