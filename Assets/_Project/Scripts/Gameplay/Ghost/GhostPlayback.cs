using System;
using UnityEngine;

namespace EdgeAbyss.Gameplay.Ghost
{
    /// <summary>
    /// Plays back recorded ghost data with interpolation.
    /// Feeds position/rotation to GhostRunner for visualization.
    /// 
    /// SETUP:
    /// 1. Attach to a manager GameObject or let TimeTrialManager create it.
    /// 2. Call LoadGhost() with ghost data.
    /// 3. Call StartPlayback() when race starts.
    /// </summary>
    public class GhostPlayback : MonoBehaviour
    {
        [Header("Playback Settings")]
        [Tooltip("Time offset for ghost (positive = ghost is ahead).")]
        [SerializeField] private float timeOffset = 0f;

        [Tooltip("Continue looping after ghost finishes.")]
        [SerializeField] private bool loopPlayback = false;

        // State
        private GhostData _ghostData;
        private bool _isPlaying;
        private float _playbackTime;
        private float _playbackStartTime;

        // Cached interpolated values
        private Vector3 _currentPosition;
        private Quaternion _currentRotation;
        private float _currentSpeed;
        private float _currentLean;

        // Events
        public event Action OnPlaybackStarted;
        public event Action OnPlaybackFinished;
        public event Action<Vector3, Quaternion, float, float> OnFrameUpdated;

        /// <summary>True if currently playing back.</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>Current playback time.</summary>
        public float PlaybackTime => _playbackTime;

        /// <summary>Total duration of the ghost.</summary>
        public float TotalDuration => _ghostData?.totalTime ?? 0f;

        /// <summary>Current interpolated position.</summary>
        public Vector3 CurrentPosition => _currentPosition;

        /// <summary>Current interpolated rotation.</summary>
        public Quaternion CurrentRotation => _currentRotation;

        /// <summary>Current interpolated speed.</summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>Current interpolated lean angle.</summary>
        public float CurrentLean => _currentLean;

        /// <summary>True if ghost data is loaded.</summary>
        public bool HasGhost => _ghostData != null && _ghostData.FrameCount > 0;

        private void Update()
        {
            if (!_isPlaying || _ghostData == null) return;

            _playbackTime = Time.time - _playbackStartTime + timeOffset;

            // Check for end of ghost
            if (_playbackTime >= _ghostData.totalTime)
            {
                if (loopPlayback)
                {
                    // Loop back to start
                    _playbackStartTime = Time.time + timeOffset;
                    _playbackTime = 0f;
                }
                else
                {
                    StopPlayback();
                    return;
                }
            }

            UpdateInterpolation();
        }

        /// <summary>
        /// Loads ghost data for playback.
        /// </summary>
        public void LoadGhost(GhostData data)
        {
            _ghostData = data;
            _playbackTime = 0f;
            _isPlaying = false;

            if (data != null)
            {
                Debug.Log($"[GhostPlayback] Loaded ghost: {data.FrameCount} frames, {data.totalTime:F2}s.");
            }
        }

        /// <summary>
        /// Loads ghost from disk for the given track.
        /// </summary>
        public bool LoadGhostFromDisk(string trackId)
        {
            var data = GhostSerializer.Load(trackId);
            if (data != null)
            {
                LoadGhost(data);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts playback from the beginning.
        /// </summary>
        public void StartPlayback()
        {
            if (_ghostData == null || _ghostData.FrameCount == 0)
            {
                Debug.LogWarning("[GhostPlayback] No ghost data to play.");
                return;
            }

            _playbackStartTime = Time.time;
            _playbackTime = timeOffset;
            _isPlaying = true;

            OnPlaybackStarted?.Invoke();
            Debug.Log("[GhostPlayback] Started playback.");
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void StopPlayback()
        {
            if (!_isPlaying) return;

            _isPlaying = false;
            OnPlaybackFinished?.Invoke();
            Debug.Log("[GhostPlayback] Finished playback.");
        }

        /// <summary>
        /// Pauses playback (for respawn sync, etc.).
        /// </summary>
        public void PausePlayback()
        {
            _isPlaying = false;
        }

        /// <summary>
        /// Resumes playback from current time.
        /// </summary>
        public void ResumePlayback()
        {
            if (_ghostData == null) return;

            // Adjust start time to maintain current playback position
            _playbackStartTime = Time.time - _playbackTime + timeOffset;
            _isPlaying = true;
        }

        /// <summary>
        /// Seeks to a specific time.
        /// </summary>
        public void SeekTo(float time)
        {
            _playbackTime = Mathf.Clamp(time, 0f, _ghostData?.totalTime ?? 0f);
            _playbackStartTime = Time.time - _playbackTime + timeOffset;
            UpdateInterpolation();
        }

        /// <summary>
        /// Sets the time offset (positive = ghost ahead).
        /// </summary>
        public void SetTimeOffset(float offset)
        {
            timeOffset = offset;
        }

        private void UpdateInterpolation()
        {
            if (_ghostData == null) return;

            if (_ghostData.GetFramesForTime(_playbackTime, out var before, out var after, out float t))
            {
                // Interpolate position and rotation
                _currentPosition = Vector3.Lerp(before.position, after.position, t);
                _currentRotation = Quaternion.Slerp(before.rotation, after.rotation, t);
                _currentSpeed = Mathf.Lerp(before.speed, after.speed, t);
                _currentLean = Mathf.Lerp(before.leanAngle, after.leanAngle, t);

                // Notify listeners
                OnFrameUpdated?.Invoke(_currentPosition, _currentRotation, _currentSpeed, _currentLean);
            }
        }

        /// <summary>
        /// Gets the time difference between current player time and ghost.
        /// Positive = player is ahead, negative = player is behind.
        /// </summary>
        public float GetTimeDifference(float playerTime)
        {
            return playerTime - _playbackTime;
        }
    }
}
