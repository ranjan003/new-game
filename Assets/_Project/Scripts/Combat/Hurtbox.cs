// Location: Assets/_Project/Scripts/Combat/Hurtbox.cs
// Assembly: EC.Combat
// Put on a child object with a trigger collider on the "Hurtbox" layer.
using UnityEngine;

namespace ElementalClash.Combat
{
    [RequireComponent(typeof(Collider))]
    public sealed class Hurtbox : MonoBehaviour
    {
        [SerializeField] private Fighter _owner;

        public Fighter Owner
        {
            get => _owner;
            set => _owner = value;
        }

        private void Reset()
        {
            _owner = GetComponentInParent<Fighter>();
        }

        private void Awake()
        {
            if (_owner == null) _owner = GetComponentInParent<Fighter>();
        }
    }
}
