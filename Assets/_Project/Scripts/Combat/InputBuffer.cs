// Location: Assets/_Project/Scripts/Combat/InputBuffer.cs
// Assembly: EC.Combat
// Remembers button presses for a short window so slightly early taps still come out.
namespace ElementalClash.Combat
{
    public sealed class InputBuffer
    {
        private readonly float _windowSeconds;
        private readonly float[] _pressTimes = new float[FighterActions.Count];

        public InputBuffer(float windowSeconds)
        {
            _windowSeconds = windowSeconds;
            Clear();
        }

        public void Register(FighterAction action, float now)
        {
            _pressTimes[(int)action] = now;
        }

        public bool Consume(FighterAction action, float now)
        {
            int index = (int)action;
            float pressedAt = _pressTimes[index];
            if (now - pressedAt > _windowSeconds) return false;
            _pressTimes[index] = float.NegativeInfinity;
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _pressTimes.Length; i++) _pressTimes[i] = float.NegativeInfinity;
        }
    }
}
