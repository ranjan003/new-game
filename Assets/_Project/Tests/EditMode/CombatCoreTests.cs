// Location: Assets/_Project/Tests/EditMode/CombatCoreTests.cs
// Assembly: EC.Tests.EditMode
using ElementalClash.Combat;
using ElementalClash.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ElementalClash.Tests
{
    public class DamageResolverTests
    {
        [Test]
        public void NeutralHit_DealsBaseDamage()
        {
            HitResult r = DamageResolver.Resolve(10f, 20, DefenderPhase.Neutral, 1);
            Assert.AreEqual(10f, r.Damage, 1e-4f);
            Assert.AreEqual(20, r.HitstunFrames);
            Assert.IsFalse(r.CounterHit);
            Assert.IsFalse(r.Punish);
        }

        [Test]
        public void CounterHit_AddsDamageAndHitstun()
        {
            HitResult r = DamageResolver.Resolve(10f, 20, DefenderPhase.Startup, 1);
            Assert.IsTrue(r.CounterHit);
            Assert.AreEqual(12.5f, r.Damage, 1e-4f);
            Assert.AreEqual(30, r.HitstunFrames);
        }

        [Test]
        public void Punish_AddsDamageOnly()
        {
            HitResult r = DamageResolver.Resolve(10f, 20, DefenderPhase.Recovery, 1);
            Assert.IsTrue(r.Punish);
            Assert.AreEqual(11.5f, r.Damage, 1e-4f);
            Assert.AreEqual(20, r.HitstunFrames);
        }

        [Test]
        public void ComboScaling_StartsAfterThirdHit_AndHasFloor()
        {
            Assert.AreEqual(1f, DamageResolver.ComboScaling(1), 1e-4f);
            Assert.AreEqual(1f, DamageResolver.ComboScaling(3), 1e-4f);
            Assert.AreEqual(0.9f, DamageResolver.ComboScaling(4), 1e-4f);
            Assert.AreEqual(0.4f, DamageResolver.ComboScaling(50), 1e-4f);
        }
    }

    public class FighterMetersTests
    {
        private FighterDefinition _definition;
        private MoveDefinition _move;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<FighterDefinition>();
            _move = ScriptableObject.CreateInstance<MoveDefinition>();
            var so = new SerializedObject(_move);
            so.FindProperty("_cooldownSeconds").floatValue = 4f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_move);
        }

        [Test]
        public void StartsFull()
        {
            var m = new FighterMeters(_definition);
            Assert.AreEqual(_definition.MaxHealth, m.Health);
            Assert.AreEqual(_definition.MaxEnergy, m.Energy);
        }

        [Test]
        public void SpendingEnergy_FailsWhenNotEnough()
        {
            var m = new FighterMeters(_definition);
            Assert.IsTrue(m.TrySpendEnergy(90f));
            Assert.IsFalse(m.TrySpendEnergy(20f));
            Assert.AreEqual(10f, m.Energy, 1e-4f);
        }

        [Test]
        public void Energy_RegeneratesOnlyAfterDelay()
        {
            var m = new FighterMeters(_definition); // defaults: 6 EP/s after 1 s
            m.TrySpendEnergy(50f);
            m.Tick(0.5f);
            Assert.AreEqual(50f, m.Energy, 1e-4f);
            m.Tick(0.5f); // reaches the 1 s delay this tick
            m.Tick(1f);
            Assert.Greater(m.Energy, 50f);
        }

        [Test]
        public void Cooldown_ExpiresAfterDuration()
        {
            var m = new FighterMeters(_definition);
            m.StartCooldown(_move);
            Assert.IsTrue(m.IsOnCooldown(_move));
            m.Tick(3.9f);
            Assert.IsTrue(m.IsOnCooldown(_move));
            m.Tick(0.2f);
            Assert.IsFalse(m.IsOnCooldown(_move));
        }

        [Test]
        public void Damage_ReportsKnockout()
        {
            var m = new FighterMeters(_definition);
            Assert.IsFalse(m.TakeDamage(99f));
            Assert.IsTrue(m.TakeDamage(5f));
            Assert.AreEqual(0f, m.Health);
        }
    }

    public class InputBufferTests
    {
        [Test]
        public void PressInsideWindow_IsConsumedOnce()
        {
            var b = new InputBuffer(0.15f);
            b.Register(FighterAction.Light, 1.0f);
            Assert.IsTrue(b.Consume(FighterAction.Light, 1.1f));
            Assert.IsFalse(b.Consume(FighterAction.Light, 1.1f));
        }

        [Test]
        public void PressOutsideWindow_IsDropped()
        {
            var b = new InputBuffer(0.15f);
            b.Register(FighterAction.Special1, 1.0f);
            Assert.IsFalse(b.Consume(FighterAction.Special1, 1.2f));
        }

        [Test]
        public void Actions_AreIndependent()
        {
            var b = new InputBuffer(0.15f);
            b.Register(FighterAction.Light, 1.0f);
            Assert.IsFalse(b.Consume(FighterAction.Special1, 1.0f));
            Assert.IsTrue(b.Consume(FighterAction.Light, 1.0f));
        }
    }
}
