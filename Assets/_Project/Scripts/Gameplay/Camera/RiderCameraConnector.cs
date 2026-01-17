using UnityEngine;
using EdgeAbyss.Gameplay.Riders;

namespace EdgeAbyss.Gameplay.Camera
{
    /// <summary>
    /// Bridges the POVCameraRig with the rider system.
    /// Automatically updates camera inputs from the active rider.
    /// 
    /// DATA FLOW:
    /// IRiderController (any rider type)
    ///       ↓
    /// RiderCameraData (struct)
    ///       ↓
    /// POVCameraRig.UpdateRiderData()
    /// 
    /// The camera never casts to or depends on BikeRiderController,
    /// HorseRiderController, or any other concrete type.
    /// 
    /// SETUP:
    /// 1. Add this component to the same GameObject as POVCameraRig.
    /// 2. Assign the RiderManager reference.
    /// 3. It will automatically connect when a rider spawns.
    /// </summary>
    [RequireComponent(typeof(POVCameraRig))]
    public class RiderCameraConnector : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The rider manager to track.")]
        [SerializeField] private RiderManager _riderManager;

        [Tooltip("Name of the child transform on rider prefab to use as POV mount.")]
        [SerializeField] private string _povMountChildName = "POVMount";

        private POVCameraRig _cameraRig;
        private IRiderController _currentRider;
        private Transform _currentRiderTransform;

        private void Awake()
        {
            _cameraRig = GetComponent<POVCameraRig>();
        }

        private void OnEnable()
        {
            if (_riderManager != null)
            {
                _riderManager.OnRiderSpawned += HandleRiderSpawned;
                _riderManager.OnRiderDespawned += HandleRiderDespawned;

                // If a rider already exists, connect to it
                if (_riderManager.ActiveRider != null)
                {
                    ConnectToRider(_riderManager.ActiveRider);
                }
            }
        }

        private void OnDisable()
        {
            if (_riderManager != null)
            {
                _riderManager.OnRiderSpawned -= HandleRiderSpawned;
                _riderManager.OnRiderDespawned -= HandleRiderDespawned;
            }
        }

        private void LateUpdate()
        {
            if (_currentRider == null || _cameraRig == null) return;

            // Build camera data from IRiderController (type-agnostic)
            RiderCameraData cameraData = BuildCameraData(_currentRider);

            // Push data to camera (single update point)
            _cameraRig.UpdateRiderData(cameraData);
        }

        /// <summary>
        /// Builds camera data from any IRiderController without casting to concrete types.
        /// </summary>
        private RiderCameraData BuildCameraData(IRiderController rider)
        {
            return new RiderCameraData(
                speed: rider.Speed,
                leanAngle: rider.LeanAngle,  // Uses interface property, not transform extraction
                stability: rider.Stability,
                isGrounded: rider.IsGrounded,
                hasFallen: rider.HasFallen
            );
        }

        private void HandleRiderSpawned(IRiderController rider)
        {
            ConnectToRider(rider);
        }

        private void HandleRiderDespawned(IRiderController rider)
        {
            if (_currentRider == rider)
            {
                DisconnectFromRider();
            }
        }

        private void ConnectToRider(IRiderController rider)
        {
            _currentRider = rider;

            // Get the rider's transform (assuming it's a MonoBehaviour)
            if (rider is MonoBehaviour mb)
            {
                _currentRiderTransform = mb.transform;

                // Find POV mount child
                Transform povMount = _currentRiderTransform.Find(_povMountChildName);

                if (povMount == null)
                {
                    Debug.LogWarning($"[{nameof(RiderCameraConnector)}] Could not find child '{_povMountChildName}' on rider. Using rider root as mount.");
                    povMount = _currentRiderTransform;
                }

                _cameraRig.SetTargets(_currentRiderTransform, povMount);
            }
        }

        private void DisconnectFromRider()
        {
            _currentRider = null;
            _currentRiderTransform = null;
            _cameraRig.ClearTargets();
        }

        /// <summary>
        /// Sets surface roughness on the camera (call from ground detection system).
        /// </summary>
        public void SetSurfaceRoughness(float roughness)
        {
            if (_cameraRig != null)
            {
                _cameraRig.SurfaceRoughness = roughness;
            }
        }

        /// <summary>
        /// Sets wind intensity on the camera (call from weather system).
        /// </summary>
        public void SetWindIntensity(float intensity)
        {
            if (_cameraRig != null)
            {
                _cameraRig.WindIntensity = intensity;
            }
        }

        /// <summary>
        /// Manually sets the rider manager reference (for runtime setup).
        /// </summary>
        public void SetRiderManager(RiderManager manager)
        {
            // Unsubscribe from old manager
            if (_riderManager != null)
            {
                _riderManager.OnRiderSpawned -= HandleRiderSpawned;
                _riderManager.OnRiderDespawned -= HandleRiderDespawned;
            }

            _riderManager = manager;

            // Subscribe to new manager
            if (_riderManager != null && enabled)
            {
                _riderManager.OnRiderSpawned += HandleRiderSpawned;
                _riderManager.OnRiderDespawned += HandleRiderDespawned;

                if (_riderManager.ActiveRider != null)
                {
                    ConnectToRider(_riderManager.ActiveRider);
                }
            }
        }
    }
}
