using System;
using HarmonyLib;
using SuperBow.Core;
using SuperBow.Runtime;
using UnityEngine;

namespace SuperBow.Patches
{
    [HarmonyPatch(typeof(Bow_Arrow), "OnTriggerEnter2D", new[] { typeof(Collider2D) })]
    internal static class BowArrowHitPatch
    {
        private static void Prefix(
            Collider2D __0,
            GameUnit ___m_Master,
            float ___m_Dmg,
            bool ___IsHit,
            ref BowHitState __state)
        {
            __state = null;
            try
            {
                CombatRuntime.ReportArrowPatchInvocation();
                if (___IsHit || ___m_Dmg <= 0f)
                {
                    return;
                }

                var queen = ___m_Master as T_Queen;
                if (!CombatRuntime.IsSupportedQueen(queen))
                {
                    return;
                }

                if (!RuntimeCombatTarget.TryFromCollision(__0, queen, out var target))
                {
                    return;
                }

                __state = new BowHitState(
                    queen,
                    target,
                    target.CurrentHealth,
                    ___m_Dmg,
                    target.CenterX,
                    target.CenterY);
            }
            catch (Exception exception)
            {
                CombatRuntime.ReportCaptureFailure(exception);
            }
        }

        private static void Postfix(BowHitState __state)
        {
            if (__state == null)
            {
                return;
            }

            try
            {
                if (!HitConfirmation.DidTakeDamage(
                        __state.HealthBeforeVanilla,
                        __state.Target.CurrentHealth))
                {
                    return;
                }

                CombatRuntime.ReportSupportedHit(__state.Target.Kind);
                CombatRuntime.ProcessHitSafely(__state);
            }
            catch (Exception exception)
            {
                CombatRuntime.ReportCaptureFailure(exception);
            }
        }
    }
}
