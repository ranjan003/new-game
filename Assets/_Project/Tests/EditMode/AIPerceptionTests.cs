// Location: Assets/_Project/Tests/EditMode/AIPerceptionTests.cs
// Assembly: EC.Tests.EditMode
using ElementalClash.AI;
using NUnit.Framework;
using UnityEngine;

namespace ElementalClash.Tests
{
    public class AIPerceptionTests
    {
        private static OpponentSnapshot At(float time, float x) =>
            new OpponentSnapshot { Time = time, Position = new Vector3(x, 0f, 0f), IsVulnerable = true };

        [Test]
        public void ReturnsSnapshotFromDelayAgo_NotTheLatest()
        {
            var p = new AIPerception();
            for (int i = 0; i <= 60; i++) p.Record(At(i / 60f, i)); // 1 s of data, x = frame index

            Assert.IsTrue(p.TryGetDelayed(1f, 0.3f, out OpponentSnapshot seen));
            Assert.LessOrEqual(seen.Time, 0.7f + 1e-4f);
            Assert.AreEqual(42f, seen.Position.x, 1.1f); // ~0.7 s * 60 = frame 42 (float rounding may give 41)
        }

        [Test]
        public void FallsBackToOldest_WhenHistoryIsShorterThanDelay()
        {
            var p = new AIPerception();
            p.Record(At(0.9f, 5f));
            p.Record(At(1.0f, 6f));
            Assert.IsTrue(p.TryGetDelayed(1.0f, 0.3f, out OpponentSnapshot seen));
            Assert.AreEqual(5f, seen.Position.x);
        }

        [Test]
        public void EmptyPerception_ReturnsFalse()
        {
            var p = new AIPerception();
            Assert.IsFalse(p.TryGetDelayed(1f, 0.3f, out _));
        }

        [Test]
        public void RingBuffer_KeepsOnlyNewestEntries()
        {
            var p = new AIPerception(capacity: 10);
            for (int i = 0; i < 25; i++) p.Record(At(i, i));
            Assert.AreEqual(10, p.Count);
            Assert.IsTrue(p.TryGetDelayed(24f, 100f, out OpponentSnapshot oldest));
            Assert.AreEqual(15f, oldest.Position.x);
        }
    }
}
