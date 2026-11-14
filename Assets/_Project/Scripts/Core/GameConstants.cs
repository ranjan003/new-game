// Location: Assets/_Project/Scripts/Core/GameConstants.cs
// Assembly: EC.Core
namespace ElementalClash.Core
{
    /// <summary>Project-wide constants shared by runtime code, editor tools and tests.</summary>
    public static class GameConstants
    {
        public const string GameTitle = "Elemental Clash: Arena Legends";

        /// <summary>Combat logic runs at a fixed 60 Hz regardless of render frame rate.</summary>
        public const int CombatTickRate = 60;
        public const float CombatFixedDeltaTime = 1f / CombatTickRate;

        /// <summary>User layers 6-11. Index = FirstUserLayer + position in <see cref="All"/>.</summary>
        public static class Layers
        {
            public const int FirstUserLayer = 6;

            public const string Fighter = "Fighter";
            public const string Hurtbox = "Hurtbox";
            public const string Hitbox = "Hitbox";
            public const string Projectile = "Projectile";
            public const string Environment = "Environment";
            public const string ArenaBounds = "ArenaBounds";

            public static readonly string[] All =
            {
                Fighter, Hurtbox, Hitbox, Projectile, Environment, ArenaBounds
            };

            public static int IndexOf(string layerName)
            {
                for (int i = 0; i < All.Length; i++)
                {
                    if (All[i] == layerName) return FirstUserLayer + i;
                }
                return -1;
            }
        }

        public static class Scenes
        {
            public const string Folder = "Assets/_Project/Scenes";
            public const string Boot = "Boot";
            public const string SandboxCombat = "Dev/Sandbox_Combat";
        }
    }
}
