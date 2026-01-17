using UnityEngine;
using EdgeAbyss.Gameplay.Riders;

namespace EdgeAbyss.Gameplay
{
    /// <summary>
    /// Simple third-person follow camera for the test track.
    /// Follows the active rider from the RiderManager.
    /// </summary>
    public class SimpleFollowCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RiderManager riderManager;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -8f);
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float lookAheadDistance = 5f;

        private Transform _target;

        private void Start()
        {
            if (riderManager == null)
            {
                riderManager = FindFirstObjectByType<RiderManager>();
            }

            if (riderManager != null)
            {
                riderManager.OnRiderSpawned += OnRiderSpawned;
                riderManager.OnRiderDespawned += OnRiderDespawned;

                // Check if rider already exists
                if (riderManager.ActiveRider != null && riderManager.ActiveRider is MonoBehaviour mb)
                {
                    _target = mb.transform;
                    SnapToTarget();
                }
            }
        }

        private void OnDestroy()
        {
            if (riderManager != null)
            {
                riderManager.OnRiderSpawned -= OnRiderSpawned;
                riderManager.OnRiderDespawned -= OnRiderDespawned;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Calculate desired position behind and above target
            Vector3 targetPosition = _target.position;
            Vector3 targetForward = _target.forward;

            Vector3 desiredPosition = targetPosition
                + _target.TransformDirection(offset);

            // Smooth follow
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            // Look at target with some look-ahead
            Vector3 lookTarget = targetPosition + targetForward * lookAheadDistance;
            lookTarget.y = targetPosition.y + 1f; // Look slightly above rider

            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void OnRiderSpawned(IRiderController rider)
        {
            if (rider is MonoBehaviour mb)
            {
                _target = mb.transform;
                SnapToTarget();
            }
        }

        private void OnRiderDespawned(IRiderController rider)
        {
            // Don't clear target - keep looking at last position
        }

        private void SnapToTarget()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _target.TransformDirection(offset);
            transform.position = desiredPosition;

            Vector3 lookTarget = _target.position + _target.forward * lookAheadDistance;
            lookTarget.y = _target.position.y + 1f;
            transform.LookAt(lookTarget);
        }
    }
}
