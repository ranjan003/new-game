// Location: Assets/_Project/Scripts/UI/CombatDebugHUD.cs
// Assembly: EC.UI
// Batch B test HUD drawn with IMGUI: HP/EP bars, states, combo, last hit, KO banner and test buttons.
// The real uGUI HUD comes in Batch D.
using ElementalClash.AI;
using ElementalClash.Combat;
using UnityEngine;

namespace ElementalClash.UI
{
    public sealed class CombatDebugHUD : MonoBehaviour
    {
        [SerializeField] private Fighter _player;
        [SerializeField] private Fighter _opponent;
        [SerializeField] private AIBrain _ai;

        private string _lastHit = "-";
        private int _bestCombo;
        private string _koText;
        private GUIStyle _label;
        private GUIStyle _big;
        private GUIStyle _button;

        private void OnEnable()
        {
            CombatEvents.Hit += OnHit;
            CombatEvents.KnockedOut += OnKnockedOut;
        }

        private void OnDisable()
        {
            CombatEvents.Hit -= OnHit;
            CombatEvents.KnockedOut -= OnKnockedOut;
        }

        private void OnHit(HitInfo hit)
        {
            string tag = hit.Result.CounterHit ? " COUNTER" : (hit.Result.Punish ? " PUNISH" : "");
            _lastHit = $"{hit.Attacker.Definition.DisplayName} {hit.Move.DisplayName}: {hit.Result.Damage:0.0} dmg, " +
                       $"{hit.Result.HitstunFrames}f stun, combo {hit.ComboCount}{tag}";
            if (hit.Attacker == _player) _bestCombo = Mathf.Max(_bestCombo, hit.ComboCount);
        }

        private void OnKnockedOut(Fighter loser, Fighter winner)
        {
            winner.SetVictory();
            _koText = $"K.O.!  {winner.Definition.DisplayName} wins";
        }

        private void OnGUI()
        {
            if (_player == null || _opponent == null || _player.Meters == null || _opponent.Meters == null) return;
            EnsureStyles();

            float w = Screen.width;
            float h = Screen.height;
            float pad = h * 0.02f;
            float barWidth = w * 0.38f;
            float barHeight = h * 0.035f;

            DrawFighterPanel(_player, new Rect(pad, pad, barWidth, barHeight), false);
            DrawFighterPanel(_opponent, new Rect(w - pad - barWidth, pad, barWidth, barHeight), true);

            float lineH = _label.fontSize * 1.5f;
            float y = pad + barHeight * 2f + lineH * 2.5f;
            GUI.Label(new Rect(pad, y, w - pad * 2f, lineH), "Last hit: " + _lastHit, _label);
            GUI.Label(new Rect(pad, y + lineH, w - pad * 2f, lineH), $"Your best combo: {_bestCombo}", _label);

            if (!string.IsNullOrEmpty(_koText))
            {
                GUI.Label(new Rect(0f, h * 0.35f, w, h * 0.15f), _koText, _big);
            }

            // Buttons sit under the info lines so they stay clear of the bars and the touch controls.
            float bw = h * 0.26f;
            float bh = h * 0.075f;
            float by = y + lineH * 2.2f;
            float bx = w * 0.5f - bw - pad * 0.5f;
            if (GUI.Button(new Rect(bx, by, bw, bh), "Reset fighters", _button))
            {
                _player.ResetToSpawn();
                _opponent.ResetToSpawn();
                _koText = null;
                _bestCombo = 0;
                _lastHit = "-";
            }

            if (_ai != null)
            {
                string label = _ai.Active ? "AI: fighting" : "AI: passive";
                if (GUI.Button(new Rect(bx + bw + pad, by, bw, bh), label, _button))
                {
                    _ai.Active = !_ai.Active;
                }
            }

            if (!Application.isMobilePlatform)
            {
                GUI.Label(new Rect(pad, h - pad - lineH, w - pad * 2f, lineH),
                    "Move WASD  |  Light J  |  Special U  |  drag right side to orbit camera  (gamepad: stick, X / Square, RB)", _label);
            }
        }

        private void DrawFighterPanel(Fighter fighter, Rect bar, bool alignRight)
        {
            FighterMeters m = fighter.Meters;
            DrawBar(bar, m.Health / m.MaxHealth, new Color(0.2f, 0.85f, 0.35f), alignRight);
            var energyBar = new Rect(bar.x, bar.yMax + 2f, bar.width * 0.7f, bar.height * 0.5f);
            if (alignRight) energyBar.x = bar.xMax - energyBar.width;
            DrawBar(energyBar, m.Energy / m.MaxEnergy, fighter.Definition.PlaceholderColor, alignRight);

            string special = "";
            if (fighter.Definition.Special1 != null)
            {
                float cd = m.CooldownRemaining(fighter.Definition.Special1);
                special = cd > 0f ? $"  {fighter.Definition.Special1.DisplayName} {cd:0.0}s" : $"  {fighter.Definition.Special1.DisplayName} ready";
            }

            string move = fighter.CurrentMove != null ? $" {fighter.CurrentMove.DisplayName} f{fighter.CurrentMoveFrame}" : "";
            string text = $"{fighter.Definition.DisplayName}  HP {m.Health:0}/{m.MaxHealth:0}  EP {m.Energy:0}{special}\n{fighter.State}{move}";
            var labelRect = new Rect(bar.x, energyBar.yMax + 2f, bar.width, _label.fontSize * 3f);
            _label.alignment = alignRight ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            GUI.Label(labelRect, text, _label);
            _label.alignment = TextAnchor.UpperLeft;
        }

        private static void DrawBar(Rect rect, float fill, Color color, bool alignRight)
        {
            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            float width = rect.width * Mathf.Clamp01(fill);
            var fillRect = new Rect(alignRight ? rect.xMax - width : rect.x, rect.y, width, rect.height);
            GUI.color = color;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void EnsureStyles()
        {
            int size = Mathf.Max(12, Mathf.RoundToInt(Screen.height * 0.025f));
            if (_label != null && _label.fontSize == size) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = size, wordWrap = false };
            _label.normal.textColor = Color.white;
            _big = new GUIStyle(_label) { fontSize = size * 4, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            _big.normal.textColor = new Color(1f, 0.85f, 0.2f);
            _button = new GUIStyle(GUI.skin.button) { fontSize = size };
        }
    }
}
