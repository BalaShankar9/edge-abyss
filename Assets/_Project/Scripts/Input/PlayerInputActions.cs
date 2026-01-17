using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EdgeAbyss.Input
{
    /// <summary>
    /// Programmatically-defined input actions for EdgeAbyss.
    /// Supports Keyboard + Gamepad without requiring an .inputactions asset.
    /// 
    /// Actions:
    /// - Throttle: W / Right Trigger [0..1]
    /// - Brake: S / Left Trigger [0..1]
    /// - Steer: A/D / Left Stick X [-1..1]
    /// - Focus: Left Shift / South Button (A/Cross) [button]
    /// - Reset: R / Start [button]
    /// </summary>
    public class PlayerInputActions : IDisposable
    {
        public InputActionMap GameplayActionMap { get; private set; }

        public InputAction ThrottleAction { get; private set; }
        public InputAction BrakeAction { get; private set; }
        public InputAction SteerAction { get; private set; }
        public InputAction FocusAction { get; private set; }
        public InputAction ResetAction { get; private set; }

        private bool _isDisposed;

        public PlayerInputActions()
        {
            CreateActions();
        }

        private void CreateActions()
        {
            GameplayActionMap = new InputActionMap("Gameplay");

            // Throttle: W key or Right Trigger (0 to 1)
            ThrottleAction = GameplayActionMap.AddAction(
                "Throttle",
                type: InputActionType.Value,
                expectedControlType: "Axis"
            );
            ThrottleAction.AddBinding("<Keyboard>/w");
            ThrottleAction.AddBinding("<Gamepad>/rightTrigger");

            // Brake: S key or Left Trigger (0 to 1)
            BrakeAction = GameplayActionMap.AddAction(
                "Brake",
                type: InputActionType.Value,
                expectedControlType: "Axis"
            );
            BrakeAction.AddBinding("<Keyboard>/s");
            BrakeAction.AddBinding("<Gamepad>/leftTrigger");

            // Steer: A/D keys or Left Stick X (-1 to 1)
            SteerAction = GameplayActionMap.AddAction(
                "Steer",
                type: InputActionType.Value,
                expectedControlType: "Axis"
            );
            // Composite for A/D keys
            SteerAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            SteerAction.AddBinding("<Gamepad>/leftStick/x");

            // Focus: Left Shift or Gamepad South (A/Cross) - held button
            FocusAction = GameplayActionMap.AddAction(
                "Focus",
                type: InputActionType.Button
            );
            FocusAction.AddBinding("<Keyboard>/leftShift");
            FocusAction.AddBinding("<Gamepad>/buttonSouth");

            // Reset: R key or Start button - pressed button
            ResetAction = GameplayActionMap.AddAction(
                "Reset",
                type: InputActionType.Button
            );
            ResetAction.AddBinding("<Keyboard>/r");
            ResetAction.AddBinding("<Gamepad>/start");
        }

        public void Enable()
        {
            GameplayActionMap?.Enable();
        }

        public void Disable()
        {
            GameplayActionMap?.Disable();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            GameplayActionMap?.Disable();
            GameplayActionMap?.Dispose();
            GameplayActionMap = null;
        }
    }
}
