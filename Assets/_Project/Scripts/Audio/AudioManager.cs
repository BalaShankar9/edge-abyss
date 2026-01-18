using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EdgeAbyss.Audio
{
    /// <summary>
    /// Central audio manager handling all game sounds.
    /// Singleton pattern for easy access throughout the game.
    /// 
    /// SETUP:
    /// 1. Create AudioManager GameObject in Boot scene
    /// 2. Attach this component
    /// 3. Create AudioTuning asset and assign it
    /// 4. Optionally assign audio clips for immediate use
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private AudioTuning tuning;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource musicSourceB; // For crossfading
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Audio Clips - Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip resultsMusic;

        [Header("Audio Clips - SFX")]
        [SerializeField] private AudioClip bikeEngine;
        [SerializeField] private AudioClip horseGallop;
        [SerializeField] private AudioClip windLoop;
        [SerializeField] private AudioClip impact;
        [SerializeField] private AudioClip fall;
        [SerializeField] private AudioClip respawn;
        [SerializeField] private AudioClip riderSwitch;

        [Header("Audio Clips - UI")]
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip pauseOpen;
        [SerializeField] private AudioClip pauseClose;
        [SerializeField] private AudioClip scoreIncrease;
        [SerializeField] private AudioClip streakIncrease;
        [SerializeField] private AudioClip streakBreak;

        #endregion

        #region Private State

        private bool _isMusicSourceA = true;
        private Coroutine _crossfadeCoroutine;
        private Dictionary<string, AudioSource> _loopingSources = new Dictionary<string, AudioSource>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
        }

        private void Start()
        {
            ApplyVolumeSettings();
        }

        #endregion

        #region Setup

        private void SetupAudioSources()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = CreateAudioSource("MusicSourceA");
                musicSource.loop = true;
            }

            if (musicSourceB == null)
            {
                musicSourceB = CreateAudioSource("MusicSourceB");
                musicSourceB.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = CreateAudioSource("SFXSource");
            }

            if (uiSource == null)
            {
                uiSource = CreateAudioSource("UISource");
            }

            if (ambientSource == null)
            {
                ambientSource = CreateAudioSource("AmbientSource");
                ambientSource.loop = true;
            }
        }

        private AudioSource CreateAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            return go.AddComponent<AudioSource>();
        }

        #endregion

        #region Volume Control

        public void ApplyVolumeSettings()
        {
            if (tuning == null) return;

            float master = tuning.masterVolume;

            if (musicSource != null)
                musicSource.volume = tuning.musicVolume * master;

            if (musicSourceB != null)
                musicSourceB.volume = tuning.musicVolume * master;

            if (sfxSource != null)
                sfxSource.volume = tuning.sfxVolume * master;

            if (uiSource != null)
                uiSource.volume = tuning.uiVolume * master;

            if (ambientSource != null)
                ambientSource.volume = tuning.ambientVolume * master;
        }

        public void SetMasterVolume(float volume)
        {
            if (tuning != null)
            {
                tuning.masterVolume = Mathf.Clamp01(volume);
                ApplyVolumeSettings();
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (tuning != null)
            {
                tuning.musicVolume = Mathf.Clamp01(volume);
                ApplyVolumeSettings();
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (tuning != null)
            {
                tuning.sfxVolume = Mathf.Clamp01(volume);
                ApplyVolumeSettings();
            }
        }

        #endregion

        #region Music

        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic);
        }

        public void PlayResultsMusic()
        {
            PlayMusic(resultsMusic);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.Log("[AudioManager] Music clip is null, skipping playback");
                return;
            }

            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
            }

            float fadeTime = tuning != null ? tuning.musicCrossfadeTime : 2f;
            _crossfadeCoroutine = StartCoroutine(CrossfadeMusic(clip, fadeTime));
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
        {
            AudioSource fadeOut = _isMusicSourceA ? musicSource : musicSourceB;
            AudioSource fadeIn = _isMusicSourceA ? musicSourceB : musicSource;

            fadeIn.clip = newClip;
            fadeIn.volume = 0f;
            fadeIn.Play();

            float targetVolume = tuning != null ? tuning.musicVolume * tuning.masterVolume : 0.7f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                fadeOut.volume = Mathf.Lerp(targetVolume, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;

            _isMusicSourceA = !_isMusicSourceA;
        }

        public void StopMusic()
        {
            musicSource?.Stop();
            musicSourceB?.Stop();
        }

        #endregion

        #region SFX

        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null || sfxSource == null) return;

            float volume = tuning != null ? tuning.sfxVolume * tuning.masterVolume * volumeMultiplier : volumeMultiplier;
            sfxSource.PlayOneShot(clip, volume);
        }

        public void PlayImpact(float velocity)
        {
            if (tuning == null || velocity < tuning.impactSoundMinVelocity) return;

            float volume = Mathf.Clamp01(velocity * tuning.impactVolumeMultiplier);
            PlaySFX(impact, volume);
        }

        public void PlayFall() => PlaySFX(fall);
        public void PlayRespawn() => PlaySFX(respawn);
        public void PlayRiderSwitch() => PlaySFX(riderSwitch);

        #endregion

        #region UI Sounds

        public void PlayButtonHover()
        {
            if (buttonHover == null || uiSource == null) return;

            float pitch = tuning != null ? tuning.buttonHoverPitchVariation : 1f;
            uiSource.pitch = pitch;
            uiSource.PlayOneShot(buttonHover);
            uiSource.pitch = 1f;
        }

        public void PlayButtonClick()
        {
            if (buttonClick == null || uiSource == null) return;

            float pitch = tuning != null ? tuning.buttonClickPitch : 1f;
            uiSource.pitch = pitch;
            uiSource.PlayOneShot(buttonClick);
            uiSource.pitch = 1f;
        }

        public void PlayPauseOpen() => PlayUI(pauseOpen);
        public void PlayPauseClose() => PlayUI(pauseClose);
        public void PlayScoreIncrease() => PlayUI(scoreIncrease);
        public void PlayStreakIncrease() => PlayUI(streakIncrease);
        public void PlayStreakBreak() => PlayUI(streakBreak);

        private void PlayUI(AudioClip clip)
        {
            if (clip == null || uiSource == null) return;

            float volume = tuning != null ? tuning.uiVolume * tuning.masterVolume : 0.8f;
            uiSource.PlayOneShot(clip, volume);
        }

        #endregion

        #region Looping Sounds (Engine, Wind)

        public void StartEngineSound(bool isBike)
        {
            StopLoopingSound("engine");

            AudioClip clip = isBike ? bikeEngine : horseGallop;
            if (clip == null) return;

            var source = CreateAudioSource("EngineLoop");
            source.clip = clip;
            source.loop = true;
            source.volume = 0f;
            source.Play();

            _loopingSources["engine"] = source;
        }

        public void UpdateEngineSound(float speedNormalized, bool isBike)
        {
            if (!_loopingSources.TryGetValue("engine", out var source)) return;
            if (tuning == null) return;

            float basePitch = isBike ? tuning.bikeEnginePitchBase : tuning.horseGallopPitchBase;
            float maxPitch = isBike ? tuning.bikeEnginePitchMax : tuning.horseGallopPitchMax;

            source.pitch = Mathf.Lerp(basePitch, maxPitch, speedNormalized);
            source.volume = Mathf.Lerp(0.1f, tuning.sfxVolume * tuning.masterVolume, speedNormalized);
        }

        public void StopEngineSound()
        {
            StopLoopingSound("engine");
        }

        public void UpdateWindSound(float speed)
        {
            if (tuning == null) return;

            if (speed < tuning.windSoundStartSpeed)
            {
                StopLoopingSound("wind");
                return;
            }

            if (!_loopingSources.ContainsKey("wind") && windLoop != null)
            {
                var source = CreateAudioSource("WindLoop");
                source.clip = windLoop;
                source.loop = true;
                source.volume = 0f;
                source.Play();
                _loopingSources["wind"] = source;
            }

            if (_loopingSources.TryGetValue("wind", out var windSource))
            {
                float t = Mathf.InverseLerp(tuning.windSoundStartSpeed, tuning.windSoundMaxSpeed, speed);
                windSource.volume = Mathf.Lerp(0f, tuning.windSoundMaxVolume * tuning.masterVolume, t);
            }
        }

        private void StopLoopingSound(string key)
        {
            if (_loopingSources.TryGetValue(key, out var source))
            {
                source.Stop();
                Destroy(source.gameObject);
                _loopingSources.Remove(key);
            }
        }

        public void StopAllLoopingSounds()
        {
            foreach (var kvp in _loopingSources)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.Stop();
                    Destroy(kvp.Value.gameObject);
                }
            }
            _loopingSources.Clear();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            StopAllLoopingSounds();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }
}
