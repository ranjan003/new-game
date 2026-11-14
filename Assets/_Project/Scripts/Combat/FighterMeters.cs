// Location: Assets/_Project/Scripts/Combat/FighterMeters.cs
// Assembly: EC.Combat
// Health, elemental energy and cooldowns. Pure C#, covered by EditMode tests.
using System.Collections.Generic;
using ElementalClash.Data;
using UnityEngine;

namespace ElementalClash.Combat
{
    public sealed class FighterMeters
    {
        private readonly FighterDefinition _definition;
        private readonly Dictionary<MoveDefinition, float> _cooldownEnds = new Dictionary<MoveDefinition, float>();
        private float _time;
        private float _lastSpendTime;

        public FighterMeters(FighterDefinition definition)
        {
            _definition = definition;
            Reset();
        }

        public float Health { get; private set; }
        public float MaxHealth => _definition.MaxHealth;
        public float Energy { get; private set; }
        public float MaxEnergy => _definition.MaxEnergy;
        public bool IsDead => Health <= 0f;

        public void Reset()
        {
            Health = _definition.MaxHealth;
            Energy = _definition.MaxEnergy;
            _cooldownEnds.Clear();
            _time = 0f;
            _lastSpendTime = float.NegativeInfinity;
        }

        public void Tick(float deltaTime)
        {
            _time += deltaTime;
            if (_time - _lastSpendTime >= _definition.EnergyRegenDelay)
            {
                Energy = Mathf.Min(MaxEnergy, Energy + _definition.EnergyRegenPerSecond * deltaTime);
            }
        }

        public bool CanAfford(float energyCost) => energyCost <= 0f || Energy >= energyCost;

        public bool TrySpendEnergy(float amount)
        {
            if (amount <= 0f) return true;
            if (Energy < amount) return false;
            Energy -= amount;
            _lastSpendTime = _time;
            return true;
        }

        public void AddEnergy(float amount)
        {
            Energy = Mathf.Clamp(Energy + amount, 0f, MaxEnergy);
        }

        /// <summary>Returns true if this damage knocked the fighter out.</summary>
        public bool TakeDamage(float amount)
        {
            if (amount > 0f) Health = Mathf.Max(0f, Health - amount);
            return Health <= 0f;
        }

        public bool IsOnCooldown(MoveDefinition move) => CooldownRemaining(move) > 0f;

        public float CooldownRemaining(MoveDefinition move)
        {
            if (move == null || !_cooldownEnds.TryGetValue(move, out float end)) return 0f;
            return Mathf.Max(0f, end - _time);
        }

        public void StartCooldown(MoveDefinition move)
        {
            if (move != null && move.CooldownSeconds > 0f) _cooldownEnds[move] = _time + move.CooldownSeconds;
        }
    }
}
