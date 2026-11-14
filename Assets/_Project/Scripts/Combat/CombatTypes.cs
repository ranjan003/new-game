// Location: Assets/_Project/Scripts/Combat/CombatTypes.cs
// Assembly: EC.Combat
using System;
using ElementalClash.Data;
using UnityEngine;

namespace ElementalClash.Combat
{
    /// <summary>Buttons a fighter can "press". Order matters: it is used as an array index.</summary>
    public enum FighterAction
    {
        Light,
        Heavy,
        Special1,
        Special2,
        Guard,
        Dodge,
        Ultimate,
    }

    public static class FighterActions
    {
        public const int Count = 7;
    }

    public enum FighterState
    {
        Idle,
        Attack,
        Hitstun,
        Knockdown,
        KO,
        Victory,
    }

    /// <summary>
    /// Shared contract for the player's input and the AI. Fighters never know who controls them.
    /// </summary>
    public interface IFighterCommandSource
    {
        /// <summary>World-space movement intent on the XZ plane, magnitude 0..1.</summary>
        Vector3 GetMoveDirection(Fighter self);

        /// <summary>True once if the action was pressed recently (buffered). Consumes the press.</summary>
        bool ConsumePress(FighterAction action);

        bool IsHeld(FighterAction action);
    }

    public readonly struct HitInfo
    {
        public readonly Fighter Attacker;
        public readonly Fighter Defender;
        public readonly MoveDefinition Move;
        public readonly HitResult Result;
        public readonly int ComboCount;
        public readonly Vector3 Point;

        public HitInfo(Fighter attacker, Fighter defender, MoveDefinition move, HitResult result, int comboCount, Vector3 point)
        {
            Attacker = attacker;
            Defender = defender;
            Move = move;
            Result = result;
            ComboCount = comboCount;
            Point = point;
        }
    }

    /// <summary>Combat-wide events. HUD, audio, VFX and camera listen; combat never references them.</summary>
    public static class CombatEvents
    {
        public static event Action<HitInfo> Hit;

        /// <summary>(loser, winner)</summary>
        public static event Action<Fighter, Fighter> KnockedOut;

        internal static void RaiseHit(in HitInfo info) => Hit?.Invoke(info);
        internal static void RaiseKnockedOut(Fighter loser, Fighter winner) => KnockedOut?.Invoke(loser, winner);
    }
}
