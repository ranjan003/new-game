// Location: Assets/_Project/Scripts/AI/AIBrain.cs
// Assembly: EC.AI
// Basic opponent AI (doc 05 §7). It "presses buttons" through the same IFighterCommandSource
// interface as the player, so it obeys the same frame data, energy and cooldowns.
//
// Inspector:
//   Profile -> ScriptableObjects/AI/AI_Normal
// Put it on the same GameObject as the Fighter (and no other command source).
using ElementalClash.Combat;
using ElementalClash.Data;
using UnityEngine;

namespace ElementalClash.AI
{
    [RequireComponent(typeof(Fighter))]
    public sealed class AIBrain : MonoBehaviour, IFighterCommandSource
    {
        private enum Mode
        {
            Neutral,
            Attacking,
            Recovering,
            Retreating,
        }

        private const float PressLifetime = 0.25f;
        private const float StationaryDistance = 0.3f;

        [SerializeField] private AIProfile _profile;
        [SerializeField] private bool _active = true;

        private readonly float[] _pendingPresses = new float[FighterActions.Count];
        private readonly AIPerception _perception = new AIPerception();
        private Fighter _self;
        private Vector3 _moveDirection;
        private Mode _mode;
        private float _modeTimer;
        private float _decisionTimer;
        private float _strafeSign = 1f;
        private int _plannedChainLength;
        private bool _plannedSpecial;
        private int _lastChainIndex = -1;
        private int _worstComboTaken;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                ResetBrain();
            }
        }

        public AIProfile Profile => _profile;

        private void Awake()
        {
            _self = GetComponent<Fighter>();
            ResetBrain();
        }

        private void ResetBrain()
        {
            for (int i = 0; i < _pendingPresses.Length; i++) _pendingPresses[i] = float.NegativeInfinity;
            _perception.Clear();
            _moveDirection = Vector3.zero;
            _mode = Mode.Neutral;
            _modeTimer = 0f;
            _decisionTimer = 0f;
            _plannedChainLength = 0;
            _plannedSpecial = false;
            _lastChainIndex = -1;
            _worstComboTaken = 0;
        }

        // ------------------------------------------------------------------ IFighterCommandSource

        public Vector3 GetMoveDirection(Fighter self) => _active ? _moveDirection : Vector3.zero;

        public bool ConsumePress(FighterAction action)
        {
            if (!_active) return false;
            int i = (int)action;
            if (Time.time - _pendingPresses[i] > PressLifetime) return false;
            _pendingPresses[i] = float.NegativeInfinity;
            return true;
        }

        public bool IsHeld(FighterAction action) => false;

        private void Press(FighterAction action) => _pendingPresses[(int)action] = Time.time;

        // ------------------------------------------------------------------ Thinking

        private void FixedUpdate()
        {
            Fighter opponent = _self != null ? _self.Opponent : null;
            if (opponent == null || _profile == null) return;

            _perception.Record(new OpponentSnapshot
            {
                Time = Time.time,
                Position = opponent.transform.position,
                IsAttacking = opponent.State == FighterState.Attack,
                IsInRecovery = opponent.IsInRecovery,
                IsVulnerable = opponent.State != FighterState.KO && opponent.State != FighterState.Victory,
                HealthFraction = opponent.Meters != null ? opponent.Meters.Health / opponent.Meters.MaxHealth : 1f,
            });

            if (!_active) return;

            _worstComboTaken = Mathf.Max(_worstComboTaken, _self.CombosTaken);
            TrackChain();

            _decisionTimer -= Time.fixedDeltaTime;
            if (_modeTimer > 0f) _modeTimer -= Time.fixedDeltaTime;
            if (_decisionTimer > 0f) return;
            _decisionTimer = _profile.DecisionInterval;

            Decide();
        }

        private void TrackChain()
        {
            if (_mode != Mode.Attacking) return;

            int chain = _self.CurrentChainIndex;
            if (chain != _lastChainIndex && chain >= 0)
            {
                _lastChainIndex = chain;
            }

            // Keep pressing (buffered) while the plan has more hits to come.
            if (_self.State == FighterState.Attack && chain >= 0)
            {
                if (chain + 1 < _plannedChainLength) Press(FighterAction.Light);
                else if (_plannedSpecial && chain + 1 >= _plannedChainLength) Press(FighterAction.Special1);
            }
        }

        private void Decide()
        {
            FighterState state = _self.State;

            // Stunned, knocked down, KO or celebrating: nothing to decide.
            if (state == FighterState.Hitstun || state == FighterState.Knockdown ||
                state == FighterState.KO || state == FighterState.Victory)
            {
                _moveDirection = Vector3.zero;
                return;
            }

            float delay = Random.Range(_profile.ReactionTimeRange.x, _profile.ReactionTimeRange.y);
            if (!_perception.TryGetDelayed(Time.time, delay, out OpponentSnapshot seen)) return;
            if (!seen.IsVulnerable)
            {
                _moveDirection = Vector3.zero;
                return;
            }

            Vector3 toOpponent = seen.Position - transform.position;
            toOpponent.y = 0f;
            float distance = toOpponent.magnitude;
            Vector3 towards = distance > 0.01f ? toOpponent / distance : transform.forward;

            // A finished attack plan turns into a short breather (gives the player an opening).
            if (_mode == Mode.Attacking && state == FighterState.Idle)
            {
                EnterMode(Mode.Recovering, RandomIn(_profile.RecoverPauseRange));
            }

            // Took a beating while behind on health: back off for a moment.
            if (state == FighterState.Idle && _worstComboTaken >= _profile.RetreatAfterComboHits &&
                _self.Meters.Health / _self.Meters.MaxHealth < seen.HealthFraction)
            {
                _worstComboTaken = 0;
                EnterMode(Mode.Retreating, RandomIn(_profile.RetreatDurationRange));
            }
            if (state == FighterState.Idle && _self.CombosTaken == 0 && _mode != Mode.Retreating)
            {
                _worstComboTaken = 0;
            }

            switch (_mode)
            {
                case Mode.Recovering:
                    _moveDirection = Strafe(towards) * 0.35f;
                    if (_modeTimer <= 0f) _mode = Mode.Neutral;
                    return;

                case Mode.Retreating:
                    _moveDirection = (-towards + Strafe(towards) * 0.5f).normalized;
                    if (_modeTimer <= 0f) _mode = Mode.Neutral;
                    return;

                case Mode.Attacking:
                    _moveDirection = Vector3.zero;
                    return;
            }

            // ---- Neutral
            if (state != FighterState.Idle) return;

            float range = _profile.AttackRange;

            // Punish a whiff we saw (late, like a human would).
            if (seen.IsInRecovery && distance < range + 0.6f && Random.value < _profile.PunishChance)
            {
                StartAttack(3, false);
                return;
            }

            // Far away and the player is standing still: try the gap closer.
            MoveDefinition special = _self.Definition.Special1;
            if (distance > _profile.GapCloserMinDistance && special != null && IsStationary(delay) &&
                _self.Meters.CanAfford(special.EnergyCost) && !_self.Meters.IsOnCooldown(special) &&
                Random.value < _profile.GapCloserChance)
            {
                _plannedChainLength = 0;
                _plannedSpecial = false;
                _mode = Mode.Attacking;
                _lastChainIndex = -1;
                Press(FighterAction.Special1);
                return;
            }

            if (distance > range)
            {
                _moveDirection = (towards + Strafe(towards) * _profile.StrafeAmount).normalized;
                return;
            }

            // In range: pick a plan.
            bool canSpecial = special != null && _self.Meters.CanAfford(special.EnergyCost) &&
                              !_self.Meters.IsOnCooldown(special);
            float roll = Random.value;
            if (canSpecial && roll < _profile.ChainIntoSpecialChance) StartAttack(2, true);
            else if (roll < _profile.ChainIntoSpecialChance + _profile.ShortChainChance) StartAttack(2, false);
            else StartAttack(3, false);
        }

        private void StartAttack(int chainLength, bool thenSpecial)
        {
            _plannedChainLength = Mathf.Clamp(chainLength, 1, Mathf.Max(1, _self.Definition.LightChain.Length));
            _plannedSpecial = thenSpecial;
            _lastChainIndex = -1;
            _mode = Mode.Attacking;
            _moveDirection = Vector3.zero;
            Press(FighterAction.Light);
        }

        private void EnterMode(Mode mode, float duration)
        {
            _mode = mode;
            _modeTimer = duration;
            if (Random.value < 0.3f) _strafeSign = -_strafeSign;
        }

        private Vector3 Strafe(Vector3 towards) => Vector3.Cross(Vector3.up, towards) * _strafeSign;

        private bool IsStationary(float delay)
        {
            if (!_perception.TryGetDelayed(Time.time, delay, out OpponentSnapshot now)) return false;
            if (!_perception.TryGetDelayed(Time.time, delay + 1f, out OpponentSnapshot before)) return false;
            if (now.Time - before.Time < 0.9f) return false;
            Vector3 moved = now.Position - before.Position;
            moved.y = 0f;
            return moved.magnitude < StationaryDistance;
        }

        private static float RandomIn(Vector2 range) => Random.Range(range.x, range.y);
    }
}
