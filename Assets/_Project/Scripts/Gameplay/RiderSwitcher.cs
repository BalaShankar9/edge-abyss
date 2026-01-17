using UnityEngine;
using EdgeAbyss.Gameplay.Riders;

namespace EdgeAbyss.Gameplay
{
    /// <summary>
    /// Allows switching between bike and horse with number keys.
    /// Press 1 for Bike, 2 for Horse.
    /// </summary>
    public class RiderSwitcher : MonoBehaviour
    {
        [SerializeField] private RiderManager riderManager;

        private void Start()
        {
            if (riderManager == null)
            {
                riderManager = FindFirstObjectByType<RiderManager>();
            }
        }

        private void Update()
        {
            if (riderManager == null) return;

            // Check for rider switch input
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
            {
                SwitchToBike();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
            {
                SwitchToHorse();
            }
        }

        public void SwitchToBike()
        {
            if (riderManager != null && riderManager.CurrentRiderType != RiderManager.RiderType.Bike)
            {
                riderManager.SwapRider(RiderManager.RiderType.Bike);
                Debug.Log("[RiderSwitcher] Switched to Bike");
            }
        }

        public void SwitchToHorse()
        {
            if (riderManager != null && riderManager.CurrentRiderType != RiderManager.RiderType.Horse)
            {
                riderManager.SwapRider(RiderManager.RiderType.Horse);
                Debug.Log("[RiderSwitcher] Switched to Horse");
            }
        }
    }
}
