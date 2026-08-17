using System;
using HarmonyLib;
using SleepAcceleration.Runtime;

namespace SleepAcceleration.Patches
{
    [HarmonyPatch(
        typeof(SystemMgr),
        nameof(SystemMgr.ApplyUserGameSpeed),
        new Type[] { typeof(float) })]
    internal static class UserSpeedChangePatch
    {
        [HarmonyPostfix]
        private static void Postfix(SystemMgr __instance, float value)
        {
            SleepAccelerationRuntime.NotifyUserSpeedChangedSafely(__instance, value);
        }
    }
}
