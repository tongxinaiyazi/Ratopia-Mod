using HarmonyLib;
using SleepAcceleration.Runtime;

namespace SleepAcceleration.Patches
{
    [HarmonyPatch(typeof(T_Queen), "Update")]
    internal static class QueenUpdatePatch
    {
        [HarmonyPostfix]
        private static void Postfix(T_Queen __instance)
        {
            SleepAccelerationRuntime.TickSafely(__instance);
        }
    }
}
