namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Immutable struct representing the current input state.
    /// Passed to riders each tick to avoid coupling to InputReader directly.
    /// </summary>
    public readonly struct InputState
    {
        /// <summary>Forward throttle input [0..1].</summary>
        public readonly float Throttle;
        
        /// <summary>Brake input [0..1].</summary>
        public readonly float Brake;
        
        /// <summary>Steering input [-1..1] (negative = left, positive = right).</summary>
        public readonly float Steer;
        
        /// <summary>True if focus/concentration button is held.</summary>
        public readonly bool FocusHeld;
        
        /// <summary>True if reset was pressed this frame.</summary>
        public readonly bool ResetPressed;

        public InputState(float throttle, float brake, float steer, bool focusHeld, bool resetPressed)
        {
            Throttle = throttle;
            Brake = brake;
            Steer = steer;
            FocusHeld = focusHeld;
            ResetPressed = resetPressed;
        }

        /// <summary>Returns an input state with all values zeroed.</summary>
        public static InputState Empty => new InputState(0f, 0f, 0f, false, false);
    }
}
