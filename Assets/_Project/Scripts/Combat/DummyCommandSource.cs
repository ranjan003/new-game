// Location: Assets/_Project/Scripts/Combat/DummyCommandSource.cs
// Assembly: EC.Combat
// Training dummy controller for Batch B (replaced by the AI in Batch C).
using UnityEngine;

namespace ElementalClash.Combat
{
    public sealed class DummyCommandSource : MonoBehaviour, IFighterCommandSource
    {
        [Tooltip("0 = passive. Otherwise the dummy throws a light attack every N seconds.")]
        [SerializeField, Min(0f)] private float _autoLightInterval;

        private float _timer;
        private bool _pendingLight;

        public float AutoLightInterval
        {
            get => _autoLightInterval;
            set
            {
                _autoLightInterval = Mathf.Max(0f, value);
                _timer = 0f;
            }
        }

        private void Update()
        {
            if (_autoLightInterval <= 0f) return;
            _timer += Time.deltaTime;
            if (_timer >= _autoLightInterval)
            {
                _timer = 0f;
                _pendingLight = true;
            }
        }

        public Vector3 GetMoveDirection(Fighter self) => Vector3.zero;

        public bool ConsumePress(FighterAction action)
        {
            if (action != FighterAction.Light || !_pendingLight) return false;
            _pendingLight = false;
            return true;
        }

        public bool IsHeld(FighterAction action) => false;
    }
}
