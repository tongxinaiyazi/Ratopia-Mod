using System;
using CasselGames.Input;
using HarmonyLib;

namespace GodViewManagement.Patches
{
    [HarmonyPatch(typeof(T_Queen), "Update")]
    internal static class QueenUpdateInputScopePatch
    {
        private static void Prefix(ref IDisposable __state)
        {
            __state = Plugin.EnterQueenInputUpdateScope();
        }

        private static void Postfix(IDisposable __state)
        {
            __state?.Dispose();
        }

        private static Exception Finalizer(Exception __exception, IDisposable __state)
        {
            __state?.Dispose();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(InputMgr), nameof(InputMgr.GetKey), new Type[] { typeof(HotKeyName), typeof(bool) })]
    internal static class DirectionalInputGetKeyPatch
    {
        private static bool Prefix(HotKeyName __0, ref bool __result)
        {
            if (!Plugin.ShouldSuppressQueenDirection(__0))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
