using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EdgeAbyss.Persistence;

namespace EdgeAbyss.UI.Menu
{
    /// <summary>
    /// Settings menu UI controller.
    /// Handles FOV, camera shake, sensitivity, and other options.
    /// 
    /// SETUP:
    /// 1. Attach to Settings panel.
    /// 2. Assign UI element references.
    /// 3. Call RefreshUI() when panel is shown.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Slider fovSlider;
        [SerializeField] private TMP_Text fovValueText;
        [SerializeField] private float fovMin = 60f;
        [SerializeField] private float fovMax = 110f;

        [SerializeField] private Toggle cameraShakeToggle;
        [SerializeField] private Slider cameraShakeSlider;
        [SerializeField] private TMP_Text cameraShakeValueText;

        [Header("Control Settings")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TMP_Text sensitivityValueText;
        [SerializeField] private float sensitivityMin = 0.5f;
        [SerializeField] private float sensitivityMax = 2f;

        [SerializeField] private Toggle invertSteeringToggle;

        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeText;

        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TMP_Text musicVolumeText;

        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Text sfxVolumeText;

        [Header("Graphics Settings")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle postProcessingToggle;
        [SerializeField] private Toggle vSyncToggle;

        [Header("Gameplay Settings")]
        [SerializeField] private Toggle showGhostToggle;
        [SerializeField] private Toggle showSpeedometerToggle;
        [SerializeField] private Toggle showTimerToggle;

        private GameSettings _settings;
        private bool _isInitializing;

        private void Awake()
        {
            SetupListeners();
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        /// <summary>
        /// Refreshes all UI elements to match current settings.
        /// </summary>
        public void RefreshUI()
        {
            _isInitializing = true;

            _settings = SaveSystem.Instance?.Settings ?? new GameSettings();

            // Camera
            if (fovSlider != null)
            {
                fovSlider.minValue = fovMin;
                fovSlider.maxValue = fovMax;
                fovSlider.value = _settings.fieldOfView;
                UpdateFOVText(_settings.fieldOfView);
            }

            if (cameraShakeToggle != null)
            {
                cameraShakeToggle.isOn = _settings.enableCameraShake;
            }

            if (cameraShakeSlider != null)
            {
                cameraShakeSlider.value = _settings.cameraShakeIntensity;
                cameraShakeSlider.interactable = _settings.enableCameraShake;
                UpdateCameraShakeText(_settings.cameraShakeIntensity);
            }

            // Controls
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = sensitivityMin;
                sensitivitySlider.maxValue = sensitivityMax;
                sensitivitySlider.value = _settings.steeringSensitivity;
                UpdateSensitivityText(_settings.steeringSensitivity);
            }

            if (invertSteeringToggle != null)
            {
                invertSteeringToggle.isOn = _settings.invertSteering;
            }

            // Audio
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = _settings.masterVolume;
                UpdateVolumeText(masterVolumeText, _settings.masterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = _settings.musicVolume;
                UpdateVolumeText(musicVolumeText, _settings.musicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = _settings.sfxVolume;
                UpdateVolumeText(sfxVolumeText, _settings.sfxVolume);
            }

            // Graphics
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.value = _settings.qualityLevel;
            }

            if (postProcessingToggle != null)
            {
                postProcessingToggle.isOn = _settings.enablePostProcessing;
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.isOn = _settings.enableVSync;
            }

            // Gameplay
            if (showGhostToggle != null)
            {
                showGhostToggle.isOn = _settings.showGhost;
            }

            if (showSpeedometerToggle != null)
            {
                showSpeedometerToggle.isOn = _settings.showSpeedometer;
            }

            if (showTimerToggle != null)
            {
                showTimerToggle.isOn = _settings.showTimer;
            }

            _isInitializing = false;
        }

        /// <summary>
        /// Saves current settings to disk.
        /// </summary>
        public void SaveSettings()
        {
            SaveSystem.Instance?.SaveSettings();
        }

        /// <summary>
        /// Resets all settings to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            SaveSystem.Instance?.ResetSettings();
            RefreshUI();
            ApplyAllSettings();
        }

        private void SetupListeners()
        {
            // Camera
            if (fovSlider != null)
            {
                fovSlider.onValueChanged.AddListener(OnFOVChanged);
            }

            if (cameraShakeToggle != null)
            {
                cameraShakeToggle.onValueChanged.AddListener(OnCameraShakeToggled);
            }

            if (cameraShakeSlider != null)
            {
                cameraShakeSlider.onValueChanged.AddListener(OnCameraShakeIntensityChanged);
            }

            // Controls
            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }

            if (invertSteeringToggle != null)
            {
                invertSteeringToggle.onValueChanged.AddListener(OnInvertSteeringChanged);
            }

            // Audio
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // Graphics
            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (postProcessingToggle != null)
            {
                postProcessingToggle.onValueChanged.AddListener(OnPostProcessingChanged);
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }

            // Gameplay
            if (showGhostToggle != null)
            {
                showGhostToggle.onValueChanged.AddListener(OnShowGhostChanged);
            }

            if (showSpeedometerToggle != null)
            {
                showSpeedometerToggle.onValueChanged.AddListener(OnShowSpeedometerChanged);
            }

            if (showTimerToggle != null)
            {
                showTimerToggle.onValueChanged.AddListener(OnShowTimerChanged);
            }
        }

        #region Camera Callbacks

        private void OnFOVChanged(float value)
        {
            if (_isInitializing) return;

            _settings.fieldOfView = value;
            UpdateFOVText(value);

            if (Camera.main != null)
            {
                Camera.main.fieldOfView = value;
            }

            SaveSettings();
        }

        private void OnCameraShakeToggled(bool enabled)
        {
            if (_isInitializing) return;

            _settings.enableCameraShake = enabled;

            if (cameraShakeSlider != null)
            {
                cameraShakeSlider.interactable = enabled;
            }

            SaveSettings();
        }

        private void OnCameraShakeIntensityChanged(float value)
        {
            if (_isInitializing) return;

            _settings.cameraShakeIntensity = value;
            UpdateCameraShakeText(value);
            SaveSettings();
        }

        #endregion

        #region Control Callbacks

        private void OnSensitivityChanged(float value)
        {
            if (_isInitializing) return;

            _settings.steeringSensitivity = value;
            UpdateSensitivityText(value);
            SaveSettings();
        }

        private void OnInvertSteeringChanged(bool inverted)
        {
            if (_isInitializing) return;

            _settings.invertSteering = inverted;
            SaveSettings();
        }

        #endregion

        #region Audio Callbacks

        private void OnMasterVolumeChanged(float value)
        {
            if (_isInitializing) return;

            _settings.masterVolume = value;
            UpdateVolumeText(masterVolumeText, value);
            AudioListener.volume = value;
            SaveSettings();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_isInitializing) return;

            _settings.musicVolume = value;
            UpdateVolumeText(musicVolumeText, value);
            SaveSettings();
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (_isInitializing) return;

            _settings.sfxVolume = value;
            UpdateVolumeText(sfxVolumeText, value);
            SaveSettings();
        }

        #endregion

        #region Graphics Callbacks

        private void OnQualityChanged(int index)
        {
            if (_isInitializing) return;

            _settings.qualityLevel = index;
            QualitySettings.SetQualityLevel(index);
            SaveSettings();
        }

        private void OnPostProcessingChanged(bool enabled)
        {
            if (_isInitializing) return;

            _settings.enablePostProcessing = enabled;
            SaveSettings();
        }

        private void OnVSyncChanged(bool enabled)
        {
            if (_isInitializing) return;

            _settings.enableVSync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            SaveSettings();
        }

        #endregion

        #region Gameplay Callbacks

        private void OnShowGhostChanged(bool show)
        {
            if (_isInitializing) return;

            _settings.showGhost = show;
            SaveSettings();
        }

        private void OnShowSpeedometerChanged(bool show)
        {
            if (_isInitializing) return;

            _settings.showSpeedometer = show;
            SaveSettings();
        }

        private void OnShowTimerChanged(bool show)
        {
            if (_isInitializing) return;

            _settings.showTimer = show;
            SaveSettings();
        }

        #endregion

        #region Text Updates

        private void UpdateFOVText(float value)
        {
            if (fovValueText != null)
            {
                fovValueText.text = $"{value:F0}°";
            }
        }

        private void UpdateCameraShakeText(float value)
        {
            if (cameraShakeValueText != null)
            {
                cameraShakeValueText.text = $"{value * 100:F0}%";
            }
        }

        private void UpdateSensitivityText(float value)
        {
            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = $"{value:F1}x";
            }
        }

        private void UpdateVolumeText(TMP_Text text, float value)
        {
            if (text != null)
            {
                text.text = $"{value * 100:F0}%";
            }
        }

        #endregion

        private void ApplyAllSettings()
        {
            if (_settings == null) return;

            // Apply FOV
            if (Camera.main != null)
            {
                Camera.main.fieldOfView = _settings.fieldOfView;
            }

            // Apply quality
            QualitySettings.SetQualityLevel(_settings.qualityLevel);

            // Apply VSync
            QualitySettings.vSyncCount = _settings.enableVSync ? 1 : 0;

            // Apply audio
            AudioListener.volume = _settings.masterVolume;
        }
    }
}
