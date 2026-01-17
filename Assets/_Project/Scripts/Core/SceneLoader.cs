using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EdgeAbyss.Core
{
    /// <summary>
    /// Handles async scene loading with progress callbacks and additive mode support.
    /// Use as a static utility or extend for custom behavior.
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>
        /// Represents the current state of a scene load operation.
        /// </summary>
        public readonly struct LoadProgress
        {
            public readonly float Progress;
            public readonly bool IsDone;

            public LoadProgress(float progress, bool isDone)
            {
                Progress = progress;
                IsDone = isDone;
            }
        }

        /// <summary>
        /// Loads a scene asynchronously.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="additive">If true, loads the scene additively without unloading the current scene.</param>
        /// <param name="onProgress">Optional callback invoked each frame with load progress (0-1).</param>
        /// <param name="onComplete">Optional callback invoked when loading completes.</param>
        /// <returns>Coroutine enumerator for use with StartCoroutine.</returns>
        public static IEnumerator LoadSceneAsync(
            string sceneName,
            bool additive = false,
            Action<LoadProgress> onProgress = null,
            Action onComplete = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"[{nameof(SceneLoader)}] Scene name is null or empty.");
                yield break;
            }

            var loadMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, loadMode);

            if (asyncOp == null)
            {
                Debug.LogError($"[{nameof(SceneLoader)}] Failed to start loading scene '{sceneName}'. Ensure it is added to Build Settings.");
                yield break;
            }

            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                // Unity reports progress from 0-0.9 during load, then jumps to 1.0 on activation
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                onProgress?.Invoke(new LoadProgress(progress, false));
                yield return null;
            }

            onProgress?.Invoke(new LoadProgress(1f, true));
            onComplete?.Invoke();
        }

        /// <summary>
        /// Loads a scene asynchronously with a delayed activation.
        /// Useful for showing a loading screen until ready.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="additive">If true, loads the scene additively.</param>
        /// <param name="onProgress">Callback invoked each frame with load progress.</param>
        /// <param name="shouldActivate">Function that returns true when the scene should activate.</param>
        /// <param name="onComplete">Callback invoked when loading and activation complete.</param>
        /// <returns>Coroutine enumerator for use with StartCoroutine.</returns>
        public static IEnumerator LoadSceneWithDelayedActivation(
            string sceneName,
            bool additive,
            Action<LoadProgress> onProgress,
            Func<bool> shouldActivate,
            Action onComplete = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"[{nameof(SceneLoader)}] Scene name is null or empty.");
                yield break;
            }

            var loadMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, loadMode);

            if (asyncOp == null)
            {
                Debug.LogError($"[{nameof(SceneLoader)}] Failed to start loading scene '{sceneName}'.");
                yield break;
            }

            asyncOp.allowSceneActivation = false;

            // Load until 90% (Unity's threshold before activation)
            while (asyncOp.progress < 0.9f)
            {
                float progress = asyncOp.progress / 0.9f;
                onProgress?.Invoke(new LoadProgress(progress, false));
                yield return null;
            }

            onProgress?.Invoke(new LoadProgress(1f, false));

            // Wait for activation signal
            while (shouldActivate != null && !shouldActivate())
            {
                yield return null;
            }

            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            onProgress?.Invoke(new LoadProgress(1f, true));
            onComplete?.Invoke();
        }

        /// <summary>
        /// Unloads a scene asynchronously.
        /// </summary>
        /// <param name="sceneName">The name of the scene to unload.</param>
        /// <param name="onComplete">Callback invoked when unloading completes.</param>
        /// <returns>Coroutine enumerator for use with StartCoroutine.</returns>
        public static IEnumerator UnloadSceneAsync(string sceneName, Action onComplete = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"[{nameof(SceneLoader)}] Scene name is null or empty.");
                yield break;
            }

            AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);

            if (asyncOp == null)
            {
                Debug.LogError($"[{nameof(SceneLoader)}] Failed to unload scene '{sceneName}'. Is it currently loaded?");
                yield break;
            }

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
