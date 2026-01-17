using System;
using System.IO;
using UnityEngine;

namespace EdgeAbyss.Persistence
{
    /// <summary>
    /// Handles saving and loading game data using JSON.
    /// Persists to Application.persistentDataPath.
    /// 
    /// SETUP:
    /// 1. Access via SaveSystem.Instance (auto-created singleton).
    /// 2. Modify settings via SaveSystem.Instance.Settings.
    /// 3. Call SaveSystem.Instance.Save() to persist changes.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private const string SETTINGS_FILE = "settings.json";
        private const string PROGRESS_FILE = "progress.json";

        private static SaveSystem _instance;
        private static bool _isQuitting;

        // Data
        private GameSettings _settings;
        private GameProgress _progress;

        /// <summary>Singleton instance.</summary>
        public static SaveSystem Instance
        {
            get
            {
                if (_isQuitting) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SaveSystem>();

                    if (_instance == null)
                    {
                        var go = new GameObject("[SaveSystem]");
                        _instance = go.AddComponent<SaveSystem>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        /// <summary>Current game settings.</summary>
        public GameSettings Settings => _settings ??= new GameSettings();

        /// <summary>Current game progress.</summary>
        public GameProgress Progress => _progress ??= new GameProgress();

        /// <summary>True if save system is ready.</summary>
        public static bool HasInstance => _instance != null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAll();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            SaveAll();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveAll();
            }
        }

        /// <summary>
        /// Saves all data to disk.
        /// </summary>
        public void SaveAll()
        {
            SaveSettings();
            SaveProgress();
        }

        /// <summary>
        /// Loads all data from disk.
        /// </summary>
        public void LoadAll()
        {
            LoadSettings();
            LoadProgress();
        }

        /// <summary>
        /// Saves settings to disk.
        /// </summary>
        public void SaveSettings()
        {
            SaveToFile(SETTINGS_FILE, _settings ?? new GameSettings());
            Debug.Log("[SaveSystem] Settings saved.");
        }

        /// <summary>
        /// Loads settings from disk.
        /// </summary>
        public void LoadSettings()
        {
            _settings = LoadFromFile<GameSettings>(SETTINGS_FILE) ?? new GameSettings();
            Debug.Log("[SaveSystem] Settings loaded.");
        }

        /// <summary>
        /// Saves progress to disk.
        /// </summary>
        public void SaveProgress()
        {
            SaveToFile(PROGRESS_FILE, _progress ?? new GameProgress());
            Debug.Log("[SaveSystem] Progress saved.");
        }

        /// <summary>
        /// Loads progress from disk.
        /// </summary>
        public void LoadProgress()
        {
            _progress = LoadFromFile<GameProgress>(PROGRESS_FILE) ?? new GameProgress();
            Debug.Log("[SaveSystem] Progress loaded.");
        }

        /// <summary>
        /// Resets settings to defaults.
        /// </summary>
        public void ResetSettings()
        {
            _settings = new GameSettings();
            SaveSettings();
        }

        /// <summary>
        /// Resets all progress (careful!).
        /// </summary>
        public void ResetProgress()
        {
            _progress = new GameProgress();
            SaveProgress();
        }

        /// <summary>
        /// Gets the best time for a track.
        /// </summary>
        public float GetBestTime(string trackId)
        {
            return Progress.GetBestTime(trackId);
        }

        /// <summary>
        /// Sets the best time for a track if it's a new record.
        /// </summary>
        public bool TrySetBestTime(string trackId, float time)
        {
            float current = Progress.GetBestTime(trackId);
            if (current <= 0 || time < current)
            {
                Progress.SetBestTime(trackId, time);
                SaveProgress();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the high score for a track.
        /// </summary>
        public int GetHighScore(string trackId)
        {
            return Progress.GetHighScore(trackId);
        }

        /// <summary>
        /// Sets the high score for a track if it's a new record.
        /// </summary>
        public bool TrySetHighScore(string trackId, int score)
        {
            int current = Progress.GetHighScore(trackId);
            if (score > current)
            {
                Progress.SetHighScore(trackId, score);
                SaveProgress();
                return true;
            }
            return false;
        }

        private void SaveToFile<T>(string filename, T data)
        {
            try
            {
                string path = GetFilePath(filename);
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save {filename}: {e.Message}");
            }
        }

        private T LoadFromFile<T>(string filename) where T : class, new()
        {
            try
            {
                string path = GetFilePath(filename);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    return JsonUtility.FromJson<T>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load {filename}: {e.Message}");
            }
            return null;
        }

        private string GetFilePath(string filename)
        {
            return Path.Combine(Application.persistentDataPath, filename);
        }
    }

    /// <summary>
    /// Game settings data.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        [Header("Camera")]
        public float fieldOfView = 75f;
        public float cameraShakeIntensity = 1f;
        public bool enableCameraShake = true;

        [Header("Controls")]
        public float steeringSensitivity = 1f;
        public bool invertSteering = false;

        [Header("Audio")]
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;

        [Header("Graphics")]
        public int qualityLevel = 2;
        public bool enablePostProcessing = true;
        public bool enableVSync = true;

        [Header("Gameplay")]
        public bool showGhost = true;
        public bool showSpeedometer = true;
        public bool showTimer = true;
    }

    /// <summary>
    /// Game progress data.
    /// </summary>
    [Serializable]
    public class GameProgress
    {
        public int highestLevelUnlocked = 1;
        public int totalRuns;
        public float totalPlayTime;

        // Serialized dictionaries aren't supported, so we use parallel arrays
        public string[] trackIds = Array.Empty<string>();
        public float[] bestTimes = Array.Empty<float>();
        public int[] highScores = Array.Empty<int>();

        public float GetBestTime(string trackId)
        {
            int index = Array.IndexOf(trackIds, trackId);
            return index >= 0 && index < bestTimes.Length ? bestTimes[index] : 0f;
        }

        public void SetBestTime(string trackId, float time)
        {
            int index = Array.IndexOf(trackIds, trackId);
            if (index >= 0)
            {
                bestTimes[index] = time;
            }
            else
            {
                AddTrack(trackId, time, 0);
            }
        }

        public int GetHighScore(string trackId)
        {
            int index = Array.IndexOf(trackIds, trackId);
            return index >= 0 && index < highScores.Length ? highScores[index] : 0;
        }

        public void SetHighScore(string trackId, int score)
        {
            int index = Array.IndexOf(trackIds, trackId);
            if (index >= 0)
            {
                highScores[index] = score;
            }
            else
            {
                AddTrack(trackId, 0f, score);
            }
        }

        private void AddTrack(string trackId, float time, int score)
        {
            int newLength = trackIds.Length + 1;

            Array.Resize(ref trackIds, newLength);
            Array.Resize(ref bestTimes, newLength);
            Array.Resize(ref highScores, newLength);

            trackIds[newLength - 1] = trackId;
            bestTimes[newLength - 1] = time;
            highScores[newLength - 1] = score;
        }
    }
}
