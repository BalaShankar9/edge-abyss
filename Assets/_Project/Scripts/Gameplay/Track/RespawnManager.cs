using System;
using System.Collections;
using UnityEngine;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.UI;

namespace EdgeAbyss.Gameplay.Track
{
    /// <summary>
    /// Manages respawning after falls/wipeouts.
    /// Coordinates fade, rider reset, and checkpoint restoration.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject "RespawnManager" in scene.
    /// 2. Attach this component.
    /// 3. Assign RiderManager, FallDetector, and FadeScreen references.
    /// 4. Set default spawn point for when no checkpoint has been reached.
    /// 5. Checkpoints will auto-register when triggered.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The rider manager.")]
        [SerializeField] private RiderManager riderManager;

        [Tooltip("The fall detector.")]
        [SerializeField] private FallDetector fallDetector;

        [Tooltip("UI fade screen for respawn transitions.")]
        [SerializeField] private FadeScreen fadeScreen;

        [Header("Default Spawn")]
        [Tooltip("Spawn point used before any checkpoint is reached.")]
        [SerializeField] private Transform defaultSpawnPoint;

        [Header("Respawn Settings")]
        [Tooltip("Delay before starting respawn after fall (seconds).")]
        [SerializeField] private float respawnDelay = 0.5f;

        [Tooltip("Duration of fade out (seconds).")]
        [SerializeField] private float fadeOutDuration = 0.3f;

        [Tooltip("Duration of fade in (seconds).")]
        [SerializeField] private float fadeInDuration = 0.5f;

        [Tooltip("Time to hold at black between position reset (seconds).")]
        [SerializeField] private float holdBlackDuration = 0.2f;

        [Tooltip("Stability value after respawn [0-1].")]
        [SerializeField] [Range(0.5f, 1f)] private float respawnStability = 0.8f;

        // Checkpoint tracking
        private Checkpoint _lastCheckpoint;
        private int _highestCheckpointIndex = -1;

        // State
        private bool _isRespawning;
        private Coroutine _respawnCoroutine;

        /// <summary>Event fired when respawn starts.</summary>
        public event Action OnRespawnStarted;

        /// <summary>Event fired when respawn completes.</summary>
        public event Action OnRespawnCompleted;

        /// <summary>The last checkpoint reached by the rider.</summary>
        public Checkpoint LastCheckpoint => _lastCheckpoint;

        /// <summary>True if currently in the respawn sequence.</summary>
        public bool IsRespawning => _isRespawning;

        private void OnEnable()
        {
            if (fallDetector != null)
            {
                fallDetector.OnFallDetected += HandleFallDetected;
            }

            if (riderManager != null)
            {
                riderManager.OnRiderSpawned += HandleRiderSpawned;
                riderManager.OnRiderFell += HandleRiderFell;
            }
        }

        private void OnDisable()
        {
            if (fallDetector != null)
            {
                fallDetector.OnFallDetected -= HandleFallDetected;
            }

            if (riderManager != null)
            {
                riderManager.OnRiderSpawned -= HandleRiderSpawned;
                riderManager.OnRiderFell -= HandleRiderFell;
            }
        }

        /// <summary>
        /// Registers that a checkpoint was reached.
        /// Called by CheckpointTrigger when rider enters checkpoint.
        /// </summary>
        public void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null) return;

            // Only update if this is a higher checkpoint (prevents going backwards)
            if (checkpoint.CheckpointIndex > _highestCheckpointIndex)
            {
                _highestCheckpointIndex = checkpoint.CheckpointIndex;
                _lastCheckpoint = checkpoint;
            }
        }

        /// <summary>
        /// Forces an immediate respawn.
        /// </summary>
        public void ForceRespawn()
        {
            if (_isRespawning) return;
            StartRespawnSequence(FallReason.ExternalForce);
        }

        /// <summary>
        /// Resets checkpoint progress (e.g., at race start).
        /// </summary>
        public void ResetCheckpoints()
        {
            _lastCheckpoint = null;
            _highestCheckpointIndex = -1;
        }

        private void HandleFallDetected(FallReason reason)
        {
            StartRespawnSequence(reason);
        }

        private void HandleRiderFell(FallReason reason)
        {
            StartRespawnSequence(reason);
        }

        private void HandleRiderSpawned(IRiderController rider)
        {
            // Start monitoring the new rider
            if (fallDetector != null && rider is MonoBehaviour mb)
            {
                fallDetector.StartMonitoring(mb.transform, rider);
            }
        }

        private void StartRespawnSequence(FallReason reason)
        {
            if (_isRespawning) return;

            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
            }

            _respawnCoroutine = StartCoroutine(RespawnSequence(reason));
        }

        private IEnumerator RespawnSequence(FallReason reason)
        {
            _isRespawning = true;
            OnRespawnStarted?.Invoke();

            // Initial delay
            yield return new WaitForSeconds(respawnDelay);

            // Fade to black
            if (fadeScreen != null)
            {
                fadeScreen.FadeOut(fadeOutDuration);
                yield return new WaitForSeconds(fadeOutDuration);
            }

            // Get respawn position
            Vector3 respawnPos;
            Quaternion respawnRot;
            float stability;

            if (_lastCheckpoint != null)
            {
                respawnPos = _lastCheckpoint.RespawnPosition;
                respawnRot = _lastCheckpoint.RespawnRotation;
                stability = _lastCheckpoint.RespawnStability;
            }
            else if (defaultSpawnPoint != null)
            {
                respawnPos = defaultSpawnPoint.position + Vector3.up * 0.5f;
                respawnRot = defaultSpawnPoint.rotation;
                stability = respawnStability;
            }
            else
            {
                Debug.LogError($"[{nameof(RespawnManager)}] No checkpoint or default spawn point available!");
                respawnPos = Vector3.zero + Vector3.up * 2f;
                respawnRot = Quaternion.identity;
                stability = respawnStability;
            }

            // Reset the rider
            ResetRider(respawnPos, respawnRot);

            // Reset fall detector state
            if (fallDetector != null)
            {
                fallDetector.ResetFallState();
            }

            // Hold at black briefly
            yield return new WaitForSeconds(holdBlackDuration);

            // Fade back in
            if (fadeScreen != null)
            {
                fadeScreen.FadeIn(fadeInDuration);
                yield return new WaitForSeconds(fadeInDuration);
            }

            _isRespawning = false;
            _respawnCoroutine = null;
            OnRespawnCompleted?.Invoke();
        }

        private void ResetRider(Vector3 position, Quaternion rotation)
        {
            if (riderManager == null || riderManager.ActiveRider == null) return;

            IRiderController rider = riderManager.ActiveRider;
            rider.ResetRider(position, rotation);
        }

        /// <summary>
        /// Sets the fall detector reference at runtime.
        /// </summary>
        public void SetFallDetector(FallDetector detector)
        {
            if (fallDetector != null)
            {
                fallDetector.OnFallDetected -= HandleFallDetected;
            }

            fallDetector = detector;

            if (fallDetector != null)
            {
                fallDetector.OnFallDetected += HandleFallDetected;
            }
        }
    }
}
