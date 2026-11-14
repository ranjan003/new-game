// Location: Assets/_Project/Scripts/Data/FighterDefinition.cs
// Assembly: EC.Data
// Create via: Assets > Create > Elemental Clash > Fighter
using UnityEngine;

namespace ElementalClash.Data
{
    /// <summary>The eleven powers. Each fighter has exactly one.</summary>
    public enum ElementPower
    {
        Lightning,
        Fire,
        Earth,
        Wind,
        Ice,
        Nature,
        Light,
        Shadow,
        Magma,
        Crystal,
        Gravity,
    }

    /// <summary>Stats and move list for one fighter. Balancing is done by editing these assets.</summary>
    [CreateAssetMenu(menuName = "Elemental Clash/Fighter", fileName = "FD_New")]
    public sealed class FighterDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "New Fighter";
        [SerializeField] private ElementPower _power;
        [SerializeField] private string _fighterClass = "All-rounder";
        [SerializeField] private Color _placeholderColor = Color.gray;

        [Header("Resources")]
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [SerializeField, Min(1f)] private float _maxEnergy = 100f;
        [SerializeField, Min(0f)] private float _energyRegenPerSecond = 6f;
        [Tooltip("Seconds after spending energy before it starts to regenerate.")]
        [SerializeField, Min(0f)] private float _energyRegenDelay = 1f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _walkSpeed = 4f;
        [SerializeField, Min(0f)] private float _runSpeed = 6f;
        [SerializeField, Min(0f)] private float _turnSpeedDegrees = 720f;

        [Header("Moves")]
        [SerializeField] private MoveDefinition[] _lightChain = new MoveDefinition[0];
        [SerializeField] private MoveDefinition _special1;

        public string DisplayName => _displayName;
        public ElementPower Power => _power;
        public string FighterClass => _fighterClass;
        public Color PlaceholderColor => _placeholderColor;
        public float MaxHealth => _maxHealth;
        public float MaxEnergy => _maxEnergy;
        public float EnergyRegenPerSecond => _energyRegenPerSecond;
        public float EnergyRegenDelay => _energyRegenDelay;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float TurnSpeedDegrees => _turnSpeedDegrees;
        public MoveDefinition[] LightChain => _lightChain;
        public MoveDefinition Special1 => _special1;
    }
}
