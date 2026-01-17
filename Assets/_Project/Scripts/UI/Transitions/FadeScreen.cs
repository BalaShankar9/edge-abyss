using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeAbyss.UI
{
    /// <summary>
    /// Simple fullscreen fade effect for scene transitions and respawns.
    /// Uses a Canvas with a fullscreen Image.
    /// 
    /// SETUP:
    /// 1. Create UI Canvas (Screen Space - Overlay, Sort Order = 999).
    /// 2. Add Image as child, stretch to fill entire canvas.
    /// 3. Set Image color to black (or desired fade color).
    /// 4. Attach this FadeScreen component to the Canvas.
    /// 5. Assign the Image reference.
    /// 6. Optionally set to start transparent or opaque.
    /// </summary>
    public class FadeScreen : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The fullscreen image used for fading.")]
        [SerializeField] private Image fadeImage;

        [Header("Settings")]
        [Tooltip("Color to fade to/from (typically black).")]
        [SerializeField] private Color fadeColor = Color.black;

        [Tooltip("Start with screen faded out (black).")]
        [SerializeField] private bool startFadedOut;

        [Tooltip("Default fade duration if not specified.")]
        [SerializeField] private float defaultFadeDuration = 0.5f;

        // State
        private float _currentAlpha;
        private Coroutine _fadeCoroutine;
        private bool _isFading;

        /// <summary>True if currently fading.</summary>
        public bool IsFading => _isFading;

        /// <summary>True if screen is fully opaque (faded out).</summary>
        public bool IsBlack => _currentAlpha >= 0.99f;

        /// <summary>True if screen is fully transparent (faded in).</summary>
        public bool IsClear => _currentAlpha <= 0.01f;

        /// <summary>Event fired when fade out completes (screen is black).</summary>
        public event Action OnFadeOutComplete;

        /// <summary>Event fired when fade in completes (screen is clear).</summary>
        public event Action OnFadeInComplete;

        private void Awake()
        {
            if (fadeImage == null)
            {
                fadeImage = GetComponentInChildren<Image>();
            }

            if (fadeImage == null)
            {
                Debug.LogError($"[{nameof(FadeScreen)}] No Image component found. Fade will not work.");
                return;
            }

            // Initialize
            _currentAlpha = startFadedOut ? 1f : 0f;
            ApplyAlpha(_currentAlpha);
        }

        /// <summary>
        /// Fades the screen to black (opaque).
        /// </summary>
        /// <param name="duration">Duration of the fade. Uses default if <= 0.</param>
        public void FadeOut(float duration = -1f)
        {
            if (duration <= 0f) duration = defaultFadeDuration;
            StartFade(1f, duration, OnFadeOutComplete);
        }

        /// <summary>
        /// Fades the screen to clear (transparent).
        /// </summary>
        /// <param name="duration">Duration of the fade. Uses default if <= 0.</param>
        public void FadeIn(float duration = -1f)
        {
            if (duration <= 0f) duration = defaultFadeDuration;
            StartFade(0f, duration, OnFadeInComplete);
        }

        /// <summary>
        /// Immediately sets screen to black.
        /// </summary>
        public void SetBlack()
        {
            StopCurrentFade();
            _currentAlpha = 1f;
            ApplyAlpha(1f);
        }

        /// <summary>
        /// Immediately sets screen to clear.
        /// </summary>
        public void SetClear()
        {
            StopCurrentFade();
            _currentAlpha = 0f;
            ApplyAlpha(0f);
        }

        /// <summary>
        /// Performs a full fade out then fade in sequence.
        /// </summary>
        /// <param name="fadeOutDuration">Duration of fade to black.</param>
        /// <param name="holdDuration">Time to hold at black.</param>
        /// <param name="fadeInDuration">Duration of fade to clear.</param>
        /// <param name="onBlack">Action to perform while screen is black.</param>
        public void DoFadeSequence(float fadeOutDuration, float holdDuration, float fadeInDuration, Action onBlack = null)
        {
            StopCurrentFade();
            _fadeCoroutine = StartCoroutine(FadeSequenceCoroutine(fadeOutDuration, holdDuration, fadeInDuration, onBlack));
        }

        private void StartFade(float targetAlpha, float duration, Action onComplete)
        {
            StopCurrentFade();
            _fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, duration, onComplete));
        }

        private void StopCurrentFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _isFading = false;
        }

        private IEnumerator FadeCoroutine(float targetAlpha, float duration, Action onComplete)
        {
            if (fadeImage == null) yield break;

            _isFading = true;
            float startAlpha = _currentAlpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled for pause-safety
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Smooth step for nicer fade
                t = t * t * (3f - 2f * t);
                
                _currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                ApplyAlpha(_currentAlpha);
                yield return null;
            }

            _currentAlpha = targetAlpha;
            ApplyAlpha(targetAlpha);
            _isFading = false;
            _fadeCoroutine = null;

            onComplete?.Invoke();
        }

        private IEnumerator FadeSequenceCoroutine(float fadeOutDuration, float holdDuration, float fadeInDuration, Action onBlack)
        {
            // Fade out
            yield return FadeCoroutine(1f, fadeOutDuration, null);

            // Perform action while black
            onBlack?.Invoke();
            OnFadeOutComplete?.Invoke();

            // Hold
            yield return new WaitForSecondsRealtime(holdDuration);

            // Fade in
            yield return FadeCoroutine(0f, fadeInDuration, null);

            OnFadeInComplete?.Invoke();
            _fadeCoroutine = null;
        }

        private void ApplyAlpha(float alpha)
        {
            if (fadeImage == null) return;

            Color c = fadeColor;
            c.a = alpha;
            fadeImage.color = c;

            // Disable raycast blocking when fully transparent
            fadeImage.raycastTarget = alpha > 0.01f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (fadeImage != null && !Application.isPlaying)
            {
                ApplyAlpha(startFadedOut ? 1f : 0f);
            }
        }
#endif
    }
}
