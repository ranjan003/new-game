// Location: Assets/_Project/Scripts/Combat/Fighter.cs
// Assembly: EC.Combat
// Root component of a fighter prefab. Runs the combat state machine on the fixed 60 Hz tick.
//
// Inspector:
//   Definition    -> FD_Zyron / FD_Noxar asset
//   Opponent      -> the other fighter in the scene
//   Hurtbox       -> child "Hurtbox" (trigger CapsuleCollider on layer Hurtbox)
//   Body Renderer -> the visible mesh (flashes white on hit)
// A component implementing IFighterCommandSource (PlayerInputSource, DummyCommandSource, later the AI)
// must be on the same GameObject.
using ElementalClash.Core;
using ElementalClash.Data;
using UnityEngine;

namespace ElementalClash.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class Fighter : MonoBehaviour
    {
        private const int KnockbackFrames = 10;
        private const int KnockdownFrames = 45;
        private const int FlashFrames = 3;
        private const float Gravity = 25f;
        private const float StopDistanceToOpponent = 1.1f;
        private const float TeleportSideOffset = 1.5f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private FighterDefinition _definition;
        [SerializeField] private Fighter _opponent;
        [SerializeField] private Hurtbox _hurtbox;
        [SerializeField] private Renderer _bodyRenderer;

        private readonly Collider[] _overlapBuffer = new Collider[8];
        private CharacterController _controller;
        private IFighterCommandSource _commands;
        private MaterialPropertyBlock _propertyBlock;
        private int _hurtboxMask;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;

        private float _verticalVelocity;
        private Vector3 _knockbackVelocity;
        private int _knockbackFramesLeft;

        private MoveDefinition _move;
        private int _moveFrame;
        private int _chainIndex = -1;
        private bool _moveHasHit;

        private int _stateFramesLeft;
        private int _hitStopFramesLeft;
        private int _flashFramesLeft;
        private bool _isFlashing;
        private int _teleportSide = 1;

        public FighterDefinition Definition => _definition;
        public FighterMeters Meters { get; private set; }
        public FighterState State { get; private set; }
        public MoveDefinition CurrentMove => _move;
        public int CurrentMoveFrame => _moveFrame;
        /// <summary>0..2 while in the light chain, -1 otherwise.</summary>
        public int CurrentChainIndex => State == FighterState.Attack ? _chainIndex : -1;
        public bool IsInStartup => State == FighterState.Attack && _move != null && _move.IsStartupFrame(_moveFrame);
        public bool IsInActiveFrames => State == FighterState.Attack && _move != null && _move.IsActiveFrame(_moveFrame);
        public bool IsInRecovery => State == FighterState.Attack && _move != null && _move.IsRecoveryFrame(_moveFrame);
        public int CombosTaken { get; private set; }
        public bool IsInHitStop => _hitStopFramesLeft > 0;
        public bool IsInvulnerable => State == FighterState.Attack && _move != null && _move.IsInvulnerableOnFrame(_moveFrame);

        public Fighter Opponent
        {
            get => _opponent;
            set => _opponent = value;
        }

        public IFighterCommandSource Commands
        {
            get => _commands;
            set => _commands = value;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _commands = GetComponent<IFighterCommandSource>();
            _propertyBlock = new MaterialPropertyBlock();

            int hurtboxLayer = LayerMask.NameToLayer(GameConstants.Layers.Hurtbox);
            if (hurtboxLayer < 0)
            {
                Debug.LogError("[Elemental Clash] Layer 'Hurtbox' is missing. Run 'Elemental Clash > 1. Run Project Setup'.");
            }
            _hurtboxMask = hurtboxLayer >= 0 ? 1 << hurtboxLayer : 0;

            if (_hurtbox != null) _hurtbox.Owner = this;

            if (_definition == null)
            {
                Debug.LogError($"[Elemental Clash] {name} has no FighterDefinition assigned.", this);
                enabled = false;
                return;
            }
            Meters = new FighterMeters(_definition);
        }

        private void Start()
        {
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            ResetToSpawn();
        }

        /// <summary>Full reset: position, health, energy, cooldowns and state.</summary>
        public void ResetToSpawn()
        {
            Teleport(_spawnPosition);
            transform.rotation = _spawnRotation;
            Meters?.Reset();
            _verticalVelocity = 0f;
            _knockbackFramesLeft = 0;
            _hitStopFramesLeft = 0;
            _flashFramesLeft = 0;
            EnterIdle();
        }

        public void SetVictory()
        {
            if (State == FighterState.KO) return;
            ClearMove();
            State = FighterState.Victory;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Meters.Tick(dt);
            TickFlash();

            if (_hitStopFramesLeft > 0)
            {
                _hitStopFramesLeft--;
                return;
            }

            Vector3 planarVelocity = Vector3.zero;
            switch (State)
            {
                case FighterState.Idle:
                    planarVelocity = TickFree(dt);
                    break;
                case FighterState.Attack:
                    planarVelocity = TickAttack(dt);
                    break;
                case FighterState.Hitstun:
                case FighterState.Knockdown:
                    if (--_stateFramesLeft <= 0) EnterIdle();
                    break;
            }

            ApplyMotion(planarVelocity, dt);
        }

        // ---------------------------------------------------------------- Free movement

        private Vector3 TickFree(float dt)
        {
            if (_commands != null)
            {
                if (_commands.ConsumePress(FighterAction.Light) && _definition.LightChain.Length > 0 &&
                    _definition.LightChain[0] != null)
                {
                    StartMove(_definition.LightChain[0], 0);
                    return Vector3.zero;
                }

                if (_commands.ConsumePress(FighterAction.Special1) && TryStartSpecial(_definition.Special1))
                {
                    return Vector3.zero;
                }
            }

            FaceOpponent(_definition.TurnSpeedDegrees * dt);

            Vector3 direction = _commands != null ? _commands.GetMoveDirection(this) : Vector3.zero;
            direction.y = 0f;
            float magnitude = Mathf.Clamp01(direction.magnitude);
            if (magnitude < 0.1f) return Vector3.zero;

            float speed = magnitude > 0.6f ? _definition.RunSpeed : _definition.WalkSpeed;
            return direction.normalized * speed;
        }

        // ---------------------------------------------------------------- Attacks

        private bool TryStartSpecial(MoveDefinition special)
        {
            if (special == null || Meters.IsOnCooldown(special) || !Meters.TrySpendEnergy(special.EnergyCost))
            {
                return false;
            }
            Meters.StartCooldown(special);
            StartMove(special, -1);
            return true;
        }

        private void StartMove(MoveDefinition move, int chainIndex)
        {
            _move = move;
            _moveFrame = 0;
            _chainIndex = chainIndex;
            _moveHasHit = false;
            State = FighterState.Attack;
            FaceOpponent(360f);
        }

        private Vector3 TickAttack(float dt)
        {
            MoveDefinition move = _move;
            if (move == null)
            {
                EnterIdle();
                return Vector3.zero;
            }

            _moveFrame++;

            // Cancels: next light in the chain, or into Special 1.
            if (_commands != null && move.CancelFromFrame > 0 && _moveFrame >= move.CancelFromFrame)
            {
                int next = _chainIndex + 1;
                if (_chainIndex >= 0 && next < _definition.LightChain.Length && _definition.LightChain[next] != null &&
                    _commands.ConsumePress(FighterAction.Light))
                {
                    StartMove(_definition.LightChain[next], next);
                    return Vector3.zero;
                }

                if (move.CanCancelIntoSpecial && _commands.ConsumePress(FighterAction.Special1) &&
                    TryStartSpecial(_definition.Special1))
                {
                    return Vector3.zero;
                }
            }

            if (move.IsStartupFrame(_moveFrame))
            {
                FaceOpponent(move.TrackingDegreesPerFrame);
            }

            if (move.Movement == MoveMovement.TeleportBeside && _moveFrame == move.StartupFrames)
            {
                TeleportBesideOpponent();
            }

            Vector3 planarVelocity = ComputeMoveVelocity(move, dt);

            if (move.IsActiveFrame(_moveFrame) && !_moveHasHit)
            {
                TryHit(move);
            }

            if (_moveFrame >= move.TotalFrames)
            {
                EnterIdle();
            }

            return planarVelocity;
        }

        private Vector3 ComputeMoveVelocity(MoveDefinition move, float dt)
        {
            if (move.TravelDistance <= 0f) return Vector3.zero;

            Vector3 velocity = Vector3.zero;
            int travelFrames = move.StartupFrames + move.ActiveFrames;

            if (move.Movement == MoveMovement.Lunge && _moveFrame <= travelFrames)
            {
                velocity = transform.forward * (move.TravelDistance / (travelFrames * dt));
            }
            else if (move.Movement == MoveMovement.Dash && move.IsActiveFrame(_moveFrame))
            {
                velocity = transform.forward * (move.TravelDistance / (move.ActiveFrames * dt));
            }

            // Don't push through the opponent.
            if (_opponent != null && velocity != Vector3.zero)
            {
                Vector3 toOpponent = _opponent.transform.position - transform.position;
                toOpponent.y = 0f;
                if (toOpponent.magnitude < StopDistanceToOpponent && Vector3.Dot(toOpponent, velocity) > 0f)
                {
                    velocity = Vector3.zero;
                }
            }
            return velocity;
        }

        private void TryHit(MoveDefinition move)
        {
            if (_hurtboxMask == 0) return;

            Vector3 center = HitboxCenter(move);
            int count = Physics.OverlapSphereNonAlloc(center, move.HitboxRadius, _overlapBuffer, _hurtboxMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Hurtbox hurtbox = _overlapBuffer[i].GetComponent<Hurtbox>();
                if (hurtbox == null || hurtbox.Owner == null || hurtbox.Owner == this) continue;

                if (hurtbox.Owner.ReceiveHit(this, move, center))
                {
                    _moveHasHit = true;
                    _hitStopFramesLeft = move.HitStopFrames;
                    Meters.AddEnergy(move.EnergyGainOnHit);
                    break;
                }
            }
        }

        private Vector3 HitboxCenter(MoveDefinition move) =>
            transform.position + transform.forward * move.HitboxForward + Vector3.up * move.HitboxHeight;

        // ---------------------------------------------------------------- Getting hit

        /// <summary>Called by the attacker. Returns false if the hit did not connect (e.g. invulnerable).</summary>
        public bool ReceiveHit(Fighter attacker, MoveDefinition move, Vector3 point)
        {
            if (State == FighterState.KO || State == FighterState.Victory || IsInvulnerable) return false;

            DefenderPhase phase = DefenderPhase.Neutral;
            if (State == FighterState.Attack && _move != null)
            {
                if (_move.IsStartupFrame(_moveFrame)) phase = DefenderPhase.Startup;
                else if (_move.IsRecoveryFrame(_moveFrame)) phase = DefenderPhase.Recovery;
            }

            CombosTaken++;
            HitResult result = DamageResolver.Resolve(move.Damage, move.HitstunFrames, phase, CombosTaken);
            bool knockedOut = Meters.TakeDamage(result.Damage);

            ClearMove();

            Vector3 away = transform.position - attacker.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = -transform.forward;
            away.Normalize();
            transform.rotation = Quaternion.LookRotation(-away);

            _knockbackVelocity = away * (2f * move.KnockbackDistance / (KnockbackFrames * Time.fixedDeltaTime));
            _knockbackFramesLeft = KnockbackFrames;
            _hitStopFramesLeft = move.HitStopFrames;
            _flashFramesLeft = FlashFrames;

            if (knockedOut)
            {
                State = FighterState.KO;
            }
            else if (move.CausesKnockdown || (result.CounterHit && move.KnockdownOnCounter))
            {
                State = FighterState.Knockdown;
                _stateFramesLeft = KnockdownFrames;
            }
            else
            {
                State = FighterState.Hitstun;
                _stateFramesLeft = Mathf.Max(1, result.HitstunFrames);
            }

            CombatEvents.RaiseHit(new HitInfo(attacker, this, move, result, CombosTaken, point));
            if (knockedOut) CombatEvents.RaiseKnockedOut(this, attacker);
            return true;
        }

        // ---------------------------------------------------------------- Helpers

        private void EnterIdle()
        {
            ClearMove();
            State = FighterState.Idle;
            CombosTaken = 0;
        }

        private void ClearMove()
        {
            _move = null;
            _moveFrame = 0;
            _chainIndex = -1;
            _moveHasHit = false;
        }

        private void ApplyMotion(Vector3 planarVelocity, float dt)
        {
            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            else _verticalVelocity -= Gravity * dt;

            Vector3 knockback = Vector3.zero;
            if (_knockbackFramesLeft > 0)
            {
                knockback = _knockbackVelocity * (_knockbackFramesLeft / (float)KnockbackFrames);
                _knockbackFramesLeft--;
            }

            Vector3 velocity = planarVelocity + knockback + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * dt);
        }

        private void FaceOpponent(float maxDegrees)
        {
            if (_opponent == null || maxDegrees <= 0f) return;
            Vector3 toOpponent = _opponent.transform.position - transform.position;
            toOpponent.y = 0f;
            if (toOpponent.sqrMagnitude < 0.0001f) return;
            Quaternion target = Quaternion.LookRotation(toOpponent);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxDegrees);
        }

        private void TeleportBesideOpponent()
        {
            if (_opponent == null) return;

            Transform opponent = _opponent.transform;
            Vector3 intent = _commands != null ? _commands.GetMoveDirection(this) : Vector3.zero;
            float sideInput = Vector3.Dot(intent, opponent.right);
            int side = Mathf.Abs(sideInput) > 0.2f ? (sideInput > 0f ? 1 : -1) : (_teleportSide = -_teleportSide);

            Vector3 target = opponent.position + opponent.right * (side * TeleportSideOffset);
            target.y = transform.position.y;
            Teleport(target);
            FaceOpponent(360f);
        }

        private void Teleport(Vector3 position)
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = true;
        }

        private void TickFlash()
        {
            if (_bodyRenderer == null) return;

            if (_flashFramesLeft > 0)
            {
                _flashFramesLeft--;
                _propertyBlock.SetColor(BaseColorId, Color.white);
                _bodyRenderer.SetPropertyBlock(_propertyBlock);
                _isFlashing = true;
            }
            else if (_isFlashing)
            {
                _propertyBlock.Clear();
                _bodyRenderer.SetPropertyBlock(_propertyBlock);
                _isFlashing = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || State != FighterState.Attack || _move == null) return;
            Gizmos.color = _move.IsActiveFrame(_moveFrame) ? Color.red : new Color(1f, 0.6f, 0f, 0.5f);
            Gizmos.DrawWireSphere(HitboxCenter(_move), _move.HitboxRadius);
        }
    }
}
