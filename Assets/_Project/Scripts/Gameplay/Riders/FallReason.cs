namespace EdgeAbyss.Gameplay.Riders
{
    /// <summary>
    /// Reasons why a rider may fall.
    /// </summary>
    public enum FallReason
    {
        /// <summary>Stability dropped below threshold.</summary>
        LostBalance,
        
        /// <summary>Collided with an obstacle.</summary>
        Collision,
        
        /// <summary>Fell off the track edge.</summary>
        FellOffEdge,
        
        /// <summary>Speed was too high for the current turn.</summary>
        Overspeed,
        
        /// <summary>External force caused the fall.</summary>
        ExternalForce
    }
}
