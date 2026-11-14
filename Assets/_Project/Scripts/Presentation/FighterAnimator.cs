// Location: Assets/_Project/Scripts/Presentation/FighterAnimator.cs
// Assembly: EC.Presentation
// Drives a rigged humanoid model (Mixamo or similar) from the Fighter's combat state.
// It only reads gameplay state; it never changes it. Root motion is off: the CharacterController moves the fighter.
//
// Attack clips are time-stretched so the clip's impact moment lands on the move's first active frame,
// so what you see matches the hitbox. Tune each slot's "impact" (0..1 of the clip) in the Inspector.
//
// Animator states it expects (FighterModelImporter builds the controller):
//   Locomotion (2D blend on MoveX/MoveZ), Light1, Light2, Light3, Special1, HitReact, Knockdown, KO, Victory
using ElementalClash.Combat;
using ElementalClash.Data;
using UnityEngine;

namespace ElementalClash.Presentation
{
    [RequireComponent(typeof(Fighter))]
    public sealed class FighterAnimator : MonoBehaviour
    {
        public const int SlotLight1 = 0;
        public const int SlotLight2 = 1;
        public const int SlotLight3 = 2;
        public const int SlotSpecial1 = 3;
        public const int SlotCount = 4;

        private static readonly int MoveXId = Animator.StringToHash("MoveX");
        private static readonly int MoveZId = Animator.StringToHash("MoveZ");
        private static readonly int LocomotionId = Animator.StringToHash("Locomotion");
        private static readonly int HitReactId = Animator.StringToHash("HitReact");
        private static readonly int KnockdownId = Animator.StringToHash("Knockdown");
        private static readonly int KOId = Animator.StringToHash("KO");
        private static readonly int VictoryId = Animator.StringToHash("Victory");
        private static readonly int[] AttackIds =
        {
            Animator.StringToHash("Light1"), Animator.StringToHash("Light2"),
            Animator.StringToHash("Light3"), Animator.StringToHash("Special1"),
        };

        [SerializeField] private Animator _animator;
        [Tooltip("Pivot above the model; scaled for Noxar's shadow sink.")]
        [SerializeField] private Transform _visualRoot;
        [Tooltip("Clip length in seconds per slot: Light1, Light2, Light3, Special1. Filled in by the builder.")]
        [SerializeField] private float[] _attackClipLengths = { 0.8f, 0.8f, 1f, 1f };
        [Tooltip("When the strike connects in each clip, as a fraction of the clip (0..1).")]
        [SerializeField] private float[] _attackImpact = { 0.35f, 0.35f, 0.4f, 0.4f };
        [SerializeField] private float _crossFade = 0.06f;
        [SerializeField] private float _locomotionFade = 0.15f;

        private Fighter _fighter;
        private FighterState _lastState = (FighterState)(-1);
        private MoveDefinition _lastMove;
        private int _lastChain = -2;
        private int _lastFrame;
        private int _attackSlot = -1;
        private float _attackSpeed = 1f;
        private Vector3 _lastPosition;
        private Vector2 _smoothedMove;
        private bool _hitThisFrame;

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
            if (_animator != null) _animator.applyRootMotion = false;
            _lastPosition = transform.position;
        }

        private void OnEnable() => CombatEvents.Hit += OnHit;
        private void OnDisable() => CombatEvents.Hit -= OnHit;

        private void OnHit(HitInfo hit)
        {
            if (hit.Defender == _fighter) _hitThisFrame = true;
        }

        private void LateUpdate()
        {
            if (_animator == null || _fighter == null || _animator.runtimeAnimatorController == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            UpdateLocomotionParams(dt);

            FighterState state = _fighter.State;
            MoveDefinition move = _fighter.CurrentMove;
            int chain = _fighter.CurrentChainIndex;
            int frame = _fighter.CurrentMoveFrame;

            bool stateChanged = state != _lastState;
            switch (state)
            {
                case FighterState.Idle:
                    if (stateChanged) Play(LocomotionId, _locomotionFade, 1f);
                    break;

                case FighterState.Attack:
                    bool newAttack = stateChanged || move != _lastMove || chain != _lastChain || frame < _lastFrame;
                    if (newAttack && move != null) StartAttack(move, chain);
                    break;

                case FighterState.Hitstun:
                    if (stateChanged || _hitThisFrame) Play(HitReactId, 0.03f, 1f, restart: true);
                    break;

                case FighterState.Knockdown:
                    if (stateChanged) Play(KnockdownId, 0.05f, 1f);
                    break;

                case FighterState.KO:
                    if (stateChanged) Play(KOId, 0.05f, 1f);
                    break;

                case FighterState.Victory:
                    if (stateChanged) Play(VictoryId, 0.2f, 1f);
                    break;
            }

            // Freeze the pose during hit-stop so impacts read as heavy.
            _animator.speed = _fighter.IsInHitStop ? 0f : (state == FighterState.Attack ? _attackSpeed : 1f);

            UpdateTeleportSink(state, move, frame);

            _lastState = state;
            _lastMove = move;
            _lastChain = chain;
            _lastFrame = frame;
            _hitThisFrame = false;
        }

        private void UpdateLocomotionParams(float dt)
        {
            Vector3 velocity = (transform.position - _lastPosition) / dt;
            _lastPosition = transform.position;
            // Ignore teleports/resets (huge jumps) so the blend tree doesn't spike.
            if (velocity.sqrMagnitude > 400f) velocity = Vector3.zero;
            Vector3 local = transform.InverseTransformDirection(velocity);
            var target = new Vector2(local.x, local.z);
            _smoothedMove = Vector2.Lerp(_smoothedMove, target, 1f - Mathf.Exp(-12f * dt));
            _animator.SetFloat(MoveXId, _smoothedMove.x);
            _animator.SetFloat(MoveZId, _smoothedMove.y);
        }

        private void StartAttack(MoveDefinition move, int chain)
        {
            int slot = move.Kind == MoveKind.Light ? Mathf.Clamp(chain, 0, 2) : SlotSpecial1;
            _attackSlot = slot;

            float length = slot < _attackClipLengths.Length ? Mathf.Max(0.05f, _attackClipLengths[slot]) : 1f;
            float impact = slot < _attackImpact.Length ? Mathf.Clamp(_attackImpact[slot], 0.05f, 0.95f) : 0.4f;
            float startupSeconds = Mathf.Max(1, move.StartupFrames) / 60f;
            // Stretch so the clip's impact moment lands on the first active frame.
            _attackSpeed = Mathf.Clamp(length * impact / startupSeconds, 0.4f, 3f);

            Play(AttackIds[slot], _crossFade, _attackSpeed, restart: true);
        }

        private void Play(int stateId, float fade, float speed, bool restart = false)
        {
            if (!_animator.HasState(0, stateId)) return;
            _animator.speed = speed;
            if (restart) _animator.CrossFadeInFixedTime(stateId, fade, 0, 0f);
            else _animator.CrossFadeInFixedTime(stateId, fade, 0);
        }

        private void UpdateTeleportSink(FighterState state, MoveDefinition move, int frame)
        {
            if (_visualRoot == null) return;
            Vector3 scale = Vector3.one;
            if (state == FighterState.Attack && move != null && move.Movement == MoveMovement.TeleportBeside &&
                move.IsStartupFrame(frame))
            {
                float sink = Mathf.Clamp01(frame / (float)Mathf.Max(1, move.StartupFrames));
                scale = new Vector3(1f + sink * 0.3f, Mathf.Max(0.15f, 1f - sink), 1f + sink * 0.3f);
            }
            _visualRoot.localScale = Vector3.Lerp(_visualRoot.localScale, scale, 1f - Mathf.Exp(-25f * Time.deltaTime));
        }
    }
}
