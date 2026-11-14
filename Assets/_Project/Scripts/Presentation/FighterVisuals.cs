// Location: Assets/_Project/Scripts/Presentation/FighterVisuals.cs
// Assembly: EC.Presentation
// Procedural placeholder "animation" for capsule fighters until real rigged models arrive:
// fists wind up and punch on the frame data, body leans when hit, falls on knockdown/KO,
// sinks for teleports, and celebrates on victory. Reads the Fighter state; never changes gameplay.
//
// Hierarchy expected (the sandbox builder creates it):
//   Fighter root
//     Visual        <- this pivot is tilted/scaled (at the feet)
//       Body, FacingVisor, Fist_L, Fist_R
using ElementalClash.Combat;
using ElementalClash.Data;
using UnityEngine;

namespace ElementalClash.Presentation
{
    [RequireComponent(typeof(Fighter))]
    public sealed class FighterVisuals : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _leftFist;
        [SerializeField] private Transform _rightFist;
        [SerializeField] private float _sharpness = 25f;

        private static readonly Vector3 GuardLeft = new Vector3(-0.32f, 1.25f, 0.35f);
        private static readonly Vector3 GuardRight = new Vector3(0.32f, 1.2f, 0.3f);

        private Fighter _fighter;
        private float _spin;
        private bool _directRotation;

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
        }

        private void LateUpdate()
        {
            if (_fighter == null || _visualRoot == null) return;

            Vector3 left = GuardLeft;
            Vector3 right = GuardRight;
            Quaternion tilt = Quaternion.identity;
            Vector3 scale = Vector3.one;
            _directRotation = false;
            float bob = Mathf.Sin(Time.time * 4f) * 0.02f;

            switch (_fighter.State)
            {
                case FighterState.Idle:
                    left.y += bob;
                    right.y -= bob;
                    _spin = 0f;
                    break;

                case FighterState.Attack:
                    PoseAttack(ref left, ref right, ref tilt, ref scale);
                    break;

                case FighterState.Hitstun:
                    tilt = Quaternion.Euler(-14f, 0f, 0f);
                    left += new Vector3(0f, 0.1f, -0.15f);
                    right += new Vector3(0f, 0.1f, -0.15f);
                    break;

                case FighterState.Knockdown:
                case FighterState.KO:
                    tilt = Quaternion.Euler(-80f, 0f, 0f);
                    break;

                case FighterState.Victory:
                    left = new Vector3(-0.35f, 2.0f + bob * 3f, 0.05f);
                    right = new Vector3(0.35f, 2.0f - bob * 3f, 0.05f);
                    break;
            }

            float t = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
            _visualRoot.localRotation = _directRotation ? tilt : Quaternion.Slerp(_visualRoot.localRotation, tilt, t);
            _visualRoot.localScale = Vector3.Lerp(_visualRoot.localScale, scale, t);
            if (_leftFist != null) _leftFist.localPosition = Vector3.Lerp(_leftFist.localPosition, left, t);
            if (_rightFist != null) _rightFist.localPosition = Vector3.Lerp(_rightFist.localPosition, right, t);
        }

        private void PoseAttack(ref Vector3 left, ref Vector3 right, ref Quaternion tilt, ref Vector3 scale)
        {
            MoveDefinition move = _fighter.CurrentMove;
            if (move == null) return;

            int frame = _fighter.CurrentMoveFrame;
            Vector3 strike = new Vector3(0f, move.HitboxHeight, move.HitboxForward + move.HitboxRadius * 0.5f);

            // 0 at the start of startup -> 1 at first active frame -> back to 0 by the end of recovery.
            float windup = move.IsStartupFrame(frame) ? frame / (float)move.StartupFrames : 0f;
            float extend = move.IsActiveFrame(frame) ? 1f :
                move.IsRecoveryFrame(frame)
                    ? 1f - (frame - move.StartupFrames - move.ActiveFrames) / (float)Mathf.Max(1, move.RecoveryFrames)
                    : 0f;

            if (move.Movement == MoveMovement.TeleportBeside && move.IsStartupFrame(frame))
            {
                // Sink into the shadow (telegraph), then pop out beside the opponent.
                float sink = Mathf.Clamp01(windup);
                scale = new Vector3(1f + sink * 0.3f, Mathf.Max(0.15f, 1f - sink), 1f + sink * 0.3f);
                return;
            }

            if (move.Kind == MoveKind.Special)
            {
                // Both fists forward, lean into the dash.
                left = Vector3.Lerp(GuardLeft, strike + new Vector3(-0.2f, 0f, 0f), Mathf.Max(extend, windup * 0.3f));
                right = Vector3.Lerp(GuardRight, strike + new Vector3(0.2f, 0f, 0f), Mathf.Max(extend, windup * 0.3f));
                tilt = Quaternion.Euler(12f * Mathf.Max(extend, windup), 0f, 0f);
                return;
            }

            int chain = _fighter.CurrentChainIndex;
            if (chain >= 2)
            {
                // Finisher: a spin with both arms out.
                _spin = Mathf.Clamp01(windup * 0.3f + extend) * 360f;
                tilt = Quaternion.Euler(0f, _spin, 0f);
                _directRotation = true;
                left = new Vector3(-0.8f * Mathf.Max(extend, 0.3f), 1.0f, 0.2f);
                right = new Vector3(0.8f * Mathf.Max(extend, 0.3f), 1.0f, 0.2f);
                return;
            }

            // Alternate hands: L1 right, L2 left. Pull back during startup, punch on active frames.
            bool useRight = chain != 1;
            Vector3 guard = useRight ? GuardRight : GuardLeft;
            Vector3 pulled = guard + new Vector3(0f, 0f, -0.25f);
            Vector3 punch = strike + new Vector3(useRight ? 0.1f : -0.1f, 0f, 0f);
            Vector3 pos = extend > 0f ? Vector3.Lerp(guard, punch, extend) : Vector3.Lerp(guard, pulled, windup);
            if (useRight) right = pos;
            else left = pos;
            tilt = Quaternion.Euler(6f * extend, 0f, 0f);
        }
    }
}
