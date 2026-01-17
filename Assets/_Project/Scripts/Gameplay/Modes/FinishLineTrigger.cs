using UnityEngine;
using EdgeAbyss.Gameplay.Riders;

namespace EdgeAbyss.Gameplay.Modes
{
    /// <summary>
    /// Trigger that completes the time trial when crossed.
    /// 
    /// SETUP:
    /// 1. Create empty GameObject at finish line position.
    /// 2. Add BoxCollider (Is Trigger = true) spanning the track width.
    /// 3. Attach this component.
    /// 4. TimeTrialManager reference is auto-found if not assigned.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FinishLineTrigger : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The time trial manager. Auto-found if null.")]
        [SerializeField] private TimeTrialManager timeTrialManager;

        [Header("Settings")]
        [Tooltip("Minimum time before finish can trigger (prevents accidental early finishes).")]
        [SerializeField] private float minimumRaceTime = 5f;

        [Tooltip("Only trigger once per trial.")]
        [SerializeField] private bool triggerOnce = true;

        private bool _hasTriggered;

        private void Awake()
        {
            // Ensure collider is trigger
            var collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }

        private void Start()
        {
            if (timeTrialManager == null)
            {
                timeTrialManager = FindFirstObjectByType<TimeTrialManager>();
            }

            if (timeTrialManager != null)
            {
                timeTrialManager.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (timeTrialManager != null)
            {
                timeTrialManager.OnStateChanged -= HandleStateChanged;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (timeTrialManager == null) return;
            if (timeTrialManager.State != TrialState.Racing) return;
            if (triggerOnce && _hasTriggered) return;
            if (timeTrialManager.CurrentTime < minimumRaceTime) return;

            // Check if it's the rider
            var rider = other.GetComponentInParent<IRiderController>();
            if (rider == null) return;

            _hasTriggered = true;
            timeTrialManager.FinishTrial();
        }

        private void HandleStateChanged(TrialState newState)
        {
            // Reset trigger when trial resets
            if (newState == TrialState.Idle || newState == TrialState.Countdown)
            {
                _hasTriggered = false;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);

            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }

            // Draw checkered flag icon
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 3f);
        }
#endif
    }
}
