// Location: Assets/_Project/Tests/EditMode/GameConstantsTests.cs
// Assembly: EC.Tests.EditMode
using System.Linq;
using ElementalClash.Core;
using NUnit.Framework;
using UnityEngine;

namespace ElementalClash.Tests
{
    public class GameConstantsTests
    {
        [Test]
        public void CombatTick_Is60Hz()
        {
            Assert.AreEqual(60, GameConstants.CombatTickRate);
            Assert.AreEqual(1f / 60f, GameConstants.CombatFixedDeltaTime, 1e-6f);
        }

        [Test]
        public void LayerNames_AreUniqueAndFitInUserLayers()
        {
            string[] all = GameConstants.Layers.All;
            Assert.AreEqual(all.Length, all.Distinct().Count(), "Layer names must be unique.");
            Assert.LessOrEqual(GameConstants.Layers.FirstUserLayer + all.Length, 32);
        }

        [Test]
        public void ProjectLayers_MatchConstants_AfterSetupHasRun()
        {
            foreach (string name in GameConstants.Layers.All)
            {
                int expected = GameConstants.Layers.IndexOf(name);
                Assert.AreEqual(expected, LayerMask.NameToLayer(name),
                    $"Layer '{name}' should be at index {expected}. Run 'Elemental Clash > 1. Run Project Setup'.");
            }
        }
    }
}
