using System;
using UnityEngine;
using EdgeAbyss.Input;

namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Manages rider spawning, input routing, and rider swapping.
    /// 
    /// ARCHITECTURE RULES:
    /// - RiderManager OWNS the active rider reference.
    /// - RiderManager is the ONLY class that may swap riders.
    /// - No external class should directly manipulate rider lifecycle.
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Create an empty GameObject named "RiderManager" in your gameplay scene.
    /// 2. Attach this RiderManager component.
    /// 3. Create rider prefabs (with BikeRiderController or HorseRiderController).
    /// 4. Create RiderTuning assets for each rider type.
    /// 5. Assign prefabs and tuning assets in the Inspector.
    /// 6. Set the spawn point transform.
    /// 7. Ensure InputReader exists in the scene.
    /// </summary>
    public sealed class RiderManager : MonoBehaviour
    {
        #region Rider Types

        /// <summary>
        /// Available rider types for future animal expansion.
        /// </summary>
        public enum RiderType
        {
            Bike,
            Horse
            // Future: Skateboard, Snowboard, etc.
        }

        #endregion

        #region Serialized Fields

        [Header("Rider Prefabs")]
        [Tooltip("Prefab with BikeRiderController attached.")]
        [SerializeField] private GameObject _bikePrefab;

        [Tooltip("Prefab with HorseRiderController attached.")]
        [SerializeField] private GameObject _horsePrefab;

        [Header("Tuning Assets")]
        [Tooltip("Tuning for bike rider.")]
        [SerializeField] private RiderTuning _bikeTuning;

        [Tooltip("Tuning for horse rider.")]
        [SerializeField] private RiderTuning _horseTuning;

        [Header("Spawn Settings")]
        [Tooltip("Where to spawn the rider.")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("Which rider type to spawn initially.")]
        [SerializeField] private RiderType _initialRiderType = RiderType.Bike;

        [Tooltip("Spawn rider automatically on Start.")]
        [SerializeField] private bool _spawnOnStart = true;

        #endregion

        #region Private State

        private IRiderController _activeRider;
        private GameObject _activeRiderObject;
        private RiderType _currentRiderType;
        private bool _isSwapping;

        #endregion

        #region Events

        /// <summary>Fired when a rider is spawned.</summary>
        public event Action<IRiderController> OnRiderSpawned;

        /// <summary>Fired when a rider is despawned.</summary>
        public event Action<IRiderController> OnRiderDespawned;

        /// <summary>Fired when the active rider falls.</summary>
        public event Action<FallReason> OnRiderFell;

        #endregion

        #region Public Properties (Read-Only)

        /// <summary>The currently active rider controller (read-only).</summary>
        public IRiderController ActiveRider => _activeRider;

        /// <summary>The current rider type (read-only).</summary>
        public RiderType CurrentRiderType => _currentRiderType;

        /// <summary>True if a rider swap is in progress.</summary>
        public bool IsSwapping => _isSwapping;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnRider(_initialRiderType);
            }
        }

        private void Update()
        {
            if (_activeRider == null) return;

            // Build input state from InputReader
            InputState input = BuildInputState();

            // Route input to active rider (only RiderManager does this)
            _activeRider.TickInput(input);

            // Handle reset input
            if (input.ResetPressed && _activeRider.HasFallen)
            {
                RespawnCurrentRider();
            }
        }

        private void FixedUpdate()
        {
            if (_activeRider == null) return;

            _activeRider.TickPhysics(Time.fixedDeltaTime);
        }

        #endregion

        #region Public API (Spawn/Despawn/Swap)

        /// <summary>
        /// Spawns a rider of the specified type.
        /// Only RiderManager should call this.
        /// </summary>
        public void SpawnRider(RiderType type)
        {
            if (_isSwapping) return;

            _isSwapping = true;

            try
            {
                // Despawn existing rider if any
                if (_activeRiderObject != null)
                {
                    DespawnCurrentRiderInternal();
                }

                // Get prefab and tuning for type
                GameObject prefab = GetPrefabForType(type);
                RiderTuning tuning = GetTuningForType(type);

                if (prefab == null)
                {
                    Debug.LogError($"[{nameof(RiderManager)}] No prefab assigned for rider type: {type}");
                    return;
                }

                if (tuning == null)
                {
                    Debug.LogError($"[{nameof(RiderManager)}] No tuning assigned for rider type: {type}");
                    return;
                }

                // Calculate spawn position
                Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
                Quaternion spawnRot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

                // Instantiate
                _activeRiderObject = Instantiate(prefab, spawnPos, spawnRot);
                _activeRider = _activeRiderObject.GetComponent<IRiderController>();

                if (_activeRider == null)
                {
                    Debug.LogError($"[{nameof(RiderManager)}] Prefab '{prefab.name}' does not have an IRiderController component.");
                    Destroy(_activeRiderObject);
                    _activeRiderObject = null;
                    return;
                }

                _currentRiderType = type;
                _activeRider.Initialize(tuning);
                _activeRider.OnFall += HandleRiderFall;

                OnRiderSpawned?.Invoke(_activeRider);
            }
            finally
            {
                _isSwapping = false;
            }
        }

        /// <summary>
        /// Despawns the current rider.
        /// </summary>
        public void DespawnCurrentRider()
        {
            if (_isSwapping) return;

            _isSwapping = true;
            try
            {
                DespawnCurrentRiderInternal();
            }
            finally
            {
                _isSwapping = false;
            }
        }
        /// <summary>
        /// Respawns the current rider at the spawn point.
        /// </summary>
        public void RespawnCurrentRider()
        {
            if (_activeRider == null) return;

            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
            Quaternion spawnRot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

            _activeRider.ResetRider(spawnPos, spawnRot);
        }

        /// <summary>
        /// Swaps to a different rider type.
        /// Only RiderManager should orchestrate rider swaps.
        /// </summary>
        public void SwapRider(RiderType newType)
        {
            if (newType == _currentRiderType && _activeRider != null) return;

            SpawnRider(newType);
        }

        /// <summary>
        /// Updates the spawn point for respawning.
        /// </summary>
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            _spawnPoint = newSpawnPoint;
        }

        #endregion

        #region Private Methods

        private void DespawnCurrentRiderInternal()
        {
            if (_activeRider != null)
            {
                _activeRider.OnFall -= HandleRiderFall;
                OnRiderDespawned?.Invoke(_activeRider);
            }

            if (_activeRiderObject != null)
            {
                Destroy(_activeRiderObject);
                _activeRiderObject = null;
            }

            _activeRider = null;
        }

        private GameObject GetPrefabForType(RiderType type)
        {
            return type switch
            {
                RiderType.Bike => _bikePrefab,
                RiderType.Horse => _horsePrefab,
                _ => null
            };
        }

        private RiderTuning GetTuningForType(RiderType type)
        {
            return type switch
            {
                RiderType.Bike => _bikeTuning,
                RiderType.Horse => _horseTuning,
                _ => null
            };
        }

        private InputState BuildInputState()
        {
            if (InputReader.Instance == null)
            {
                return InputState.Empty;
            }

            return new InputState(
                InputReader.Instance.Throttle,
                InputReader.Instance.Brake,
                InputReader.Instance.Steer,
                InputReader.Instance.FocusHeld,
                InputReader.Instance.ResetPressed
            );
        }

        private void HandleRiderFall(FallReason reason)
        {
            OnRiderFell?.Invoke(reason);
        }

        #endregion
    }
}
