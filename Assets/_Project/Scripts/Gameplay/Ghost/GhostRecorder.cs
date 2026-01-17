using System;
using UnityEngine;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Track;

namespace EdgeAbyss.Gameplay.Ghost
{
    /// <summary>
    /// Records rider position/rotation at fixed intervals for ghost playback.
    /// Handles respawn by trimming or pausing recording.
    /// 
    /// SETUP:
    /// 1. Attach to a manager GameObject in the gameplay scene.
    /// 2. Assign RiderManager and RespawnManager references.
    /// 3. TimeTrialManager controls start/stop of recording.
    /// </summary>
    public class GhostRecorder : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The rider manager.")]
        [SerializeField] private RiderManager riderManager;

        [Tooltip("The respawn manager (optional, for pause during respawn).")]
        [SerializeField] private RespawnManager respawnManager;

        [Header("Recording Settings")]
        [Tooltip("Recording interval in seconds (0.05 = 20Hz).")]
        [SerializeField] private float recordInterval = 0.05f;

        [Tooltip("Maximum recording duration in seconds (safety limit).")]
        [SerializeField] private float maxRecordingDuration = 600f; // 10 minutes

        // State
        private GhostData _currentRecording;
        private bool _isRecording;
        private bool _isPaused;
        private float _recordTimer;
        private float _runStartTime;
        private Transform _riderTransform;
        private IRiderController _riderController;

        // Events
        public event Action OnRecordingStarted;
        public event Action<GhostData> OnRecordingCompleted;
        public event Action OnRecordingPaused;
        public event Action OnRecordingResumed;

        /// <summary>True if currently recording.</summary>
        public bool IsRecording => _isRecording;

        /// <summary>True if recording is paused (e.g., during respawn).</summary>
        public bool IsPaused => _isPaused;

        /// <summary>Current recording time in seconds.</summary>
        public float CurrentRecordTime => _isRecording ? Time.time - _runStartTime : 0f;

        /// <summary>Number of frames recorded so far.</summary>
        public int RecordedFrameCount => _currentRecording?.FrameCount ?? 0;

        private void OnEnable()
        {
            if (respawnManager != null)
            {
                respawnManager.OnRespawnStarted += HandleRespawnStarted;
                respawnManager.OnRespawnCompleted += HandleRespawnCompleted;
            }
        }

        private void OnDisable()
        {
            if (respawnManager != null)
            {
                respawnManager.OnRespawnStarted -= HandleRespawnStarted;
                respawnManager.OnRespawnCompleted -= HandleRespawnCompleted;
            }
        }

        private void Update()
        {
            if (!_isRecording || _isPaused) return;

            // Safety check for max duration
            if (CurrentRecordTime >= maxRecordingDuration)
            {
                Debug.LogWarning("[GhostRecorder] Max recording duration reached.");
                StopRecording();
                return;
            }

            // Record at fixed interval
            _recordTimer += Time.deltaTime;
            while (_recordTimer >= recordInterval)
            {
                _recordTimer -= recordInterval;
                RecordFrame();
            }
        }

        /// <summary>
        /// Starts a new recording.
        /// </summary>
        public void StartRecording(string trackId)
        {
            if (_isRecording)
            {
                Debug.LogWarning("[GhostRecorder] Already recording. Stop first.");
                return;
            }

            // Get rider reference
            if (riderManager != null && riderManager.ActiveRider != null)
            {
                _riderController = riderManager.ActiveRider;
                if (_riderController is MonoBehaviour mb)
                {
                    _riderTransform = mb.transform;
                }
            }

            if (_riderTransform == null)
            {
                Debug.LogError("[GhostRecorder] No rider available to record.");
                return;
            }

            _currentRecording = new GhostData(trackId, recordInterval);
            _runStartTime = Time.time;
            _recordTimer = 0f;
            _isRecording = true;
            _isPaused = false;

            // Record initial frame immediately
            RecordFrame();

            OnRecordingStarted?.Invoke();
            Debug.Log($"[GhostRecorder] Started recording for track '{trackId}'.");
        }

        /// <summary>
        /// Stops recording and returns the ghost data.
        /// </summary>
        public GhostData StopRecording()
        {
            if (!_isRecording)
            {
                return null;
            }

            // Record final frame
            RecordFrame();

            _isRecording = false;
            _isPaused = false;

            var result = _currentRecording;
            OnRecordingCompleted?.Invoke(result);

            Debug.Log($"[GhostRecorder] Stopped recording. {result.FrameCount} frames, {result.totalTime:F2}s.");
            return result;
        }

        /// <summary>
        /// Cancels recording without returning data.
        /// </summary>
        public void CancelRecording()
        {
            _isRecording = false;
            _isPaused = false;
            _currentRecording = null;
        }

        /// <summary>
        /// Pauses recording (e.g., during respawn).
        /// </summary>
        public void PauseRecording()
        {
            if (!_isRecording || _isPaused) return;

            _isPaused = true;
            OnRecordingPaused?.Invoke();
        }

        /// <summary>
        /// Resumes recording after pause.
        /// Optionally trims frames after a certain time (for respawn rollback).
        /// </summary>
        public void ResumeRecording(float? trimAfterTime = null)
        {
            if (!_isRecording || !_isPaused) return;

            // Optionally trim frames if respawning to earlier checkpoint
            if (trimAfterTime.HasValue && _currentRecording != null)
            {
                _currentRecording.TrimAfterTime(trimAfterTime.Value);
            }

            // Update rider reference in case it changed
            if (riderManager != null && riderManager.ActiveRider != null)
            {
                _riderController = riderManager.ActiveRider;
                if (_riderController is MonoBehaviour mb)
                {
                    _riderTransform = mb.transform;
                }
            }

            _isPaused = false;
            _recordTimer = 0f; // Reset timer to avoid burst of frames

            OnRecordingResumed?.Invoke();
        }

        private void RecordFrame()
        {
            if (_riderTransform == null || _currentRecording == null) return;

            float time = Time.time - _runStartTime;

            float speed = _riderController?.Speed ?? 0f;
            
            // Get lean angle from transform Z rotation
            float leanAngle = _riderTransform.localEulerAngles.z;
            if (leanAngle > 180f) leanAngle -= 360f;

            var frame = new GhostFrame(
                time,
                _riderTransform.position,
                _riderTransform.rotation,
                speed,
                leanAngle
            );

            _currentRecording.AddFrame(frame);
        }

        private void HandleRespawnStarted()
        {
            PauseRecording();
        }

        private void HandleRespawnCompleted()
        {
            // Resume without trimming - the ghost shows the full run including mistakes
            // Alternatively, could trim to checkpoint time for "clean" ghost
            ResumeRecording();
        }
    }
}
