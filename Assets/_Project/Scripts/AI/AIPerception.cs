// Location: Assets/_Project/Scripts/AI/AIPerception.cs
// Assembly: EC.AI
// Delayed view of the opponent. The AI only ever reads snapshots that are reactionTime old,
// so it cannot react instantly and never reads the player's buttons. Pure C#, covered by tests.
using UnityEngine;

namespace ElementalClash.AI
{
    public struct OpponentSnapshot
    {
        public float Time;
        public Vector3 Position;
        public bool IsAttacking;
        public bool IsInRecovery;
        public bool IsVulnerable; // not KO/victory
        public float HealthFraction;
    }

    public sealed class AIPerception
    {
        private readonly OpponentSnapshot[] _ring;
        private int _head;
        private int _count;

        public AIPerception(int capacity = 180)
        {
            _ring = new OpponentSnapshot[Mathf.Max(2, capacity)];
        }

        public int Count => _count;

        public void Clear()
        {
            _head = 0;
            _count = 0;
        }

        public void Record(OpponentSnapshot snapshot)
        {
            _ring[_head] = snapshot;
            _head = (_head + 1) % _ring.Length;
            if (_count < _ring.Length) _count++;
        }

        /// <summary>
        /// Newest snapshot that is at least <paramref name="delay"/> seconds old.
        /// Falls back to the oldest one stored if nothing is that old yet.
        /// </summary>
        public bool TryGetDelayed(float now, float delay, out OpponentSnapshot snapshot)
        {
            snapshot = default;
            if (_count == 0) return false;

            float cutoff = now - delay;
            for (int i = 1; i <= _count; i++)
            {
                int index = (_head - i + _ring.Length) % _ring.Length;
                if (_ring[index].Time <= cutoff)
                {
                    snapshot = _ring[index];
                    return true;
                }
            }

            int oldest = (_head - _count + _ring.Length) % _ring.Length;
            snapshot = _ring[oldest];
            return true;
        }
    }
}
