using System;
using HarmonyLib;
using RestroomBathFun.Core;
using RestroomBathFun.Runtime;

namespace RestroomBathFun.Patches
{
    [HarmonyPatch(
        typeof(T_Citizen),
        nameof(T_Citizen.OnServiceChoreographyEnd),
        new Type[] { typeof(Building) })]
    internal static class ServiceCompletionPatch
    {
        [HarmonyPrefix]
        private static void Prefix(
            T_Citizen __instance,
            Building _b,
            out ServiceCompletionState __state)
        {
            __state = new ServiceCompletionState(FacilityKind.Unsupported, true);

            try
            {
                if (__instance == null || _b == null || _b.m_Info == null)
                {
                    return;
                }

                __state = new ServiceCompletionState(
                    FacilityClassifier.Classify((int)_b.m_Info.Name),
                    __instance.ServiceAborted);
            }
            catch (Exception exception)
            {
                FunRewardRuntime.LogPatchException(exception);
            }
        }

        [HarmonyPostfix]
        private static void Postfix(T_Citizen __instance, ServiceCompletionState __state)
        {
            FunRewardRuntime.ApplySafely(__instance, __state);
        }
    }
}
