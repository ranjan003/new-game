// Location: Assets/_Project/Scripts/Combat/DamageResolver.cs
// Assembly: EC.Combat
// Pure damage math (no MonoBehaviour), covered by EditMode tests.
using UnityEngine;

namespace ElementalClash.Combat
{
    /// <summary>What the defender was doing when the hit landed.</summary>
    public enum DefenderPhase
    {
        Neutral,
        Startup,
        Recovery,
    }

    public readonly struct HitResult
    {
        public readonly float Damage;
        public readonly int HitstunFrames;
        public readonly bool CounterHit;
        public readonly bool Punish;

        public HitResult(float damage, int hitstunFrames, bool counterHit, bool punish)
        {
            Damage = damage;
            HitstunFrames = hitstunFrames;
            CounterHit = counterHit;
            Punish = punish;
        }
    }

    /// <summary>GDD §3.4: counter hits, punishes and combo scaling.</summary>
    public static class DamageResolver
    {
        public const float CounterDamageMultiplier = 1.25f;
        public const float CounterHitstunMultiplier = 1.5f;
        public const float PunishDamageMultiplier = 1.15f;
        public const int UnscaledHits = 3;
        public const float ScalingPerHit = 0.1f;
        public const float MinScaling = 0.4f;

        /// <param name="hitNumberInCombo">1 for the first hit of a combo, 2 for the second, ...</param>
        public static float ComboScaling(int hitNumberInCombo)
        {
            if (hitNumberInCombo <= UnscaledHits) return 1f;
            return Mathf.Max(MinScaling, 1f - ScalingPerHit * (hitNumberInCombo - UnscaledHits));
        }

        public static HitResult Resolve(float baseDamage, int baseHitstunFrames, DefenderPhase phase, int hitNumberInCombo)
        {
            bool counter = phase == DefenderPhase.Startup;
            bool punish = phase == DefenderPhase.Recovery;

            float damage = baseDamage;
            int hitstun = baseHitstunFrames;

            if (counter)
            {
                damage *= CounterDamageMultiplier;
                hitstun = Mathf.RoundToInt(hitstun * CounterHitstunMultiplier);
            }
            else if (punish)
            {
                damage *= PunishDamageMultiplier;
            }

            damage *= ComboScaling(hitNumberInCombo);
            return new HitResult(damage, hitstun, counter, punish);
        }
    }
}
