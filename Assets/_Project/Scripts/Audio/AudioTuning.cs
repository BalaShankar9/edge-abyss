using UnityEngine;

namespace EdgeAbyss.Audio
{
    /// <summary>
    /// Audio tuning parameters for the game.
    /// Controls volume levels, pitch variations, and audio behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioTuning", menuName = "EdgeAbyss/Audio Tuning", order = 5)]
    public class AudioTuning : ScriptableObject
    {
        [Header("Master Volume")]
        [Range(0f, 1f)]
        [Tooltip("Master volume for all game audio.")]
        public float masterVolume = 1f;

        [Range(0f, 1f)]
        [Tooltip("Volume for background music.")]
        public float musicVolume = 0.7f;

        [Range(0f, 1f)]
        [Tooltip("Volume for sound effects.")]
        public float sfxVolume = 1f;

        [Range(0f, 1f)]
        [Tooltip("Volume for UI sounds.")]
        public float uiVolume = 0.8f;

        [Header("Engine Sounds")]
        [Tooltip("Base pitch for bike engine sound.")]
        public float bikeEnginePitchBase = 0.8f;

        [Tooltip("Maximum pitch increase at max speed for bike.")]
        public float bikeEnginePitchMax = 1.5f;

        [Tooltip("Base pitch for horse gallop sound.")]
        public float horseGallopPitchBase = 0.9f;

        [Tooltip("Maximum pitch increase at max speed for horse.")]
        public float horseGallopPitchMax = 1.3f;

        [Header("Speed-Based Audio")]
        [Tooltip("Speed threshold where wind sound starts.")]
        public float windSoundStartSpeed = 10f;

        [Tooltip("Speed at which wind sound reaches max volume.")]
        public float windSoundMaxSpeed = 30f;

        [Range(0f, 1f)]
        [Tooltip("Maximum volume for wind sound.")]
        public float windSoundMaxVolume = 0.5f;

        [Header("Impact Sounds")]
        [Tooltip("Minimum impact velocity to trigger sound.")]
        public float impactSoundMinVelocity = 2f;

        [Tooltip("Volume multiplier based on impact velocity.")]
        public float impactVolumeMultiplier = 0.1f;

        [Header("UI Audio")]
        [Tooltip("Pitch variation for button hover sounds.")]
        [Range(0.9f, 1.1f)]
        public float buttonHoverPitchVariation = 1.05f;

        [Tooltip("Pitch for button click sounds.")]
        [Range(0.9f, 1.1f)]
        public float buttonClickPitch = 1f;

        [Header("Ambient")]
        [Tooltip("Volume for ambient wind/environment sounds.")]
        [Range(0f, 1f)]
        public float ambientVolume = 0.3f;

        [Tooltip("Crossfade time between music tracks.")]
        public float musicCrossfadeTime = 2f;
    }
}
