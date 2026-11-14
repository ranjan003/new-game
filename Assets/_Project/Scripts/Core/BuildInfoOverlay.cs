// Location: Assets/_Project/Scripts/Core/BuildInfoOverlay.cs
// Assembly: EC.Core
// Milestone 1 smoke-test overlay: proves the build runs on device and shows FPS.
// It stays useful later as a debug overlay (toggle it off in release builds).
using UnityEngine;

namespace ElementalClash.Core
{
    /// <summary>Draws build, device and FPS info, and lets you switch the 30/60 FPS cap on device.</summary>
    public sealed class BuildInfoOverlay : MonoBehaviour
    {
        [SerializeField] private Transform _spinTarget;
        [SerializeField] private float _spinDegreesPerSecond = 90f;
        [SerializeField] private int _defaultTargetFps = 60;

        private float _smoothedDeltaTime;
        private float _worstDeltaThisSecond;
        private float _worstDeltaShown;
        private float _secondTimer;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;

        private void Awake()
        {
            Application.targetFrameRate = _defaultTargetFps;
            Time.fixedDeltaTime = GameConstants.CombatFixedDeltaTime;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _smoothedDeltaTime = 1f / Mathf.Max(1, _defaultTargetFps);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, dt, 0.1f);
            _worstDeltaThisSecond = Mathf.Max(_worstDeltaThisSecond, dt);
            _secondTimer += dt;
            if (_secondTimer >= 1f)
            {
                _worstDeltaShown = _worstDeltaThisSecond;
                _worstDeltaThisSecond = 0f;
                _secondTimer = 0f;
            }

            if (_spinTarget != null)
            {
                _spinTarget.Rotate(0f, _spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            Rect safe = Screen.safeArea;
            float pad = Screen.height * 0.02f;
            float lineHeight = _labelStyle.fontSize * 1.4f;
            float x = safe.xMin + pad;
            // OnGUI's origin is top-left; Screen.safeArea's is bottom-left.
            float y = (Screen.height - safe.yMax) + pad;

            float fps = 1f / Mathf.Max(0.0001f, _smoothedDeltaTime);
            float worstMs = _worstDeltaShown * 1000f;

            string[] lines =
            {
                GameConstants.GameTitle,
                $"Build {Application.version}  |  Unity {Application.unityVersion}",
                $"FPS {fps:0}  (cap {Application.targetFrameRate})  worst frame {worstMs:0} ms",
                $"{SystemInfo.deviceModel}  |  {SystemInfo.graphicsDeviceType}",
                $"Screen {Screen.width}x{Screen.height}  |  Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}",
                $"Fixed tick {1f / Time.fixedDeltaTime:0} Hz",
            };

            for (int i = 0; i < lines.Length; i++)
            {
                GUI.Label(new Rect(x, y + i * lineHeight, safe.width - pad * 2f, lineHeight), lines[i], _labelStyle);
            }

            float buttonWidth = Screen.height * 0.32f;
            float buttonHeight = Screen.height * 0.1f;
            float buttonY = y + lines.Length * lineHeight + pad;

            int nextCap = Application.targetFrameRate == 60 ? 30 : 60;
            if (GUI.Button(new Rect(x, buttonY, buttonWidth, buttonHeight), $"Cap to {nextCap} FPS", _buttonStyle))
            {
                Application.targetFrameRate = nextCap;
            }

            if (GUI.Button(new Rect(x + buttonWidth + pad, buttonY, buttonWidth, buttonHeight), "Next quality", _buttonStyle))
            {
                int next = (QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length;
                QualitySettings.SetQualityLevel(next, true);
            }
        }

        private void EnsureStyles()
        {
            int fontSize = Mathf.Max(14, Mathf.RoundToInt(Screen.height * 0.035f));
            if (_labelStyle != null && _labelStyle.fontSize == fontSize) return;

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize };
            _labelStyle.normal.textColor = Color.white;
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };
        }
    }
}
