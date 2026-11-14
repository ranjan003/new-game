// Location: Assets/_Project/Scripts/Data/MoveDefinition.cs
// Assembly: EC.Data
// Create via: Assets > Create > Elemental Clash > Move
using UnityEngine;

namespace ElementalClash.Data
{
    public enum MoveKind
    {
        Light,
        Heavy,
        Special,
        Ultimate,
    }

    /// <summary>How the attacker travels during a move.</summary>
    public enum MoveMovement
    {
        /// <summary>No travel.</summary>
        None,
        /// <summary>Steps forward evenly across startup and active frames.</summary>
        Lunge,
        /// <summary>Fast travel only during active frames (stops at the opponent).</summary>
        Dash,
        /// <summary>Teleports beside the opponent on the last startup frame.</summary>
        TeleportBeside,
    }

    /// <summary>
    /// All data for one attack. Frame counts are at 60 Hz (the combat tick).
    /// Frame 1 is the first frame of the move. Active frames are startup+1 .. startup+active.
    /// </summary>
    [CreateAssetMenu(menuName = "Elemental Clash/Move", fileName = "MV_New")]
    public sealed class MoveDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "New Move";
        [SerializeField] private MoveKind _kind = MoveKind.Light;

        [Header("Frame data (60 Hz)")]
        [SerializeField, Min(1)] private int _startupFrames = 5;
        [SerializeField, Min(1)] private int _activeFrames = 3;
        [SerializeField, Min(0)] private int _recoveryFrames = 10;
        [Tooltip("From this frame on, the move can be cancelled into the next chain hit (or a special if allowed). 0 = never.")]
        [SerializeField, Min(0)] private int _cancelFromFrame;
        [SerializeField] private bool _canCancelIntoSpecial;

        [Header("On hit")]
        [SerializeField, Min(0f)] private float _damage = 4f;
        [SerializeField, Min(0)] private int _hitstunFrames = 14;
        [SerializeField, Min(0f)] private float _knockbackDistance = 0.3f;
        [SerializeField] private bool _causesKnockdown;
        [SerializeField] private bool _knockdownOnCounter;
        [SerializeField, Min(0)] private int _hitStopFrames = 4;
        [SerializeField, Min(0f)] private float _energyGainOnHit;

        [Header("Cost")]
        [SerializeField, Min(0f)] private float _energyCost;
        [SerializeField, Min(0f)] private float _cooldownSeconds;

        [Header("Hitbox (sphere, relative to the fighter's feet)")]
        [SerializeField, Min(0.05f)] private float _hitboxRadius = 0.5f;
        [SerializeField] private float _hitboxForward = 1f;
        [SerializeField] private float _hitboxHeight = 1.1f;

        [Header("Movement")]
        [Tooltip("How fast the attacker can turn toward the opponent during startup, in degrees per frame.")]
        [SerializeField, Min(0f)] private float _trackingDegreesPerFrame = 12f;
        [SerializeField] private MoveMovement _movement = MoveMovement.Lunge;
        [SerializeField, Min(0f)] private float _travelDistance = 0.3f;

        [Header("Invulnerability (0 = none)")]
        [SerializeField, Min(0)] private int _invulnerableFromFrame;
        [SerializeField, Min(0)] private int _invulnerableToFrame;

        public string DisplayName => _displayName;
        public MoveKind Kind => _kind;
        public int StartupFrames => _startupFrames;
        public int ActiveFrames => _activeFrames;
        public int RecoveryFrames => _recoveryFrames;
        public int TotalFrames => _startupFrames + _activeFrames + _recoveryFrames;
        public int CancelFromFrame => _cancelFromFrame;
        public bool CanCancelIntoSpecial => _canCancelIntoSpecial;
        public float Damage => _damage;
        public int HitstunFrames => _hitstunFrames;
        public float KnockbackDistance => _knockbackDistance;
        public bool CausesKnockdown => _causesKnockdown;
        public bool KnockdownOnCounter => _knockdownOnCounter;
        public int HitStopFrames => _hitStopFrames;
        public float EnergyGainOnHit => _energyGainOnHit;
        public float EnergyCost => _energyCost;
        public float CooldownSeconds => _cooldownSeconds;
        public float HitboxRadius => _hitboxRadius;
        public float HitboxForward => _hitboxForward;
        public float HitboxHeight => _hitboxHeight;
        public float TrackingDegreesPerFrame => _trackingDegreesPerFrame;
        public MoveMovement Movement => _movement;
        public float TravelDistance => _travelDistance;

        public bool IsStartupFrame(int frame) => frame >= 1 && frame <= _startupFrames;
        public bool IsActiveFrame(int frame) => frame > _startupFrames && frame <= _startupFrames + _activeFrames;
        public bool IsRecoveryFrame(int frame) => frame > _startupFrames + _activeFrames;

        public bool IsInvulnerableOnFrame(int frame) =>
            _invulnerableFromFrame > 0 && frame >= _invulnerableFromFrame && frame <= _invulnerableToFrame;

        private void OnValidate()
        {
            if (_invulnerableToFrame < _invulnerableFromFrame) _invulnerableToFrame = _invulnerableFromFrame;
        }
    }
}
