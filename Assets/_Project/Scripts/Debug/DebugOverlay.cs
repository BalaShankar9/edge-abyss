using UnityEngine;
using UnityEngine.InputSystem;
using EdgeAbyss.Gameplay.Riders;
using EdgeAbyss.Gameplay.Score;
using TMPro;

namespace EdgeAbyss.Debug
{
    /// <summary>
    /// Runtime debug overlay toggled with F3 key.
    /// Shows detailed game state information.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [Header("Toggle Key")]
        [SerializeField] private Key toggleKey = Key.F3;

        [Header("Display Settings")]
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);
        [SerializeField] private Color textColor = Color.green;

        private bool _showOverlay = false;
        private RiderManager _riderManager;
        private ScoreManager _scoreManager;

        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;

        // Performance tracking
        private float _deltaTime;
        private float _fixedDeltaTime;

        private void Start()
        {
            _riderManager = FindFirstObjectByType<RiderManager>();
            _scoreManager = ScoreManager.Instance;
        }

        private void Update()
        {
            // Track performance
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

            if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                _showOverlay = !_showOverlay;
                UnityEngine.Debug.Log($"[DebugOverlay] {(_showOverlay ? "Enabled" : "Disabled")}");
            }
        }

        private void FixedUpdate()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnGUI()
        {
            if (!_showOverlay) return;

            // Initialize styles
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = MakeTexture(2, 2, backgroundColor);

                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = fontSize;
                _labelStyle.normal.textColor = textColor;
            }

            float panelWidth = 320f;
            float panelHeight = 400f;
            float x = Screen.width - panelWidth - 10f;
            float y = 10f;

            GUILayout.BeginArea(new Rect(x, y, panelWidth, panelHeight), _boxStyle);
            GUILayout.Space(5);

            // Header
            GUILayout.Label("═══ DEBUG OVERLAY (F3) ═══", _labelStyle);
            GUILayout.Space(5);

            // Performance
            float fps = 1f / _deltaTime;
            float ms = _deltaTime * 1000f;
            GUILayout.Label($"FPS: {fps:F1} ({ms:F2}ms)", _labelStyle);
            GUILayout.Label($"Fixed Delta: {_fixedDeltaTime * 1000:F2}ms", _labelStyle);
            GUILayout.Label($"Time Scale: {Time.timeScale:F2}", _labelStyle);
            GUILayout.Space(10);

            // Rider Info
            if (_riderManager != null && _riderManager.ActiveRider != null)
            {
                var rider = _riderManager.ActiveRider;
                GUILayout.Label("─── RIDER ───", _labelStyle);
                GUILayout.Label($"Type: {_riderManager.CurrentRiderType}", _labelStyle);
                GUILayout.Label($"Speed: {rider.Speed:F2} m/s", _labelStyle);
                GUILayout.Label($"Stability: {rider.Stability:F3}", _labelStyle);
                GUILayout.Label($"Is Grounded: {rider.IsGrounded}", _labelStyle);
                GUILayout.Label($"Is Falling: {rider.IsFalling}", _labelStyle);

                if (rider is MonoBehaviour mb)
                {
                    var pos = mb.transform.position;
                    var rot = mb.transform.eulerAngles;
                    GUILayout.Label($"Position: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})", _labelStyle);
                    GUILayout.Label($"Rotation: ({rot.x:F1}, {rot.y:F1}, {rot.z:F1})", _labelStyle);

                    var rb = mb.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        GUILayout.Label($"Velocity: {rb.linearVelocity.magnitude:F2} m/s", _labelStyle);
                        GUILayout.Label($"Angular: {rb.angularVelocity.magnitude:F2} rad/s", _labelStyle);
                    }
                }
            }
            else
            {
                GUILayout.Label("─── RIDER ───", _labelStyle);
                GUILayout.Label("No active rider", _labelStyle);
            }
            GUILayout.Space(10);

            // Score Info
            if (_scoreManager != null)
            {
                GUILayout.Label("─── SCORE ───", _labelStyle);
                GUILayout.Label($"Score: {_scoreManager.CurrentScore}", _labelStyle);
                GUILayout.Label($"Streak: {_scoreManager.CurrentStreak}", _labelStyle);
                GUILayout.Label($"Multiplier: {_scoreManager.CurrentMultiplier:F2}x", _labelStyle);
            }
            GUILayout.Space(10);

            // Input Info
            if (Keyboard.current != null)
            {
                GUILayout.Label("─── INPUT ───", _labelStyle);
                float v = 0f, h = 0f;
                if (Keyboard.current.wKey.isPressed) v += 1f;
                if (Keyboard.current.sKey.isPressed) v -= 1f;
                if (Keyboard.current.dKey.isPressed) h += 1f;
                if (Keyboard.current.aKey.isPressed) h -= 1f;
                GUILayout.Label($"Movement: V={v:F1} H={h:F1}", _labelStyle);
                GUILayout.Label($"Focus: {Keyboard.current.spaceKey.isPressed}", _labelStyle);
            }

            GUILayout.EndArea();
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
