using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EdgeAbyss.Input
{
    /// <summary>
    /// MonoBehaviour that reads player input and exposes values/events.
    /// Attach to a GameObject in your scene (e.g., on the Player or a dedicated InputManager).
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Ensure the Unity Input System package is installed (com.unity.inputsystem).
    /// 2. In Project Settings > Player > Other Settings, set Active Input Handling to "Input System Package (New)" or "Both".
    /// 3. Create an empty GameObject in your gameplay scene, name it "InputReader".
    /// 4. Attach this InputReader component to it.
    /// 5. Optionally, access via InputReader.Instance if you want singleton-like access (set enableOnAwake = true).
    /// 
    /// Properties (read each frame, no allocation):
    /// - Throttle: 0..1 (W or Right Trigger)
    /// - Brake: 0..1 (S or Left Trigger)
    /// - Steer: -1..1 (A/D or Left Stick X)
    /// - FocusHeld: true while Shift or South button is held
    /// - ResetPressed: true on the frame Reset was pressed
    /// 
    /// Events:
    /// - OnFocusStarted / OnFocusCanceled
    /// - OnResetPerformed
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("If true, input is enabled automatically on Awake.")]
        [SerializeField] private bool enableOnAwake = true;

        // Events (subscribe to these for event-driven input handling)
        public event Action OnFocusStarted;
        public event Action OnFocusCanceled;
        public event Action OnResetPerformed;

        // Cached input values (no allocation on read)
        private float _throttle;
        private float _brake;
        private float _steer;
        private bool _focusHeld;
        private bool _resetPressed;

        // Public read-only properties
        public float Throttle => _throttle;
        public float Brake => _brake;
        public float Steer => _steer;
        public bool FocusHeld => _focusHeld;
        public bool ResetPressed => _resetPressed;

        /// <summary>
        /// Returns true if any input is currently being received.
        /// </summary>
        public bool HasAnyInput => _throttle > 0.01f || _brake > 0.01f || Mathf.Abs(_steer) > 0.01f;

        private PlayerInputActions _inputActions;
        private bool _isEnabled;

        // Optional static instance for easy access (not a full singleton to allow multiple readers)
        public static InputReader Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            _inputActions = new PlayerInputActions();
            SubscribeToActions();

            if (enableOnAwake)
            {
                EnableInput();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            UnsubscribeFromActions();
            _inputActions?.Dispose();
            _inputActions = null;
        }

        private void OnEnable()
        {
            if (enableOnAwake && _inputActions != null)
            {
                EnableInput();
            }
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void Update()
        {
            if (!_isEnabled || _inputActions == null) return;

            // Read axis values each frame (no allocation)
            _throttle = _inputActions.ThrottleAction.ReadValue<float>();
            _brake = _inputActions.BrakeAction.ReadValue<float>();
            _steer = _inputActions.SteerAction.ReadValue<float>();

            // Reset the "pressed this frame" flag after it's been consumed
            // ResetPressed is set true in callback, cleared here after one frame
            if (_resetPressed)
            {
                _resetPressed = false;
            }
        }

        private void LateUpdate()
        {
            // Clear one-frame flags at end of frame if needed
        }

        /// <summary>
        /// Enables input processing.
        /// </summary>
        public void EnableInput()
        {
            if (_isEnabled || _inputActions == null) return;

            _inputActions.Enable();
            _isEnabled = true;
        }

        /// <summary>
        /// Disables input processing. Values will remain at their last state.
        /// </summary>
        public void DisableInput()
        {
            if (!_isEnabled || _inputActions == null) return;

            _inputActions.Disable();
            _isEnabled = false;

            // Reset values when disabled
            _throttle = 0f;
            _brake = 0f;
            _steer = 0f;
            _focusHeld = false;
            _resetPressed = false;
        }

        /// <summary>
        /// Resets all input values to zero/false.
        /// </summary>
        public void ResetInputValues()
        {
            _throttle = 0f;
            _brake = 0f;
            _steer = 0f;
            _focusHeld = false;
            _resetPressed = false;
        }

        private void SubscribeToActions()
        {
            if (_inputActions == null) return;

            _inputActions.FocusAction.started += OnFocusActionStarted;
            _inputActions.FocusAction.canceled += OnFocusActionCanceled;
            _inputActions.ResetAction.performed += OnResetActionPerformed;
        }

        private void UnsubscribeFromActions()
        {
            if (_inputActions == null) return;

            _inputActions.FocusAction.started -= OnFocusActionStarted;
            _inputActions.FocusAction.canceled -= OnFocusActionCanceled;
            _inputActions.ResetAction.performed -= OnResetActionPerformed;
        }

        // Input callbacks (no allocation - uses cached delegates via method references)
        private void OnFocusActionStarted(InputAction.CallbackContext ctx)
        {
            _focusHeld = true;
            OnFocusStarted?.Invoke();
        }

        private void OnFocusActionCanceled(InputAction.CallbackContext ctx)
        {
            _focusHeld = false;
            OnFocusCanceled?.Invoke();
        }

        private void OnResetActionPerformed(InputAction.CallbackContext ctx)
        {
            _resetPressed = true;
            OnResetPerformed?.Invoke();
        }
    }
}
