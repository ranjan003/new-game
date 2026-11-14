// Location: Assets/_Project/Scripts/Data/AIProfile.cs
// Assembly: EC.Data
// Create via: Assets > Create > Elemental Clash > AI Profile
using UnityEngine;

namespace ElementalClash.Data
{
    /// <summary>Difficulty settings for the AI (GDD §8.2). The prototype ships "Normal" only.</summary>
    [CreateAssetMenu(menuName = "Elemental Clash/AI Profile", fileName = "AI_New")]
    public sealed class AIProfile : ScriptableObject
    {
        [Header("Perception")]
        [Tooltip("The AI sees the world this many seconds late (random in range, re-rolled each decision).")]
        [SerializeField] private Vector2 _reactionTimeRange = new Vector2(0.25f, 0.35f);
        [SerializeField, Range(0.05f, 0.5f)] private float _decisionInterval = 0.1f;

        [Header("Spacing")]
        [SerializeField, Min(0.5f)] private float _attackRange = 2.0f;
        [SerializeField, Range(0f, 1f)] private float _strafeAmount = 0.35f;

        [Header("Offense")]
        [Tooltip("Chance to stop after 2 hits instead of finishing the chain.")]
        [SerializeField, Range(0f, 1f)] private float _shortChainChance = 0.6f;
        [Tooltip("Chance to cancel the chain into Special 1 (if energy allows).")]
        [SerializeField, Range(0f, 1f)] private float _chainIntoSpecialChance = 0.15f;
        [SerializeField, Range(0f, 1f)] private float _punishChance = 0.3f;
        [Tooltip("Chance per check to use a gap-closing special when the player is far and standing still.")]
        [SerializeField, Range(0f, 1f)] private float _gapCloserChance = 0.35f;
        [SerializeField, Min(0f)] private float _gapCloserMinDistance = 5f;

        [Header("Pacing")]
        [SerializeField] private Vector2 _recoverPauseRange = new Vector2(0.3f, 0.8f);
        [SerializeField] private Vector2 _retreatDurationRange = new Vector2(1f, 2f);
        [Tooltip("After taking a combo of at least this many hits while behind on health, the AI backs off.")]
        [SerializeField, Min(1)] private int _retreatAfterComboHits = 3;

        public Vector2 ReactionTimeRange => _reactionTimeRange;
        public float DecisionInterval => _decisionInterval;
        public float AttackRange => _attackRange;
        public float StrafeAmount => _strafeAmount;
        public float ShortChainChance => _shortChainChance;
        public float ChainIntoSpecialChance => _chainIntoSpecialChance;
        public float PunishChance => _punishChance;
        public float GapCloserChance => _gapCloserChance;
        public float GapCloserMinDistance => _gapCloserMinDistance;
        public Vector2 RecoverPauseRange => _recoverPauseRange;
        public Vector2 RetreatDurationRange => _retreatDurationRange;
        public int RetreatAfterComboHits => _retreatAfterComboHits;
    }
}
